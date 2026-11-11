using Microsoft.EntityFrameworkCore;
using Npgsql;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class StockRepository : IStockRepository
{
    private readonly StockFlowDbContext _context;

    public StockRepository(StockFlowDbContext context)
    {
        _context = context;
    }

    public async Task ApplyAsync(
        IReadOnlyCollection<Inventory> newInventories,
        IReadOnlyCollection<StockMovement> movements,
        CancellationToken cancellationToken = default)
    {
        if (newInventories.Count > 0)
        {
            await _context.Inventories.AddRangeAsync(
                newInventories, cancellationToken);
        }

        await _context.StockMovements.AddRangeAsync(
            movements, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Stock was changed by another request. Please retry.");
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            throw new ConflictException(
                "An inventory record for this product and warehouse " +
                "was created by another request. Please retry.");
        }
    }

    public async Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        Guid? productId,
        Guid? warehouseId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockMovements.AsNoTracking();

        if (productId.HasValue)
            query = query.Where(x => x.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(x => x.WarehouseId == warehouseId.Value);

        return await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}