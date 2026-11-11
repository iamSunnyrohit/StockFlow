using Moq;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Stock;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Services;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;

namespace StockFlow.Api.Tests.Stock;

public class StockServiceTests
{
    private sealed class Fixture
    {
        public Mock<IInventoryRepository> Inventories { get; } = new();
        public Mock<IWarehouseRepository> Warehouses { get; } = new();
        public Mock<IStockRepository> Stock { get; } = new();

        public Guid ProductId { get; } = Guid.NewGuid();
        public List<Inventory> SavedInventories { get; } = new();
        public List<StockMovement> SavedMovements { get; } = new();

        public Fixture()
        {
            Inventories
                .Setup(x => x.ProductExistsAsync(
                    ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

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

        public StockService Service =>
            new(Inventories.Object, Warehouses.Object, Stock.Object);

        public Warehouse AddWarehouse(bool active = true)
        {
            var warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Code = "WH-" + Guid.NewGuid().ToString("N")[..4],
                IsActive = active
            };

            Warehouses
                .Setup(x => x.GetByIdAsync(
                    warehouse.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            return warehouse;
        }

        public Inventory AddInventory(Guid warehouseId, decimal quantity)
        {
            var inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = ProductId,
                WarehouseId = warehouseId,
                QuantityOnHand = quantity
            };

            Inventories
                .Setup(x => x.GetByProductAndWarehouseAsync(
                    ProductId, warehouseId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);

            return inventory;
        }

        public void VerifyNothingSaved() =>
            Stock.Verify(x => x.ApplyAsync(
                It.IsAny<IReadOnlyCollection<Inventory>>(),
                It.IsAny<IReadOnlyCollection<StockMovement>>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    private static StockChangeRequest Change(
        Fixture f, Guid warehouseId, decimal quantity) => new()
    {
        ProductId = f.ProductId,
        WarehouseId = warehouseId,
        Quantity = quantity
    };

    [Fact]
    public async Task Receive_WhenInventoryExists_IncreasesQuantity()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        var inventory = f.AddInventory(warehouse.Id, 10);

        var result = await f.Service.ReceiveAsync(Change(f, warehouse.Id, 5));

        Assert.Equal(15, inventory.QuantityOnHand);
        Assert.Equal(15, result.QuantityOnHand);
        var movement = Assert.Single(f.SavedMovements);
        Assert.Equal(StockMovementType.Purchase, movement.Type);
        Assert.Equal(5, movement.Quantity);
    }

    [Fact]
    public async Task Receive_WhenNoInventoryRow_CreatesRow()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        f.Inventories
            .Setup(x => x.GetByProductAndWarehouseAsync(
                f.ProductId, warehouse.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var result = await f.Service.ReceiveAsync(Change(f, warehouse.Id, 8));

        var created = Assert.Single(f.SavedInventories);
        Assert.Equal(8, created.QuantityOnHand);
        Assert.Equal(8, result.QuantityOnHand);
    }

    [Fact]
    public async Task Receive_WhenWarehouseInactive_Throws()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse(active: false);

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.ReceiveAsync(Change(f, warehouse.Id, 5)));

        f.VerifyNothingSaved();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Receive_WhenQuantityNotPositive_Throws(decimal quantity)
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.ReceiveAsync(Change(f, warehouse.Id, quantity)));

        f.VerifyNothingSaved();
    }

    [Fact]
    public async Task Issue_WhenEnoughStock_DecreasesQuantity()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        var inventory = f.AddInventory(warehouse.Id, 10);

        await f.Service.IssueAsync(Change(f, warehouse.Id, 4));

        Assert.Equal(6, inventory.QuantityOnHand);
        Assert.Equal(StockMovementType.Sale, Assert.Single(f.SavedMovements).Type);
    }

    [Fact]
    public async Task Issue_WhenInsufficientStock_ThrowsAndSavesNothing()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        var inventory = f.AddInventory(warehouse.Id, 3);

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.IssueAsync(Change(f, warehouse.Id, 4)));

        Assert.Equal(3, inventory.QuantityOnHand);
        f.VerifyNothingSaved();
    }

    [Fact]
    public async Task Adjust_WhenNotesMissing_Throws()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        f.AddInventory(warehouse.Id, 10);

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.AdjustAsync(new AdjustStockRequest
            {
                ProductId = f.ProductId,
                WarehouseId = warehouse.Id,
                NewQuantity = 7,
                Notes = "  "
            }));

        f.VerifyNothingSaved();
    }

    [Fact]
    public async Task Adjust_SetsNewQuantity_AndRecordsSignedDifference()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        var inventory = f.AddInventory(warehouse.Id, 10);

        await f.Service.AdjustAsync(new AdjustStockRequest
        {
            ProductId = f.ProductId,
            WarehouseId = warehouse.Id,
            NewQuantity = 7,
            Notes = "Damaged goods"
        });

        Assert.Equal(7, inventory.QuantityOnHand);
        var movement = Assert.Single(f.SavedMovements);
        Assert.Equal(StockMovementType.Adjustment, movement.Type);
        Assert.Equal(-3, movement.Quantity);
    }

    [Fact]
    public async Task Transfer_WhenSameWarehouse_Throws()
    {
        var f = new Fixture();
        var warehouse = f.AddWarehouse();
        f.AddInventory(warehouse.Id, 10);

        await Assert.ThrowsAsync<ValidationException>(() =>
            f.Service.TransferAsync(new TransferStockRequest
            {
                ProductId = f.ProductId,
                SourceWarehouseId = warehouse.Id,
                DestinationWarehouseId = warehouse.Id,
                Quantity = 2
            }));

        f.VerifyNothingSaved();
    }

    [Fact]
    public async Task Transfer_MovesStock_AndWritesTwoLinkedMovements()
    {
        var f = new Fixture();
        var source = f.AddWarehouse();
        var destination = f.AddWarehouse();
        var sourceInventory = f.AddInventory(source.Id, 10);
        f.Inventories
            .Setup(x => x.GetByProductAndWarehouseAsync(
                f.ProductId, destination.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        var result = await f.Service.TransferAsync(new TransferStockRequest
        {
            ProductId = f.ProductId,
            SourceWarehouseId = source.Id,
            DestinationWarehouseId = destination.Id,
            Quantity = 4
        });

        Assert.Equal(6, sourceInventory.QuantityOnHand);
        Assert.Equal(6, result.QuantityOnHand);
        Assert.Equal(4, result.DestinationQuantityOnHand);

        Assert.Equal(2, f.SavedMovements.Count);
        var outMove = f.SavedMovements.Single(m => m.Type == StockMovementType.TransferOut);
        var inMove = f.SavedMovements.Single(m => m.Type == StockMovementType.TransferIn);
        Assert.Equal(outMove.ReferenceNumber, inMove.ReferenceNumber);
        Assert.NotNull(outMove.ReferenceNumber);

        var created = Assert.Single(f.SavedInventories);
        Assert.Equal(destination.Id, created.WarehouseId);
    }
}