namespace Popsies.Shared.Abstractions.Queries;

/// <summary>
/// Base class for paginated queries
/// </summary>
public abstract record PagedQuery : IQuery<PagedResult<object>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    protected PagedQuery()
    {
    }

    protected PagedQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
    }
}

/// <summary>
/// Base class for typed paginated queries
/// </summary>
public abstract record PagedQuery<T> : IQuery<PagedResult<T>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    protected PagedQuery()
    {
    }

    protected PagedQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
    }
}
