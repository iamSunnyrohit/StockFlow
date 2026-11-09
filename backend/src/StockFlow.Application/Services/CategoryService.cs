
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (parameters.Page < 1)
        {
            throw new ValidationException(
                "Page number must be greater than zero."
            );
        }

        if (parameters.PageSize < 1 || parameters.PageSize > 100)
        {
            throw new ValidationException(
                "Page size must be between 1 and 100."
            );
        }

        var products = await _productRepository.GetAllAsync(
            parameters,
            cancellationToken
        );

        var totalCount = await _productRepository.CountAsync(
            parameters,
            cancellationToken
        );

        return new PagedResult<ProductDto>
        {
            Items = products.Select(MapToDto).ToList(),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            throw new NotFoundException(
                $"Product with ID '{id}' was not found."
            );
        }

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateProduct(
            request.SKU,
            request.Name,
            request.Description,
            request.UnitPrice,
            request.UnitCost
        );

        var sku = request.SKU.Trim();

        var existingProduct = await _productRepository.GetBySkuAsync(
            sku,
            cancellationToken
        );

        if (existingProduct is not null)
        {
            throw new ConflictException(
                $"A product with SKU '{sku}' already exists."
            );
        }

        if (!await _productRepository.CategoryIsActiveAsync(
            request.CategoryId,
            cancellationToken))
        {
            throw new ValidationException(
                "The selected category does not exist or is inactive."
            );
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken
        );

        if (category is null)
        {
            throw new ValidationException(
                "The selected category could not be found."
            );
        }

        var product = new Product
        {
            SKU = sku,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            Category = category,
            UnitPrice = request.UnitPrice,
            UnitCost = request.UnitCost,
            IsActive = true
        };

        await _productRepository.AddAsync(
            product,
            cancellationToken
        );

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            throw new NotFoundException(
                $"Product with ID '{id}' was not found."
            );
        }

        ValidateProduct(
            product.SKU,
            request.Name,
            request.Description,
            request.UnitPrice,
            request.UnitCost
        );

        if (!await _productRepository.CategoryIsActiveAsync(
            request.CategoryId,
            cancellationToken))
        {
            throw new ValidationException(
                "The selected category does not exist or is inactive."
            );
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken
        );

        if (category is null)
        {
            throw new ValidationException(
                "The selected category could not be found."
            );
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.CategoryId = request.CategoryId;
        product.Category = category;
        product.UnitPrice = request.UnitPrice;
        product.UnitCost = request.UnitCost;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _productRepository.UpdateAsync(
            product,
            cancellationToken
        );

        return MapToDto(product);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            throw new NotFoundException(
                $"Product with ID '{id}' was not found."
            );
        }

        if (!product.IsActive)
        {
            throw new ConflictException(
                "This product is already inactive."
            );
        }

        product.IsActive = false;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _productRepository.UpdateAsync(
            product,
            cancellationToken
        );

        return true;
    }

    private static void ValidateProduct(
        string sku,
        string name,
        string? description,
        decimal unitPrice,
        decimal unitCost)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ValidationException(
                "Product SKU is required."
            );
        }

        if (sku.Trim().Length > 64)
        {
            throw new ValidationException(
                "Product SKU cannot exceed 64 characters."
            );
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Product name is required."
            );
        }

        if (name.Trim().Length > 150)
        {
            throw new ValidationException(
                "Product name cannot exceed 150 characters."
            );
        }

        if (description?.Length > 1000)
        {
            throw new ValidationException(
                "Product description cannot exceed 1000 characters."
            );
        }

        if (unitPrice < 0)
        {
            throw new ValidationException(
                "Unit price cannot be negative."
            );
        }

        if (unitCost < 0)
        {
            throw new ValidationException(
                "Unit cost cannot be negative."
            );
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            UnitPrice = product.UnitPrice,
            UnitCost = product.UnitCost,
            IsActive = product.IsActive,
            CreatedAtUtc = product.CreatedAtUtc
        };
    }
}
