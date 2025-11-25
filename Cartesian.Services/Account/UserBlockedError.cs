using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Account;

public class UserBlockedError()
    : CartesianError("This user has blocked you or you have blocked them.");
