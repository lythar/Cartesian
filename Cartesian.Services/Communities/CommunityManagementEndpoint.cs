using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class CommunityManagementEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/community/api/{communityId}/edit", PutEditCommunity)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(CommunityDto))
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));

        app.MapDelete("/community/api/{communityId}", DeleteCommunity)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));
    }

    async Task<IResult> PutEditCommunity(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId, PutEditCommunityBody body)
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

        if (body.Name is {} name) community.Name = name;
        if (body.Description is {} description) community.Description = description;
        if (body.InviteOnly is {} inviteOnly) community.InviteOnly = inviteOnly;
        if (body.MemberPermissions is {} memberPermissions) community.MemberPermissions = memberPermissions;

        await dbContext.SaveChangesAsync();

        return Results.Ok(community.ToDto());
    }

    async Task<IResult> DeleteCommunity(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
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

        if (!membership.Permissions.HasFlag(Permissions.Owner))
            return Results.Json(new MissingPermissionError(Permissions.Owner, communityId), statusCode: 403);

        dbContext.Communities.Remove(community);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }

    public record PutEditCommunityBody(string? Name, string? Description, bool? InviteOnly, Permissions? MemberPermissions);
}
