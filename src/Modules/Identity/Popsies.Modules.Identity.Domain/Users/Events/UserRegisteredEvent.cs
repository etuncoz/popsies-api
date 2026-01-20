using Popsies.Shared.Abstractions.Events;

namespace Popsies.Modules.Identity.Domain.Users.Events;

public sealed record UserRegisteredEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }

    public UserRegisteredEvent(Guid userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
    }
}
