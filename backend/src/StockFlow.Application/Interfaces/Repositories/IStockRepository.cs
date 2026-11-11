using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface IStockRepository
{
    Task ApplyAsync(
        IReadOnlyCollection<Inventory> newInventories,
        IReadOnlyCollection<StockMovement> movements,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        Guid? productId,
        Guid? warehouseId,
        int take,
        CancellationToken cancellationToken = default);
}