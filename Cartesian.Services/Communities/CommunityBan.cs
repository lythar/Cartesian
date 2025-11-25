using Cartesian.Services.Account;

namespace Cartesian.Services.Communities;

public class CommunityBan
{
    public required Guid Id { get; init; }
    public required Guid CommunityId { get; init; }
    public Community Community { get; init; } = null!;
    public required string UserId { get; init; }
    public CartesianUser User { get; init; } = null!;
    public required string BannedById { get; init; }
    public CartesianUser BannedBy { get; init; } = null!;
    public string? Reason { get; set; }
    public required DateTime CreatedAt { get; init; }
}

public record CommunityBanDto(
    Guid Id,
    Guid CommunityId,
    string UserId,
    CartesianUserDto User,
    string BannedById,
    CartesianUserDto BannedBy,
    string? Reason,
    DateTime CreatedAt);
