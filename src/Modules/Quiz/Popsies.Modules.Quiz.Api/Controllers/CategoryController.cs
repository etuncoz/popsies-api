using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.CreateCategory;
using Popsies.Modules.Quiz.Application.UseCases.UpdateCategory;
using Popsies.Modules.Quiz.Contracts.Requests;
using Popsies.Modules.Quiz.Contracts.Responses;
using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Api.Controllers;

/// <summary>
/// Category management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/categories")]
public sealed class CategoryController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICommandDispatcher commandDispatcher, ICategoryRepository categoryRepository)
    {
        _commandDispatcher = commandDispatcher;
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateCategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Description,
            request.ParentCategoryId);

        var result = await _commandDispatcher.SendAsync<CreateCategoryCommand, Guid>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        var categoryId = result.Value;
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        var response = new CreateCategoryResponse(
            categoryId,
            category!.Name,
            category.ParentCategoryId,
            "Category created successfully");

        return CreatedAtAction(nameof(CreateCategory), new { id = categoryId }, response);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description);

        var result = await _commandDispatcher.SendAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Category.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { message = "Category updated successfully" });
    }
}
