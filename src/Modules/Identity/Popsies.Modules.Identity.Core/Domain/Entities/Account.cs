using Popsies.Modules.Identity.Core.Domain.Enums;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Core.Domain.Entities;

/// <summary>
/// Account entity
/// Invariants:
/// - Account state management (Pending, Active, Suspended, Deleted)
/// - Email verification tracking
/// - Last login tracking
/// NOTE: Password and lockout management delegated to Keycloak
/// </summary>
public sealed class Account : Entity
{
    public Guid UserId { get; private set; }
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public AccountState State { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private Account() { }

    private Account(Guid id, Guid userId, Username username, Email email) : base(id)
    {
        UserId = userId;
        Username = username;
        Email = email;
        State = AccountState.Pending;
        IsEmailVerified = false;
    }

    public static Account Create(Guid accountId, Guid userId, Username username, Email email)
    {
        return new Account(accountId, userId, username, email);
    }

    public void VerifyEmail()
    {
        if (!IsEmailVerified)
        {
            IsEmailVerified = true;
            if (State == AccountState.Pending)
            {
                State = AccountState.Active;
            }
        }
    }

    public Result RecordSuccessfulLogin()
    {
        if (State == AccountState.Deleted)
        {
            return Result.Failure(Error.Create("Account.Deleted", "Cannot login to deleted account"));
        }

        if (State == AccountState.Suspended)
        {
            return Result.Failure(Error.Create("Account.Suspended", "Account is suspended"));
        }

        LastLoginAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void Suspend()
    {
        if (State != AccountState.Suspended)
        {
            State = AccountState.Suspended;
        }
    }

    public void Activate()
    {
        if (State == AccountState.Suspended)
        {
            State = AccountState.Active;
        }
    }

    public void Delete()
    {
        if (State != AccountState.Deleted)
        {
            State = AccountState.Deleted;
        }
    }
}
