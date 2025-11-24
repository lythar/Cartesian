using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Chat;

public class ChatMessageEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/chat/api/channel/{channelId}/send", PostSendMessage)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(ChatMessageDto))
            .Produces(400, typeof(ValidationError))
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatChannelNotFoundError));

        app.MapGet("/chat/api/messages", GetMessages)
            .RequireAuthorization()
            .Produces(200, typeof(GetMessagesResponse))
            .Produces(403)
            .Produces(404);

        app.MapGet("/chat/api/dm/channel", GetDirectMessageChannel)
            .RequireAuthorization()
            .Produces(200, typeof(ChatChannelDto));

        app.MapGet("/chat/api/community/channel", GetCommunityChannel)
            .RequireAuthorization()
            .Produces(200, typeof(ChatChannelDto))
            .Produces(403);

        app.MapGet("/chat/api/event/channel", GetEventChannel)
            .RequireAuthorization()
            .Produces(200, typeof(ChatChannelDto))
            .Produces(403)
            .Produces(404);

        app.MapGet("/chat/api/subscribe", SubscribeToChat)
            .RequireAuthorization();

        app.MapGet("/chat/api/channels", GetChannels)
            .RequireAuthorization()
            .Produces(200, typeof(GetChannelsResponse));
    }

    private async Task<IResult> PostSendMessage(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        ChatSseService sseService,
        Guid channelId,
        SendMessageRequest req)
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

        if (!channel.IsEnabled)
            return Results.Json(new ChatDisabledError(), statusCode: 403);

        var hasAccess = channel.Type switch
        {
            ChatChannelType.DirectMessage => await ValidateDirectMessageAccess(db, userId, channel),
            ChatChannelType.Community => await db.Memberships.AnyAsync(m => m.CommunityId == channel.CommunityId && m.UserId == userId),
            ChatChannelType.Event => channel.Event!.Subscribers.Any(s => s.Id == userId) || channel.Event.AuthorId == userId,
            _ => false
        };

        if (!hasAccess)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var isMuted = await db.ChatMutes.AnyAsync(m =>
            m.ChannelId == channel.Id && m.UserId == userId &&
            (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow));
        if (isMuted)
            return Results.Json(new ChatUserMutedError(), statusCode: 403);

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChannelId = channel.Id,
            AuthorId = userId,
            Content = req.Content,
            CreatedAt = DateTime.UtcNow
        };

        if (req.MentionedUserIds?.Count > 0)
        {
            foreach (var mentionedUserId in req.MentionedUserIds.Distinct())
            {
                message.Mentions.Add(new ChatMention
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    UserId = mentionedUserId
                });
            }
        }

        if (req.AttachmentIds?.Count > 0)
        {
            var attachments = await db.Media
                .Where(m => req.AttachmentIds.Contains(m.Id))
                .ToListAsync();
            foreach (var attachment in attachments)
                message.Attachments.Add(attachment);
        }

        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        var memberIds = await GetChannelMemberIds(db, channel, userId);
        foreach (var memberId in memberIds)
            await sseService.SendMessageAsync(memberId, message);

        return Results.Ok(new ChatMessageDto
        {
            Id = message.Id,
            ChannelId = message.ChannelId,
            AuthorId = message.AuthorId,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            MentionedUserIds = message.Mentions.Select(m => m.UserId).ToList(),
            AttachmentIds = message.Attachments.Select(a => a.Id).ToList(),
            ReactionSummary = []
        });
    }

    private async Task<bool> ValidateDirectMessageAccess(CartesianDbContext db, string userId, ChatChannel channel)
    {
        if (channel.Participant1Id != userId && channel.Participant2Id != userId)
            return false;

        var senderSettings = await db.ChatUserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (senderSettings?.DirectMessagesEnabled == false)
            return false;

        var otherUserId = channel.Participant1Id == userId ? channel.Participant2Id : channel.Participant1Id;
        var recipientSettings = await db.ChatUserSettings.FirstOrDefaultAsync(s => s.UserId == otherUserId);
        if (recipientSettings?.DirectMessagesEnabled == false)
            return false;

        return true;
    }

    private async Task<List<string>> GetChannelMemberIds(CartesianDbContext db, ChatChannel channel, string excludeUserId)
    {
        var memberIds = channel.Type switch
        {
            ChatChannelType.DirectMessage => new List<string> { channel.Participant1Id!, channel.Participant2Id! },
            ChatChannelType.Community => await db.Memberships
                .Where(m => m.CommunityId == channel.CommunityId)
                .Select(m => m.UserId)
                .ToListAsync(),
            ChatChannelType.Event => await db.Users
                .Where(u => u.SubscribedEvents.Any(e => e.Id == channel.EventId))
                .Select(u => u.Id)
                .ToListAsync(),
            _ => new List<string>()
        };

        return memberIds.Where(id => id != excludeUserId).ToList();
    }

    private async Task<IResult> GetMessages(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid channelId, int? limit, Guid? before)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var channel = await db.ChatChannels
            .Include(c => c.Community)
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
            .Where(m => m.ChannelId == channelId && !m.IsDeleted)
            .Include(m => m.Mentions)
            .Include(m => m.Attachments)
            .Include(m => m.Reactions)
            .ThenInclude(r => r.User)
            .OrderByDescending(m => m.CreatedAt);

        if (before.HasValue)
        {
            var beforeMessage = await db.ChatMessages.FirstOrDefaultAsync(m => m.Id == before.Value);
            if (beforeMessage != null)
            {
                query = (IOrderedQueryable<ChatMessage>)query.Where(m => m.CreatedAt < beforeMessage.CreatedAt);
            }
        }

        var pageLimit = limit ?? 50;
        var messages = await query.Take(pageLimit + 1).ToListAsync();

        var hasMore = messages.Count > pageLimit;
        if (hasMore)
            messages = messages.Take(pageLimit).ToList();

        var dtos = messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChannelId = m.ChannelId,
            AuthorId = m.AuthorId,
            Content = m.Content,
            CreatedAt = m.CreatedAt,
            EditedAt = m.EditedAt,
            IsDeleted = m.IsDeleted,
            MentionedUserIds = m.Mentions.Select(x => x.UserId).ToList(),
            AttachmentIds = m.Attachments.Select(a => a.Id).ToList(),
            ReactionSummary = m.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionSummaryDto(
                    g.Key,
                    g.Count(),
                    g.Select(r => r.UserId).ToList(),
                    g.Any(r => r.UserId == userId)
                ))
                .ToList()
        }).ToList();

        return Results.Ok(new GetMessagesResponse(dtos, hasMore));
    }

    private async Task<IResult> GetDirectMessageChannel(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, string recipientId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c =>
                c.Type == ChatChannelType.DirectMessage &&
                ((c.Participant1Id == userId && c.Participant2Id == recipientId) ||
                 (c.Participant1Id == recipientId && c.Participant2Id == userId)));

        if (channel == null)
        {
            channel = new ChatChannel
            {
                Id = Guid.NewGuid(),
                Type = ChatChannelType.DirectMessage,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = true,
                Participant1Id = userId,
                Participant2Id = recipientId
            };
            db.ChatChannels.Add(channel);
            await db.SaveChangesAsync();
        }

        return Results.Ok(new ChatChannelDto(
            channel.Id,
            channel.Type,
            channel.IsEnabled,
            channel.Participant1Id,
            channel.Participant2Id,
            channel.CommunityId,
            channel.EventId
        ));
    }

    private async Task<IResult> GetCommunityChannel(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid communityId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var isMember = await db.Memberships.AnyAsync(m => m.CommunityId == communityId && m.UserId == userId);
        if (!isMember)
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
                IsEnabled = true,
                CommunityId = communityId
            };
            db.ChatChannels.Add(channel);
            await db.SaveChangesAsync();
        }

        return Results.Ok(new ChatChannelDto(
            channel.Id,
            channel.Type,
            channel.IsEnabled,
            channel.Participant1Id,
            channel.Participant2Id,
            channel.CommunityId,
            channel.EventId
        ));
    }

    private async Task<IResult> GetEventChannel(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, Guid eventId)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var eventEntity = await db.Events
            .Include(e => e.Subscribers)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
            return Results.NotFound();

        var hasAccess = eventEntity.Subscribers.Any(s => s.Id == userId) || eventEntity.AuthorId == userId;
        if (!hasAccess)
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
                IsEnabled = true,
                EventId = eventId
            };
            db.ChatChannels.Add(channel);
            await db.SaveChangesAsync();
        }

        return Results.Ok(new ChatChannelDto(
            channel.Id,
            channel.Type,
            channel.IsEnabled,
            channel.Participant1Id,
            channel.Participant2Id,
            channel.CommunityId,
            channel.EventId
        ));
    }

    private IResult SubscribeToChat(UserManager<CartesianUser> userManager,
        ChatSseService sseService, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null)
            return Results.Unauthorized();

        return TypedResults.ServerSentEvents(
            sseService.Subscribe(userId, cancellationToken),
            eventType: "chat"
        );
    }

    private async Task<IResult> GetChannels(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        // Get community channels where user is a member
        var communityChannels = await db.ChatChannels
            .Include(c => c.Community)
            .Where(c => c.Type == ChatChannelType.Community &&
                        c.Community != null &&
                        c.Community.Memberships.Any(m => m.UserId == userId))
            .Select(c => new ChannelListItemDto(
                c.Id,
                c.Type,
                c.IsEnabled,
                c.Community!.Name,
                c.Community.Id,
                null,
                null,
                c.CreatedAt
            ))
            .ToListAsync();

        // Get event channels where user is subscribed or is the author
        var eventChannels = await db.ChatChannels
            .Include(c => c.Event)
            .ThenInclude(e => e!.Subscribers)
            .Where(c => c.Type == ChatChannelType.Event &&
                        c.Event != null &&
                        (c.Event.Subscribers.Any(s => s.Id == userId) || c.Event.AuthorId == userId))
            .Select(c => new ChannelListItemDto(
                c.Id,
                c.Type,
                c.IsEnabled,
                c.Event!.Name,
                null,
                c.Event.Id,
                c.Event.CreatedAt,
                c.CreatedAt
            ))
            .ToListAsync();

        // Combine and sort by event creation date (for events) or channel creation date (for communities)
        var allChannels = communityChannels.Concat(eventChannels)
            .OrderByDescending(c => c.EntityCreatedAt)
            .ToList();

        return Results.Ok(new GetChannelsResponse(allChannels));
    }

    public record SendMessageRequest(string Content, List<string>? MentionedUserIds, List<Guid>? AttachmentIds);
    public record GetMessagesResponse(List<ChatMessageDto> Messages, bool HasMore);
    public record ChatChannelDto(Guid Id, ChatChannelType Type, bool IsEnabled, string? Participant1Id, string? Participant2Id, Guid? CommunityId, Guid? EventId);
    public record GetChannelsResponse(List<ChannelListItemDto> Channels);
    public record ChannelListItemDto(Guid ChannelId, ChatChannelType Type, bool IsEnabled, string Name, Guid? CommunityId, Guid? EventId, DateTime? EntityCreatedAt, DateTime ChannelCreatedAt);
}
