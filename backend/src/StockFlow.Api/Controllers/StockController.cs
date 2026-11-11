using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Stock;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost("receive")]
    public async Task<IActionResult> Receive(
        [FromBody] StockChangeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _stockService.ReceiveAsync(request, cancellationToken));
    }

    [HttpPost("issue")]
    public async Task<IActionResult> Issue(
        [FromBody] StockChangeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _stockService.IssueAsync(request, cancellationToken));
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _stockService.AdjustAsync(request, cancellationToken));
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
        [FromBody] TransferStockRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _stockService.TransferAsync(request, cancellationToken));
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _stockService.GetMovementsAsync(
            productId, warehouseId, take, cancellationToken));
    }
}
