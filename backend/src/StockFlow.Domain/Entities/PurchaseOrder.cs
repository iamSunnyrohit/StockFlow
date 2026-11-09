using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;

namespace StockFlow.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDateUtc { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;

    public ICollection<PurchaseOrderItem> Items { get; set; }
        = new List<PurchaseOrderItem>();
}
