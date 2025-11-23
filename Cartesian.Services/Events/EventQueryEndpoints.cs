using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Events;

public class EventQueryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/event/api/list", GetEventList)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<EventDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/event/api/{eventId}", GetEvent)
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
            query = query.Where(e => e.Visibility == req.Visibility);
        }
        else
        {
            query = query.Where(e => e.Visibility != EventVisibility.Draft);
        }

        // Filter by tags
        if (req.Tags != null && req.Tags.Length > 0)
        {
            query = query.Where(e => e.Tags.Any(t => req.Tags.Contains(t)));
        }

        // Filter by community
        if (req.CommunityId != null)
        {
            query = query.Where(e => e.CommunityId == req.CommunityId);
        }

        // Filter by timing
        if (req.Timing != null)
        {
            query = query.Where(e => e.Timing == req.Timing);
        }

        // Filter by date range based on event windows
        if (req.StartDate != null)
        {
            query = query.Where(e => e.Windows.Any(w => w.StartTime == null || w.StartTime >= req.StartDate));
        }

        if (req.EndDate != null)
        {
            query = query.Where(e => e.Windows.Any(w => w.EndTime == null || w.EndTime <= req.EndDate));
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
}
