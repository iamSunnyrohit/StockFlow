using System.Net;
using System.Net.Http.Json;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Inventories;

namespace StockFlow.Api.Tests.Inventories;

public class InventoryApiTests
{
    [Fact]
    public async Task GetByWarehouse_WhenWarehouseExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var warehouseId = Guid.NewGuid();

        factory.InventoryServiceMock
            .Setup(x => x.GetByWarehouseAsync(
                warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryDto>
            {
                new() { Id = Guid.NewGuid(), WarehouseId = warehouseId }
            });

        var response = await client.GetAsync(
            $"/api/inventory/warehouse/{warehouseId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByWarehouse_WhenWarehouseDoesNotExist_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var warehouseId = Guid.NewGuid();

        factory.InventoryServiceMock
            .Setup(x => x.GetByWarehouseAsync(
                warehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<InventoryDto>?)null);

        var response = await client.GetAsync(
            $"/api/inventory/warehouse/{warehouseId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByProduct_WhenProductExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var productId = Guid.NewGuid();

        factory.InventoryServiceMock
            .Setup(x => x.GetByProductAsync(
                productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryDto>
            {
                new() { Id = Guid.NewGuid(), ProductId = productId }
            });

        var response = await client.GetAsync(
            $"/api/inventory/product/{productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductTotal_WhenProductExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var productId = Guid.NewGuid();

        factory.InventoryServiceMock
            .Setup(x => x.GetProductStockSummaryAsync(
                productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductStockSummaryDto
            {
                ProductId = productId,
                TotalQuantityOnHand = 150,
                WarehouseCount = 2
            });

        var response = await client.GetAsync(
            $"/api/inventory/product/{productId}/total");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<ProductStockSummaryDto>();

        Assert.NotNull(body);
        Assert.Equal(150, body!.TotalQuantityOnHand);
    }

    [Fact]
    public async Task Create_WhenRecordIsNew_Returns201()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.InventoryServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateInventoryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InventoryDto { Id = Guid.NewGuid() });

        var response = await client.PostAsJsonAsync("/api/inventory", new
        {
            productId = Guid.NewGuid(),
            warehouseId = Guid.NewGuid(),
            reorderLevel = 10
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenProductAndWarehouseAlreadyHaveRecord_Returns409()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.InventoryServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateInventoryRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException(
                "An inventory record for this product and warehouse already exists."));

        var response = await client.PostAsJsonAsync("/api/inventory", new
        {
            productId = Guid.NewGuid(),
            warehouseId = Guid.NewGuid(),
            reorderLevel = 10
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReorderLevel_WhenInventoryExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.InventoryServiceMock
            .Setup(x => x.UpdateReorderLevelAsync(
                id,
                It.IsAny<UpdateReorderLevelRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InventoryDto { Id = id, ReorderLevel = 25 });

        var response = await client.PatchAsJsonAsync(
            $"/api/inventory/{id}/reorder-level",
            new { reorderLevel = 25 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReorderLevel_WhenInventoryDoesNotExist_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.InventoryServiceMock
            .Setup(x => x.UpdateReorderLevelAsync(
                id,
                It.IsAny<UpdateReorderLevelRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryDto?)null);

        var response = await client.PatchAsJsonAsync(
            $"/api/inventory/{id}/reorder-level",
            new { reorderLevel = 25 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}