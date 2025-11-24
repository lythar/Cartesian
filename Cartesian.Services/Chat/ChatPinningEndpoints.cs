using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Chat;

public class ChatPinningEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/chat/api/channel/{channelId}/pin/{messageId}", PostPinMessage)
            .RequireAuthorization()
            .Produces(200, typeof(PinnedChatMessageDto))
            .Produces(400, typeof(ChatMessageAlreadyPinnedError))
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatMessageNotFoundError));

        app.MapDelete("/chat/api/channel/{channelId}/pin/{messageId}", DeleteUnpinMessage)
            .RequireAuthorization()
            .Produces(204)
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatPinNotFoundError));

        app.MapGet("/chat/api/channel/{channelId}/pins", GetPinnedMessages)
            .RequireAuthorization()
            .Produces(200, typeof(GetPinnedMessagesResponse))
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatChannelNotFoundError));
    }

    private async Task<IResult> PostPinMessage(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        ChatSseService sseService,
        Guid channelId,
        Guid messageId)
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

        var message = await db.ChatMessages
            .Include(m => m.Author)
            .ThenInclude(a => a!.Avatar)
            .Include(m => m.Attachments)
            .Include(m => m.Reactions)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ChannelId == channelId);

        if (message == null)
            return Results.NotFound(new ChatMessageNotFoundError());

        if (message.IsDeleted)
            return Results.BadRequest(new CartesianError("cannot_pin_deleted", "Cannot pin a deleted message"));

        var existingPin = await db.ChatPinnedMessages
            .FirstOrDefaultAsync(p => p.ChannelId == channelId && p.MessageId == messageId);

        if (existingPin != null)
            return Results.BadRequest(new ChatMessageAlreadyPinnedError());

        var pin = new ChatPinnedMessage
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            MessageId = messageId,
            PinnedById = userId,
            PinnedAt = DateTime.UtcNow
        };

        db.ChatPinnedMessages.Add(pin);
        await db.SaveChangesAsync();

        var memberIds = await GetChannelMemberIds(db, channel);
        foreach (var memberId in memberIds)
            await sseService.SendMessagePinnedAsync(memberId, pin.Id, pin.MessageId, pin.ChannelId, pin.PinnedById, pin.PinnedAt);

        var user = await userManager.FindByIdAsync(userId);
        return Results.Ok(new PinnedChatMessageDto(
            pin.Id,
            new ChatMessageDto
            {
                Id = message.Id,
                ChannelId = message.ChannelId,
                AuthorId = message.AuthorId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                EditedAt = message.EditedAt,
                IsDeleted = message.IsDeleted,
                AttachmentIds = message.Attachments.Select(a => a.Id).ToList(),
                ReactionSummary = message.Reactions
                    .GroupBy(r => r.Emoji)
                    .Select(g => new ReactionSummaryDto(
                        g.Key,
                        g.Count(),
                        g.Select(r => r.UserId).ToList(),
                        g.Any(r => r.UserId == userId)
                    ))
                    .ToList()
            },
            pin.PinnedById,
            user?.UserName ?? "Unknown",
            pin.PinnedAt
        ));
    }

    private async Task<IResult> DeleteUnpinMessage(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        ChatSseService sseService,
        Guid channelId,
        Guid messageId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var pin = await db.ChatPinnedMessages
            .Include(p => p.Channel)
            .ThenInclude(c => c!.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(p => p.Channel)
            .ThenInclude(c => c!.Event)
            .Include(p => p.Message)
            .FirstOrDefaultAsync(p => p.ChannelId == channelId && p.MessageId == messageId);

        if (pin == null)
            return Results.NotFound(new ChatPinNotFoundError());

        var channel = pin.Channel!;
        var canUnpin = false;

        if (pin.PinnedById == userId)
        {
            canUnpin = true;
        }
        else if (channel.Type == ChatChannelType.DirectMessage)
        {
            canUnpin = channel.Participant1Id == userId || channel.Participant2Id == userId;
        }
        else if (channel.Type == ChatChannelType.Community)
        {
            var membership = channel.Community!.Memberships.FirstOrDefault(m => m.UserId == userId);
            canUnpin = membership?.Permissions.HasFlag(Permissions.ManageChat) ?? false;
        }
        else if (channel.Type == ChatChannelType.Event)
        {
            canUnpin = channel.Event!.AuthorId == userId;
        }

        if (!canUnpin)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        db.ChatPinnedMessages.Remove(pin);
        await db.SaveChangesAsync();

        var memberIds = await GetChannelMemberIds(db, channel);
        foreach (var memberId in memberIds)
            await sseService.SendMessageUnpinnedAsync(memberId, pin.Id, pin.MessageId, pin.ChannelId);

        return Results.NoContent();
    }

    private async Task<IResult> GetPinnedMessages(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid channelId,
        int limit = 50,
        DateTime? before = null)
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
            ChatChannelType.Community => channel.Community!.Memberships.Any(m => m.UserId == userId),
            ChatChannelType.Event => channel.Event!.Subscribers.Any(s => s.Id == userId) || channel.Event.AuthorId == userId,
            _ => false
        };

        if (!hasAccess)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var query = db.ChatPinnedMessages
            .Where(p => p.ChannelId == channelId)
            .Include(p => p.Message)
            .ThenInclude(m => m!.Author)
            .ThenInclude(a => a!.Avatar)
            .Include(p => p.Message)
            .ThenInclude(m => m!.Attachments)
            .Include(p => p.Message)
            .ThenInclude(m => m!.Reactions)
            .Include(p => p.PinnedBy)
            .Where(p => !p.Message!.IsDeleted)
            .OrderByDescending(p => p.PinnedAt);

        if (before.HasValue)
            query = (IOrderedQueryable<ChatPinnedMessage>)query.Where(p => p.PinnedAt < before.Value);

        var pins = await query.Take(limit + 1).ToListAsync();
        var hasMore = pins.Count > limit;
        if (hasMore)
            pins = pins.Take(limit).ToList();

        var dtos = pins.Select(p => new PinnedChatMessageDto(
            p.Id,
            new ChatMessageDto
            {
                Id = p.Message!.Id,
                ChannelId = p.Message.ChannelId,
                AuthorId = p.Message.AuthorId,
                Content = p.Message.Content,
                CreatedAt = p.Message.CreatedAt,
                EditedAt = p.Message.EditedAt,
                IsDeleted = p.Message.IsDeleted,
                AttachmentIds = p.Message.Attachments.Select(a => a.Id).ToList(),
                ReactionSummary = p.Message.Reactions
                    .GroupBy(r => r.Emoji)
                    .Select(g => new ReactionSummaryDto(
                        g.Key,
                        g.Count(),
                        g.Select(r => r.UserId).ToList(),
                        g.Any(r => r.UserId == userId)
                    ))
                    .ToList()
            },
            p.PinnedById,
            p.PinnedBy?.UserName ?? "Unknown",
            p.PinnedAt
        )).ToList();

        return Results.Ok(new GetPinnedMessagesResponse(dtos, hasMore));
    }

    private async Task<List<string>> GetChannelMemberIds(CartesianDbContext db, ChatChannel channel)
    {
        return channel.Type switch
        {
            ChatChannelType.DirectMessage => [channel.Participant1Id!, channel.Participant2Id!],
            ChatChannelType.Community => await db.Memberships
                .Where(m => m.CommunityId == channel.CommunityId)
                .Select(m => m.UserId)
                .ToListAsync(),
            ChatChannelType.Event => await db.Users
                .Where(u => u.SubscribedEvents.Any(e => e.Id == channel.EventId))
                .Select(u => u.Id)
                .ToListAsync(),
            _ => []
        };
    }

    public record PinnedChatMessageDto(
        Guid PinId,
        ChatMessageDto Message,
        string PinnedById,
        string PinnedByUsername,
        DateTime PinnedAt
    );

    public record GetPinnedMessagesResponse(
        List<PinnedChatMessageDto> Pins,
        bool HasMore
    );
}
