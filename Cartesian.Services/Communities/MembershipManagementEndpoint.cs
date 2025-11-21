using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class MembershipManagementEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/community/api/{communityId}/members", GetCommunityMembers)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<MembershipDto>))
            .Produces(400, typeof(ValidationError))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapPost("/community/api/{communityId}/members/join", PostJoinCommunity)
            .RequireAuthorization()
            .Produces(200, typeof(MembershipDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapPost("/community/api/{communityId}/members/invite", PostInviteMember)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(MembershipDto))
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));

        app.MapPut("/community/api/{communityId}/members/{targetUserId}/permissions", PutUpdateMemberPermissions)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(MembershipDto))
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));

        app.MapDelete("/community/api/{communityId}/members/{targetUserId}", DeleteMember)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError))
            .Produces(404, typeof(MembershipNotFoundError));
    }

    async Task<IResult> GetCommunityMembers(CartesianDbContext dbContext, Guid communityId,
        [AsParameters] GetCommunityMembersRequest req)
    {
        var community = await dbContext.Communities
            .Where(c => c.Id == communityId)
            .FirstOrDefaultAsync();

        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var query = dbContext.Memberships
            .Include(m => m.User)
            .ThenInclude(u => u.Avatar)
            .Where(m => m.CommunityId == communityId);

        // Fetch all memberships (they're already filtered by community)
        var allMemberships = await query.ToArrayAsync();

        // Apply sorting in-memory based on SortBy parameter
        IEnumerable<Membership> orderedMemberships = req.SortBy switch
        {
            MemberSortBy.Authority => allMemberships
                .OrderByDescending(m => CountPermissions(m.Permissions))
                .ThenBy(m => m.CreatedAt),
            MemberSortBy.JoinDate => allMemberships
                .OrderBy(m => m.CreatedAt),
            _ => allMemberships
                .OrderBy(m => m.CreatedAt)
        };

        var memberships = orderedMemberships
            .Skip(req.Skip)
            .Take(req.Limit)
            .ToArray();

        return Results.Ok(memberships.Select(m => m.ToDto()));
    }

    private static int CountPermissions(Permissions permissions)
    {
        // Count the number of set bits (permissions) in the flags enum
        var value = (uint)permissions;
        int count = 0;
        while (value != 0)
        {
            count += (int)(value & 1);
            value >>= 1;
        }
        return count;
    }

    async Task<IResult> PostJoinCommunity(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
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

        if (community.InviteOnly)
            return Results.Json(new MissingPermissionError(Permissions.Member, communityId), statusCode: 403);

        var existingMembership = await dbContext.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();

        if (existingMembership != null)
            return Results.Ok(existingMembership.ToDto());

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CommunityId = communityId,
            Permissions = community.MemberPermissions
        };

        await dbContext.Memberships.AddAsync(membership);
        await dbContext.SaveChangesAsync();

        membership.User = user;
        return Results.Ok(membership.ToDto());
    }

    async Task<IResult> PostInviteMember(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId, PostInviteMemberBody body)
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

        var inviterMembership = await dbContext.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();

        if (inviterMembership == null)
            return Results.NotFound(new MembershipNotFoundError(userId, communityId.ToString()));

        if (inviterMembership.TryAssertPermission(Permissions.ManagePeople, out var error))
            return Results.Json(error, statusCode: 403);

        var targetUser = await userManager.FindByIdAsync(body.UserId);
        if (targetUser == null)
            return Results.BadRequest(new AccountNotFoundError(body.UserId));

        var existingMembership = await dbContext.Memberships
            .Include(m => m.User)
            .ThenInclude(u => u.Avatar)
            .WhereMember(body.UserId, communityId)
            .FirstOrDefaultAsync();

        if (existingMembership != null)
            return Results.Ok(existingMembership.ToDto());

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            UserId = body.UserId,
            CommunityId = communityId,
            Permissions = community.MemberPermissions
        };

        await dbContext.Memberships.AddAsync(membership);
        await dbContext.SaveChangesAsync();

        membership.User = targetUser;
        return Results.Ok(membership.ToDto());
    }

    async Task<IResult> PutUpdateMemberPermissions(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId, string targetUserId, PutUpdateMemberPermissionsBody body)
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

        var requesterMembership = await dbContext.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();

        if (requesterMembership == null)
            return Results.NotFound(new MembershipNotFoundError(userId, communityId.ToString()));

        var targetMembership = await dbContext.Memberships
            .Include(m => m.User)
            .ThenInclude(u => u.Avatar)
            .WhereMember(targetUserId, communityId)
            .FirstOrDefaultAsync();

        if (targetMembership == null)
            return Results.NotFound(new MembershipNotFoundError(targetUserId, communityId.ToString()));

        // Check if requester has ManagePeople permission
        if (requesterMembership.TryAssertPermission(Permissions.ManagePeople, out var error))
            return Results.Json(error, statusCode: 403);

        // Prevent modifying Owner/Admin permissions without being Admin
        var protectedPermissions = Permissions.Owner | Permissions.Admin;
        var hasProtectedPermissions = (targetMembership.Permissions & protectedPermissions) != 0 ||
                                      (body.Permissions & protectedPermissions) != 0;

        if (hasProtectedPermissions && !requesterMembership.Permissions.HasFlag(Permissions.Admin))
            return Results.Json(new MissingPermissionError(Permissions.Admin, communityId), statusCode: 403);

        targetMembership.Permissions = body.Permissions;
        await dbContext.SaveChangesAsync();

        return Results.Ok(targetMembership.ToDto());
    }

    async Task<IResult> DeleteMember(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId, string targetUserId)
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

        var targetMembership = await dbContext.Memberships
            .WhereMember(targetUserId, communityId)
            .FirstOrDefaultAsync();

        if (targetMembership == null)
            return Results.NotFound(new MembershipNotFoundError(targetUserId, communityId.ToString()));

        // Check if owner is being removed
        if (targetMembership.Permissions.HasFlag(Permissions.Owner))
            return Results.Json(new MissingPermissionError(Permissions.Owner, communityId), statusCode: 403);

        // Allow self-removal (leaving) or removal by someone with ManagePeople permission
        if (userId != targetUserId)
        {
            var requesterMembership = await dbContext.Memberships
                .WhereMember(userId, communityId)
                .FirstOrDefaultAsync();

            if (requesterMembership == null)
                return Results.NotFound(new MembershipNotFoundError(userId, communityId.ToString()));

            if (requesterMembership.TryAssertPermission(Permissions.ManagePeople, out var error))
                return Results.Json(error, statusCode: 403);
        }

        dbContext.Memberships.Remove(targetMembership);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }

    public record PostInviteMemberBody(string UserId);
    public record PutUpdateMemberPermissionsBody(Permissions Permissions);
    public record GetCommunityMembersRequest(
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0,
        [FromQuery(Name = "sortBy")] MemberSortBy SortBy = MemberSortBy.JoinDate);
}
