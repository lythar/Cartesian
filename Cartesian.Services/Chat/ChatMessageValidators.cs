using FluentValidation;

namespace Cartesian.Services.Chat;

public class SendDirectMessageRequestValidator : AbstractValidator<ChatMessageEndpoints.SendDirectMessageRequest>
{
    public SendDirectMessageRequestValidator()
    {
        RuleFor(x => x.RecipientId)
            .NotEmpty().WithMessage("Recipient ID is required");

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

public class SendCommunityMessageRequestValidator : AbstractValidator<ChatMessageEndpoints.SendCommunityMessageRequest>
{
    public SendCommunityMessageRequestValidator()
    {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required");

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

public class SendEventMessageRequestValidator : AbstractValidator<ChatMessageEndpoints.SendEventMessageRequest>
{
    public SendEventMessageRequestValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required");

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
