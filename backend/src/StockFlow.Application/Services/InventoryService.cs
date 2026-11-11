using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Inventories;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IWarehouseRepository warehouseRepository)
    {
        _inventoryRepository = inventoryRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<InventoryDto?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(
            id, cancellationToken);

        return inventory is null ? null : MapToDto(inventory);
    }

    public async Task<IReadOnlyList<InventoryDto>?> GetByWarehouseAsync(
        Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(
            warehouseId, cancellationToken);

        if (warehouse is null)
        {
            return null;
        }

        var items = await _inventoryRepository.GetByWarehouseAsync(
            warehouseId, cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<InventoryDto>?> GetByProductAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        if (!await _inventoryRepository.ProductExistsAsync(
                productId, cancellationToken))
        {
            return null;
        }

        var items = await _inventoryRepository.GetByProductAsync(
            productId, cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<ProductStockSummaryDto?> GetProductStockSummaryAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        if (!await _inventoryRepository.ProductExistsAsync(
                productId, cancellationToken))
        {
            return null;
        }

        var items = await _inventoryRepository.GetByProductAsync(
            productId, cancellationToken);

        return new ProductStockSummaryDto
        {
            ProductId = productId,
            TotalQuantityOnHand = items.Sum(x => x.QuantityOnHand),
            WarehouseCount = items.Count
        };
    }

    public async Task<InventoryDto> CreateAsync(
        CreateInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateReorderLevel(request.ReorderLevel);

        if (!await _inventoryRepository.ProductExistsAsync(
                request.ProductId, cancellationToken))
        {
            throw new ValidationException("The specified product does not exist.");
        }

        var warehouse = await _warehouseRepository.GetByIdAsync(
            request.WarehouseId, cancellationToken);

        if (warehouse is null)
        {
            throw new ValidationException("The specified warehouse does not exist.");
        }

        if (!warehouse.IsActive)
        {
            throw new ValidationException(
                "Cannot create inventory in an inactive warehouse.");
        }

        var existing = await _inventoryRepository.GetByProductAndWarehouseAsync(
            request.ProductId, request.WarehouseId, cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException(
                "An inventory record for this product and warehouse already exists.");
        }

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            QuantityOnHand = 0,
            ReorderLevel = request.ReorderLevel,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _inventoryRepository.AddAsync(inventory, cancellationToken);

        // Reload so Product and Warehouse navigations are populated for the DTO.
        var created = await _inventoryRepository.GetByIdAsync(
            inventory.Id, cancellationToken);

        return MapToDto(created!);
    }

    public async Task<InventoryDto?> UpdateReorderLevelAsync(
        Guid id, UpdateReorderLevelRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateReorderLevel(request.ReorderLevel);

        var inventory = await _inventoryRepository.GetByIdAsync(
            id, cancellationToken);

        if (inventory is null)
        {
            return null;
        }

        inventory.ReorderLevel = request.ReorderLevel;
        inventory.UpdatedAtUtc = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

        return MapToDto(inventory);
    }

    private static void ValidateReorderLevel(decimal reorderLevel)
    {
        if (reorderLevel < 0)
            throw new ValidationException("Reorder level cannot be negative.");
    }

    private static InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductName = inventory.Product.Name,
            ProductSku = inventory.Product.SKU,
            WarehouseId = inventory.WarehouseId,
            WarehouseName = inventory.Warehouse.Name,
            WarehouseCode = inventory.Warehouse.Code,
            QuantityOnHand = inventory.QuantityOnHand,
            ReorderLevel = inventory.ReorderLevel,
            IsBelowReorderLevel = inventory.QuantityOnHand <= inventory.ReorderLevel,
            CreatedAtUtc = inventory.CreatedAtUtc
        };
    }
}