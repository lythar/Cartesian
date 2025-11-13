using System.Security.Claims;
using Cartesian.Services.Endpoints;

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

    private async Task<IResult> GetVerify(ClaimsPrincipal claimsPrincipal)
    {
        var name = claimsPrincipal.Identity?.Name;
        var email = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var claims = claimsPrincipal.Claims.Select(c => new ClaimSummary(c.Type, c.Value));

        return Results.Ok(new VerifySuccess(name, email, claims));
    }

    private record ClaimSummary(string Type, string Value);

    private record VerifySuccess(string? Name, string? Email, IEnumerable<ClaimSummary> Claims);
}
