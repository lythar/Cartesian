using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class CommunityListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/community/api/list", GetCommunityList).Produces(200, typeof(IEnumerable<CommunityDto>));
    }

    async Task<IResult> GetCommunityList(CartesianDbContext dbContext, ClaimsPrincipal principal,
        UserManager<CartesianUser> userManager, [AsParameters] GetCommunityListRequest req)
    {
        var userId = userManager.GetUserId(principal);

        var communities = await dbContext.Communities.Include(c => c.Avatar)
            .OrderByDescending(c => c.CreatedAt)
            .Where(c => !c.InviteOnly || req.ShowPrivate)
            .Where(c => userId == null || !req.OnlyJoined ||
                        dbContext.Memberships.Any(m => m.UserId == userId && m.CommunityId == c.Id))
            .Select(c => c.ToDto())
            .Take(req.Limit)
            .Skip(req.Skip)
            .ToArrayAsync();

        return Results.Ok(communities);
    }

    record GetCommunityListRequest(bool OnlyJoined = false, bool ShowPrivate = true, int Limit = 50, int Skip = 0);
}
