using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.PurchaseOrders;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PurchaseOrderQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(parameters, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var order = await _service.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _service.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _service.UpdateAsync(id, request, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        Guid id, CancellationToken cancellationToken)
    {
        var order = await _service.ConfirmAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _service.ReceiveAsync(id, request, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id, CancellationToken cancellationToken)
    {
        var order = await _service.CancelAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}