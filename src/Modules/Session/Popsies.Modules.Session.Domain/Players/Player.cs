using Popsies.Modules.Session.Domain.Players.Events;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Domain.Players;

/// <summary>
/// Player entity - represents a player in a quiz session
/// Invariants:
/// - Display name must be 1-50 characters
/// - Score cannot be negative
/// - Player must be active to submit answers
/// </summary>
public sealed class Player : Entity
{
    private const int MinDisplayNameLength = 1;
    private const int MaxDisplayNameLength = 50;

    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; }
    public int TotalScore { get; private set; }
    public int CorrectAnswers { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    private Player() { }

    private Player(Guid id, Guid sessionId, Guid userId, string displayName, DateTime joinedAt) : base(id)
    {
        SessionId = sessionId;
        UserId = userId;
        DisplayName = displayName;
        TotalScore = 0;
        CorrectAnswers = 0;
        IsActive = true;
        JoinedAt = joinedAt;
    }

    /// <summary>
    /// Creates a new player
    /// </summary>
    public static Result<Player> Create(Guid id, Guid sessionId, Guid userId, string displayName)
    {
        if (sessionId == Guid.Empty)
        {
            return Result.Failure<Player>(Error.Validation("SessionId", "Session ID cannot be empty"));
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure<Player>(Error.Validation("UserId", "User ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<Player>(Error.Validation("DisplayName", "Display name cannot be empty"));
        }

        if (displayName.Length < MinDisplayNameLength || displayName.Length > MaxDisplayNameLength)
        {
            return Result.Failure<Player>(Error.Validation("DisplayName",
                $"Display name must be {MinDisplayNameLength}-{MaxDisplayNameLength} characters long"));
        }

        var player = new Player(id, sessionId, userId, displayName, DateTime.UtcNow);

        return Result.Success(player);
    }

    /// <summary>
    /// Adds points to the player's score
    /// </summary>
    public Result AddScore(int points, bool isCorrect)
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Create("Player.Inactive", "Cannot add score to inactive player"));
        }

        if (points < 0)
        {
            return Result.Failure(Error.Validation("Points", "Points cannot be negative"));
        }

        TotalScore += points;

        if (isCorrect)
        {
            CorrectAnswers++;
        }

        return Result.Success();
    }

    /// <summary>
    /// Marks the player as having left the session
    /// </summary>
    public Result Leave()
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Create("Player.AlreadyLeft", "Player has already left"));
        }

        IsActive = false;
        LeftAt = DateTime.UtcNow;

        return Result.Success();
    }
}
