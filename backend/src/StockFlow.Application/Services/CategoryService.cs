
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Categories;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(
            cancellationToken
        );

        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with ID '{id}' was not found."
            );
        }

        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Category name is required."
            );
        }

        if (name.Length > 100)
        {
            throw new ValidationException(
                "Category name cannot exceed 100 characters."
            );
        }

        var existingCategory = await _categoryRepository.GetByNameAsync(
            name,
            cancellationToken
        );

        if (existingCategory is not null)
        {
            throw new ConflictException(
                $"A category named '{name}' already exists."
            );
        }

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        await _categoryRepository.AddAsync(
            category,
            cancellationToken
        );

        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with ID '{id}' was not found."
            );
        }

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Category name is required."
            );
        }

        if (name.Length > 100)
        {
            throw new ValidationException(
                "Category name cannot exceed 100 characters."
            );
        }

        var existingCategory = await _categoryRepository.GetByNameAsync(
            name,
            cancellationToken
        );

        if (existingCategory is not null &&
            existingCategory.Id != category.Id)
        {
            throw new ConflictException(
                $"A category named '{name}' already exists."
            );
        }

        category.Name = name;
        category.Description = request.Description?.Trim();
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(
            category,
            cancellationToken
        );

        return MapToDto(category);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with ID '{id}' was not found."
            );
        }

        if (!category.IsActive)
        {
            throw new ConflictException(
                "This category is already inactive."
            );
        }

        category.IsActive = false;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(
            category,
            cancellationToken
        );

        return true;
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }
}
