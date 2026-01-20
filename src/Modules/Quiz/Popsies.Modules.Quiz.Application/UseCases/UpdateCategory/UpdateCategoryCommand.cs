using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Quiz.Application.UseCases.UpdateCategory;

/// <summary>
/// Command to update an existing category's details
/// </summary>
/// <param name="CategoryId">The ID of the category to update</param>
/// <param name="Name">The new category name</param>
/// <param name="Description">The new category description</param>
public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string Description) : ICommand;
