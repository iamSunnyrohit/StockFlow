using System.Net;
using System.Net.Http.Json;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Stock;

namespace StockFlow.Api.Tests.Stock;

public class StockApiTests
{
    private static object ChangeBody() => new
    {
        productId = Guid.NewGuid(),
        warehouseId = Guid.NewGuid(),
        quantity = 5
    };

    [Fact]
    public async Task Receive_WhenValid_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.StockServiceMock
            .Setup(x => x.ReceiveAsync(
                It.IsAny<StockChangeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockChangeResultDto { QuantityOnHand = 5 });

        var response = await client.PostAsJsonAsync(
            "/api/stock/receive", ChangeBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Receive_WhenConcurrentChange_Returns409()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.StockServiceMock
            .Setup(x => x.ReceiveAsync(
                It.IsAny<StockChangeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Stock was changed."));

        var response = await client.PostAsJsonAsync(
            "/api/stock/receive", ChangeBody());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Issue_WhenInsufficientStock_Returns400()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.StockServiceMock
            .Setup(x => x.IssueAsync(
                It.IsAny<StockChangeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Insufficient stock."));

        var response = await client.PostAsJsonAsync(
            "/api/stock/issue", ChangeBody());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Adjust_WhenValid_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.StockServiceMock
            .Setup(x => x.AdjustAsync(
                It.IsAny<AdjustStockRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockChangeResultDto { QuantityOnHand = 12 });

        var response = await client.PostAsJsonAsync("/api/stock/adjust", new
        {
            productId = Guid.NewGuid(),
            warehouseId = Guid.NewGuid(),
            newQuantity = 12,
            notes = "Stocktake correction"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_WhenValid_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.StockServiceMock
            .Setup(x => x.TransferAsync(
                It.IsAny<TransferStockRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockChangeResultDto
            {
                QuantityOnHand = 3,
                DestinationQuantityOnHand = 7
            });

        var response = await client.PostAsJsonAsync("/api/stock/transfer", new
        {
            productId = Guid.NewGuid(),
            sourceWarehouseId = Guid.NewGuid(),
            destinationWarehouseId = Guid.NewGuid(),
            quantity = 4
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMovements_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.StockServiceMock
            .Setup(x => x.GetMovementsAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockMovementDto>());

        var response = await client.GetAsync("/api/stock/movements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}