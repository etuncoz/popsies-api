namespace Popsies.Modules.Identity.Contracts;

public sealed record CreateGuestResponse(
    Guid GuestId,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");
