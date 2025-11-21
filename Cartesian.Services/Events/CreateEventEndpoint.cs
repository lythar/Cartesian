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
            .Produces(400, typeof(CommunityNotFoundError))
            .Produces(403, typeof(MissingPermissionError));

        app.MapPut("/event/api/{eventId}/edit", PutEditEvent)
            .RequireAuthorization()
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(400, typeof(CommunityNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapPost("/event/api/{eventId}/window/create", PostCreateEventWindow)
            .RequireAuthorization()
            .Produces(200, typeof(EventWindowDto))
            .Produces(400, typeof(EventNotFoundError))
            .Produces(403, typeof(MissingPermissionError));
    }

    async Task<IResult> PostCreateEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, CreateEventBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        Membership? membership = null;

        if (body.CommunityId is { } communityId)
        {
            membership = await dbContext.Memberships.WhereMember(userId, communityId).FirstOrDefaultAsync();
            if (membership == null) return Results.BadRequest(new CommunityNotFoundError(communityId.ToString()));
            if (membership.TryAssertPermission(Permissions.ManageEvents, out var error))
                return Results.Json(error, statusCode: 403);
        }

        var newEvent = new Event()
        {
            Id = Guid.NewGuid(),
            Name = body.Name,
            Description = body.Description,
            Author = user,
            CommunityId = membership?.CommunityId,
            Tags = body.Tags
        };

        await dbContext.AddAsync(newEvent);
        await dbContext.SaveChangesAsync();

        return Results.Ok(newEvent.ToDto());
    }

    async Task<IResult> PutEditEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, PutEditEventBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var existingEvent = await dbContext.Events.Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null)
            return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        if (existingEvent.CommunityId is { } communityId)
        {
            var membership = await dbContext.Memberships.WhereMember(userId, communityId).FirstOrDefaultAsync();
            if (membership == null) return Results.BadRequest(new CommunityNotFoundError(communityId.ToString()));
            if (membership.TryAssertPermission(Permissions.ManageEvents, out var error))
                return Results.Json(error, statusCode: 403);
        }

        if (body.Name is {} name) existingEvent.Name = name;
        if (body.Description is {} description) existingEvent.Description = description;
        if (body.Tags is {} tags) existingEvent.Tags = tags;
        if (body.Timing is {} timing) existingEvent.Timing = timing;
        if (body.Visibility is {} visibility) existingEvent.Visibility = visibility;

        return Results.Ok();
    }

    async Task<IResult> PostCreateEventWindow(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, CreateEventWindowBody body)
    {
        return Results.Ok();
    }

    record CreateEventBody(string Name, string Description, Guid? CommunityId, List<EventTag> Tags);
    record PutEditEventBody(string? Name, string? Description, List<EventTag>? Tags, EventTiming? Timing, EventVisibility? Visibility);
    record CreateEventWindowBody(string Name, string Description);
}
