using StockFlow.Application.DTOs.Inventories;

namespace StockFlow.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<InventoryDto?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default);

    // Returns null when the warehouse / product does not exist.
    Task<IReadOnlyList<InventoryDto>?> GetByWarehouseAsync(
        Guid warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryDto>?> GetByProductAsync(
        Guid productId, CancellationToken cancellationToken = default);

    Task<ProductStockSummaryDto?> GetProductStockSummaryAsync(
        Guid productId, CancellationToken cancellationToken = default);

    Task<InventoryDto> CreateAsync(
        CreateInventoryRequest request,
        CancellationToken cancellationToken = default);

    Task<InventoryDto?> UpdateReorderLevelAsync(
        Guid id, UpdateReorderLevelRequest request,
        CancellationToken cancellationToken = default);
}