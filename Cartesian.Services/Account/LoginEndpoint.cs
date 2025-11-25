using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/account/api/login", PostLogin)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(LoginSuccess))
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(InvalidCredentialsError));
    }

    private async Task<IResult> PostLogin(ILogger<LoginEndpoint> logger, UserManager<CartesianUser> userManager,
        ClaimsService claimsService, LoginBody body, HttpContext httpContext)
    {
        var user = await userManager.FindByEmailAsync(body.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, body.Password))
            return Results.Json(new InvalidCredentialsError(), statusCode: 400);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true
        };

        var principal = claimsService.GetPrincipalForUser(user);

        await httpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);

        logger.LogInformation("User {Email} logged in at {Time}.", user.Email, DateTime.UtcNow);

        return Results.Ok(new LoginSuccess(user.ToMyUserDto()));
    }

    public record LoginBody(string Email, string Password);

    private record LoginSuccess(MyUserDto Me);
}
