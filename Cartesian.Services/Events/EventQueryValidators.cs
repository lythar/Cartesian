using FluentValidation;

namespace Cartesian.Services.Events;

public class GetEventListRequestValidator : AbstractValidator<EventQueryEndpoints.GetEventListRequest>
{
    public GetEventListRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");

        When(x => x.Tags != null, () =>
        {
            RuleFor(x => x.Tags!.Length)
                .LessThanOrEqualTo(10).WithMessage("Maximum 10 tags allowed");
        });

        When(x => x.StartDate != null && x.EndDate != null, () =>
        {
            RuleFor(x => x)
                .Must(x => x.StartDate < x.EndDate)
                .WithMessage("Start date must be before end date")
                .WithName("StartDate");
        });

        When(x => x.Visibility != null, () =>
        {
            RuleFor(x => x.Visibility)
                .IsInEnum().WithMessage("Invalid visibility value");
        });

        When(x => x.Timing != null, () =>
        {
            RuleFor(x => x.Timing)
                .IsInEnum().WithMessage("Invalid timing value");
        });
    }
}
