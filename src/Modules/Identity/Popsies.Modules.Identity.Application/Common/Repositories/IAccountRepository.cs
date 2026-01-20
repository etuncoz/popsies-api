using Popsies.Modules.Identity.Domain.Accounts;
using Popsies.Modules.Identity.Domain.ValueObjects;

namespace Popsies.Modules.Identity.Application.Common.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Account?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<Account?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    void Update(Account account);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default);
}
