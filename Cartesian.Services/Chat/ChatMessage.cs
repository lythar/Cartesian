using Cartesian.Services.Account;
using Cartesian.Services.Content;

namespace Cartesian.Services.Chat;

public sealed class ChatMessage
{
    public required Guid Id { get; init; }
    public required Guid ChannelId { get; init; }
    public ChatChannel? Channel { get; init; }
    public required string AuthorId { get; init; }
    public CartesianUser? Author { get; init; }
    public required string Content { get; set; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Media> Attachments { get; init; } = [];
    public ICollection<ChatPinnedMessage> PinnedIn { get; init; } = [];
    public ICollection<ChatReaction> Reactions { get; init; } = [];
}
