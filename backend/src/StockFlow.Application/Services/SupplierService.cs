using System.Net.Mail;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Application.DTOs.Common;
using StockFlow.Application.DTOs.Suppliers;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<PagedResult<SupplierDto>> GetAllAsync(
        SupplierQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        // Normalize once so the repository and the response agree.
        var normalized = new SupplierQueryParameters
        {
            Search = string.IsNullOrWhiteSpace(parameters.Search)
                ? null
                : parameters.Search.Trim(),
            IsActive = parameters.IsActive,
            Page = Math.Max(parameters.Page, 1),
            PageSize = Math.Clamp(parameters.PageSize, 1, 100)
        };

        var suppliers = await _supplierRepository.GetAllAsync(
            normalized, cancellationToken);

        var totalCount = await _supplierRepository.CountAsync(
            normalized, cancellationToken);

        return new PagedResult<SupplierDto>
        {
            Items = suppliers.Select(MapToDto).ToList(),
            Page = normalized.Page,
            PageSize = normalized.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SupplierDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            id, cancellationToken);

        return supplier is null ? null : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request.Name, request.ContactPerson, request.Email,
            request.Phone, request.Address, request.City);

        var name = request.Name.Trim();

        var existing = await _supplierRepository.GetByNameAsync(
            name, cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException(
                $"A supplier named '{name}' already exists.");
        }

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContactPerson = NormalizeOptional(request.ContactPerson),
            Email = NormalizeOptional(request.Email),
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            City = NormalizeOptional(request.City),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _supplierRepository.AddAsync(supplier, cancellationToken);

        return MapToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(
        Guid id,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            id, cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        Validate(request.Name, request.ContactPerson, request.Email,
            request.Phone, request.Address, request.City);

        var name = request.Name.Trim();

        var sameName = await _supplierRepository.GetByNameAsync(
            name, cancellationToken);

        if (sameName is not null && sameName.Id != id)
        {
            throw new ConflictException(
                $"A supplier named '{name}' already exists.");
        }

        supplier.Name = name;
        supplier.ContactPerson = NormalizeOptional(request.ContactPerson);
        supplier.Email = NormalizeOptional(request.Email);
        supplier.Phone = NormalizeOptional(request.Phone);
        supplier.Address = NormalizeOptional(request.Address);
        supplier.City = NormalizeOptional(request.City);
        supplier.UpdatedAtUtc = DateTime.UtcNow;

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        return MapToDto(supplier);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            id, cancellationToken);

        if (supplier is null)
        {
            return false;
        }

        supplier.IsActive = false;
        supplier.UpdatedAtUtc = DateTime.UtcNow;

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        return true;
    }

    private static void Validate(
        string name, string? contactPerson, string? email,
        string? phone, string? address, string? city)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Supplier name is required.");

        if (name.Trim().Length > 150)
            throw new ValidationException(
                "Supplier name cannot exceed 150 characters.");

        if (contactPerson?.Trim().Length > 150)
            throw new ValidationException(
                "Contact person cannot exceed 150 characters.");

        if (!string.IsNullOrWhiteSpace(email))
        {
            var trimmed = email.Trim();

            if (trimmed.Length > 254 || !IsValidEmail(trimmed))
                throw new ValidationException("Supplier email is not valid.");
        }

        if (phone?.Trim().Length > 32)
            throw new ValidationException(
                "Phone cannot exceed 32 characters.");

        if (address?.Trim().Length > 500)
            throw new ValidationException(
                "Address cannot exceed 500 characters.");

        if (city?.Trim().Length > 100)
            throw new ValidationException(
                "City cannot exceed 100 characters.");
    }

    private static bool IsValidEmail(string email)
    {
        return MailAddress.TryCreate(email, out var parsed)
               && parsed.Address == email;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            City = supplier.City,
            IsActive = supplier.IsActive,
            CreatedAtUtc = supplier.CreatedAtUtc
        };
    }
}