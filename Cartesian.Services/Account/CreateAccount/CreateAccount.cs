using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account.CreateAccount;

public class CreateAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/account/api/create", PostCreateAccount)
            .Produces(200, typeof(CreateAccountSuccess))
            .Produces(403, typeof(UserAlreadyExistsError));
    }

    public async Task<IResult> PostCreateAccount(ILogger<CreateAccount> logger, UserManager<CartesianUser> userManager, CreateAccountBody body)
    {
        var user = new CartesianUser
        {
            Email = body.Email
        };

        var result = await userManager.CreateAsync(user, body.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("Created new user with ID {UserId}", user.Id);
            return Results.Ok(new CreateAccountSuccess());
        }
        
        throw new Exception("User creation failed for an unknown reason.");
    }
}
