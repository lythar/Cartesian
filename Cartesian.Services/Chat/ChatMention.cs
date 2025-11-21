using Cartesian.Services.Account;

namespace Cartesian.Services.Chat;

public sealed class ChatMention
{
    public required Guid Id { get; init; }
    public required Guid MessageId { get; init; }
    public ChatMessage? Message { get; init; }
    public required string UserId { get; init; }
    public CartesianUser? User { get; init; }
}
