using FluentValidation;

namespace Cartesian.Services.Communities;

public class PostInviteMemberBodyValidator : AbstractValidator<MembershipManagementEndpoint.PostInviteMemberBody>
{
    public PostInviteMemberBodyValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(450).WithMessage("User ID must not exceed 450 characters");
    }
}

public class PutUpdateMemberPermissionsBodyValidator : AbstractValidator<MembershipManagementEndpoint.PutUpdateMemberPermissionsBody>
{
    public PutUpdateMemberPermissionsBodyValidator()
    {
        RuleFor(x => x.Permissions)
            .IsInEnum().WithMessage("Invalid permissions value");
    }
}

public class GetCommunityMembersRequestValidator : AbstractValidator<MembershipManagementEndpoint.GetCommunityMembersRequest>
{
    public GetCommunityMembersRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.SortBy)
            .IsInEnum().WithMessage("Invalid sort option");
    }
}
