using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Warehouse?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Warehouse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default);
}
