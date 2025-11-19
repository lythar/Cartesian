using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Communities;

public class MissingPermissionError(Permissions required, Guid communityId) : CartesianError($"Missing '{required}' for community with id '{communityId}'.")
{
    public uint RequiredPermissions { get; } = (uint)required;
    public IEnumerable<string> DisplayPermissions { get; } = required.ToString().Split(", ");
}
