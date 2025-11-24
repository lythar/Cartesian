using Cartesian.Services.Account;

namespace Cartesian.Services.Chat;

public sealed class ChatPinnedMessage
{
    public required Guid Id { get; init; }
    public required Guid ChannelId { get; init; }
    public ChatChannel? Channel { get; init; }
    public required Guid MessageId { get; init; }
    public ChatMessage? Message { get; init; }
    public required string PinnedById { get; init; }
    public CartesianUser? PinnedBy { get; init; }
    public required DateTime PinnedAt { get; init; }
}
