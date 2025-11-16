using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Communities;

public class CommunityNotFoundError(string communityId) : CartesianError($"Community {communityId} not found.")
{
    public string CommunityId { get; set; } = communityId;
}
