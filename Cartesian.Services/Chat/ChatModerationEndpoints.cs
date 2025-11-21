using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Chat;

public class ChatModerationEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/chat/api/message/{messageId}", DeleteMessage)
            .RequireAuthorization()
            .Produces(204)
            .Produces(403)
            .Produces(404);

        app.MapPost("/chat/api/mute", PostMuteUser)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(204)
            .Produces(400, typeof(ValidationError))
            .Produces(403)
            .Produces(404);

        app.MapDelete("/chat/api/mute", DeleteUnmuteUser)
            .RequireAuthorization()
            .Produces(204)
            .Produces(403)
            .Produces(404);

        app.MapPut("/chat/api/settings/dm", PutToggleDirectMessages)
            .RequireAuthorization()
            .Produces(204);

        app.MapPut("/chat/api/community/{communityId}/toggle", PutToggleCommunityChannel)
            .RequireAuthorization()
            .Produces(204)
            .Produces(403)
            .Produces(404);

        app.MapPut("/chat/api/event/{eventId}/toggle", PutToggleEventChannel)
            .RequireAuthorization()
            .Produces(204)
            .Produces(403)
            .Produces(404);
    }

    private async Task<IResult> DeleteMessage(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, ChatSseService sseService, Guid messageId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var message = await db.ChatMessages
            .Include(m => m.Channel)
            .ThenInclude(c => c!.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(m => m.Channel)
            .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return Results.NotFound(new ChatMessageNotFoundError());

        var channel = message.Channel!;
        var isAuthor = message.AuthorId == userId;
        var canDelete = isAuthor;

        if (!canDelete && channel.Type == ChatChannelType.Community)
        {
            var membership = channel.Community!.Memberships.FirstOrDefault(m => m.UserId == userId);
            canDelete = membership?.Permissions.HasFlag(Permissions.ManageChat) ?? false;
        }
        else if (!canDelete && channel.Type == ChatChannelType.Event)
        {
            canDelete = channel.Event!.AuthorId == userId;
        }

        if (!canDelete)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        message.IsDeleted = true;
        await db.SaveChangesAsync();

        var userIds = channel.Type switch
        {
            ChatChannelType.DirectMessage => new[] { channel.Participant1Id!, channel.Participant2Id! },
            ChatChannelType.Community => await db.Memberships
                .Where(m => m.CommunityId == channel.CommunityId)
                .Select(m => m.UserId)
                .ToArrayAsync(),
            ChatChannelType.Event => await db.Users
                .Where(u => u.SubscribedEvents.Any(e => e.Id == channel.EventId))
                .Select(u => u.Id)
                .ToArrayAsync(),
            _ => []
        };

        foreach (var id in userIds)
            await sseService.SendMessageDeletedAsync(id, message.Id, message.ChannelId);

        return Results.NoContent();
    }

    private async Task<IResult> PostMuteUser(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, MuteUserRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var channel = await db.ChatChannels
            .Include(c => c.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == req.ChannelId);

        if (channel == null)
            return Results.NotFound(new ChatChannelNotFoundError());

        var canMute = channel.Type switch
        {
            ChatChannelType.Community => channel.Community!.Memberships.Any(m => m.UserId == userId && m.Permissions.HasFlag(Permissions.ManageChat)),
            ChatChannelType.Event => channel.Event!.AuthorId == userId,
            _ => false
        };

        if (!canMute)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var existingMute = await db.ChatMutes
            .FirstOrDefaultAsync(m => m.ChannelId == req.ChannelId && m.UserId == req.UserId &&
                                      (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow));

        if (existingMute != null)
            return Results.BadRequest(new CartesianError("already_muted", "User is already muted"));

        var mute = new ChatMute
        {
            Id = Guid.NewGuid(),
            ChannelId = req.ChannelId,
            UserId = req.UserId,
            MutedById = userId,
            MutedAt = DateTime.UtcNow,
            ExpiresAt = req.ExpiresAt,
            Reason = req.Reason
        };

        db.ChatMutes.Add(mute);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private async Task<IResult> DeleteUnmuteUser(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid channelId, string targetUserId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var channel = await db.ChatChannels
            .Include(c => c.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null)
            return Results.NotFound(new ChatChannelNotFoundError());

        var canUnmute = channel.Type switch
        {
            ChatChannelType.Community => channel.Community!.Memberships.Any(m => m.UserId == userId && m.Permissions.HasFlag(Permissions.ManageChat)),
            ChatChannelType.Event => channel.Event!.AuthorId == userId,
            _ => false
        };

        if (!canUnmute)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var mutes = await db.ChatMutes
            .Where(m => m.ChannelId == channelId && m.UserId == targetUserId &&
                        (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();

        db.ChatMutes.RemoveRange(mutes);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private async Task<IResult> PutToggleDirectMessages(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, ToggleDirectMessagesRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var settings = await db.ChatUserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new ChatUserSettings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DirectMessagesEnabled = req.Enabled
            };
            db.ChatUserSettings.Add(settings);
        }
        else
        {
            settings.DirectMessagesEnabled = req.Enabled;
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private async Task<IResult> PutToggleCommunityChannel(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId, ToggleChannelRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = await db.Communities
            .Include(c => c.Memberships)
            .FirstOrDefaultAsync(c => c.Id == communityId);

        if (community == null)
            return Results.NotFound();

        var membership = community.Memberships.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || !membership.Permissions.HasFlag(Permissions.ManageChat))
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c => c.Type == ChatChannelType.Community && c.CommunityId == communityId);

        if (channel == null)
        {
            channel = new ChatChannel
            {
                Id = Guid.NewGuid(),
                Type = ChatChannelType.Community,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = req.Enabled,
                CommunityId = communityId
            };
            db.ChatChannels.Add(channel);
        }
        else
        {
            channel.IsEnabled = req.Enabled;
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private async Task<IResult> PutToggleEventChannel(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId, ToggleChannelRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var eventEntity = await db.Events
            .Include(e => e.Community)
            .ThenInclude(c => c!.Memberships)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
            return Results.NotFound();

        var canToggle = eventEntity.AuthorId == userId;
        if (!canToggle && eventEntity.CommunityId.HasValue)
        {
            var membership = eventEntity.Community!.Memberships.FirstOrDefault(m => m.UserId == userId);
            canToggle = membership?.Permissions.HasFlag(Permissions.ManageChat) ?? false;
        }

        if (!canToggle)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c => c.Type == ChatChannelType.Event && c.EventId == eventId);

        if (channel == null)
        {
            channel = new ChatChannel
            {
                Id = Guid.NewGuid(),
                Type = ChatChannelType.Event,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = req.Enabled,
                EventId = eventId
            };
            db.ChatChannels.Add(channel);
        }
        else
        {
            channel.IsEnabled = req.Enabled;
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    public record MuteUserRequest(Guid ChannelId, string UserId, DateTime? ExpiresAt, string? Reason);
    public record ToggleDirectMessagesRequest(bool Enabled);
    public record ToggleChannelRequest(bool Enabled);
}
