using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Session.Application.UseCases.JoinSession;

/// <summary>
/// Command for a player to join a session
/// </summary>
public sealed record JoinSessionCommand(
    string SessionCode,
    Guid UserId,
    string DisplayName) : ICommand<Guid>;
