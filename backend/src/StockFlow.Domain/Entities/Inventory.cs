using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal ReorderLevel { get; set; }

    public Product Product { get; set; } = null!;

    public Warehouse Warehouse { get; set; } = null!;
}
