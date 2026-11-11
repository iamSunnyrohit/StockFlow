namespace StockFlow.Application.DTOs.Stock;

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

public class StockChangeResultDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal? DestinationQuantityOnHand { get; set; }

    public IReadOnlyList<StockMovementDto> Movements { get; set; }
        = Array.Empty<StockMovementDto>();
}