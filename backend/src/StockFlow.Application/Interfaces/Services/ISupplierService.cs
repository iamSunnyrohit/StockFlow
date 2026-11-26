using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.Suppliers;

namespace StockFlow.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetAllAsync(
        SupplierQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<SupplierDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SupplierDto> CreateAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierDto?> UpdateAsync(
        Guid id,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}