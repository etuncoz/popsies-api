using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Session.Application.UseCases.StartSession;

/// <summary>
/// Command to start a quiz session
/// </summary>
public sealed record StartSessionCommand(Guid SessionId) : ICommand;
