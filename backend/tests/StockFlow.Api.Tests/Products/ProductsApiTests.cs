using System.Net;
using System.Net.Http.Json;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Products;
using System.Text.Json;
using StockFlow.Application.DTOs.Common;
using Xunit;

namespace StockFlow.Api.Tests.Products;

public class ProductsApiTests
{
    [Fact]
    public async Task GetById_WhenProductDoesNotExist_Returns404()
    {
        // Arrange
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var productId = Guid.NewGuid();

        factory.ProductServiceMock
            .Setup(service => service.GetByIdAsync(
                productId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new NotFoundException(
                    $"Product with ID '{productId}' was not found."
                )
            );

        // Act
        var response = await client.GetAsync(
            $"/api/products/{productId}"
        );

        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Resource not found", body);
        Assert.Contains(productId.ToString(), body);
    }

    [Fact]
    public async Task Create_WhenSkuAlreadyExists_Returns409()
    {
        // Arrange
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.ProductServiceMock
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateProductRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ConflictException(
                    "A product with SKU 'GAME-001' already exists."
                )
            );

        var request = new CreateProductRequest
        {
            SKU = "GAME-001",
            Name = "Duplicate Gaming Mouse",
            Description = "Testing duplicate SKU",
            CategoryId = Guid.NewGuid(),
            UnitPrice = 99.99m,
            UnitCost = 50.00m
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/products",
            request
        );

        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Resource conflict", body);
        Assert.Contains("GAME-001", body);
    }

    [Fact]
    public async Task Create_WhenProductDataIsInvalid_Returns400()
    {
        // Arrange
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.ProductServiceMock
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateProductRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ValidationException(
                    "Product price and cost cannot be negative."
                )
            );

        var request = new CreateProductRequest
        {
            SKU = "INVALID-001",
            Name = "Invalid Product",
            Description = "Testing validation",
            CategoryId = Guid.NewGuid(),
            UnitPrice = -100m,
            UnitCost = 50m
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/products",
            request
        );

        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Validation failed", body);
        Assert.Contains("cannot be negative", body);
    }



[Fact]
public async Task Create_WhenProductIsValid_Returns201()
{
    // Arrange
    using var factory = new StockFlowApiFactory();
    using var client = factory.CreateClient();

    var product = new ProductDto
    {
        Id = Guid.NewGuid(),
        SKU = "TEST-001",
        Name = "Test Product",
        Description = "Created by integration test",
        CategoryId = Guid.NewGuid(),
        CategoryName = "Electronics",
        UnitPrice = 99.99m,
        UnitCost = 50.00m,
        IsActive = true,
        CreatedAtUtc = DateTime.UtcNow
    };

    factory.ProductServiceMock
        .Setup(service => service.CreateAsync(
            It.IsAny<CreateProductRequest>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(product);

    var request = new CreateProductRequest
    {
        SKU = "TEST-001",
        Name = "Test Product",
        Description = "Created by integration test",
        CategoryId = product.CategoryId,
        UnitPrice = 99.99m,
        UnitCost = 50.00m
    };

    // Act
    var response = await client.PostAsJsonAsync(
        "/api/products",
        request
    );

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.Contains(product.Id.ToString(),
        response.Headers.Location.ToString());
   }


[Fact]
public async Task GetAll_ReturnsPagedProducts()
{
    // Arrange
    using var factory = new StockFlowApiFactory();
    using var client = factory.CreateClient();

    var product = new ProductDto
    {
        Id = Guid.NewGuid(),
        SKU = "TEST-001",
        Name = "Test Product",
        Description = "Test product description",
        CategoryId = Guid.NewGuid(),
        CategoryName = "Electronics",
        UnitPrice = 99.99m,
        UnitCost = 50.00m,
        IsActive = true,
        CreatedAtUtc = DateTime.UtcNow
    };

    var pagedResult = new PagedResult<ProductDto>
    {
        Items = new List<ProductDto> { product },
        Page = 1,
        PageSize = 20,
        TotalCount = 1
    };

    factory.ProductServiceMock
        .Setup(service => service.GetAllAsync(
            It.IsAny<ProductQueryParameters>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(pagedResult);

    // Act
    var response = await client.GetAsync("/api/products");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var json = await response.Content.ReadAsStringAsync();
    using var document = JsonDocument.Parse(json);

    var root = document.RootElement;

    Assert.Equal(1, root.GetProperty("page").GetInt32());
    Assert.Equal(20, root.GetProperty("pageSize").GetInt32());
    Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
    Assert.Single(root.GetProperty("items").EnumerateArray());
}


[Fact]
public async Task GetAll_WithPaginationParameters_PassesParametersToService()
{
    // Arrange
    using var factory = new StockFlowApiFactory();
    using var client = factory.CreateClient();

    var pagedResult = new PagedResult<ProductDto>
    {
        Items = new List<ProductDto>(),
        Page = 2,
        PageSize = 2,
        TotalCount = 4
    };

    factory.ProductServiceMock
        .Setup(service => service.GetAllAsync(
            It.IsAny<ProductQueryParameters>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(pagedResult);

    // Act
    var response = await client.GetAsync(
        "/api/products?page=2&pageSize=2"
    );

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    factory.ProductServiceMock.Verify(
        service => service.GetAllAsync(
            It.Is<ProductQueryParameters>(parameters =>
                parameters.Page == 2 &&
                parameters.PageSize == 2),
            It.IsAny<CancellationToken>()),
        Times.Once
    );
}


}
