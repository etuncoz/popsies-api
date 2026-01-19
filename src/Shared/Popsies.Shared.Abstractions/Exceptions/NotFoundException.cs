namespace Popsies.Shared.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : PopsiesException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName}.NotFound", $"{entityName} with key '{key}' was not found")
    {
    }

    public NotFoundException(string message)
        : base("NotFound", message)
    {
    }
}
