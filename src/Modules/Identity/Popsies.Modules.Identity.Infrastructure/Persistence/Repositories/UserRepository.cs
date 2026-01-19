using Microsoft.EntityFrameworkCore;
using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Modules.Identity.Core.Repositories;

namespace Popsies.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    private readonly IdentityDbContext _dbContext = context;

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Username.DisplayName == username.DisplayName &&
                u.Username.Discriminator == username.Discriminator,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u =>
                u.Username.DisplayName == username.DisplayName &&
                u.Username.Discriminator == username.Discriminator,
                cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersWithActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Where(u => u.ActiveQuizSessionId != null)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        _dbContext.Users.Remove(user);
    }

    public async Task<User?> GetByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.KeycloakUserId == keycloakUserId, cancellationToken);
    }

    public async Task<bool> ExistsByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.KeycloakUserId == keycloakUserId, cancellationToken);
    }
}
