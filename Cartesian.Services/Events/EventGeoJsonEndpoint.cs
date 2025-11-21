using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events;

public class EventGeoJsonEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/event/api/geojson", GetEventGeoJson)
            .Produces(200, typeof(FeatureCollection));
    }

    async Task<IResult> GetEventGeoJson(CartesianDbContext dbContext, [AsParameters] GetEventGeoJsonRequest req)
    {
        var query = dbContext.Events
            .Include(e => e.Windows)
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

        // Filter by bounding box
        if (req.MinLat != null && req.MaxLat != null && req.MinLon != null && req.MaxLon != null)
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var envelope = new Envelope(req.MinLon.Value, req.MaxLon.Value, req.MinLat.Value, req.MaxLat.Value);
            var bbox = geometryFactory.ToGeometry(envelope);

            query = query.Where(e => e.Windows.Any(w => w.Location.Intersects(bbox)));
        }

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Take(req.Limit)
            .Skip(req.Skip)
            .ToArrayAsync();

        var features = new List<IFeature>();

        foreach (var evt in events)
        {
            foreach (var window in evt.Windows)
            {
                var attributes = new AttributesTable
                {
                    { "eventId", evt.Id },
                    { "eventName", evt.Name },
                    { "eventDescription", evt.Description },
                    { "windowId", window.Id },
                    { "windowTitle", window.Title },
                    { "windowDescription", window.Description },
                    { "authorId", evt.Author.Id },
                    { "authorName", evt.Author.UserName },
                    { "authorAvatar", evt.Author.Avatar?.Id },
                    { "communityId", evt.Community?.Id },
                    { "communityName", evt.Community?.Name },
                    { "communityAvatar", evt.Community?.Avatar?.Id },
                    { "visibility", evt.Visibility.ToString() },
                    { "timing", evt.Timing.ToString() },
                    { "tags", evt.Tags.Select(t => t.ToString()).ToArray() },
                    { "startTime", window.StartTime },
                    { "endTime", window.EndTime },
                    { "createdAt", evt.CreatedAt }
                };

                var feature = new Feature(window.Location, attributes);
                features.Add(feature);
            }
        }

        var featureCollection = new FeatureCollection();
        foreach (var feature in features)
        {
            featureCollection.Add(feature);
        }
        return Results.Ok(featureCollection);
    }

    record GetEventGeoJsonRequest(
        [FromQuery(Name = "visibility")] EventVisibility? Visibility,
        [FromQuery(Name = "tags")] EventTag[]? Tags,
        [FromQuery(Name = "communityId")] Guid? CommunityId,
        [FromQuery(Name = "timing")] EventTiming? Timing,
        [FromQuery(Name = "startDate")] DateTime? StartDate,
        [FromQuery(Name = "endDate")] DateTime? EndDate,
        [FromQuery(Name = "minLat")] double? MinLat,
        [FromQuery(Name = "maxLat")] double? MaxLat,
        [FromQuery(Name = "minLon")] double? MinLon,
        [FromQuery(Name = "maxLon")] double? MaxLon,
        [FromQuery(Name = "limit")] int Limit = 500,
        [FromQuery(Name = "skip")] int Skip = 0);
}
