namespace Popsies.Shared.Abstractions.Exceptions;

/// <summary>
/// Base exception for all Popsies domain exceptions
/// </summary>
public abstract class PopsiesException : Exception
{
    public string ErrorCode { get; }

    protected PopsiesException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected PopsiesException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
