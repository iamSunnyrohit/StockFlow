namespace StockFlow.Application.DTOs.Products;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal UnitCost { get; set; }
}