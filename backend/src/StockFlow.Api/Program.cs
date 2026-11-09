using Microsoft.EntityFrameworkCore;
using StockFlow.Infrastructure.Persistence;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Services;
using StockFlow.Infrastructure.Persistence.Repositories;
using StockFlow.Api.Middleware;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found."
    );

builder.Services.AddDbContext<StockFlowDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StockFlowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<StockFlowDbContext>();

    await DbInitializer.InitializeAsync(dbContext);
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("StockFlowFrontend");

app.MapControllers();


app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        application = "StockFlow API",
        version = "1.0.0"
    });
});

app.MapGet("/api/health/db", async (StockFlowDbContext dbContext) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync();

    if (!canConnect)
    {
        return Results.Problem(
            title: "Database connection failed",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }

    return Results.Ok(new
    {
        status = "connected",
        database = "stockflow_db",
        provider = "PostgreSQL"
    });
});

app.Run();

public partial class Program { }