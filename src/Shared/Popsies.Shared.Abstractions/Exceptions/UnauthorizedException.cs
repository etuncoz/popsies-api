namespace Popsies.Shared.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a user is not authenticated
/// </summary>
public class UnauthorizedException : PopsiesException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base("Unauthorized", message)
    {
    }
}

/// <summary>
/// Exception thrown when a user is authenticated but not authorized
/// </summary>
public class ForbiddenException : PopsiesException
{
    public ForbiddenException(string message = "Forbidden access")
        : base("Forbidden", message)
    {
    }
}
