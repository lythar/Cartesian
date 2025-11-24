using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Events;

public class ParticipationEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/event/api/{eventId:guid}/participate", PostParticipateEvent)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapDelete("/event/api/{eventId:guid}/participate", DeleteParticipateEvent)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapGet("/event/api/{eventId:guid}/participants", GetEventParticipants)
            .Produces(200, typeof(IEnumerable<CartesianUserDto>))
            .Produces(404, typeof(EventNotFoundError));

        app.MapGet("/event/api/participating", GetMyParticipatingEvents)
            .RequireAuthorization()
            .Produces(200, typeof(IEnumerable<EventDto>))
            .Produces(400, typeof(AccountNotFoundError));
    }

    async Task<IResult> GetMyParticipatingEvents(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var events = await dbContext.Events
            .Include(e => e.Windows)
            .Include(e => e.Participants)
            .ThenInclude(p => p.Avatar)
            .Include(e => e.Author)
            .ThenInclude(u => u.Avatar)
            .Include(e => e.Community)
            .ThenInclude(c => c!.Avatar)
            .Where(e => e.Participants.Any(u => u.Id == userId))
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToListAsync();

        return Results.Ok(events);
    }

    async Task<IResult> GetEventParticipants(CartesianDbContext dbContext, Guid eventId)
    {
        var eventExists = await dbContext.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists) return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var participants = await dbContext.Events
            .Where(e => e.Id == eventId)
            .SelectMany(e => e.Participants)
            .Include(p => p.Avatar)
            .Select(p => p.ToDto())
            .ToListAsync();

        return Results.Ok(participants);
    }

    async Task<IResult> PostParticipateEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, Guid eventId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var existingEvent = await dbContext.Events
            .Include(e => e.Participants)
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null) return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        if (!existingEvent.Participants.Any(u => u.Id == userId))
        {
            existingEvent.Participants.Add(user);
            await dbContext.SaveChangesAsync();
        }

        return Results.Ok();
    }

    async Task<IResult> DeleteParticipateEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, Guid eventId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var existingEvent = await dbContext.Events
            .Include(e => e.Participants)
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null) return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var user = existingEvent.Participants.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            existingEvent.Participants.Remove(user);
            await dbContext.SaveChangesAsync();
        }

        return Results.Ok();
    }
}
