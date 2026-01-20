namespace Popsies.Modules.Quiz.Domain.Quizzes;

/// <summary>
/// Represents the lifecycle state of a quiz
/// </summary>
public enum QuizState
{
    /// <summary>
    /// Quiz is in draft mode and can be edited
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Quiz is published and available for play
    /// </summary>
    Published = 1,

    /// <summary>
    /// Quiz is archived and no longer available for play
    /// </summary>
    Archived = 2
}
