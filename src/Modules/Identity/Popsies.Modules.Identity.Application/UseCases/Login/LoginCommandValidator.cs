using FluentValidation;

namespace Popsies.Modules.Identity.Application.UseCases.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty().WithMessage("Device info is required")
            .MaximumLength(500).WithMessage("Device info must not exceed 500 characters");
    }
}
