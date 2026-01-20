namespace Popsies.Modules.Quiz.Contracts.Responses;

/// <summary>
/// Response after creating a new category
/// </summary>
/// <param name="CategoryId">The ID of the created category</param>
/// <param name="Name">The category name</param>
/// <param name="ParentCategoryId">The parent category ID, if any</param>
/// <param name="Message">Success message</param>
public sealed record CreateCategoryResponse(
    Guid CategoryId,
    string Name,
    Guid? ParentCategoryId,
    string Message);
