using Cartesian.Services.Content;

namespace Cartesian.Services.Communities;

public record CommunityDto(Guid Id, string Name, string Description, bool InviteOnly, Permissions MemberPermissions, MediaDto? Avatar);
