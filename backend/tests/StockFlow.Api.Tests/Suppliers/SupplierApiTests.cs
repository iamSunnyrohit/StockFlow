using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.Suppliers;

namespace StockFlow.Api.Tests.Suppliers;

public class SupplierApiTests
{
    private static readonly object ValidBody = new
    {
        name = "Acme Traders",
        contactPerson = "Priya",
        email = "priya@acme.example",
        city = "Chennai"
    };

    [Fact]
    public async Task GetAll_PassesQueryParametersToService()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.SupplierServiceMock
            .Setup(x => x.GetAllAsync(
                It.IsAny<SupplierQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<SupplierDto>
            {
                Items = new List<SupplierDto>(),
                Page = 2,
                PageSize = 5,
                TotalCount = 0
            });

        var response = await client.GetAsync(
            "/api/suppliers?search=acme&isActive=false&page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        factory.SupplierServiceMock.Verify(x => x.GetAllAsync(
            It.Is<SupplierQueryParameters>(p =>
                p.Search == "acme" &&
                p.IsActive == false &&
                p.Page == 2 &&
                p.PageSize == 5),
            It.IsAny<CancellationToken>()), Times.Once);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(2, document.RootElement.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task GetById_WhenSupplierExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SupplierDto { Id = id, Name = "Acme" });

        var response = await client.GetAsync($"/api/suppliers/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenSupplierMissing_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupplierDto?)null);

        var response = await client.GetAsync($"/api/suppliers/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenValid_Returns201WithLocation()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateSupplierRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SupplierDto { Id = id, Name = "Acme Traders" });

        var response = await client.PostAsJsonAsync("/api/suppliers", ValidBody);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(id.ToString(), response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Create_WhenNameExists_Returns409()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.SupplierServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateSupplierRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException(
                "A supplier named 'Acme Traders' already exists."));

        var response = await client.PostAsJsonAsync("/api/suppliers", ValidBody);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Acme Traders", body);
    }

    [Fact]
    public async Task Create_WhenDataInvalid_Returns400()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.SupplierServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateSupplierRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Supplier email is not valid."));

        var response = await client.PostAsJsonAsync("/api/suppliers", ValidBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenSupplierExists_Returns200()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.UpdateAsync(
                id,
                It.IsAny<UpdateSupplierRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SupplierDto { Id = id, Name = "Acme Traders" });

        var response = await client.PutAsJsonAsync($"/api/suppliers/{id}", ValidBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenSupplierMissing_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.UpdateAsync(
                id,
                It.IsAny<UpdateSupplierRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupplierDto?)null);

        var response = await client.PutAsJsonAsync($"/api/suppliers/{id}", ValidBody);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_WhenSupplierExists_Returns204()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.DeactivateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await client.PatchAsync(
            $"/api/suppliers/{id}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_WhenSupplierMissing_Returns404()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();

        factory.SupplierServiceMock
            .Setup(x => x.DeactivateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await client.PatchAsync(
            $"/api/suppliers/{id}/deactivate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}