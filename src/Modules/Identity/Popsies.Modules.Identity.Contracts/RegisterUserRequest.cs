using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Identity.Contracts;

public sealed record RegisterUserRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(20)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(64)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;
}
