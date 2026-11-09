using StockFlow.Domain.Common;

namespace StockFlow.Domain.Entities;

public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal UnitCost { get; set; }

    public bool IsActive { get; set; } = true;

    public Category Category { get; set; } = null!;

    public ICollection<Inventory> Inventories { get; set; }
        = new List<Inventory>();

    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        = new List<PurchaseOrderItem>();
    
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; }
        = new List<SalesOrderItem>();
    
    public ICollection<StockMovement> StockMovements { get; set; }
        = new List<StockMovement>();
}
