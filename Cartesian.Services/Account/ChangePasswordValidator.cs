using FluentValidation;

namespace Cartesian.Services.Account;

public class ChangePasswordBodyValidator : AbstractValidator<ChangePasswordEndpoint.ChangePasswordBody>
{
    public ChangePasswordBodyValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .Length(8, 128).WithMessage("Password must be between 8 and 128 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character")
            .Must((model, newPassword) => newPassword != model.CurrentPassword)
            .WithMessage("New password must be different from current password");
    }
}
