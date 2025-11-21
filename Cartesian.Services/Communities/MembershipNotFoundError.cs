using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Communities;

public class MembershipNotFoundError(string userId, string communityId) 
    : CartesianError($"Membership for user {userId} in community {communityId} not found.")
{
    public string UserId { get; set; } = userId;
    public string CommunityId { get; set; } = communityId;
}
