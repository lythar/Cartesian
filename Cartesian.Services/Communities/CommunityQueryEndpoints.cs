using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Chat;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class CommunityQueryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/community/api/public/{communityId:guid}", GetCommunityById)
            .Produces(200, typeof(CommunityDto))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapGet("/community/api/public/list", GetCommunityList)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<CommunityDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/community/api/me/memberships", GetMyMemberships)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<MembershipDto>))
            .Produces(400, typeof(ValidationError));
    }

    async Task<IResult> GetCommunityById(CartesianDbContext dbContext, Guid communityId) =>
        await dbContext.Communities.Include(c => c.Avatar).Where(c => c.Id == communityId).FirstOrDefaultAsync() is
        {
        } community
            ? Results.Ok(community.ToDto())
            : Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

    async Task<IResult> GetCommunityList(CartesianDbContext dbContext, ClaimsPrincipal principal,
        UserManager<CartesianUser> userManager, [AsParameters] GetCommunityListRequest req)
    {
        var userId = userManager.GetUserId(principal);

        var communities = await dbContext.Communities.Include(c => c.Avatar)
            .OrderByDescending(c => c.CreatedAt)
            .Where(c => !c.InviteOnly || req.ShowInviteOnly)
            .Where(c =>
                !req.OnlyJoined ||
                (userId != null && dbContext.Memberships.Any(m => m.UserId == userId && m.CommunityId == c.Id))
            )
            .Take(req.Limit)
            .Skip(req.Skip)
            .ToArrayAsync();

        return Results.Ok(communities.Select(c => c.ToDto()));
    }

    async Task<IResult> GetMyMemberships(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, [AsParameters] GetMyMembershipsRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var memberships = await dbContext.Memberships
            .Include(m => m.Community)
            .ThenInclude(c => c.Avatar)
            .Include(m => m.User)
            .ThenInclude(u => u.Avatar)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .ToArrayAsync();

        var communityIds = memberships.Select(m => m.CommunityId).ToList();
        var channels = await dbContext.ChatChannels
            .Where(c => c.Type == ChatChannelType.Community && c.CommunityId != null && communityIds.Contains(c.CommunityId.Value))
            .ToDictionaryAsync(c => c.CommunityId!.Value);

        return Results.Ok(memberships.Select(m => m.ToDto(channels.GetValueOrDefault(m.CommunityId))));
    }

    public record GetMyMembershipsRequest(
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);

    public record GetCommunityListRequest(
        [FromQuery(Name = "onlyJoined")] bool OnlyJoined = false,
        [FromQuery(Name = "showInviteOnly")] bool ShowInviteOnly = true,
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
}
