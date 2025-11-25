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

        // Filter by bounding box
        if (req.MinLat != null && req.MaxLat != null && req.MinLon != null && req.MaxLon != null)
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var envelope = new Envelope(req.MinLon.Value, req.MaxLon.Value, req.MinLat.Value, req.MaxLat.Value);
            var bbox = geometryFactory.ToGeometry(envelope);

            query = query.Where(e => e.Location.Intersects(bbox));
        }

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .ToArrayAsync();

        var features = new List<IFeature>();

        foreach (var evt in events)
        {
            var attributes = new AttributesTable
            {
                { "eventId", evt.Id },
                { "eventName", evt.Name },
                { "eventDescription", evt.Description },
                { "authorId", evt.Author.Id },
                { "authorName", evt.Author.UserName },
                { "authorAvatar", evt.Author.Avatar?.Id },
                { "communityId", evt.Community?.Id },
                { "communityName", evt.Community?.Name },
                { "communityAvatar", evt.Community?.Avatar?.Id },
                { "visibility", evt.Visibility.ToString() },
                { "timing", evt.Timing.ToString() },
                { "tags", evt.Tags.Select(t => t.ToString()).ToArray() },
                { "startTime", evt.Windows.Where(w => w.StartTime != null).MinBy(w => w.StartTime)?.StartTime },
                { "endTime", evt.Windows.Where(w => w.EndTime != null).MaxBy(w => w.EndTime)?.EndTime },
                { "createdAt", evt.CreatedAt }
            };

            var feature = new Feature(evt.Location, attributes);
            features.Add(feature);
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
