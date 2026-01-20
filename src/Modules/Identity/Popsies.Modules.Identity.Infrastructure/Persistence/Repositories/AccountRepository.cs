using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.ValueObjects;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(IdentityDbContext context) : IAccountRepository
{
    private readonly IdentityDbContext _dbContext = context;

    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts.FindAsync(new object[] { accountId }, cancellationToken);
    }

    public async Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }

    public async Task<Account?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Email.Value == email.Value, cancellationToken);
    }

    public async Task<Account?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a =>
                a.Username.DisplayName == username.DisplayName &&
                a.Username.Discriminator == username.Discriminator,
                cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public void Update(Account account)
    {
        _dbContext.Accounts.Update(account);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .AnyAsync(a => a.Email.Value == email.Value, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .AnyAsync(a =>
                a.Username.DisplayName == username.DisplayName &&
                a.Username.Discriminator == username.Discriminator,
                cancellationToken);
    }
}
