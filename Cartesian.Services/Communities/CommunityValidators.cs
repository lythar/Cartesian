using FluentValidation;

namespace Cartesian.Services.Communities;

public class PostCreateCommunityBodyValidator : AbstractValidator<CreateCommunityEndpoint.PostCreateCommunityBody>
{
    public PostCreateCommunityBodyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Community name is required")
            .Length(3, 50).WithMessage("Community name must be between 3 and 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Community description is required")
            .MaximumLength(500).WithMessage("Community description must not exceed 500 characters");
    }
}

public class PutEditCommunityBodyValidator : AbstractValidator<CommunityManagementEndpoint.PutEditCommunityBody>
{
    public PutEditCommunityBodyValidator()
    {
        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .Length(3, 50).WithMessage("Community name must be between 3 and 50 characters");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Community description must not exceed 500 characters");
        });
    }
}
