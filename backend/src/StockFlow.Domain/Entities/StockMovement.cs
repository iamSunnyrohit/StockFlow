using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;

namespace StockFlow.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public StockMovementType Type { get; set; }

    public decimal Quantity { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;

    public Warehouse Warehouse { get; set; } = null!;
}