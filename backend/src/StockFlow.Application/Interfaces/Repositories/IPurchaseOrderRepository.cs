using StockFlow.Application.DTOs.PurchaseOrders;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(
        PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PurchaseOrder order, CancellationToken cancellationToken = default);

    void ReplaceItems(
        PurchaseOrder order, IReadOnlyCollection<PurchaseOrderItem> newItems);

    Task UpdateAsync(
        PurchaseOrder order, CancellationToken cancellationToken = default);
}