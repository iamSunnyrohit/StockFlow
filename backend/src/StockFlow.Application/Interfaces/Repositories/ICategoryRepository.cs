using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Category?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Category category,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasProductsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    );
}