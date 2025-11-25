using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Chat;

public class ChatUnreadEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/chat/api/unread", GetUnreadCount)
            .RequireAuthorization()
            .Produces(200, typeof(UnreadCountResponse))
            .Produces(403)
            .Produces(404);

        app.MapPost("/chat/api/unread/bulk", GetBulkUnreadCounts)
            .RequireAuthorization()
            .Produces(200, typeof(BulkUnreadCountResponse));
    }

    private async Task<IResult> GetUnreadCount(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        [FromQuery] Guid channelId,
        [FromQuery] Guid? afterMessageId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var channel = await db.ChatChannels
            .Include(c => c.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(c => c.Event)
            .ThenInclude(e => e!.Subscribers)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null)
            return Results.NotFound(new ChatChannelNotFoundError());

        var hasAccess = channel.Type switch
        {
            ChatChannelType.DirectMessage => channel.Participant1Id == userId || channel.Participant2Id == userId,
            ChatChannelType.Community => await db.Memberships.AnyAsync(m => m.CommunityId == channel.CommunityId && m.UserId == userId),
            ChatChannelType.Event => channel.Event!.Subscribers.Any(s => s.Id == userId) || channel.Event.AuthorId == userId,
            _ => false
        };

        if (!hasAccess)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var query = db.ChatMessages
            .Where(m => m.ChannelId == channelId && !m.IsDeleted && m.AuthorId != userId);

        if (afterMessageId.HasValue)
        {
            var afterMessage = await db.ChatMessages.FirstOrDefaultAsync(m => m.Id == afterMessageId.Value);
            if (afterMessage != null)
            {
                query = query.Where(m => m.CreatedAt > afterMessage.CreatedAt);
            }
        }

        var count = await query.CountAsync();

        return Results.Ok(new UnreadCountResponse(channelId, count));
    }

    private async Task<IResult> GetBulkUnreadCounts(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        [FromBody] BulkUnreadRequest request)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var results = new List<ChannelUnreadInfo>();

        if (request.Channels == null || request.Channels.Count == 0)
            return Results.Ok(new BulkUnreadCountResponse(results));

        var channelData = request.Channels
            .Select(c => new { ChannelId = c.ChannelId, AfterMessageId = c.AfterMessageId })
            .ToList();

        var channelIds = channelData.Select(c => c.ChannelId).ToList();

        var channels = await db.ChatChannels
            .Where(c => channelIds.Contains(c.Id))
            .Include(c => c.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(c => c.Event)
            .ThenInclude(e => e!.Subscribers)
            .ToListAsync();

        foreach (var channelInfo in channelData)
        {
            var channel = channels.FirstOrDefault(c => c.Id == channelInfo.ChannelId);
            if (channel == null) continue;

            var hasAccess = channel.Type switch
            {
                ChatChannelType.DirectMessage => channel.Participant1Id == userId || channel.Participant2Id == userId,
                ChatChannelType.Community => channel.Community?.Memberships.Any(m => m.UserId == userId) ?? false,
                ChatChannelType.Event => (channel.Event?.Subscribers.Any(s => s.Id == userId) ?? false) || channel.Event?.AuthorId == userId,
                _ => false
            };

            if (!hasAccess) continue;

            var query = db.ChatMessages
                .Where(m => m.ChannelId == channel.Id && !m.IsDeleted && m.AuthorId != userId);

            if (channelInfo.AfterMessageId.HasValue)
            {
                var afterMessage = await db.ChatMessages.FirstOrDefaultAsync(m => m.Id == channelInfo.AfterMessageId.Value);
                if (afterMessage != null)
                {
                    query = query.Where(m => m.CreatedAt > afterMessage.CreatedAt);
                }
            }

            var count = await query.CountAsync();
            results.Add(new ChannelUnreadInfo(channel.Id, count, channel.CommunityId, channel.EventId));
        }

        return Results.Ok(new BulkUnreadCountResponse(results));
    }

    public record UnreadCountResponse(Guid ChannelId, int Count);
    public record BulkUnreadCountResponse(List<ChannelUnreadInfo> Channels);
    public record ChannelUnreadInfo(Guid ChannelId, int UnreadCount, Guid? CommunityId, Guid? EventId);
    public record BulkUnreadRequest(List<ChannelUnreadRequestItem> Channels);
    public record ChannelUnreadRequestItem(Guid ChannelId, Guid? AfterMessageId);
}
