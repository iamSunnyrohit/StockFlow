using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs.Suppliers;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Persistence;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly StockFlowDbContext _context;

    public SupplierRepository(StockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(
                supplier => supplier.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(
        SupplierQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);

        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        return await query
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        SupplierQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery(parameters)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default)
    {
        await _context.Suppliers.AddAsync(
            supplier,
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Supplier> BuildQuery(
        SupplierQueryParameters parameters)
    {
        IQueryable<Supplier> query = _context.Suppliers;

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchPattern = $"%{parameters.Search.Trim()}%";

            query = query.Where(supplier =>
                EF.Functions.ILike(supplier.Name, searchPattern) ||
                (supplier.ContactPerson != null &&
                 EF.Functions.ILike(
                     supplier.ContactPerson,
                     searchPattern
                 )) ||
                (supplier.Email != null &&
                 EF.Functions.ILike(
                     supplier.Email,
                     searchPattern
                 )) ||
                (supplier.Phone != null &&
                 EF.Functions.ILike(
                     supplier.Phone,
                     searchPattern
                 )) ||
                (supplier.City != null &&
                 EF.Functions.ILike(
                     supplier.City,
                     searchPattern
                 ))
            );
        }

        if (parameters.IsActive.HasValue)
        {
            query = query.Where(supplier =>
                supplier.IsActive == parameters.IsActive.Value
            );
        }

        return query;
    }
}
