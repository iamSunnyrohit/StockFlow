using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Warehouses;
using StockFlow.Application.Interfaces.Services;

using StockFlow.Api.Tests.Infrastructure;

namespace StockFlow.Api.Tests.Warehouses;

public class WarehouseApiTests
{
    [Fact]
    public async Task GetAll_ShouldReturnOk_WithWarehouses()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var warehouses = new List<WarehouseDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Main Warehouse",
                Code = "WH-MAIN",
                Address = "Main Street",
                City = "Hyderabad",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Mumbai Distribution Center",
                Code = "WH-MUM",
                Address = "Industrial Area",
                City = "Mumbai",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        factory.WarehouseServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouses);

        var response = await client.GetAsync("/api/warehouses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<WarehouseDto>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Main Warehouse", result[0].Name);
        Assert.Equal("WH-MAIN", result[0].Code);
    }


    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenWarehouseDoesNotExist()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var warehouseId = Guid.NewGuid();

        factory.WarehouseServiceMock
            .Setup(x => x.GetByIdAsync(
                warehouseId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((WarehouseDto?)null);

        var response = await client.GetAsync(
            $"/api/warehouses/{warehouseId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var request = new CreateWarehouseRequest
        {
            Name = "Main Warehouse",
            Code = "WH-MAIN",
            Address = "Main Street",
            City = "Hyderabad"
        };

        var createdWarehouse = new WarehouseDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            City = request.City,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        factory.WarehouseServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateWarehouseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdWarehouse);

        var response = await client.PostAsJsonAsync(
            "/api/warehouses",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<WarehouseDto>();

        Assert.NotNull(result);
        Assert.Equal("Main Warehouse", result.Name);
        Assert.Equal("WH-MAIN", result.Code);

        factory.WarehouseServiceMock.Verify(
            x => x.CreateAsync(
                It.IsAny<CreateWarehouseRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task Create_ShouldReturnConflict_WhenCodeAlreadyExists()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var request = new CreateWarehouseRequest
        {
            Name = "Duplicate Warehouse",
            Code = "WH-MAIN",
            Address = "Another Street",
            City = "Hyderabad"
        };

        factory.WarehouseServiceMock
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateWarehouseRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ConflictException(
                    "A warehouse with code 'WH-MAIN' already exists."));

        var response = await client.PostAsJsonAsync(
            "/api/warehouses",
            request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }


    [Fact]
    public async Task Update_ShouldReturnOk_WhenWarehouseExists()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var warehouseId = Guid.NewGuid();

        var request = new UpdateWarehouseRequest
        {
            Name = "Updated Main Warehouse",
            Address = "Updated Street",
            City = "Bangalore"
        };

        var updatedWarehouse = new WarehouseDto
        {
            Id = warehouseId,
            Name = request.Name,
            Code = "WH-MAIN",
            Address = request.Address,
            City = request.City,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        factory.WarehouseServiceMock
            .Setup(x => x.UpdateAsync(
                warehouseId,
                It.IsAny<UpdateWarehouseRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedWarehouse);

        var response = await client.PutAsJsonAsync(
            $"/api/warehouses/{warehouseId}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<WarehouseDto>();

        Assert.NotNull(result);
        Assert.Equal("Updated Main Warehouse", result.Name);
        Assert.Equal("Bangalore", result.City);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNoContent_WhenWarehouseExists()
    {
        using var factory = new StockFlowApiFactory();
        using var client = factory.CreateClient();

        var warehouseId = Guid.NewGuid();

        factory.WarehouseServiceMock
            .Setup(x => x.DeactivateAsync(
                warehouseId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await client.PatchAsync(
            $"/api/warehouses/{warehouseId}/deactivate",
            null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        factory.WarehouseServiceMock.Verify(
            x => x.DeactivateAsync(
                warehouseId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}