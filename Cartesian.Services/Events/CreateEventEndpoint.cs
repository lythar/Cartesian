using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Events;

public class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/event/api/create", PostCreateEvent)
            .RequireAuthorization()
            .Produces(200, typeof(EventDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(400, typeof(CommunityNotFoundError));

        app.MapPost("/event/api/{eventId}/window/create", PostCreateEventWindow)
            .RequireAuthorization()
            .Produces(200, typeof(EventWindowDto))
            .Produces(400, typeof(EventNotFoundError));
    }

    async Task<IResult> PostCreateEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, CreateEventBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        Community? community = null;

        if (body.CommunityId != null)
        {
            community = await dbContext.Communities.Where(c => c.Id == body.CommunityId).FirstOrDefaultAsync();
            if (community == null) return Results.BadRequest(new CommunityNotFoundError(body.CommunityId.ToString()!));
        }

        var newEvent = new Event()
        {
            Id = Guid.NewGuid(),
            Name = body.Name,
            Description = body.Description,
            Author = user,
            Community = community,
            Tags = body.Tags
        };

        await dbContext.AddAsync(newEvent);
        await dbContext.SaveChangesAsync();

        return Results.Ok(newEvent.ToDto());
    }

    async Task<IResult> PostCreateEventWindow(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, CreateEventBody body)
    {
        return Results.Ok();
    }

    record CreateEventBody(string Name, string Description, Guid? CommunityId, List<EventTag> Tags);
    record CreateEventWindowBody(string Name, string Description);
}
