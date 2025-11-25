using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Cartesian.Services.Account;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Events;

public class EventQueryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/event/api/list", GetEventList)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<EventDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/event/api/my", GetMyEvents)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<EventDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/event/api/{eventId:guid}", GetEvent)
            .Produces(200, typeof(EventDto))
            .Produces(404, typeof(EventNotFoundError));
    }

    async Task<IResult> GetEvent(CartesianDbContext dbContext, Guid eventId)
    {
        var @event = await dbContext.Events
            .Include(e => e.Windows)
            .Include(e => e.Participants)
            .ThenInclude(p => p.Avatar)
            .Include(e => e.Author)
            .ThenInclude(u => u.Avatar)
            .Include(e => e.Community)
            .ThenInclude(c => c!.Avatar)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (@event == null)
        {
            return Results.NotFound(new EventNotFoundError(eventId.ToString()));
        }

        return Results.Ok(@event.ToDto());
    }

    async Task<IResult> GetMyEvents(CartesianDbContext dbContext, UserManager<CartesianUser> userManager, ClaimsPrincipal principal, [AsParameters] GetMyEventsRequest req)
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
            .Where(e => e.AuthorId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(e => e.ToDto())
            .ToArrayAsync();

        return Results.Ok(events);
    }

    async Task<IResult> GetEventList(CartesianDbContext dbContext, [AsParameters] GetEventListRequest req)
    {
        var query = dbContext.Events
            .Include(e => e.Windows)
            .Include(e => e.Participants)
            .ThenInclude(p => p.Avatar)
            .Include(e => e.Author)
            .ThenInclude(u => u.Avatar)
            .Include(e => e.Community)
            .ThenInclude(c => c!.Avatar)
            .AsQueryable();

        // Filter by visibility
        if (req.Visibility != null)
        {
            var visibility = req.Visibility;
            query = query.Where(e => e.Visibility == visibility);
        }
        else
        {
            query = query.Where(e => e.Visibility != EventVisibility.Draft);
        }

        // Filter by tags
        if (req.Tags != null && req.Tags.Length > 0)
        {
            var tags = req.Tags.ToList();
            query = query.Where(e => e.Tags.Any(t => tags.Contains(t)));
        }

        // Filter by community
        if (req.CommunityId != null)
        {
            var communityId = req.CommunityId;
            query = query.Where(e => e.CommunityId == communityId);
        }

        // Filter by timing
        if (req.Timing != null)
        {
            var timing = req.Timing;
            query = query.Where(e => e.Timing == timing);
        }

        // Filter by date range based on event windows
        if (req.StartDate != null)
        {
            var startDate = req.StartDate;
            query = query.Where(e => e.Windows.Any(w => w.StartTime == null || w.StartTime >= startDate));
        }

        if (req.EndDate != null)
        {
            var endDate = req.EndDate;
            query = query.Where(e => e.Windows.Any(w => w.EndTime == null || w.EndTime <= endDate));
        }

        var events = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(e => e.ToDto())
            .ToArrayAsync();

        return Results.Ok(events);
    }

    public record GetEventListRequest(
        [FromQuery(Name = "visibility")] EventVisibility? Visibility,
        [FromQuery(Name = "tags")] EventTag[]? Tags,
        [FromQuery(Name = "communityId")] Guid? CommunityId,
        [FromQuery(Name = "timing")] EventTiming? Timing,
        [FromQuery(Name = "startDate")] DateTime? StartDate,
        [FromQuery(Name = "endDate")] DateTime? EndDate,
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);

    public record GetMyEventsRequest(
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
}
