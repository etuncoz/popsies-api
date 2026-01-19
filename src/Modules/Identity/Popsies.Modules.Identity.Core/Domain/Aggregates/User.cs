using Popsies.Modules.Identity.Core.Domain.Events;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Core.Domain.Aggregates;

/// <summary>
/// User aggregate root
/// Invariants:
/// - User cannot participate in multiple active quiz sessions simultaneously
/// - User cannot update profile when deleted
/// </summary>
public sealed class User : AggregateRoot
{
    private const int MinDisplayNameLength = 3;
    private const int MaxDisplayNameLength = 20;

    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public string? KeycloakUserId { get; private set; }
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Quiz session tracking
    public Guid? ActiveQuizSessionId { get; private set; }

    // Statistics
    public int TotalQuizzesPlayed { get; private set; }
    public int TotalWins { get; private set; }
    public double AverageScore { get; private set; }

    private User() { }

    private User(Guid id, Username username, Email email) : base(id)
    {
        Username = username;
        Email = email;
        DisplayName = username.DisplayName;
        IsDeleted = false;
        TotalQuizzesPlayed = 0;
        TotalWins = 0;
        AverageScore = 0;
    }

    public static User Create(Guid userId, Username username, Email email)
    {
        var user = new User(userId, username, email);

        user.RaiseDomainEvent(new UserRegisteredEvent(
            userId,
            username.FullUsername,
            email.Value));

        return user;
    }

    public Result UpdateProfile(string displayName, string? avatarUrl)
    {
        if (IsDeleted)
        {
            return Result.Failure(Error.Create("User.CannotUpdate", "Cannot update profile of deleted user"));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(Error.Validation("DisplayName", "Display name cannot be empty"));
        }

        if (displayName.Length < MinDisplayNameLength || displayName.Length > MaxDisplayNameLength)
        {
            return Result.Failure(Error.Validation("DisplayName",
                $"Display name must be {MinDisplayNameLength}-{MaxDisplayNameLength} characters long"));
        }

        if (avatarUrl is not null && !IsValidUrl(avatarUrl))
        {
            return Result.Failure(Error.Validation("AvatarUrl", "Invalid avatar URL. Must be a valid HTTP/HTTPS URL"));
        }

        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        MarkAsUpdated();

        RaiseDomainEvent(new UserProfileUpdatedEvent(Id, displayName, avatarUrl));

        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
        {
            return Result.Failure(Error.Create("User.AlreadyDeleted", "User is already deleted"));
        }

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();

        RaiseDomainEvent(new UserDeletedEvent(Id));

        return Result.Success();
    }

    public Result LinkToKeycloak(string keycloakUserId)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return Result.Failure(Error.Validation("KeycloakUserId", "Keycloak user ID cannot be empty"));
        }

        if (KeycloakUserId is not null)
        {
            return Result.Failure(Error.Create("User.AlreadyLinked", "User is already linked to Keycloak"));
        }

        KeycloakUserId = keycloakUserId;
        MarkAsUpdated();

        return Result.Success();
    }

    public Result StartQuizSession(Guid sessionId)
    {
        if (ActiveQuizSessionId.HasValue)
        {
            return Result.Failure(Error.Create("User.ActiveSession",
                "User is already participating in an active quiz session"));
        }

        ActiveQuizSessionId = sessionId;
        MarkAsUpdated();

        return Result.Success();
    }

    public void EndQuizSession()
    {
        ActiveQuizSessionId = null;
        MarkAsUpdated();
    }

    public void UpdateStatistics(int totalQuizzes, int wins, double averageScore)
    {
        TotalQuizzesPlayed = totalQuizzes;
        TotalWins = wins;
        AverageScore = averageScore;
        MarkAsUpdated();
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
