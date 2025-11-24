using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Events;

public class FavoriteEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/event/api/{eventId:guid}/favorite", PostFavoriteEvent)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapDelete("/event/api/{eventId:guid}/favorite", DeleteFavoriteEvent)
            .RequireAuthorization()
            .Produces(200)
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapGet("/event/api/{eventId:guid}/favorite", GetFavoriteStatus)
            .RequireAuthorization()
            .Produces(200, typeof(bool))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapGet("/event/api/favorites", GetMyFavorites)
            .RequireAuthorization()
            .Produces(200, typeof(IEnumerable<EventDto>))
            .Produces(400, typeof(AccountNotFoundError));
    }

    async Task<IResult> GetMyFavorites(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal)
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
            .Where(e => e.FavoritedBy.Any(u => u.Id == userId))
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToListAsync();

        return Results.Ok(events);
    }

    async Task<IResult> PostFavoriteEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, Guid eventId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var existingEvent = await dbContext.Events
            .Include(e => e.FavoritedBy)
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null) return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        if (!existingEvent.FavoritedBy.Any(u => u.Id == userId))
        {
            existingEvent.FavoritedBy.Add(user);
            await dbContext.SaveChangesAsync();
        }

        return Results.Ok();
    }

    async Task<IResult> DeleteFavoriteEvent(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, Guid eventId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var existingEvent = await dbContext.Events
            .Include(e => e.FavoritedBy)
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (existingEvent == null) return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var user = existingEvent.FavoritedBy.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            existingEvent.FavoritedBy.Remove(user);
            await dbContext.SaveChangesAsync();
        }

        return Results.Ok();
    }

    async Task<IResult> GetFavoriteStatus(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, Guid eventId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var eventExists = await dbContext.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists) return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var isFavorited = await dbContext.Events
            .Where(e => e.Id == eventId)
            .AnyAsync(e => e.FavoritedBy.Any(u => u.Id == userId));

        return Results.Ok(isFavorited);
    }
}
