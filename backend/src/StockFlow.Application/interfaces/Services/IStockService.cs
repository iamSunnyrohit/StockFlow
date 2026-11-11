using StockFlow.Application.DTOs.Stock;

namespace StockFlow.Application.Interfaces.Services;

public interface IStockService
{
    Task<StockChangeResultDto> ReceiveAsync(
        StockChangeRequest request, CancellationToken cancellationToken = default);

    Task<StockChangeResultDto> IssueAsync(
        StockChangeRequest request, CancellationToken cancellationToken = default);

    Task<StockChangeResultDto> AdjustAsync(
        AdjustStockRequest request, CancellationToken cancellationToken = default);

    Task<StockChangeResultDto> TransferAsync(
        TransferStockRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(
        Guid? productId, Guid? warehouseId, int take,
        CancellationToken cancellationToken = default);
}