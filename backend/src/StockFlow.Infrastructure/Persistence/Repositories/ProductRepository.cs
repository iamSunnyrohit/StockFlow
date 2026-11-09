using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Persistence;


namespace StockFlow.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly StockFlowDbContext _context;

    public ProductRepository(StockFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(product => product.Category)
            .FirstOrDefaultAsync(
                product => product.Id == id,
                cancellationToken
            );
    }

    public async Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(
                product => EF.Functions.ILike(product.SKU, sku),
                cancellationToken
            );
    }

    public async Task<bool> CategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(
                category => category.Id == categoryId,
                cancellationToken
            );
    }

    public async Task<bool> CategoryIsActiveAsync(
    Guid categoryId,
    CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(
                category => category.Id == categoryId && category.IsActive,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(parameters);

        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        return await query
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderBy(product => product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery(parameters)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(
            product,
            cancellationToken
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Product> BuildQuery(
        ProductQueryParameters parameters)
    {
        IQueryable<Product> query = _context.Products;

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchPattern = $"%{parameters.Search.Trim()}%";

            query = query.Where(product =>
                EF.Functions.ILike(product.Name, searchPattern) ||
                EF.Functions.ILike(product.SKU, searchPattern)
            );
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(product =>
                product.CategoryId == parameters.CategoryId.Value
            );
        }

        if (parameters.IsActive.HasValue)
        {
            query = query.Where(product =>
                product.IsActive == parameters.IsActive.Value
            );
        }

        return query;
    }
}