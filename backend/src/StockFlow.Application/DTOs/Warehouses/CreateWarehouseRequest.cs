namespace StockFlow.Application.DTOs.Warehouses;

public class CreateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? City { get; set; }
}
