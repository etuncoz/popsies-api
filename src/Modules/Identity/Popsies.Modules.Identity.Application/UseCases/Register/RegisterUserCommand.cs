using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Identity.Application.UseCases.Register;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand<Guid>;
