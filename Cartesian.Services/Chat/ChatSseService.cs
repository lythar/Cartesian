using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Cartesian.Services.Chat;

public sealed class ChatSseService
{
    private readonly Dictionary<string, ChatUserEventChannel> _userChannels = new();
    private readonly object _lock = new();

    public async IAsyncEnumerable<ChatEvent> Subscribe(string userId, [EnumeratorCancellation] CancellationToken ct)
    {
        var channel = GetOrCreateChannel(userId);

        yield return new ChatEvent.Connected();

        await foreach (var evt in channel.Reader.ReadAllAsync(ct))
        {
            yield return evt;
        }
    }

    public async Task SendMessageAsync(string userId, ChatMessage message, CancellationToken cancellationToken = default)
    {
        var channel = GetOrCreateChannel(userId);
        var evt = new ChatEvent.NewMessage(new ChatMessageDto
        {
            Id = message.Id,
            ChannelId = message.ChannelId,
            AuthorId = message.AuthorId,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            AttachmentIds = message.Attachments.Select(a => a.Id).ToList()
        });

        await channel.Writer.WriteAsync(evt, cancellationToken);
    }

    public async Task SendMessageDeletedAsync(string userId, Guid messageId, Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = GetOrCreateChannel(userId);
        var evt = new ChatEvent.MessageDeleted(messageId, channelId);
        await channel.Writer.WriteAsync(evt, cancellationToken);
    }

    public async Task SendMessagePinnedAsync(string userId, Guid pinId, Guid messageId, Guid channelId, string pinnedById, DateTime pinnedAt, CancellationToken cancellationToken = default)
    {
        var channel = GetOrCreateChannel(userId);
        var evt = new ChatEvent.MessagePinned(pinId, messageId, channelId, pinnedById, pinnedAt);
        await channel.Writer.WriteAsync(evt, cancellationToken);
    }

    public async Task SendMessageUnpinnedAsync(string userId, Guid pinId, Guid messageId, Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = GetOrCreateChannel(userId);
        var evt = new ChatEvent.MessageUnpinned(pinId, messageId, channelId);
        await channel.Writer.WriteAsync(evt, cancellationToken);
    }

    public async Task SendReactionAddedAsync(string userId, Guid reactionId, Guid messageId, Guid channelId, string reactorUserId, string emoji, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        var channel = GetOrCreateChannel(userId);
        var evt = new ChatEvent.ReactionAdded(reactionId, messageId, channelId, reactorUserId, emoji, createdAt);
        await channel.Writer.WriteAsync(evt, cancellationToken);
    }

    public async Task SendReactionRemovedAsync(string userId, Guid reactionId, Guid messageId, Guid channelId, string reactorUserId, string emoji, CancellationToken cancellationToken = default)
    {
        var channel = GetOrCreateChannel(userId);
        var evt = new ChatEvent.ReactionRemoved(reactionId, messageId, channelId, reactorUserId, emoji);
        await channel.Writer.WriteAsync(evt, cancellationToken);
    }

    private ChatUserEventChannel GetOrCreateChannel(string userId)
    {
        lock (_lock)
        {
            if (!_userChannels.TryGetValue(userId, out var channel))
            {
                channel = new ChatUserEventChannel(Channel.CreateUnbounded<ChatEvent>());
                _userChannels[userId] = channel;
            }
            return channel;
        }
    }

    private sealed record ChatUserEventChannel(Channel<ChatEvent> Channel)
    {
        public ChannelWriter<ChatEvent> Writer => Channel.Writer;
        public ChannelReader<ChatEvent> Reader => Channel.Reader;
    }
}

public abstract record ChatEvent
{
    public sealed record Connected : ChatEvent;
    public sealed record NewMessage(ChatMessageDto Message) : ChatEvent;
    public sealed record MessageDeleted(Guid MessageId, Guid ChannelId) : ChatEvent;
    public sealed record MessagePinned(Guid PinId, Guid MessageId, Guid ChannelId, string PinnedById, DateTime PinnedAt) : ChatEvent;
    public sealed record MessageUnpinned(Guid PinId, Guid MessageId, Guid ChannelId) : ChatEvent;
    public sealed record ReactionAdded(Guid ReactionId, Guid MessageId, Guid ChannelId, string UserId, string Emoji, DateTime CreatedAt) : ChatEvent;
    public sealed record ReactionRemoved(Guid ReactionId, Guid MessageId, Guid ChannelId, string UserId, string Emoji) : ChatEvent;
}

public sealed record ChatMessageDto
{
    public required Guid Id { get; init; }
    public required Guid ChannelId { get; init; }
    public required string AuthorId { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? EditedAt { get; init; }
    public required bool IsDeleted { get; init; }
    public required List<Guid> AttachmentIds { get; init; }
    public List<ReactionSummaryDto>? ReactionSummary { get; init; }
}

public sealed record ReactionSummaryDto(
    string Emoji,
    int Count,
    List<string> UserIds,
    bool CurrentUserReacted
);
