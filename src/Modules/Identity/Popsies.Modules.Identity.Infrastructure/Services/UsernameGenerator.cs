using Popsies.Modules.Identity.Application.Common.Repositories;
using Popsies.Modules.Identity.Application.Common.Services;
using Popsies.Modules.Identity.Domain.ValueObjects;
using Popsies.Shared.Abstractions.Exceptions;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Identity.Infrastructure.Services;

internal sealed class UsernameGenerator : IUsernameGenerator
{
    private readonly IUserRepository _userRepository;
    private readonly Random _random = new();

    public UsernameGenerator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<Username>> GenerateUniqueUsernameAsync(string displayName, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var discriminator = _random.Next(1, 10000);
            var usernameResult = Username.Create(displayName, discriminator);

            if (usernameResult.IsFailure)
            {
                return usernameResult;
            }

            var exists = await _userRepository.ExistsByUsernameAsync(usernameResult.Value, cancellationToken);
            if (!exists)
            {
                return usernameResult;
            }
        }

        // If all attempts failed, return failure
        return Result.Failure<Username>(Error.Create("UsernameGenerator.Failed",
            $"Failed to generate unique username for '{displayName}' after {maxAttempts} attempts"));
    }
}
