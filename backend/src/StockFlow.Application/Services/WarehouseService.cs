using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Warehouses;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var warehouses = await _warehouseRepository.GetAllAsync(
            cancellationToken
        );

        return warehouses.Select(MapToDto).ToList();
    }

    public async Task<WarehouseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        return warehouse is null ? null : MapToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(
        CreateWarehouseRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateWarehouse(
            request.Name,
            request.Code,
            request.Address,
            request.City
        );

        var code = request.Code.Trim();

        var existingWarehouse =
            await _warehouseRepository.GetByCodeAsync(
                code,
                cancellationToken
            );

        if (existingWarehouse is not null)
        {
            throw new ConflictException(
                $"A warehouse with code '{code}' already exists."
            );
        }

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Code = code,
            Address = NormalizeOptional(request.Address),
            City = NormalizeOptional(request.City),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _warehouseRepository.AddAsync(
            warehouse,
            cancellationToken
        );

        return MapToDto(warehouse);
    }

    public async Task<WarehouseDto?> UpdateAsync(
        Guid id,
        UpdateWarehouseRequest request,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (warehouse is null)
        {
            return null;
        }

        ValidateWarehouse(
            request.Name,
            warehouse.Code,
            request.Address,
            request.City
        );

        warehouse.Name = request.Name.Trim();
        warehouse.Address = NormalizeOptional(request.Address);
        warehouse.City = NormalizeOptional(request.City);
        warehouse.UpdatedAtUtc = DateTime.UtcNow;

        await _warehouseRepository.UpdateAsync(
            warehouse,
            cancellationToken
        );

        return MapToDto(warehouse);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        if (warehouse is null)
        {
            return false;
        }

        warehouse.IsActive = false;
        warehouse.UpdatedAtUtc = DateTime.UtcNow;

        await _warehouseRepository.UpdateAsync(
            warehouse,
            cancellationToken
        );

        return true;
    }

    private static void ValidateWarehouse(
        string name,
        string code,
        string? address,
        string? city)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Warehouse name is required.");

        if (name.Trim().Length > 150)
            throw new ValidationException(
                "Warehouse name cannot exceed 150 characters."
            );

        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Warehouse code is required.");

        if (code.Trim().Length > 32)
            throw new ValidationException(
                "Warehouse code cannot exceed 32 characters."
            );

        if (address?.Length > 500)
            throw new ValidationException(
                "Warehouse address cannot exceed 500 characters."
            );

        if (city?.Length > 100)
            throw new ValidationException(
                "Warehouse city cannot exceed 100 characters."
            );
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        return new WarehouseDto
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Code = warehouse.Code,
            Address = warehouse.Address,
            City = warehouse.City,
            IsActive = warehouse.IsActive,
            CreatedAtUtc = warehouse.CreatedAtUtc
        };
    }
}
