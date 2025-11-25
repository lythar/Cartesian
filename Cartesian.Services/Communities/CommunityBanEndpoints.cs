using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class CommunityBanEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/community/api/{communityId}/ban/{targetUserId}", PostBanUser)
            .RequireAuthorization()
            .Produces(200, typeof(CommunityBanDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapDelete("/community/api/{communityId}/ban/{targetUserId}", DeleteUnbanUser)
            .RequireAuthorization()
            .Produces(200)
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404);

        app.MapGet("/community/api/{communityId}/bans", GetBannedUsers)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<CommunityBanDto>))
            .Produces(400, typeof(ValidationError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapGet("/community/api/{communityId}/ban/{targetUserId}/status", GetBanStatus)
            .RequireAuthorization()
            .Produces(200, typeof(BanStatusResponse));
    }

    async Task<IResult> PostBanUser(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid communityId,
        string targetUserId,
        [FromBody] BanUserRequest? req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = await db.Communities.FirstOrDefaultAsync(c => c.Id == communityId);
        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var requesterMembership = await db.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();
        if (requesterMembership == null)
            return Results.Json(new MissingPermissionError(Permissions.ManagePeople, communityId), statusCode: 403);
        if (requesterMembership.TryAssertPermission(Permissions.ManagePeople, out var error))
            return Results.Json(error, statusCode: 403);

        var targetUser = await db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (targetUser == null)
            return Results.BadRequest(new AccountNotFoundError(targetUserId));

        var currentUser = await db.Users
            .Include(u => u.Avatar)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var existingBan = await db.CommunityBans
            .FirstOrDefaultAsync(b => b.CommunityId == communityId && b.UserId == targetUserId);
        if (existingBan != null)
            return Results.Ok(new CommunityBanDto(
                existingBan.Id,
                communityId,
                targetUserId,
                targetUser.ToDto(),
                existingBan.BannedById,
                currentUser!.ToDto(),
                existingBan.Reason,
                existingBan.CreatedAt));

        // Remove membership if exists
        var targetMembership = await db.Memberships
            .WhereMember(targetUserId, communityId)
            .FirstOrDefaultAsync();
        if (targetMembership != null)
        {
            // Don't allow banning owners
            if (targetMembership.Permissions.HasFlag(Permissions.Owner))
                return Results.Json(new MissingPermissionError(Permissions.Owner, communityId), statusCode: 403);

            db.Memberships.Remove(targetMembership);
        }

        var ban = new CommunityBan
        {
            Id = Guid.NewGuid(),
            CommunityId = communityId,
            UserId = targetUserId,
            BannedById = userId,
            Reason = req?.Reason,
            CreatedAt = DateTime.UtcNow
        };

        db.CommunityBans.Add(ban);
        await db.SaveChangesAsync();

        return Results.Ok(new CommunityBanDto(
            ban.Id,
            communityId,
            targetUserId,
            targetUser.ToDto(),
            userId,
            currentUser!.ToDto(),
            ban.Reason,
            ban.CreatedAt));
    }

    async Task<IResult> DeleteUnbanUser(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid communityId,
        string targetUserId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = await db.Communities.FirstOrDefaultAsync(c => c.Id == communityId);
        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var requesterMembership = await db.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();
        if (requesterMembership == null)
            return Results.Json(new MissingPermissionError(Permissions.ManagePeople, communityId), statusCode: 403);
        if (requesterMembership.TryAssertPermission(Permissions.ManagePeople, out var error))
            return Results.Json(error, statusCode: 403);

        var ban = await db.CommunityBans
            .FirstOrDefaultAsync(b => b.CommunityId == communityId && b.UserId == targetUserId);
        if (ban == null)
            return Results.NotFound();

        db.CommunityBans.Remove(ban);
        await db.SaveChangesAsync();

        return Results.Ok();
    }

    async Task<IResult> GetBannedUsers(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid communityId,
        [AsParameters] GetBannedUsersRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = await db.Communities.FirstOrDefaultAsync(c => c.Id == communityId);
        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var requesterMembership = await db.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();
        if (requesterMembership == null)
            return Results.Json(new MissingPermissionError(Permissions.ManagePeople, communityId), statusCode: 403);
        if (requesterMembership.TryAssertPermission(Permissions.ManagePeople, out var error))
            return Results.Json(error, statusCode: 403);

        var bans = await db.CommunityBans
            .Include(b => b.User)
            .ThenInclude(u => u.Avatar)
            .Include(b => b.BannedBy)
            .ThenInclude(u => u.Avatar)
            .Where(b => b.CommunityId == communityId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(b => new CommunityBanDto(
                b.Id,
                b.CommunityId,
                b.UserId,
                b.User.ToDto(),
                b.BannedById,
                b.BannedBy.ToDto(),
                b.Reason,
                b.CreatedAt))
            .ToListAsync();

        return Results.Ok(bans);
    }

    async Task<IResult> GetBanStatus(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid communityId,
        string targetUserId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var isBanned = await db.CommunityBans
            .AnyAsync(b => b.CommunityId == communityId && b.UserId == targetUserId);

        return Results.Ok(new BanStatusResponse(isBanned));
    }

    public record BanUserRequest(string? Reason);
    public record GetBannedUsersRequest(
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
    public record BanStatusResponse(bool IsBanned);
}
