using Cartesian.Services.Account;

namespace Cartesian.Services.Communities;

public record MembershipDto(Guid Id, string UserId, CartesianUserDto User, Guid CommunityId, CommunityDto? Community, Guid? ChannelId, Permissions Permissions, DateTime CreatedAt);
