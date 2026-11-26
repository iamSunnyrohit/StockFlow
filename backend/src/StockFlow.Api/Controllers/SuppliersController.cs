using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Suppliers;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] SupplierQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var suppliers = await _supplierService.GetAllAsync(
            parameters,
            cancellationToken
        );

        return Ok(suppliers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (supplier is null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = supplier.Id },
            supplier
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.UpdateAsync(
            id,
            request,
            cancellationToken
        );

        if (supplier is null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deactivated = await _supplierService.DeactivateAsync(
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