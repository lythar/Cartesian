using FluentValidation;

namespace Cartesian.Services.Account;

public class GetPublicAccountsByIdRequestValidator : AbstractValidator<UserQueryEndpoints.GetPublicAccountsByIdRequest>
{
    public GetPublicAccountsByIdRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.AccountIds)
            .NotNull().WithMessage("Account IDs are required")
            .Must(ids => ids.Length > 0).WithMessage("At least one account ID is required")
            .Must(ids => ids.Length <= 100).WithMessage("Maximum 100 account IDs allowed");
    }
}
