using Cartesian.Services.Account;

namespace Cartesian.Services.Chat;

public record DmChannelInfoDto(
    Guid Id,
    ChatChannelType Type,
    bool IsEnabled,
    string? Participant1Id,
    string? Participant2Id,
    CartesianUserDto? OtherUser
);
