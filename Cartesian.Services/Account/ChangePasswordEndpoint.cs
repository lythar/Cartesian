using System.Security.Claims;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/account/api/change-password", HandleAsync)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200)
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(CartesianIdentityError))
            .Produces(400, typeof(InvalidCredentialsError));
    }

    private async Task<IResult> HandleAsync(
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal user,
        ChangePasswordRequest request)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Results.Unauthorized();

        var appUser = await userManager.FindByIdAsync(userId);
        if (appUser == null) return Results.Unauthorized();

        var result = await userManager.ChangePasswordAsync(appUser, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            return Results.Json(new CartesianIdentityError(result.Errors), statusCode: 400);
        }

        return Results.Ok();
    }

    public record ChangePasswordRequest(string OldPassword, string NewPassword);
}
