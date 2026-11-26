using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.PurchaseOrders;

namespace StockFlow.Application.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<PagedResult<PurchaseOrderDto>> GetAllAsync(
        PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> CreateAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto?> UpdateAsync(
        Guid id, UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto?> ConfirmAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto?> ReceiveAsync(
        Guid id, ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto?> CancelAsync(
        Guid id, CancellationToken cancellationToken = default);
}