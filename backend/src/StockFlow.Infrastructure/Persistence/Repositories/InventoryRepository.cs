using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly StockFlowDbContext _context;

    public InventoryRepository(StockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Inventory?> GetByProductAndWarehouseAsync(
        Guid productId, Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .FirstOrDefaultAsync(
                x => x.ProductId == productId && x.WarehouseId == warehouseId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Inventory>> GetByWarehouseAsync(
        Guid warehouseId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .Where(x => x.WarehouseId == warehouseId)
            .OrderBy(x => x.Product.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Inventory>> GetByProductAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Inventories
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.Warehouse.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ProductExistsAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AnyAsync(x => x.Id == productId, cancellationToken);
    }

    public async Task AddAsync(
        Inventory inventory, CancellationToken cancellationToken = default)
    {
        await _context.Inventories.AddAsync(inventory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Inventory inventory, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}