namespace StockFlow.Application.DTOs.Products;

public class ProductDto
{
    public Guid Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public decimal UnitCost { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}