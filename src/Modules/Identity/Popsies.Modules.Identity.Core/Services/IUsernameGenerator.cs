using Popsies.Modules.Identity.Core.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Core.Services;

public interface IUsernameGenerator
{
    Task<Result<Username>> GenerateUniqueUsernameAsync(string displayName, CancellationToken cancellationToken = default);
}
