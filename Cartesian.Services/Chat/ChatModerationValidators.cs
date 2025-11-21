using FluentValidation;

namespace Cartesian.Services.Chat;

public class MuteUserRequestValidator : AbstractValidator<ChatModerationEndpoints.MuteUserRequest>
{
    public MuteUserRequestValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty().WithMessage("Channel ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Mute reason must not exceed 500 characters");

        RuleFor(x => x.ExpiresAt)
            .Must(expiry => expiry == null || expiry > DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future");
    }
}
