using FluentValidation;

namespace Cartesian.Services.Chat;

public class AddReactionRequestValidator : AbstractValidator<AddReactionRequest>
{
    public AddReactionRequestValidator()
    {
        RuleFor(x => x.Emoji)
            .NotEmpty()
            .WithMessage("Emoji is required")
            .Must(EmojiValidator.IsValidEmoji)
            .WithMessage("Must be a valid single emoji (supports skin tones, flags, and sequences)");
    }
}

public record AddReactionRequest(string Emoji);
