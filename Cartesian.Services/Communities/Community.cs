using Cartesian.Services.Content;
using Cartesian.Services.Events;

namespace Cartesian.Services.Communities;

public class Community
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Permissions MemberPermissions { get; set; }
    public Guid? AvatarId { get; set; }
    public Media? Avatar { get; set; }
    public List<Event> Events { get; set; } = [];
    public List<Membership> Memberships { get; set; } = [];
    public bool InviteOnly { get; set; } = false;

    public CommunityDto ToDto() => new();
}
