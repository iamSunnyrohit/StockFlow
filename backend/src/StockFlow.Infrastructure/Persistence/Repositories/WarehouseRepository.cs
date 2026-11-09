using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly StockFlowDbContext _context;

    public WarehouseRepository(StockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Warehouse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(
                warehouse => warehouse.Id == id,
                cancellationToken
            );
    }

    public async Task<Warehouse?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(
                warehouse => EF.Functions.ILike(warehouse.Code, code),
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default)
    {
        await _context.Warehouses.AddAsync(
            warehouse,
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
