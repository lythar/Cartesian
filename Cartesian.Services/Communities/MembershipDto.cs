using Cartesian.Services.Account;

namespace Cartesian.Services.Communities;

public record MembershipDto(Guid Id, string UserId, CartesianUserDto User, Guid CommunityId, Permissions Permissions, DateTime CreatedAt);
