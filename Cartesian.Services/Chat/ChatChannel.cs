using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Events;

namespace Cartesian.Services.Chat;

public sealed class ChatChannel
{
    public required Guid Id { get; init; }
    public required ChatChannelType Type { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required bool IsEnabled { get; set; }

    // For DM channels - both participants
    public string? Participant1Id { get; init; }
    public CartesianUser? Participant1 { get; init; }
    public string? Participant2Id { get; init; }
    public CartesianUser? Participant2 { get; init; }

    // For Community channels
    public Guid? CommunityId { get; init; }
    public Community? Community { get; init; }

    // For Event channels
    public Guid? EventId { get; init; }
    public Event? Event { get; init; }

    public ICollection<ChatMessage> Messages { get; init; } = [];
    public ICollection<ChatMute> Mutes { get; init; } = [];
}
