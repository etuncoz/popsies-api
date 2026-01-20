using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Application.Common.Services;

public interface IUsernameGenerator
{
    Task<Result<Username>> GenerateUniqueUsernameAsync(string displayName, CancellationToken cancellationToken = default);
}
