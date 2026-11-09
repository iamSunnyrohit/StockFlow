using StockFlow.Application.DTOs.Products;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default
    );

    Task<bool> CategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Product>> GetAllAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default
    );
}