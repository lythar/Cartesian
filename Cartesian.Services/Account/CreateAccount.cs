using Cartesian.Services.Account.Errors;
using Cartesian.Services.Account.Types;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account;

public class CreateAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/account/api/create", PostCreateAccount)
            .AllowAnonymous()
            .Produces(200, typeof(CreateAccountSuccess))
            .Produces(403, typeof(CartesianIdentityError));
    }

    private async Task<IResult> PostCreateAccount(ILogger<CreateAccount> logger, UserManager<CartesianUser> userManager,
        ClaimsService claimsService, CreateAccountBody body, HttpContext httpContext)
    {
        var user = new CartesianUser { UserName = body.Username, Email = body.Email };

        var result = await userManager.CreateAsync(user, body.Password);

        if (!result.Succeeded) return Results.Json(new CartesianIdentityError(result.Errors), statusCode: 403);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow
        };

        var principal = claimsService.GetPrincipalForUser(user);

        await httpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);

        logger.LogInformation("Created new user with ID {UserId}", user.Id);
        return Results.Ok(new CreateAccountSuccess(new MyUser(user)));
    }

    private record CreateAccountBody(string Username, string Email, string Password);

    private record CreateAccountSuccess(MyUser Me);
}
