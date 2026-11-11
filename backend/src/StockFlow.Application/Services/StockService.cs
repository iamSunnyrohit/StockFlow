using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Stock;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;

namespace StockFlow.Application.Services;

public class StockService : IStockService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IStockRepository _stockRepository;

    public StockService(
        IInventoryRepository inventoryRepository,
        IWarehouseRepository warehouseRepository,
        IStockRepository stockRepository)
    {
        _inventoryRepository = inventoryRepository;
        _warehouseRepository = warehouseRepository;
        _stockRepository = stockRepository;
    }

    public async Task<StockChangeResultDto> ReceiveAsync(
        StockChangeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateQuantity(request.Quantity);
        ValidateText(request.ReferenceNumber, request.Notes);
        await EnsureProductExistsAsync(request.ProductId, cancellationToken);

        var warehouse = await GetWarehouseAsync(
            request.WarehouseId, cancellationToken);
        EnsureActive(warehouse);

        var newInventories = new List<Inventory>();
        var inventory = await GetOrCreateInventoryAsync(
            request.ProductId, request.WarehouseId,
            newInventories, cancellationToken);

        inventory.QuantityOnHand += request.Quantity;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        var movement = CreateMovement(
            request.ProductId, request.WarehouseId,
            StockMovementType.Purchase, request.Quantity,
            request.ReferenceNumber, request.Notes);

        await _stockRepository.ApplyAsync(
            newInventories, new[] { movement }, cancellationToken);

        return BuildResult(inventory, null, movement);
    }

    public async Task<StockChangeResultDto> IssueAsync(
        StockChangeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateQuantity(request.Quantity);
        ValidateText(request.ReferenceNumber, request.Notes);
        await EnsureProductExistsAsync(request.ProductId, cancellationToken);
        await GetWarehouseAsync(request.WarehouseId, cancellationToken);

        var inventory = await GetExistingInventoryAsync(
            request.ProductId, request.WarehouseId, request.Quantity,
            cancellationToken);

        inventory.QuantityOnHand -= request.Quantity;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        var movement = CreateMovement(
            request.ProductId, request.WarehouseId,
            StockMovementType.Sale, request.Quantity,
            request.ReferenceNumber, request.Notes);

        await _stockRepository.ApplyAsync(
            Array.Empty<Inventory>(), new[] { movement }, cancellationToken);

        return BuildResult(inventory, null, movement);
    }

    public async Task<StockChangeResultDto> AdjustAsync(
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.NewQuantity < 0)
            throw new ValidationException("New quantity cannot be negative.");

        if (string.IsNullOrWhiteSpace(request.Notes))
            throw new ValidationException(
                "A reason is required when adjusting stock.");

        ValidateText(null, request.Notes);
        await EnsureProductExistsAsync(request.ProductId, cancellationToken);
        await GetWarehouseAsync(request.WarehouseId, cancellationToken);

        var inventory = await _inventoryRepository
            .GetByProductAndWarehouseAsync(
                request.ProductId, request.WarehouseId, cancellationToken);

        if (inventory is null)
        {
            throw new ValidationException(
                "No inventory record exists for this product and warehouse. " +
                "Receive stock first.");
        }

        var difference = request.NewQuantity - inventory.QuantityOnHand;

        if (difference == 0)
        {
            throw new ValidationException(
                "The new quantity matches the current quantity.");
        }

        inventory.QuantityOnHand = request.NewQuantity;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        // Adjustment movements store the signed difference.
        var movement = CreateMovement(
            request.ProductId, request.WarehouseId,
            StockMovementType.Adjustment, difference,
            null, request.Notes);

        await _stockRepository.ApplyAsync(
            Array.Empty<Inventory>(), new[] { movement }, cancellationToken);

        return BuildResult(inventory, null, movement);
    }

    public async Task<StockChangeResultDto> TransferAsync(
        TransferStockRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateQuantity(request.Quantity);
        ValidateText(request.ReferenceNumber, request.Notes);

        if (request.SourceWarehouseId == request.DestinationWarehouseId)
        {
            throw new ValidationException(
                "Source and destination warehouses must be different.");
        }

        await EnsureProductExistsAsync(request.ProductId, cancellationToken);
        await GetWarehouseAsync(request.SourceWarehouseId, cancellationToken);

        var destination = await GetWarehouseAsync(
            request.DestinationWarehouseId, cancellationToken);
        EnsureActive(destination);

        var source = await GetExistingInventoryAsync(
            request.ProductId, request.SourceWarehouseId, request.Quantity,
            cancellationToken);

        var newInventories = new List<Inventory>();
        var target = await GetOrCreateInventoryAsync(
            request.ProductId, request.DestinationWarehouseId,
            newInventories, cancellationToken);

        var now = DateTime.UtcNow;
        source.QuantityOnHand -= request.Quantity;
        source.UpdatedAtUtc = now;
        target.QuantityOnHand += request.Quantity;
        target.UpdatedAtUtc = now;

        var reference = string.IsNullOrWhiteSpace(request.ReferenceNumber)
            ? $"TRF-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
            : request.ReferenceNumber.Trim();

        var outMovement = CreateMovement(
            request.ProductId, request.SourceWarehouseId,
            StockMovementType.TransferOut, request.Quantity,
            reference, request.Notes);

        var inMovement = CreateMovement(
            request.ProductId, request.DestinationWarehouseId,
            StockMovementType.TransferIn, request.Quantity,
            reference, request.Notes);

        await _stockRepository.ApplyAsync(
            newInventories, new[] { outMovement, inMovement },
            cancellationToken);

        return BuildResult(source, target.QuantityOnHand, outMovement, inMovement);
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(
        Guid? productId,
        Guid? warehouseId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 500);

        var movements = await _stockRepository.GetMovementsAsync(
            productId, warehouseId, limit, cancellationToken);

        return movements.Select(MapToDto).ToList();
    }

    // ---------- helpers ----------

    private async Task EnsureProductExistsAsync(
        Guid productId, CancellationToken cancellationToken)
    {
        if (!await _inventoryRepository.ProductExistsAsync(
                productId, cancellationToken))
        {
            throw new ValidationException("The specified product does not exist.");
        }
    }

    private async Task<Warehouse> GetWarehouseAsync(
        Guid warehouseId, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(
            warehouseId, cancellationToken);

        return warehouse ?? throw new ValidationException(
            "The specified warehouse does not exist.");
    }

    private static void EnsureActive(Warehouse warehouse)
    {
        if (!warehouse.IsActive)
        {
            throw new ValidationException(
                $"Warehouse '{warehouse.Code}' is inactive.");
        }
    }

    private async Task<Inventory> GetExistingInventoryAsync(
        Guid productId, Guid warehouseId, decimal required,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository
            .GetByProductAndWarehouseAsync(
                productId, warehouseId, cancellationToken);

        if (inventory is null || inventory.QuantityOnHand < required)
        {
            var available = inventory?.QuantityOnHand ?? 0;
            throw new ValidationException(
                $"Insufficient stock. Available: {available}, requested: {required}.");
        }

        return inventory;
    }

    private async Task<Inventory> GetOrCreateInventoryAsync(
        Guid productId, Guid warehouseId,
        List<Inventory> newInventories,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository
            .GetByProductAndWarehouseAsync(
                productId, warehouseId, cancellationToken);

        if (inventory is not null)
        {
            return inventory;
        }

        inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityOnHand = 0,
            ReorderLevel = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

        newInventories.Add(inventory);
        return inventory;
    }

    private static StockMovement CreateMovement(
        Guid productId, Guid warehouseId, StockMovementType type,
        decimal quantity, string? reference, string? notes)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            Type = type,
            Quantity = quantity,
            ReferenceNumber = string.IsNullOrWhiteSpace(reference)
                ? null : reference.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OccurredAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ValidationException("Quantity must be greater than zero.");
    }

    private static void ValidateText(string? reference, string? notes)
    {
        if (reference?.Trim().Length > 64)
            throw new ValidationException(
                "Reference number cannot exceed 64 characters.");

        if (notes?.Trim().Length > 1000)
            throw new ValidationException(
                "Notes cannot exceed 1000 characters.");
    }

    private static StockChangeResultDto BuildResult(
        Inventory inventory,
        decimal? destinationQuantity,
        params StockMovement[] movements)
    {
        return new StockChangeResultDto
        {
            ProductId = inventory.ProductId,
            WarehouseId = inventory.WarehouseId,
            QuantityOnHand = inventory.QuantityOnHand,
            DestinationQuantityOnHand = destinationQuantity,
            Movements = movements.Select(MapToDto).ToList()
        };
    }

    private static StockMovementDto MapToDto(StockMovement movement)
    {
        return new StockMovementDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            WarehouseId = movement.WarehouseId,
            Type = movement.Type.ToString(),
            Quantity = movement.Quantity,
            ReferenceNumber = movement.ReferenceNumber,
            Notes = movement.Notes,
            OccurredAtUtc = movement.OccurredAtUtc
        };
    }
}