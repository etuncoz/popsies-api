namespace Popsies.Modules.Identity.Contracts;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Username,
    string Email,
    string Message);
