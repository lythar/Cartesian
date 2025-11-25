using Cartesian.Services.Account;
using Cartesian.Services.Chat;

namespace Cartesian.Services.Communities;

public class Membership
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Permissions Permissions { get; set; } = Permissions.None;
    public required string UserId { get; set; }
    public CartesianUser User { get; set; } = null!;
    public required Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public MembershipDto ToDto(ChatChannel? channel = null) => new(Id, UserId, User.ToDto(), CommunityId, Community?.ToDto(), channel?.Id, Permissions, CreatedAt);
}
