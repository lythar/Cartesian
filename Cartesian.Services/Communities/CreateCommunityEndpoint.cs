using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Communities;

public class CreateCommunityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/community/api/create", PostCreateCommunity)
            .RequireAuthorization()
            .Produces(200, typeof(CommunityDto));
    }

    async Task<IResult> PostCreateCommunity(CartesianDbContext dbContext, PostCreateCommunityBody body)
    {
        var community = new Community()
        {
            Id = Guid.NewGuid(),
            Name = body.Name,
            Description = body.Description,
            InviteOnly = body.InviteOnly,
        };

        await dbContext.AddAsync(community);
        await dbContext.SaveChangesAsync();

        return Results.Ok(community.ToDto());
    }

    record PostCreateCommunityBody(string Name, string Description, bool InviteOnly);
}
