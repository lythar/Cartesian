using FluentValidation;

namespace Cartesian.Services.Events;

public class CreateEventBodyValidator : AbstractValidator<CreateEventEndpoint.CreateEventBody>
{
    public CreateEventBodyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required")
            .Length(3, 100).WithMessage("Event name must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Event description is required")
            .MaximumLength(2000).WithMessage("Event description must not exceed 2000 characters");

        RuleFor(x => x.Location)
            .NotNull().WithMessage("Location is required");

        RuleFor(x => x.Tags)
            .NotNull().WithMessage("Tags are required")
            .Must(tags => tags.Count <= 10).WithMessage("Maximum 10 tags allowed");

        RuleForEach(x => x.Windows).SetValidator(new CreateEventWindowBodyValidator());
    }
}

public class CreateEventWindowBodyValidator : AbstractValidator<CreateEventEndpoint.CreateEventWindowBody>
{
    public CreateEventWindowBodyValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Event window title is required")
            .Length(3, 100).WithMessage("Event window title must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Event window description is required")
            .MaximumLength(2000).WithMessage("Event window description must not exceed 2000 characters");

        RuleFor(x => x.StartTime)
            .NotNull().WithMessage("Start time is required");

        RuleFor(x => x.EndTime)
            .NotNull().WithMessage("End time is required");

        RuleFor(x => x)
            .Must(x => x.StartTime == null || x.EndTime == null || x.StartTime < x.EndTime)
            .WithMessage("Start time must be before end time")
            .WithName("StartTime");
    }
}
