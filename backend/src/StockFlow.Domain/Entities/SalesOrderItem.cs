using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
