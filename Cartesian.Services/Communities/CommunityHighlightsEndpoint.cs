using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Chat;
using Cartesian.Services.Communities;
using Cartesian.Services.Content;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Cartesian.Services.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Communities;

public class CommunityHighlightsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/community/api/{communityId:guid}/highlights", GetCommunityHighlights)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(CommunityHighlightsDto))
            .Produces(400, typeof(ValidationError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError));
    }

    private async Task<IResult> GetCommunityHighlights(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid communityId,
        [AsParameters] CommunityHighlightsRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = await db.Communities
            .Include(c => c.Avatar)
            .FirstOrDefaultAsync(c => c.Id == communityId);

        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var membership = await db.Memberships
            .FirstOrDefaultAsync(m => m.CommunityId == communityId && m.UserId == userId);

        if (membership == null)
            return Results.Json(new MissingPermissionError(Permissions.Member, communityId), statusCode: 403);

        // Get latest events for this community
        var events = await db.Events
            .Include(e => e.Author)
            .ThenInclude(a => a.Avatar)
            .Include(e => e.Community)
            .ThenInclude(c => c!.Avatar)
            .Include(e => e.Windows)
            .Include(e => e.Participants)
            .ThenInclude(p => p.Avatar)
            .Where(e => e.CommunityId == communityId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(req.EventLimit)
            .ToListAsync();

        // Get the community chat channel
        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c => c.CommunityId == communityId && c.Type == ChatChannelType.Community);

        List<HighlightItemDto> highlights = [];

        // Add events to highlights
        highlights.AddRange(events.Select(e => new HighlightItemDto(
            "event",
            e.CreatedAt,
            Event: e.ToDto(),
            PinnedMessage: null
        )));

        if (channel != null)
        {
            // Get pinned messages from the community channel
            var pins = await db.ChatPinnedMessages
                .Where(p => p.ChannelId == channel.Id)
                .Include(p => p.Message)
                .ThenInclude(m => m!.Author)
                .ThenInclude(a => a!.Avatar)
                .Include(p => p.Message)
                .ThenInclude(m => m!.Attachments)
                .Include(p => p.Message)
                .ThenInclude(m => m!.Reactions)
                .Include(p => p.PinnedBy)
                .ThenInclude(pb => pb!.Avatar)
                .Where(p => !p.Message!.IsDeleted)
                .OrderByDescending(p => p.PinnedAt)
                .Take(req.PinnedMessageLimit)
                .ToListAsync();

            // Add pinned messages to highlights
            highlights.AddRange(pins.Select(p => new HighlightItemDto(
                "pinned_message",
                p.PinnedAt,
                Event: null,
                PinnedMessage: new PinnedMessageHighlightDto(
                    p.Id,
                    p.Message!.Id,
                    p.Message.Content,
                    p.Message.Author?.ToDto(),
                    p.Message.CreatedAt,
                    p.PinnedBy?.ToDto(),
                    p.PinnedAt,
                    p.Message.Attachments.Select(a => a.ToDto()).ToList(),
                    p.Message.Reactions
                        .GroupBy(r => r.Emoji)
                        .Select(g => new ReactionSummaryDto(
                            g.Key,
                            g.Count(),
                            g.Select(r => r.UserId).ToList(),
                            g.Any(r => r.UserId == userId)
                        ))
                        .ToList()
                )
            )));
        }

        // Sort all highlights by date (most recent first)
        var sortedHighlights = highlights
            .OrderByDescending(h => h.Date)
            .Take(req.Limit)
            .ToList();

        return Results.Ok(new CommunityHighlightsDto(
            community.ToDto(),
            sortedHighlights
        ));
    }
}

public record CommunityHighlightsRequest(
    [FromQuery(Name = "limit")] int Limit = 20,
    [FromQuery(Name = "eventLimit")] int EventLimit = 10,
    [FromQuery(Name = "pinnedMessageLimit")] int PinnedMessageLimit = 10);

public record CommunityHighlightsDto(
    CommunityDto Community,
    List<HighlightItemDto> Highlights);

public record HighlightItemDto(
    string Type,
    DateTime Date,
    EventDto? Event,
    PinnedMessageHighlightDto? PinnedMessage);

public record PinnedMessageHighlightDto(
    Guid PinId,
    Guid MessageId,
    string Content,
    CartesianUserDto? Author,
    DateTime CreatedAt,
    CartesianUserDto? PinnedBy,
    DateTime PinnedAt,
    List<MediaDto> Attachments,
    List<ReactionSummaryDto> ReactionSummary);
