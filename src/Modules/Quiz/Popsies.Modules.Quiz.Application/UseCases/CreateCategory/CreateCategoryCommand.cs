using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.CreateCategory;

/// <summary>
/// Command to create a new category
/// </summary>
/// <param name="Name">The category name</param>
/// <param name="Description">The category description</param>
/// <param name="ParentCategoryId">Optional parent category ID for hierarchical categories</param>
public sealed record CreateCategoryCommand(
    string Name,
    string Description,
    Guid? ParentCategoryId) : ICommand<Guid>;
