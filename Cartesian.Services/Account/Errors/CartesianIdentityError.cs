using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account.Errors;

public class CartesianIdentityError(IEnumerable<IdentityError> identityErrors) : CartesianError("IdentityError")
{
    public IEnumerable<IdentityError> IdentityErrors { get; } = identityErrors;
}
