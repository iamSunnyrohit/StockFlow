using Microsoft.EntityFrameworkCore;
using StockFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found."
    );

builder.Services.AddDbContext<StockFlowDbContext>(options =>
    options.UseNpgsql(connectionString)
);

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

app.UseCors("StockFlowFrontend");


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