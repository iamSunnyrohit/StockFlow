using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Warehouses;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseService.GetAllAsync(
            cancellationToken
        );

        return Ok(warehouses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (warehouse is null)
        {
            return NotFound();
        }

        return Ok(warehouse);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = warehouse.Id },
            warehouse
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.UpdateAsync(
            id,
            request,
            cancellationToken
        );

        if (warehouse is null)
        {
            return NotFound();
        }

        return Ok(warehouse);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deactivated = await _warehouseService.DeactivateAsync(
            id,
            cancellationToken
        );

        if (!deactivated)
        {
            return NotFound();
        }

        return NoContent();
    }
}
