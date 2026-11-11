using StockFlow.Application.DTOs.Categories;

namespace StockFlow.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<CategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default
    );

    Task<CategoryDto?> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default
    );

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}