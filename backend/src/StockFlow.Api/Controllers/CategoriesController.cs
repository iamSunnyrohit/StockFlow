using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Categories;
using StockFlow.Application.Interfaces.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<CategoryDto>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(
            cancellationToken
        );

        return Ok(categories);
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(CategoryDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }


    [HttpPost]
    [ProducesResponseType(
        typeof(CategoryDto),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            category
        );
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(CategoryDto),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.UpdateAsync(
            id,
            request,
            cancellationToken
        );

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }


    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeactivateAsync(
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
