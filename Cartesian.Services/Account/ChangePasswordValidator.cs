using FluentValidation;

namespace Cartesian.Services.Account;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordEndpoint.ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}
