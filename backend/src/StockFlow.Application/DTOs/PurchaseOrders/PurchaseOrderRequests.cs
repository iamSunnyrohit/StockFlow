using StockFlow.Domain.Enums;

namespace StockFlow.Application.DTOs.PurchaseOrders;

public class PurchaseOrderItemRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }

    public decimal? UnitCost { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public Guid SupplierId { get; set; }
    public DateTime? ExpectedDeliveryDateUtc { get; set; }
    public decimal TaxRatePercent { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemRequest> Items { get; set; } = new();
}

public class UpdatePurchaseOrderRequest : CreatePurchaseOrderRequest
{
}

public class ReceivePurchaseOrderRequest
{
    public Guid WarehouseId { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseOrderQueryParameters
{
    public OrderStatus? Status { get; set; }
    public Guid? SupplierId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}