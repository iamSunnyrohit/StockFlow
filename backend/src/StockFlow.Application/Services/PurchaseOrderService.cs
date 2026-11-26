using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.PurchaseOrders;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;

namespace StockFlow.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _orders;
    private readonly ISupplierRepository _suppliers;
    private readonly IProductRepository _products;
    private readonly IWarehouseRepository _warehouses;
    private readonly IInventoryRepository _inventories;
    private readonly IStockRepository _stock;

    public PurchaseOrderService(
        IPurchaseOrderRepository orders,
        ISupplierRepository suppliers,
        IProductRepository products,
        IWarehouseRepository warehouses,
        IInventoryRepository inventories,
        IStockRepository stock)
    {
        _orders = orders;
        _suppliers = suppliers;
        _products = products;
        _warehouses = warehouses;
        _inventories = inventories;
        _stock = stock;
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetAllAsync(
        PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var normalized = new PurchaseOrderQueryParameters
        {
            Status = parameters.Status,
            SupplierId = parameters.SupplierId,
            Page = Math.Max(parameters.Page, 1),
            PageSize = Math.Clamp(parameters.PageSize, 1, 100)
        };

        var orders = await _orders.GetAllAsync(normalized, cancellationToken);
        var total = await _orders.CountAsync(normalized, cancellationToken);

        return new PagedResult<PurchaseOrderDto>
        {
            Items = orders.Select(o => MapToDto(o, o.Items)).ToList(),
            Page = normalized.Page,
            PageSize = normalized.PageSize,
            TotalCount = total
        };
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);
        return order is null ? null : MapToDto(order, order.Items);
    }

    public async Task<PurchaseOrderDto> CreateAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateHeader(request);

        var supplier = await GetActiveSupplierAsync(
            request.SupplierId, cancellationToken);

        var orderId = Guid.NewGuid();
        var items = await BuildItemsAsync(
            orderId, request.Items, cancellationToken);

        var order = new PurchaseOrder
        {
            Id = orderId,
            OrderNumber = GenerateOrderNumber(),
            SupplierId = supplier.Id,
            Supplier = supplier,
            OrderDateUtc = DateTime.UtcNow,
            ExpectedDeliveryDateUtc = request.ExpectedDeliveryDateUtc,
            Status = OrderStatus.Draft,
            Notes = NormalizeOptional(request.Notes),
            CreatedAtUtc = DateTime.UtcNow,
            Items = items
        };

        ApplyTotals(order, items, request.TaxRatePercent);

        await _orders.AddAsync(order, cancellationToken);

        return MapToDto(order, items);
    }

    public async Task<PurchaseOrderDto?> UpdateAsync(
        Guid id,
        UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.Status != OrderStatus.Draft)
        {
            throw new ValidationException("Only draft purchase orders can be edited.");
        }

        ValidateHeader(request);

        var supplier = await GetActiveSupplierAsync(
            request.SupplierId, cancellationToken);

        var items = await BuildItemsAsync(
            order.Id, request.Items, cancellationToken);

        _orders.ReplaceItems(order, items);

        order.SupplierId = supplier.Id;
        order.Supplier = supplier;
        order.ExpectedDeliveryDateUtc = request.ExpectedDeliveryDateUtc;
        order.Notes = NormalizeOptional(request.Notes);
        order.UpdatedAtUtc = DateTime.UtcNow;
        ApplyTotals(order, items, request.TaxRatePercent);

        await _orders.UpdateAsync(order, cancellationToken);

        return MapToDto(order, items);
    }

    public async Task<PurchaseOrderDto?> ConfirmAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.Status != OrderStatus.Draft)
        {
            throw new ValidationException(
                $"Only draft purchase orders can be confirmed. Current status: {order.Status}.");
        }

        if (order.Items.Count == 0)
        {
            throw new ValidationException(
                "A purchase order needs at least one item before it can be confirmed.");
        }

        if (order.Supplier is { IsActive: false })
        {
            throw new ValidationException("The supplier is inactive.");
        }

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _orders.UpdateAsync(order, cancellationToken);

        return MapToDto(order, order.Items);
    }

    public async Task<PurchaseOrderDto?> ReceiveAsync(
        Guid id,
        ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Notes?.Trim().Length > 1000)
        {
            throw new ValidationException("Notes cannot exceed 1000 characters.");
        }

        var order = await _orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.Status != OrderStatus.Confirmed)
        {
            throw new ValidationException(
                $"Only confirmed purchase orders can be received. Current status: {order.Status}.");
        }

        var warehouse = await _warehouses.GetByIdAsync(
            request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            throw new ValidationException("The specified warehouse does not exist.");
        }

        if (!warehouse.IsActive)
        {
            throw new ValidationException(
                $"Warehouse '{warehouse.Code}' is inactive.");
        }

        var now = DateTime.UtcNow;
        var newInventories = new List<Inventory>();
        var movements = new List<StockMovement>();

        foreach (var item in order.Items)
        {
            var inventory = await _inventories.GetByProductAndWarehouseAsync(
                item.ProductId, warehouse.Id, cancellationToken);

            if (inventory is null)
            {
                inventory = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    WarehouseId = warehouse.Id,
                    QuantityOnHand = 0,
                    ReorderLevel = 0,
                    CreatedAtUtc = now
                };

                newInventories.Add(inventory);
            }

            inventory.QuantityOnHand += item.Quantity;
            inventory.UpdatedAtUtc = now;

            movements.Add(new StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                WarehouseId = warehouse.Id,
                Type = StockMovementType.Purchase,
                Quantity = item.Quantity,
                ReferenceNumber = order.OrderNumber,
                Notes = NormalizeOptional(request.Notes),
                OccurredAtUtc = now,
                CreatedAtUtc = now
            });
        }

        order.Status = OrderStatus.Completed;
        order.UpdatedAtUtc = now;

        // The order, inventory changes and movements all live in the same
        // DbContext, so this single save commits them in ONE transaction.
        // The xmin token on the order makes a concurrent double-receive
        // fail with a 409 instead of adding the stock twice.
        await _stock.ApplyAsync(newInventories, movements, cancellationToken);

        return MapToDto(order, order.Items);
    }

    public async Task<PurchaseOrderDto?> CancelAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (order.Status is not (OrderStatus.Draft
            or OrderStatus.Pending
            or OrderStatus.Confirmed))
        {
            throw new ValidationException(
                $"A {order.Status} purchase order cannot be cancelled.");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _orders.UpdateAsync(order, cancellationToken);

        return MapToDto(order, order.Items);
    }

    // ---------- helpers ----------

    private static void ValidateHeader(CreatePurchaseOrderRequest request)
    {
        if (request.TaxRatePercent < 0 || request.TaxRatePercent > 100)
            throw new ValidationException(
                "Tax rate must be between 0 and 100 percent.");

        if (request.Notes?.Trim().Length > 1000)
            throw new ValidationException("Notes cannot exceed 1000 characters.");
    }

    private async Task<Supplier> GetActiveSupplierAsync(
        Guid supplierId, CancellationToken cancellationToken)
    {
        var supplier = await _suppliers.GetByIdAsync(
            supplierId, cancellationToken);

        if (supplier is null)
            throw new ValidationException("The specified supplier does not exist.");

        if (!supplier.IsActive)
            throw new ValidationException("The specified supplier is inactive.");

        return supplier;
    }

    private async Task<List<PurchaseOrderItem>> BuildItemsAsync(
        Guid orderId,
        List<PurchaseOrderItemRequest> lines,
        CancellationToken cancellationToken)
    {
        if (lines is null || lines.Count == 0)
            throw new ValidationException(
                "A purchase order needs at least one item.");

        if (lines.Select(l => l.ProductId).Distinct().Count() != lines.Count)
            throw new ValidationException(
                "Each product can appear only once per purchase order.");

        var items = new List<PurchaseOrderItem>();

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw new ValidationException(
                    "Item quantity must be greater than zero.");

            var product = await _products.GetByIdAsync(
                line.ProductId, cancellationToken);

            if (product is null)
                throw new ValidationException(
                    $"Product '{line.ProductId}' does not exist.");

            if (!product.IsActive)
                throw new ValidationException(
                    $"Product '{product.SKU}' is inactive.");

            var unitCost = line.UnitCost ?? product.UnitCost;

            if (unitCost < 0)
                throw new ValidationException("Unit cost cannot be negative.");

            items.Add(new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = orderId,
                ProductId = product.Id,
                Product = product,
                Quantity = line.Quantity,
                UnitCost = unitCost,
                LineTotal = Round(line.Quantity * unitCost),
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        return items;
    }

    private static void ApplyTotals(
        PurchaseOrder order,
        IEnumerable<PurchaseOrderItem> items,
        decimal taxRatePercent)
    {
        var subtotal = items.Sum(i => i.LineTotal);
        var tax = Round(subtotal * taxRatePercent / 100m);

        order.Subtotal = subtotal;
        order.TaxAmount = tax;
        order.TotalAmount = subtotal + tax;
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string GenerateOrderNumber() =>
        $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PurchaseOrderDto MapToDto(
        PurchaseOrder order, IEnumerable<PurchaseOrderItem> items)
    {
        return new PurchaseOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SupplierId = order.SupplierId,
            SupplierName = order.Supplier?.Name ?? string.Empty,
            OrderDateUtc = order.OrderDateUtc,
            ExpectedDeliveryDateUtc = order.ExpectedDeliveryDateUtc,
            Status = order.Status.ToString(),
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            CreatedAtUtc = order.CreatedAtUtc,
            Items = items.Select(i => new PurchaseOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductSku = i.Product?.SKU ?? string.Empty,
                ProductName = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}
