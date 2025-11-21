using FluentValidation;

namespace Cartesian.Services.Communities;

public class GetMyMembershipsRequestValidator : AbstractValidator<CommunityQueryEndpoints.GetMyMembershipsRequest>
{
    public GetMyMembershipsRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");
    }
}

public class GetCommunityListRequestValidator : AbstractValidator<CommunityQueryEndpoints.GetCommunityListRequest>
{
    public GetCommunityListRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");
    }
}
