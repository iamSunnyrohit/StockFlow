using Moq;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.PurchaseOrders;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Services;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;

namespace StockFlow.Api.Tests.PurchaseOrders;

public class PurchaseOrderServiceTests
{
    private sealed class Fixture
    {
        public Mock<IPurchaseOrderRepository> Orders { get; } = new();
        public Mock<ISupplierRepository> Suppliers { get; } = new();
        public Mock<IProductRepository> Products { get; } = new();
        public Mock<IWarehouseRepository> Warehouses { get; } = new();
        public Mock<IInventoryRepository> Inventories { get; } = new();
        public Mock<IStockRepository> Stock { get; } = new();

        public List<Inventory> SavedInventories { get; } = new();
        public List<StockMovement> SavedMovements { get; } = new();

        public Fixture()
        {
            Stock
                .Setup(x => x.ApplyAsync(
                    It.IsAny<IReadOnlyCollection<Inventory>>(),
                    It.IsAny<IReadOnlyCollection<StockMovement>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<Inventory>,
                          IReadOnlyCollection<StockMovement>,
                          CancellationToken>((inv, mov, _) =>
                {
                    SavedInventories.AddRange(inv);
                    SavedMovements.AddRange(mov);
                })
                .Returns(Task.CompletedTask);
        }

        public PurchaseOrderService Service => new(
            Orders.Object, Suppliers.Object, Products.Object,
            Warehouses.Object, Inventories.Object, Stock.Object);

        public Supplier AddSupplier(bool active = true)
        {
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = "Acme",
                IsActive = active
            };

            Suppliers
                .Setup(x => x.GetByIdAsync(
                    supplier.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplier);

            return supplier;
        }

        public Product AddProduct(decimal cost = 10m, bool active = true)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                SKU = "SKU-" + Guid.NewGuid().ToString("N")[..4],
                Name = "Widget",
                UnitCost = cost,
                IsActive = active
            };

            Products
                .Setup(x => x.GetByIdAsync(
                    product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            return product;
        }

        public Warehouse AddWarehouse(bool active = true)
        {
            var warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = "Main",
                Code = "WH-" + Guid.NewGuid().ToString("N")[..4],
                IsActive = active
            };

            Warehouses
                .Setup(x => x.GetByIdAsync(
                    warehouse.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            return warehouse;
        }

        public PurchaseOrder AddOrder(
            OrderStatus status,
            params (Product Product, decimal Quantity)[] lines)
        {
            var supplier = AddSupplier();

            var order = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = "PO-TEST-0001",
                SupplierId = supplier.Id,
                Supplier = supplier,
                Status = status
            };

            foreach (var (product, quantity) in lines)
            {
                order.Items.Add(new PurchaseOrderItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = order.Id,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = quantity,
                    UnitCost = product.UnitCost,
                    LineTotal = quantity * product.UnitCost
                });
            }

            Orders
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            return order;
        }

        public Inventory SetInventory(Guid productId, Guid warehouseId, decimal quantity)
        {
            var inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                WarehouseId = warehouseId,
                QuantityOnHand = quantity
            };

            Inventories
                .Setup(x => x.GetByProductAndWarehouseAsync(
                    productId, warehouseId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            return inventory;
        }

        public void NoInventory(Guid productId, Guid warehouseId) =>
            Inventories
                .Setup(x => x.GetByProductAndWarehouseAsync(
                    productId, warehouseId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Inventory?)null);

        public void VerifyNothingReceived() =>
            Stock.Verify(x => x.ApplyAsync(
                It.IsAny<IReadOnlyCollection<Inventory>>(),
                It.IsAny<IReadOnlyCollection<StockMovement>>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- create ----------

    [Fact]
    public async Task Create_ComputesLineTotalsSubtotalTaxAndTotal()
    {
        var f = new Fixture();
        var supplier = f.AddSupplier();
        var p1 = f.AddProduct();
        var p2 = f.AddProduct();

        var result = await f.Service.CreateAsync(new CreatePurchaseOrderRequest
        {
            SupplierId = supplier.Id,
            TaxRatePercent = 18,
            Items = new()
            {
                new() { ProductId = p1.Id, Quantity = 2, UnitCost = 10 },
                new() { ProductId = p2.Id, Quantity = 3, UnitCost = 5 }
            }
        });

        Assert.Equal(35m, result.Subtotal);
        Assert.Equal(6.30m, result.TaxAmount);
        Assert.Equal(41.30m, result.TotalAmount);
        Assert.Equal("Draft", result.Status);
        Assert.StartsWith("PO-", result.OrderNumber);
        Assert.Equal(2, result.Items.Count);

        f.Orders.Verify(x => x.AddAsync(
            It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnitCostOmitted_UsesProductCost()
    {
        var f = new Fixture();
        var supplier = f.AddSupplier();
        var product = f.AddProduct(cost: 12.50m);

        var result = await f.Service.CreateAsync(new CreatePurchaseOrderRequest
        {
            SupplierId = supplier.Id,
            Items = new() { new() { ProductId = product.Id, Quantity = 4 } }
        });

        var item = Assert.Single(result.Items);
        Assert.Equal(12.50m, item.UnitCost);
        Assert.Equal(50m, item.LineTotal);
    }

    [Fact]
    public async Task Create_WhenSameProductTwice_Throws()
    {
        var f = new Fixture();
        var supplier = f.AddSupplier();
        var product = f.AddProduct();

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.CreateAsync(new CreatePurchaseOrderRequest
            {
                SupplierId = supplier.Id,
                Items = new()
                {
                    new() { ProductId = product.Id, Quantity = 1 },
                    new() { ProductId = product.Id, Quantity = 2 }
                }
            }));
    }

    [Fact]
    public async Task Create_WhenSupplierInactive_Throws()
    {
        var f = new Fixture();
        var supplier = f.AddSupplier(active: false);
        var product = f.AddProduct();

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.CreateAsync(new CreatePurchaseOrderRequest
            {
                SupplierId = supplier.Id,
                Items = new() { new() { ProductId = product.Id, Quantity = 1 } }
            }));

        f.Orders.Verify(x => x.AddAsync(
            It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenNoItems_Throws()
    {
        var f = new Fixture();
        var supplier = f.AddSupplier();

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.CreateAsync(new CreatePurchaseOrderRequest
            {
                SupplierId = supplier.Id
            }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Create_WhenQuantityNotPositive_Throws(decimal quantity)
    {
        var f = new Fixture();
        var supplier = f.AddSupplier();
        var product = f.AddProduct();

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.CreateAsync(new CreatePurchaseOrderRequest
            {
                SupplierId = supplier.Id,
                Items = new() { new() { ProductId = product.Id, Quantity = quantity } }
            }));
    }

    // ---------- update ----------

    [Fact]
    public async Task Update_WhenOrderMissing_ReturnsNull()
    {
        var f = new Fixture();

        f.Orders
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        var result = await f.Service.UpdateAsync(
            Guid.NewGuid(), new UpdatePurchaseOrderRequest());

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_WhenNotDraft_Throws()
    {
        var f = new Fixture();
        var product = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Confirmed, (product, 1));

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.UpdateAsync(order.Id, new UpdatePurchaseOrderRequest
            {
                SupplierId = order.SupplierId,
                Items = new() { new() { ProductId = product.Id, Quantity = 5 } }
            }));

        f.Orders.Verify(x => x.UpdateAsync(
            It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenDraft_ReplacesItemsAndRecalculatesTotals()
    {
        var f = new Fixture();
        var oldProduct = f.AddProduct();
        var newProduct = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Draft, (oldProduct, 1));

        var result = await f.Service.UpdateAsync(order.Id, new UpdatePurchaseOrderRequest
        {
            SupplierId = order.SupplierId,
            TaxRatePercent = 10,
            Items = new() { new() { ProductId = newProduct.Id, Quantity = 4, UnitCost = 25 } }
        });

        Assert.NotNull(result);
        Assert.Equal(100m, result!.Subtotal);
        Assert.Equal(10m, result.TaxAmount);
        Assert.Equal(110m, result.TotalAmount);

        f.Orders.Verify(x => x.ReplaceItems(
            order, It.Is<IReadOnlyCollection<PurchaseOrderItem>>(i => i.Count == 1)),
            Times.Once);
        f.Orders.Verify(x => x.UpdateAsync(
            order, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- confirm / cancel ----------

    [Fact]
    public async Task Confirm_WhenDraft_SetsConfirmed()
    {
        var f = new Fixture();
        var product = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Draft, (product, 2));

        var result = await f.Service.ConfirmAsync(order.Id);

        Assert.Equal("Confirmed", result!.Status);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task Confirm_WhenNotDraft_Throws()
    {
        var f = new Fixture();
        var product = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Completed, (product, 2));

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.ConfirmAsync(order.Id));
    }

    [Fact]
    public async Task Cancel_WhenDraft_Cancels()
    {
        var f = new Fixture();
        var product = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Draft, (product, 2));

        var result = await f.Service.CancelAsync(order.Id);

        Assert.Equal("Cancelled", result!.Status);
    }

    [Fact]
    public async Task Cancel_WhenCompleted_Throws()
    {
        var f = new Fixture();
        var product = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Completed, (product, 2));

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.CancelAsync(order.Id));
    }

    // ---------- receive ----------

    [Fact]
    public async Task Receive_AddsStockWritesMovementsAndCompletesOrder()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        var existingProduct = f.AddProduct();
        var newProduct = f.AddProduct();
        var order = f.AddOrder(
            OrderStatus.Confirmed, (existingProduct, 5), (newProduct, 8));

        var existing = f.SetInventory(existingProduct.Id, warehouse.Id, 10);
        f.NoInventory(newProduct.Id, warehouse.Id);

        var result = await f.Service.ReceiveAsync(
            order.Id, new ReceivePurchaseOrderRequest { WarehouseId = warehouse.Id });

        Assert.Equal("Completed", result!.Status);
        Assert.Equal(OrderStatus.Completed, order.Status);

        Assert.Equal(15, existing.QuantityOnHand);

        var created = Assert.Single(f.SavedInventories);
        Assert.Equal(newProduct.Id, created.ProductId);
        Assert.Equal(8, created.QuantityOnHand);

        Assert.Equal(2, f.SavedMovements.Count);
        Assert.All(f.SavedMovements, m =>
        {
            Assert.Equal(StockMovementType.Purchase, m.Type);
            Assert.Equal("PO-TEST-0001", m.ReferenceNumber);
            Assert.Equal(warehouse.Id, m.WarehouseId);
        });

        f.Stock.Verify(x => x.ApplyAsync(
            It.IsAny<IReadOnlyCollection<Inventory>>(),
            It.IsAny<IReadOnlyCollection<StockMovement>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Draft)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Receive_WhenOrderNotConfirmed_ThrowsAndChangesNothing(
        OrderStatus status)
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        var product = f.AddProduct();
        var order = f.AddOrder(status, (product, 5));

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.ReceiveAsync(
                order.Id, new ReceivePurchaseOrderRequest { WarehouseId = warehouse.Id }));

        Assert.Equal(status, order.Status);
        f.VerifyNothingReceived();
    }

    [Fact]
    public async Task Receive_WhenWarehouseInactive_ThrowsAndChangesNothing()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse(active: false);
        var product = f.AddProduct();
        var order = f.AddOrder(OrderStatus.Confirmed, (product, 5));

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.ReceiveAsync(
                order.Id, new ReceivePurchaseOrderRequest { WarehouseId = warehouse.Id }));

        Assert.Equal(OrderStatus.Confirmed, order.Status);
        f.VerifyNothingReceived();
    }
}
