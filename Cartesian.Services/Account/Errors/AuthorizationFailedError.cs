using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Account.Errors;

public class AuthorizationFailedError(string path) : CartesianError($"Authorization failed for {path}")
{
    public string Path { get; } = path;
}
