using FluentValidation;

namespace Cartesian.Services.Search;

public class SearchQueryValidator : AbstractValidator<SearchEndpoints.SearchQueryRequest>
{
    public SearchQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);
    }
}
