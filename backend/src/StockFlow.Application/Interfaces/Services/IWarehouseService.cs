using StockFlow.Application.DTOs.Warehouses;

namespace StockFlow.Application.Interfaces.Services;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<WarehouseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<WarehouseDto> CreateAsync(
        CreateWarehouseRequest request,
        CancellationToken cancellationToken = default);

    Task<WarehouseDto?> UpdateAsync(
        Guid id,
        UpdateWarehouseRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
