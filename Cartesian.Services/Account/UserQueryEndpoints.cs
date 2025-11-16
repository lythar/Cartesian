using System.Security.Claims;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Account;

public class UserQueryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/account/api/me", GetMe)
            .RequireAuthorization()
            .Produces(200, typeof(MyUserDto))
            .Produces(404, typeof(AccountNotFoundError));

        app.MapGet("/account/api/public", GetPublicAccountsById)
            .AllowAnonymous()
            .Produces(200, typeof(IEnumerable<CartesianUserDto>));

        app.MapGet("/account/api/public/{accountId}", GetPublicAccountById)
            .AllowAnonymous()
            .Produces(200, typeof(CartesianUserDto))
            .Produces(404, typeof(AccountNotFoundError));
    }

    async Task<IResult> GetMe(UserManager<CartesianUser> userManager, ClaimsPrincipal principal)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.NotFound(new AccountNotFoundError(userId));

        return Results.Ok(user.ToMyUserDto());
    }

    async Task<IResult> GetPublicAccountById(CartesianDbContext dbContext, string accountId) =>
        await dbContext.Users.Where(u => u.Id == accountId).Select(u => u.ToDto()).FirstOrDefaultAsync() is { } account
            ? Results.Ok(account)
            : Results.NotFound(new AccountNotFoundError(accountId));

    async Task<IResult> GetPublicAccountsById(CartesianDbContext dbContext, [FromQuery] string[] accountIds) =>
        Results.Ok(await dbContext.Users.Where(u => accountIds.AsEnumerable().Contains(u.Id))
            .Select(u => u.ToDto())
            .ToArrayAsync());
}
