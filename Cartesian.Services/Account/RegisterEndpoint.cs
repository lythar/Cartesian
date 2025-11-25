using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/account/api/register", PostRegister)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(RegisterSuccess))
            .Produces(400, typeof(ValidationError))
            .Produces(403, typeof(CartesianIdentityError));
    }

    private async Task<IResult> PostRegister(ILogger<RegisterEndpoint> logger, UserManager<CartesianUser> userManager,
        ClaimsService claimsService, RegisterBody body, HttpContext httpContext)
    {
        var user = new CartesianUser { UserName = body.Username, Email = body.Email };

        var result = await userManager.CreateAsync(user, body.Password);

        if (!result.Succeeded) return Results.Json(new CartesianIdentityError(result.Errors), statusCode: 403);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true
        };

        var principal = claimsService.GetPrincipalForUser(user);

        await httpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);

        logger.LogInformation("Created new user with ID {UserId}", user.Id);
        return Results.Ok(new RegisterSuccess(user.ToMyUserDto()));
    }

    public record RegisterBody(string Username, string Email, string Password);

    private record RegisterSuccess(MyUserDto Me);
}
