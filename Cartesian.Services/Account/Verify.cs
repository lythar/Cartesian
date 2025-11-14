using System.Security.Claims;
using Cartesian.Services.Account.Types;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class Verify : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/account/api/verify", GetVerify)
            .RequireAuthorization()
            .Produces(200, typeof(VerifySuccess))
            .Produces(401);
    }

    private async Task<IResult> GetVerify(ClaimsPrincipal claimsPrincipal, UserManager<CartesianUser> userManager)
    {
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == null) return Results.Unauthorized();

        var claims = claimsPrincipal.Claims.Select(c => new ClaimSummary(c.Type, c.Value));

        return Results.Ok(new VerifySuccess(new MyUser(user), claims));
    }

    private record ClaimSummary(string Type, string Value);

    private record VerifySuccess(MyUser Me, IEnumerable<ClaimSummary> Claims);
}
