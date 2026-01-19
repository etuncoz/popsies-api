using Popsies.Shared.Abstractions.Results;

namespace Popsies.Shared.Abstractions.Queries;

/// <summary>
/// Handler for queries that return a value
/// </summary>
public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
}
