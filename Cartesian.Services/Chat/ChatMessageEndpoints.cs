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
        app.MapPost("/chat/api/dm/send", PostSendDirectMessage)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(ChatMessageDto))
            .Produces(400, typeof(ValidationError))
            .Produces(403);

        app.MapPost("/chat/api/community/send", PostSendCommunityMessage)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(ChatMessageDto))
            .Produces(400, typeof(ValidationError))
            .Produces(403);

        app.MapPost("/chat/api/event/send", PostSendEventMessage)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(ChatMessageDto))
            .Produces(400, typeof(ValidationError))
            .Produces(403);

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
    }

    private async Task<IResult> PostSendDirectMessage(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, ChatSseService sseService, SendDirectMessageRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var senderSettings = await db.ChatUserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (senderSettings?.DirectMessagesEnabled == false)
            return Results.Json(new ChatDisabledError(), statusCode: 403);

        var recipientSettings = await db.ChatUserSettings.FirstOrDefaultAsync(s => s.UserId == req.RecipientId);
        if (recipientSettings?.DirectMessagesEnabled == false)
            return Results.Json(new ChatDisabledError(), statusCode: 403);

        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c =>
                c.Type == ChatChannelType.DirectMessage &&
                ((c.Participant1Id == userId && c.Participant2Id == req.RecipientId) ||
                 (c.Participant1Id == req.RecipientId && c.Participant2Id == userId)));

        if (channel == null)
        {
            channel = new ChatChannel
            {
                Id = Guid.NewGuid(),
                Type = ChatChannelType.DirectMessage,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = true,
                Participant1Id = userId,
                Participant2Id = req.RecipientId
            };
            db.ChatChannels.Add(channel);
        }

        if (!channel.IsEnabled)
            return Results.Json(new ChatDisabledError(), statusCode: 403);

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

        await sseService.SendMessageAsync(req.RecipientId, message);

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
            AttachmentIds = message.Attachments.Select(a => a.Id).ToList()
        });
    }

    private async Task<IResult> PostSendCommunityMessage(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, ChatSseService sseService, SendCommunityMessageRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var membership = await db.Memberships
            .FirstOrDefaultAsync(m => m.CommunityId == req.CommunityId && m.UserId == userId);
        if (membership == null)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c => c.Type == ChatChannelType.Community && c.CommunityId == req.CommunityId);

        if (channel == null)
        {
            channel = new ChatChannel
            {
                Id = Guid.NewGuid(),
                Type = ChatChannelType.Community,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = true,
                CommunityId = req.CommunityId
            };
            db.ChatChannels.Add(channel);
        }

        if (!channel.IsEnabled)
            return Results.Json(new ChatDisabledError(), statusCode: 403);

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

        var memberIds = await db.Memberships
            .Where(m => m.CommunityId == req.CommunityId && m.UserId != userId)
            .Select(m => m.UserId)
            .ToListAsync();

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
            AttachmentIds = message.Attachments.Select(a => a.Id).ToList()
        });
    }

    private async Task<IResult> PostSendEventMessage(CartesianDbContext db, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, ChatSseService sseService, SendEventMessageRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var eventEntity = await db.Events
            .Include(e => e.Subscribers)
            .FirstOrDefaultAsync(e => e.Id == req.EventId);

        if (eventEntity == null)
            return Results.NotFound();

        var isSubscribed = eventEntity.Subscribers.Any(s => s.Id == userId);
        if (!isSubscribed && eventEntity.AuthorId != userId)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var channel = await db.ChatChannels
            .FirstOrDefaultAsync(c => c.Type == ChatChannelType.Event && c.EventId == req.EventId);

        if (channel == null)
        {
            channel = new ChatChannel
            {
                Id = Guid.NewGuid(),
                Type = ChatChannelType.Event,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = true,
                EventId = req.EventId
            };
            db.ChatChannels.Add(channel);
        }

        if (!channel.IsEnabled)
            return Results.Json(new ChatDisabledError(), statusCode: 403);

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

        var subscriberIds = eventEntity.Subscribers
            .Where(s => s.Id != userId)
            .Select(s => s.Id)
            .ToList();

        foreach (var subscriberId in subscriberIds)
            await sseService.SendMessageAsync(subscriberId, message);

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
            AttachmentIds = message.Attachments.Select(a => a.Id).ToList()
        });
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
            AttachmentIds = m.Attachments.Select(a => a.Id).ToList()
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

    public record SendDirectMessageRequest(string RecipientId, string Content, List<string>? MentionedUserIds, List<Guid>? AttachmentIds);
    public record SendCommunityMessageRequest(Guid CommunityId, string Content, List<string>? MentionedUserIds, List<Guid>? AttachmentIds);
    public record SendEventMessageRequest(Guid EventId, string Content, List<string>? MentionedUserIds, List<Guid>? AttachmentIds);
    public record GetMessagesResponse(List<ChatMessageDto> Messages, bool HasMore);
    public record ChatChannelDto(Guid Id, ChatChannelType Type, bool IsEnabled, string? Participant1Id, string? Participant2Id, Guid? CommunityId, Guid? EventId);
}
