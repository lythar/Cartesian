using Cartesian.Services.Account;

namespace Cartesian.Services.Chat;

public sealed class ChatMute
{
    public required Guid Id { get; init; }
    public required Guid ChannelId { get; init; }
    public ChatChannel? Channel { get; init; }
    public required string UserId { get; init; }
    public CartesianUser? User { get; init; }
    public required string MutedById { get; init; }
    public CartesianUser? MutedBy { get; init; }
    public required DateTime MutedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Reason { get; init; }
}
