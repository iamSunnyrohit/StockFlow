namespace StockFlow.Application.DTOs.Inventories;

public class CreateInventoryRequest
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal ReorderLevel { get; set; }
}