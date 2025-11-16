using Cartesian.Services.Account;

namespace Cartesian.Services.Communities;

public class Membership
{
    public Guid Id { get; set; }
    public Permissions Permissions { get; set; } = Permissions.None;
    public required string UserId { get; set; }
    public CartesianUser User { get; set; } = null!;
    public required Guid CommunityId { get; set; }
    public Community Community { get; set; } = null!;
}
