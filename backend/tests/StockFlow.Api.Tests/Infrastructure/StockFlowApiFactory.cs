using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Tests.Infrastructure;

public class StockFlowApiFactory : WebApplicationFactory<Program>
{
    public Mock<IProductService> ProductServiceMock { get; } = new();

    public Mock<ICategoryService> CategoryServiceMock { get; } = new();

    public Mock<IWarehouseService> WarehouseServiceMock { get; } = new();

    public Mock<IInventoryService> InventoryServiceMock { get; } = new();

    public Mock<IStockService> StockServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Port=15432;Database=stockflow_test;Username=test;Password=test"
            };

            configuration.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IProductService>();
            services.RemoveAll<ICategoryService>();
            services.RemoveAll<IWarehouseService>();
            services.RemoveAll<IInventoryService>();
            services.RemoveAll<IStockService>();


            services.AddSingleton(ProductServiceMock.Object);
            services.AddSingleton(CategoryServiceMock.Object);
            services.AddSingleton(WarehouseServiceMock.Object);
            services.AddSingleton(InventoryServiceMock.Object);
            services.AddSingleton(StockServiceMock.Object);
        });
    }
}
