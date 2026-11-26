using StockFlow.Application.DTOs.Suppliers;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Supplier>> GetAllAsync(
        SupplierQueryParameters parameters,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        SupplierQueryParameters parameters,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default
    );
}
