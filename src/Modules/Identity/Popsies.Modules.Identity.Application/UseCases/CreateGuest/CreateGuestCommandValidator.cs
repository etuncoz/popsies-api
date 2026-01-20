using FluentValidation;

namespace Popsies.Modules.Identity.Application.UseCases.CreateGuest;

public sealed class CreateGuestCommandValidator : AbstractValidator<CreateGuestCommand>
{
    public CreateGuestCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty().WithMessage("Device info is required")
            .MaximumLength(500).WithMessage("Device info must not exceed 500 characters");
    }
}
