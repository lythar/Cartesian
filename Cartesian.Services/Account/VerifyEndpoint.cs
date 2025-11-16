using System.Security.Claims;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class VerifyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/account/api/verify", GetVerify)
            .RequireAuthorization()
            .Produces(200, typeof(VerifySuccess));
    }

    private async Task<IResult> GetVerify(ClaimsPrincipal claimsPrincipal, UserManager<CartesianUser> userManager)
    {
        var user = await userManager.GetUserAsync(claimsPrincipal);

        if (user == null) return Results.Unauthorized();

        var claims = claimsPrincipal.Claims.Select(c => new ClaimSummary(c.Type, c.Value));

        return Results.Ok(new VerifySuccess(user.ToMyUserDto(), claims));
    }

    private record ClaimSummary(string Type, string Value);

    private record VerifySuccess(MyUserDto Me, IEnumerable<ClaimSummary> Claims);
}
