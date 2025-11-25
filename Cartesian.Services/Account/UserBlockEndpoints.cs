using System.Security.Claims;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Account;

public class UserBlockEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/account/api/block/{targetUserId}", PostBlockUser)
            .RequireAuthorization()
            .Produces(200, typeof(UserBlockDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(400);

        app.MapDelete("/account/api/block/{targetUserId}", DeleteUnblockUser)
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);

        app.MapGet("/account/api/blocks", GetBlockedUsers)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<UserBlockDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/account/api/block/{targetUserId}/status", GetBlockStatus)
            .RequireAuthorization()
            .Produces(200, typeof(BlockStatusResponse));
    }

    async Task<IResult> PostBlockUser(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        string targetUserId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        if (userId == targetUserId)
            return Results.BadRequest("Cannot block yourself.");

        var targetUser = await db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (targetUser == null)
            return Results.BadRequest(new AccountNotFoundError(targetUserId));

        var existingBlock = await db.UserBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);
        if (existingBlock != null)
            return Results.Ok(new UserBlockDto(existingBlock.Id, targetUserId, targetUser.ToDto(), existingBlock.CreatedAt));

        var block = new UserBlock
        {
            Id = Guid.NewGuid(),
            BlockerId = userId,
            BlockedId = targetUserId,
            CreatedAt = DateTime.UtcNow
        };

        db.UserBlocks.Add(block);
        await db.SaveChangesAsync();

        return Results.Ok(new UserBlockDto(block.Id, targetUserId, targetUser.ToDto(), block.CreatedAt));
    }

    async Task<IResult> DeleteUnblockUser(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        string targetUserId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var block = await db.UserBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);
        if (block == null)
            return Results.NotFound();

        db.UserBlocks.Remove(block);
        await db.SaveChangesAsync();

        return Results.Ok();
    }

    async Task<IResult> GetBlockedUsers(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        [AsParameters] GetBlockedUsersRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var blocks = await db.UserBlocks
            .Include(b => b.Blocked)
            .ThenInclude(u => u.Avatar)
            .Where(b => b.BlockerId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(b => new UserBlockDto(b.Id, b.BlockedId, b.Blocked.ToDto(), b.CreatedAt))
            .ToListAsync();

        return Results.Ok(blocks);
    }

    async Task<IResult> GetBlockStatus(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        string targetUserId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var youBlockedThem = await db.UserBlocks
            .AnyAsync(b => b.BlockerId == userId && b.BlockedId == targetUserId);
        var theyBlockedYou = await db.UserBlocks
            .AnyAsync(b => b.BlockerId == targetUserId && b.BlockedId == userId);

        return Results.Ok(new BlockStatusResponse(youBlockedThem, theyBlockedYou));
    }

    public record GetBlockedUsersRequest(
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);

    public record BlockStatusResponse(bool YouBlockedThem, bool TheyBlockedYou);
}
