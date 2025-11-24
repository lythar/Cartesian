using System.Security.Claims;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/account/api/me/password", PutChangePassword)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200)
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(InvalidCredentialsError))
            .Produces(400, typeof(CartesianIdentityError));
    }

    private async Task<IResult> PutChangePassword(
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        ChangePasswordBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, body.CurrentPassword, body.NewPassword);

        if (!result.Succeeded)
        {
            // If the error is about the current password being incorrect, return InvalidCredentialsError
            if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return Results.Json(new InvalidCredentialsError(), statusCode: 400);
            }

            return Results.Json(new CartesianIdentityError(result.Errors), statusCode: 400);
        }

        return Results.Ok();
    }

    public record ChangePasswordBody(string CurrentPassword, string NewPassword);
}
