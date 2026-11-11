namespace StockFlow.Application.DTOs.Inventories;

public class ProductStockSummaryDto
{
    public Guid ProductId { get; set; }
    public decimal TotalQuantityOnHand { get; set; }
    public int WarehouseCount { get; set; }
}