namespace StockFlow.Application.DTOs.Stock;

public class StockChangeRequest
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class AdjustStockRequest
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal NewQuantity { get; set; }
    public string? Notes { get; set; }
}

public class TransferStockRequest
{
    public Guid ProductId { get; set; }
    public Guid SourceWarehouseId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}