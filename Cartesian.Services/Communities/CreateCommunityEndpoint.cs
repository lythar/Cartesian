using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Communities;

public class CreateCommunityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/community/api/create", PostCreateCommunity)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(CommunityDto))
            .Produces(400, typeof(ValidationError));
    }

    async Task<IResult> PostCreateCommunity(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, PostCreateCommunityBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = new Community()
        {
            Id = Guid.NewGuid(),
            Name = body.Name,
            Description = body.Description,
            InviteOnly = body.InviteOnly
        };

        var membership = new Membership()
        {
            UserId = userId,
            CommunityId = community.Id,
            Permissions = Permissions.Owner
        };

        await dbContext.AddAsync(community);
        await dbContext.AddAsync(membership);
        await dbContext.SaveChangesAsync();

        return Results.Ok(community.ToDto());
    }

    public record PostCreateCommunityBody(string Name, string Description, bool InviteOnly);
}
