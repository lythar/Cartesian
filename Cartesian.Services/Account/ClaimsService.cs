using System.Security.Claims;
using Cartesian.Services.Account.Entities;
using Cartesian.Services.Database;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class ClaimsService
{
    public ClaimsPrincipal GetPrincipalForUser(CartesianUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, IdentityConstants.ApplicationScheme);

        return new ClaimsPrincipal(claimsIdentity);
    }
}
