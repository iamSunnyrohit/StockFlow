using System.Net;
using Moq;
using StockFlow.Api.Tests.Infrastructure;
using System.Net.Http.Json;
using StockFlow.Application.DTOs.Categories;
using Xunit;

namespace StockFlow.Api.Tests.Categories;

public class CategoryApiTests
{
    [Fact]
    public async Task GetAll_Returns200()
    {
        // Arrange
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        factory.CategoryServiceMock
            .Setup(service => service.GetAllAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CategoryDto>());

        // Act
        var response = await client.GetAsync("/api/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenCategoryDoesNotExist_Returns404()
    {
        // Arrange
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var categoryId = Guid.NewGuid();

        factory.CategoryServiceMock
            .Setup(service => service.GetByIdAsync(
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var response = await client.GetAsync(
            $"/api/categories/{categoryId}"
        );

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


[Fact]
public async Task Create_WhenCategoryIsValid_Returns201()
{
    // Arrange
    using var factory = new StockFlowApiFactory();
    using var client = factory.CreateClient();

    var category = new CategoryDto
    {
        Id = Guid.NewGuid(),
        Name = "Test Category",
        Description = "Created by integration test",
        IsActive = true,
        CreatedAtUtc = DateTime.UtcNow
    };

    factory.CategoryServiceMock
        .Setup(service => service.CreateAsync(
            It.IsAny<CreateCategoryRequest>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(category);

    var request = new CreateCategoryRequest
    {
        Name = "Test Category",
        Description = "Created by integration test"
    };

    // Act
    var response = await client.PostAsJsonAsync(
        "/api/categories",
        request
    );

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(response.Headers.Location);
    Assert.Contains(category.Id.ToString(),
        response.Headers.Location.ToString());
   
   }

}
