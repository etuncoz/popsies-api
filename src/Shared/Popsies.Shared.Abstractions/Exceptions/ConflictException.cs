namespace Popsies.Shared.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with existing state
/// </summary>
public class ConflictException : PopsiesException
{
    public ConflictException(string message)
        : base("Conflict", message)
    {
    }

    public ConflictException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
