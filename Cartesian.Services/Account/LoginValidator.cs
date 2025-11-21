using FluentValidation;

namespace Cartesian.Services.Account;

public class LoginBodyValidator : AbstractValidator<LoginEndpoint.LoginBody>
{
    public LoginBodyValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters");
    }
}
