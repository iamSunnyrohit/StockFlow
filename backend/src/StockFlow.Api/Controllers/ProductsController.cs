
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Common;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }


    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<ProductDto>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] ProductQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(
            parameters,
            cancellationToken
        );

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ProductDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ProductDto),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product
        );
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(ProductDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateAsync(
            id,
            request,
            cancellationToken
        );

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.DeactivateAsync(
            id,
            cancellationToken
        );

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
