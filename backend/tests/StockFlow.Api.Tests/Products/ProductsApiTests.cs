using System.Net;
using System.Net.Http.Json;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Products;
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
}
