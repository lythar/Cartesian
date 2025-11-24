using FluentValidation;

namespace Cartesian.Services.Chat;

public class GetPinnedMessagesQueryValidator : AbstractValidator<GetPinnedMessagesQuery>
{
    public GetPinnedMessagesQueryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 200)
            .WithMessage("Limit must be between 1 and 200");
    }
}

public record GetPinnedMessagesQuery(int Limit = 50, DateTime? Before = null);
