using Popsies.Modules.Identity.Domain.Guests.Events;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Domain.Guests;

/// <summary>
/// Guest entity - temporary identity for anonymous users
/// Invariants:
/// - Guest session expires after inactivity timeout (default: 24 hours)
/// - Guest can convert to registered user
/// - Guest cannot participate in multiple active quiz sessions simultaneously
/// </summary>
public sealed class Guest : Entity
{
    private const int DefaultExpirationHours = 24;
    private const int MinDisplayNameLength = 3;
    private const int MaxDisplayNameLength = 20;

    public string DisplayName { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsExpired { get; private set; }
    public bool IsConverted { get; private set; }
    public Guid? ConvertedToUserId { get; private set; }

    // Quiz session tracking
    public Guid? ActiveQuizSessionId { get; private set; }

    private Guest() { }

    private Guest(Guid id, string displayName) : base(id)
    {
        DisplayName = displayName;
        ExpiresAt = DateTime.UtcNow.AddHours(DefaultExpirationHours);
        IsExpired = false;
        IsConverted = false;
    }

    public static Result<Guest> Create(Guid guestId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<Guest>(Error.Validation("DisplayName", "Display name cannot be empty"));
        }

        if (displayName.Length < MinDisplayNameLength || displayName.Length > MaxDisplayNameLength)
        {
            return Result.Failure<Guest>(Error.Validation("DisplayName",
                $"Display name must be {MinDisplayNameLength}-{MaxDisplayNameLength} characters long"));
        }

        var guest = new Guest(guestId, displayName);

        guest.RaiseDomainEvent(new GuestCreatedEvent(guestId, displayName));

        return Result.Success(guest);
    }

    public void CheckExpiration(DateTime currentTime)
    {
        if (!IsExpired && currentTime >= ExpiresAt)
        {
            IsExpired = true;
            RaiseDomainEvent(new GuestExpiredEvent(Id));
        }
    }

    public Result ConvertToUser(Guid userId)
    {
        if (IsConverted)
        {
            return Result.Failure(Error.Create("Guest.AlreadyConverted",
                "Guest has already been converted to a user"));
        }

        if (IsExpired)
        {
            return Result.Failure(Error.Create("Guest.Expired",
                "Cannot convert expired guest to user"));
        }

        IsConverted = true;
        ConvertedToUserId = userId;

        RaiseDomainEvent(new GuestConvertedEvent(Id, userId));

        return Result.Success();
    }

    public Result StartQuizSession(Guid sessionId)
    {
        if (ActiveQuizSessionId.HasValue)
        {
            return Result.Failure(Error.Create("Guest.ActiveSession",
                "Guest is already participating in an active quiz session"));
        }

        ActiveQuizSessionId = sessionId;

        return Result.Success();
    }

    public void EndQuizSession()
    {
        ActiveQuizSessionId = null;
    }
}
