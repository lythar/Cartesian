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
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<CartesianUserDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/account/api/public/{accountId}", GetPublicAccountById)
            .AllowAnonymous()
            .Produces(200, typeof(CartesianUserDto))
            .Produces(404, typeof(AccountNotFoundError));
    }

    async Task<IResult> GetMe(UserManager<CartesianUser> userManager, CartesianDbContext dbContext, ClaimsPrincipal principal)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await dbContext.Users
            .Include(u => u.Avatar)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        
        if (user == null) return Results.NotFound(new AccountNotFoundError(userId));

        return Results.Ok(user.ToMyUserDto());
    }

    async Task<IResult> GetPublicAccountById(CartesianDbContext dbContext, string accountId) =>
        await dbContext.Users
            .Include(u => u.Avatar)
            .Where(u => u.Id == accountId)
            .FirstOrDefaultAsync() is { } account
            ? Results.Ok(account.ToDto())
            : Results.NotFound(new AccountNotFoundError(accountId));

    async Task<IResult> GetPublicAccountsById(CartesianDbContext dbContext, [AsParameters] GetPublicAccountsByIdRequest req) =>
        Results.Ok(await dbContext.Users
            .Include(u => u.Avatar)
            .Where(u => req.AccountIds.AsEnumerable().Contains(u.Id))
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(u => u.ToDto())
            .ToArrayAsync());

    public record GetPublicAccountsByIdRequest(
        [FromQuery(Name = "accountIds")] string[] AccountIds,
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
}
