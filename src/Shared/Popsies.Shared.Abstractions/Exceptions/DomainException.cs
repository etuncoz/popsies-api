namespace Popsies.Shared.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated
/// </summary>
public class DomainException : PopsiesException
{
    public DomainException(string errorCode, string message)
        : base(errorCode, message)
    {
    }

    public DomainException(string errorCode, string message, Exception innerException)
        : base(errorCode, message, innerException)
    {
    }
}
