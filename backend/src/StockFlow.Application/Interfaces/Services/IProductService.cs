using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.Products;

namespace StockFlow.Application.Interfaces.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(
        ProductQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}