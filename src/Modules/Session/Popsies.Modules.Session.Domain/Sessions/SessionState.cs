namespace Popsies.Modules.Session.Domain.Sessions;

/// <summary>
/// Represents the state of a quiz session
/// </summary>
public enum SessionState
{
    /// <summary>
    /// Session created, waiting for players
    /// </summary>
    Waiting = 0,

    /// <summary>
    /// Session started, players can answer questions
    /// </summary>
    Active = 1,

    /// <summary>
    /// Session completed, showing final results
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Session was cancelled before completion
    /// </summary>
    Cancelled = 3
}
