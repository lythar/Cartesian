using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Communities;

public class MissingPermissionError(string permission, string communityId) : CartesianError($"Missing [{permission}] for community with id {communityId}.")
{
    public MissingPermissionError(Permissions permission, Guid communityId) : this(permission.ToString(), communityId.ToString())
    {
        
    }
}
