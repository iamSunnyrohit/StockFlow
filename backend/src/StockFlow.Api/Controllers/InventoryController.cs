using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Inventories;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryService.GetByIdAsync(id, cancellationToken);
        return inventory is null ? NotFound() : Ok(inventory);
    }

    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetByWarehouse(
        Guid warehouseId, CancellationToken cancellationToken)
    {
        var items = await _inventoryService.GetByWarehouseAsync(
            warehouseId, cancellationToken);

        return items is null ? NotFound() : Ok(items);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(
        Guid productId, CancellationToken cancellationToken)
    {
        var items = await _inventoryService.GetByProductAsync(
            productId, cancellationToken);

        return items is null ? NotFound() : Ok(items);
    }

    [HttpGet("product/{productId:guid}/total")]
    public async Task<IActionResult> GetProductTotal(
        Guid productId, CancellationToken cancellationToken)
    {
        var summary = await _inventoryService.GetProductStockSummaryAsync(
            productId, cancellationToken);

        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryService.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById), new { id = inventory.Id }, inventory);
    }

    [HttpPatch("{id:guid}/reorder-level")]
    public async Task<IActionResult> UpdateReorderLevel(
        Guid id,
        [FromBody] UpdateReorderLevelRequest request,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryService.UpdateReorderLevelAsync(
            id, request, cancellationToken);

        return inventory is null ? NotFound() : Ok(inventory);
    }
}