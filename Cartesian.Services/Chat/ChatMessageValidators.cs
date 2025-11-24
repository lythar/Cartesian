using FluentValidation;

namespace Cartesian.Services.Chat;

public class SendMessageRequestValidator : AbstractValidator<ChatMessageEndpoints.SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(2000).WithMessage("Message content must not exceed 2000 characters");

        RuleFor(x => x.MentionedUserIds)
            .Must(mentions => mentions == null || mentions.Count <= 50)
            .WithMessage("Maximum 50 mentions allowed per message");

        RuleFor(x => x.AttachmentIds)
            .Must(attachments => attachments == null || attachments.Count <= 10)
            .WithMessage("Maximum 10 attachments allowed per message");
    }
}
