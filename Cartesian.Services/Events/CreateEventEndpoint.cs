using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events;

public class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/event/api/create", PostCreateEvent)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(EventDto))
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(400, typeof(CommunityNotFoundError))
            .Produces(403, typeof(MissingPermissionError));

        app.MapPut("/event/api/{eventId}/edit", PutEditEvent)
            .RequireAuthorization()
            .Produces(200, typeof(EventDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(400, typeof(CommunityNotFoundError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapPost("/event/api/{eventId}/window/create", PostCreateEventWindow)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(EventWindowDto))
            .Produces(400, typeof(ValidationError))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError))
            .Produces(403, typeof(MissingPermissionError));

        app.MapPut("/event/api/{eventId}/window/{windowId}/edit", PutEditEventWindow)
            .RequireAuthorization()
            .Produces(200, typeof(EventWindowDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError))
            .Produces(404, typeof(EventWindowNotFoundError))
            .Produces(403, typeof(MissingPermissionError));

        app.MapDelete("/event/api/{eventId}/window/{windowId}", DeleteEventWindow)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError))
            .Produces(404, typeof(EventWindowNotFoundError))
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
            Tags = body.Tags,
            Visibility = EventVisibility.Public
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

        await dbContext.SaveChangesAsync();

        return Results.Ok(existingEvent.ToDto());
    }

    async Task<IResult> PostCreateEventWindow(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, CreateEventWindowBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var existingEvent = await dbContext.Events
            .Include(e => e.Windows)
            .Where(e => e.Id == eventId)
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

        var newWindow = new EventWindow
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Title = body.Title,
            Description = body.Description,
            Location = body.Location,
            StartTime = body.StartTime,
            EndTime = body.EndTime
        };

        await dbContext.EventWindows.AddAsync(newWindow);
        await dbContext.SaveChangesAsync();

        return Results.Ok(newWindow.ToDto());
    }

    async Task<IResult> PutEditEventWindow(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, Guid windowId, PutEditEventWindowBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var existingEvent = await dbContext.Events
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null)
            return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var existingWindow = await dbContext.EventWindows
            .Where(w => w.Id == windowId && w.EventId == eventId)
            .FirstOrDefaultAsync();

        if (existingWindow == null)
            return Results.NotFound(new EventWindowNotFoundError(windowId.ToString()));

        if (existingEvent.CommunityId is { } communityId)
        {
            var membership = await dbContext.Memberships.WhereMember(userId, communityId).FirstOrDefaultAsync();
            if (membership == null) return Results.BadRequest(new CommunityNotFoundError(communityId.ToString()));
            if (membership.TryAssertPermission(Permissions.ManageEvents, out var error))
                return Results.Json(error, statusCode: 403);
        }

        if (body.Title is {} title) existingWindow.Title = title;
        if (body.Description is {} description) existingWindow.Description = description;
        if (body.Location is {} location) existingWindow.Location = location;
        if (body.StartTime is {} startTime) existingWindow.StartTime = startTime;
        if (body.EndTime is {} endTime) existingWindow.EndTime = endTime;

        await dbContext.SaveChangesAsync();

        return Results.Ok(existingWindow.ToDto());
    }

    async Task<IResult> DeleteEventWindow(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, Guid windowId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var existingEvent = await dbContext.Events
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null)
            return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var existingWindow = await dbContext.EventWindows
            .Where(w => w.Id == windowId && w.EventId == eventId)
            .FirstOrDefaultAsync();

        if (existingWindow == null)
            return Results.NotFound(new EventWindowNotFoundError(windowId.ToString()));

        if (existingEvent.CommunityId is { } communityId)
        {
            var membership = await dbContext.Memberships.WhereMember(userId, communityId).FirstOrDefaultAsync();
            if (membership == null) return Results.BadRequest(new CommunityNotFoundError(communityId.ToString()));
            if (membership.TryAssertPermission(Permissions.ManageEvents, out var error))
                return Results.Json(error, statusCode: 403);
        }

        dbContext.EventWindows.Remove(existingWindow);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }

    public record CreateEventBody(string Name, string Description, Guid? CommunityId, List<EventTag> Tags);
    record PutEditEventBody(string? Name, string? Description, List<EventTag>? Tags, EventTiming? Timing, EventVisibility? Visibility);
    public record CreateEventWindowBody(string Title, string Description, Point Location, DateTime? StartTime, DateTime? EndTime);
    record PutEditEventWindowBody(string? Title, string? Description, Point? Location, DateTime? StartTime, DateTime? EndTime);
}
