namespace StockFlow.Application.DTOs.Warehouses;

public class UpdateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? City { get; set; }
}
