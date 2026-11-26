using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.PurchaseOrders;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Entities;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly StockFlowDbContext _context;

    public PurchaseOrderRepository(StockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Supplier)
            .Include(x => x.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(
        PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        return await BuildQuery(parameters)
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Items)
                .ThenInclude(item => item.Product)
            .OrderByDescending(x => x.OrderDateUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery(parameters).CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        PurchaseOrder order, CancellationToken cancellationToken = default)
    {
        await _context.PurchaseOrders.AddAsync(order, cancellationToken);
        await SaveAsync(cancellationToken);
    }

    public void ReplaceItems(
        PurchaseOrder order, IReadOnlyCollection<PurchaseOrderItem> newItems)
    {
        _context.PurchaseOrderItems.RemoveRange(order.Items.ToList());
        _context.PurchaseOrderItems.AddRange(newItems);
    }

    public async Task UpdateAsync(
        PurchaseOrder order, CancellationToken cancellationToken = default)
    {
        await SaveAsync(cancellationToken);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The purchase order was changed by another request. Please reload and retry.");
        }
    }

    private IQueryable<PurchaseOrder> BuildQuery(
        PurchaseOrderQueryParameters parameters)
    {
        IQueryable<PurchaseOrder> query = _context.PurchaseOrders;

        if (parameters.Status.HasValue)
            query = query.Where(x => x.Status == parameters.Status.Value);

        if (parameters.SupplierId.HasValue)
            query = query.Where(x => x.SupplierId == parameters.SupplierId.Value);

        return query;
    }
}