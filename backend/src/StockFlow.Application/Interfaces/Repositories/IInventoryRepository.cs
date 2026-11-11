using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<Inventory?> GetByProductAndWarehouseAsync(
        Guid productId, Guid warehouseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Inventory>> GetByWarehouseAsync(
        Guid warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Inventory>> GetByProductAsync(
        Guid productId, CancellationToken cancellationToken = default);

    Task<bool> ProductExistsAsync(
        Guid productId, CancellationToken cancellationToken = default);

    Task AddAsync(
        Inventory inventory, CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Inventory inventory, CancellationToken cancellationToken = default);
}