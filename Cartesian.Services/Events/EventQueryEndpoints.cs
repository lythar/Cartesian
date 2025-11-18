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
            .Produces(200, typeof(EventDto));
    }

    async Task<IResult> GetEventList(CartesianDbContext dbContext, [AsParameters] GetEventListRequest req) =>
        Results.Ok(await dbContext.Events.Include(e => e.Windows)
            .Include(e => e.Author)
            .ThenInclude(u => u.Avatar)
            .Include(e => e.Community)
            .ThenInclude(c => c.Avatar)
            .OrderByDescending(c => c.CreatedAt)
            .Where(e => req.Visibility != null ? e.Visibility == req.Visibility : e.Visibility != EventVisibility.Draft)
            .Select(e => e.ToDto())
            .Take(req.Limit)
            .Skip(req.Skip)
            .ToArrayAsync());

    record GetEventListRequest(
        [FromQuery(Name = "visibility")] EventVisibility? Visibility,
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
}
