using System.Security.Claims;
using Cartesian.Services.Account;
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
        app.MapGet("/community/api/public/{communityId}", GetCommunityById)
            .Produces(200, typeof(CommunityDto))
            .Produces(404, typeof(CommunityNotFoundError));

        // app.MapGet("/community/api/public/{communityId}/members", GetCommunityMembers)
        //     .Produces(200, typeof(IEnumerable<CartesianUserDto>))
        //     .Produces(404, typeof(CommunityNotFoundError));

        app.MapGet("/community/api/public/list", GetCommunityList).Produces(200, typeof(IEnumerable<CommunityDto>));
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

    record GetCommunityListRequest(
        [FromQuery(Name = "onlyJoined")] bool OnlyJoined = false,
        [FromQuery(Name = "showInviteOnly")] bool ShowInviteOnly = true,
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
}
