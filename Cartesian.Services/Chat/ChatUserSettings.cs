using Cartesian.Services.Account;

namespace Cartesian.Services.Chat;

public sealed class ChatUserSettings
{
    public required Guid Id { get; init; }
    public required string UserId { get; init; }
    public CartesianUser? User { get; init; }
    public required bool DirectMessagesEnabled { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
