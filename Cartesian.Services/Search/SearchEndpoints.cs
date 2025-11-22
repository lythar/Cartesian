using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Cartesian.Services.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Search;

public class SearchEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/search/api/events", SearchEvents)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<EventDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/search/api/communities", SearchCommunities)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<CommunityDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/search/api/users", SearchUsers)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(IEnumerable<CartesianUserDto>))
            .Produces(400, typeof(ValidationError));

        app.MapGet("/search/api/all", SearchAll)
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(MergedSearchResultsDto))
            .Produces(400, typeof(ValidationError));
    }

    async Task<IResult> SearchEvents(CartesianDbContext dbContext, [AsParameters] SearchQueryRequest req)
    {
        var searchQuery = PrepareSearchQuery(req.Query);

        // Use FromSqlRaw to properly utilize the full-text search index that includes tags
        var events = await dbContext.Events
            .FromSqlRaw(@"
                SELECT * FROM ""Events"" e
                WHERE e.""Visibility"" <> 0
                AND to_tsvector('simple', e.""Name"" || ' ' || e.""Description"" || ' ' || COALESCE(int_array_to_text(e.""Tags""), ''))
                    @@ to_tsquery('simple', {0})
                ORDER BY ts_rank(
                    to_tsvector('simple', e.""Name"" || ' ' || e.""Description"" || ' ' || COALESCE(int_array_to_text(e.""Tags""), '')),
                    to_tsquery('simple', {0})
                ) DESC
                LIMIT {1} OFFSET {2}
            ", searchQuery, req.Limit, req.Skip)
            .Include(e => e.Windows)
            .Include(e => e.Author)
            .ThenInclude(u => u.Avatar)
            .Include(e => e.Community)
            .ThenInclude(c => c!.Avatar)
            .Select(e => e.ToDto())
            .ToArrayAsync();

        return Results.Ok(events);
    }

    async Task<IResult> SearchCommunities(CartesianDbContext dbContext, [AsParameters] SearchQueryRequest req)
    {
        var searchQuery = PrepareSearchQuery(req.Query);

        var communities = await dbContext.Communities
            .Include(c => c.Avatar)
            .Where(c => !c.InviteOnly)
            .Where(c => EF.Functions.ToTsVector("simple", c.Name + " " + c.Description)
                .Matches(EF.Functions.ToTsQuery("simple", searchQuery)))
            .OrderByDescending(c => 
                EF.Functions.ToTsVector("simple", c.Name + " " + c.Description)
                    .Rank(EF.Functions.ToTsQuery("simple", searchQuery)))
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(c => c.ToDto())
            .ToArrayAsync();

        return Results.Ok(communities);
    }

    async Task<IResult> SearchUsers(CartesianDbContext dbContext, [AsParameters] SearchQueryRequest req)
    {
        var searchQuery = PrepareSearchQuery(req.Query);

        var users = await dbContext.Users
            .Include(u => u.Avatar)
            .Where(u => EF.Functions.ToTsVector("simple", u.UserName!)
                .Matches(EF.Functions.ToTsQuery("simple", searchQuery)))
            .OrderByDescending(u => 
                EF.Functions.ToTsVector("simple", u.UserName!)
                    .Rank(EF.Functions.ToTsQuery("simple", searchQuery)))
            .Skip(req.Skip)
            .Take(req.Limit)
            .Select(u => u.ToDto())
            .ToArrayAsync();

        return Results.Ok(users);
    }

    async Task<IResult> SearchAll(IDbContextFactory<CartesianDbContext> dbContextFactory, [AsParameters] SearchQueryRequest req)
    {
        var searchQuery = PrepareSearchQuery(req.Query);
        var limit = Math.Min(req.Limit / 3, 10); // Distribute limit across all types

        var eventsTask = Task.Run(async () =>
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Events
                .FromSqlRaw(@"
                    SELECT * FROM ""Events"" e
                    WHERE e.""Visibility"" <> 0
                    AND to_tsvector('simple', e.""Name"" || ' ' || e.""Description"" || ' ' || COALESCE(int_array_to_text(e.""Tags""), ''))
                        @@ to_tsquery('simple', {0})
                    ORDER BY ts_rank(
                        to_tsvector('simple', e.""Name"" || ' ' || e.""Description"" || ' ' || COALESCE(int_array_to_text(e.""Tags""), '')),
                        to_tsquery('simple', {0})
                    ) DESC
                    LIMIT {1}
                ", searchQuery, limit)
                .Include(e => e.Windows)
                .Include(e => e.Author)
                .ThenInclude(u => u.Avatar)
                .Include(e => e.Community)
                .ThenInclude(c => c!.Avatar)
                .Select(e => e.ToDto())
                .ToArrayAsync();
        });

        var communitiesTask = Task.Run(async () =>
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Communities
                .Include(c => c.Avatar)
                .Where(c => !c.InviteOnly)
                .Where(c => EF.Functions.ToTsVector("simple", c.Name + " " + c.Description)
                    .Matches(EF.Functions.ToTsQuery("simple", searchQuery)))
                .OrderByDescending(c => 
                    EF.Functions.ToTsVector("simple", c.Name + " " + c.Description)
                        .Rank(EF.Functions.ToTsQuery("simple", searchQuery)))
                .Take(limit)
                .Select(c => c.ToDto())
                .ToArrayAsync();
        });

        var usersTask = Task.Run(async () =>
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await dbContext.Users
                .Include(u => u.Avatar)
                .Where(u => EF.Functions.ToTsVector("simple", u.UserName!)
                    .Matches(EF.Functions.ToTsQuery("simple", searchQuery)))
                .OrderByDescending(u => 
                    EF.Functions.ToTsVector("simple", u.UserName!)
                        .Rank(EF.Functions.ToTsQuery("simple", searchQuery)))
                .Take(limit)
                .Select(u => u.ToDto())
                .ToArrayAsync();
        });

        await Task.WhenAll(eventsTask, communitiesTask, usersTask);

        return Results.Ok(new MergedSearchResultsDto(
            eventsTask.Result,
            communitiesTask.Result,
            usersTask.Result));
    }

    private static string PrepareSearchQuery(string query)
    {
        // Convert search query to tsquery format
        // Split by spaces and join with & for AND operation
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" & ", terms.Select(t => t.Trim() + ":*"));
    }

    public record SearchQueryRequest(
        [FromQuery(Name = "query")] string Query,
        [FromQuery(Name = "limit")] int Limit = 20,
        [FromQuery(Name = "skip")] int Skip = 0);
}
