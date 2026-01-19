using Popsies.Modules.Identity.Core.Domain.Aggregates;
using Popsies.Modules.Identity.Core.Domain.ValueObjects;

namespace Popsies.Modules.Identity.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    Task<User?> GetByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByKeycloakUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    void Update(User user);

    void Remove(User user);

    Task<IReadOnlyList<User>> GetUsersWithActiveSessionsAsync(CancellationToken cancellationToken = default);
}
