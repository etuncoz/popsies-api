using MediatR;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Shared.Abstractions.Queries;

/// <summary>
/// Marker interface for queries that return a value
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
