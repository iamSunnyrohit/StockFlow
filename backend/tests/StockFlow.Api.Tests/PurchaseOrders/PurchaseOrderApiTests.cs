using System.Net;
using System.Net.Http.Json;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.PurchaseOrders;
using StockFlow.Domain.Enums;

namespace StockFlow.Api.Tests.PurchaseOrders;

public class PurchaseOrderApiTests
{
    private static object CreateBody() => new
    {
        supplierId = Guid.NewGuid(),
        taxRatePercent = 18,
        items = new[]
        {
            new { productId = Guid.NewGuid(), quantity = 2, unitCost = 10 }
        }
    };

    [Fact]
    public async Task GetAll_PassesFiltersToService()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var supplierId = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<PurchaseOrderQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<PurchaseOrderDto>
            {
                Items = new List<PurchaseOrderDto>(),
                Page = 2,
                PageSize = 5,
                TotalCount = 0
            });

        var response = await client.GetAsync(
            $"/api/purchase-orders?status=Confirmed&supplierId={supplierId}&page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        factory.PurchaseOrderServiceMock.Verify(x => x.GetAllAsync(
            It.Is<PurchaseOrderQueryParameters>(p =>
                p.Status == OrderStatus.Confirmed &&
                p.SupplierId == supplierId &&
                p.Page == 2 &&
                p.PageSize == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenMissing_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrderDto?)null);

        var response = await client.GetAsync($"/api/purchase-orders/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenValid_Returns201WithLocation()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreatePurchaseOrderRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PurchaseOrderDto { Id = id, Status = "Draft" });

        var response = await client.PostAsJsonAsync("/api/purchase-orders", CreateBody());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains(id.ToString(), response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Create_WhenInvalid_Returns400()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreatePurchaseOrderRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("The specified supplier is inactive."));

        var response = await client.PostAsJsonAsync("/api/purchase-orders", CreateBody());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenOrderMissing_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.UpdateAsync(
                id,
                It.IsAny<UpdatePurchaseOrderRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrderDto?)null);

        var response = await client.PutAsJsonAsync(
            $"/api/purchase-orders/{id}", CreateBody());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Confirm_WhenOrderExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.ConfirmAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PurchaseOrderDto { Id = id, Status = "Confirmed" });

        var response = await client.PostAsync(
            $"/api/purchase-orders/{id}/confirm", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Receive_WhenOrderExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.ReceiveAsync(
                id,
                It.IsAny<ReceivePurchaseOrderRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PurchaseOrderDto { Id = id, Status = "Completed" });

        var response = await client.PostAsJsonAsync(
            $"/api/purchase-orders/{id}/receive",
            new { warehouseId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Receive_WhenConcurrentChange_Returns409()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.ReceiveAsync(
                id,
                It.IsAny<ReceivePurchaseOrderRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Stock was changed by another request."));

        var response = await client.PostAsJsonAsync(
            $"/api/purchase-orders/{id}/receive",
            new { warehouseId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_WhenOrderMissing_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.PurchaseOrderServiceMock
            .Setup(x => x.CancelAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrderDto?)null);

        var response = await client.PostAsync(
            $"/api/purchase-orders/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
