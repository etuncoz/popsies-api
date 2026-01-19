using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Identity.Core.Commands;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand<Guid>;
