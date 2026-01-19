using Popsies.Shared.Abstractions.Results;

namespace Popsies.Shared.Abstractions.Commands;

/// <summary>
/// Dispatcher for sending commands
/// </summary>
public interface ICommandDispatcher
{
    Task<Result> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<Result<TResponse>> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;
}
