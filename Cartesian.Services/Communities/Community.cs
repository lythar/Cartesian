using Cartesian.Services.Content;
using Cartesian.Services.Events;

namespace Cartesian.Services.Communities;

public class Community
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string Name { get; set; }
    public required string Description { get; set; }
    public Permissions MemberPermissions { get; set; } = Permissions.Member;
    public Guid? AvatarId { get; set; }
    public Media? Avatar { get; set; }
    public List<Event> Events { get; set; } = [];
    public List<Membership> Memberships { get; set; } = [];
    public List<Media> Images { get; set; } = [];
    public bool InviteOnly { get; set; } = false;

    public CommunityDto ToDto() => new(Id, Name, Description, InviteOnly, MemberPermissions, Avatar?.ToDto());
}
