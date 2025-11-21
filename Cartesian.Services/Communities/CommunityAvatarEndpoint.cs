using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Content;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class CommunityAvatarEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/community/api/{communityId}/avatar", PutSetAvatar)
            .RequireAuthorization()
            .Produces(200, typeof(CommunityDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MediaNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));

        app.MapDelete("/community/api/{communityId}/avatar", DeleteRemoveAvatar)
            .RequireAuthorization()
            .Produces(200, typeof(CommunityDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));
    }

    async Task<IResult> PutSetAvatar(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId, SetAvatarBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var community = await dbContext.Communities
            .Include(c => c.Avatar)
            .Where(c => c.Id == communityId)
            .FirstOrDefaultAsync();

        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var membership = await dbContext.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();

        if (membership == null)
            return Results.NotFound(new MembershipNotFoundError(userId, communityId.ToString()));

        if (membership.TryAssertPermission(Permissions.ManageCommunity, out var error))
            return Results.Json(error, statusCode: 403);

        var media = await dbContext.Media.FindAsync(body.MediaId);
        if (media == null || media.IsDeleted)
            return Results.NotFound(new MediaNotFoundError(body.MediaId));

        if (media.FileName != "avatar.jpg")
            return Results.BadRequest(new InvalidMediaTypeError("Media must be uploaded via avatar endpoint"));

        community.AvatarId = body.MediaId;
        await dbContext.SaveChangesAsync();

        return Results.Ok(community.ToDto());
    }

    async Task<IResult> DeleteRemoveAvatar(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var community = await dbContext.Communities
            .Where(c => c.Id == communityId)
            .FirstOrDefaultAsync();

        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var membership = await dbContext.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();

        if (membership == null)
            return Results.NotFound(new MembershipNotFoundError(userId, communityId.ToString()));

        if (membership.TryAssertPermission(Permissions.ManageCommunity, out var error))
            return Results.Json(error, statusCode: 403);

        community.AvatarId = null;
        community.Avatar = null;
        await dbContext.SaveChangesAsync();

        return Results.Ok(community.ToDto());
    }

    record SetAvatarBody(Guid MediaId);
}
