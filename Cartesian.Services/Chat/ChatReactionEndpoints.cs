using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Chat;

public class ChatReactionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/chat/api/message/{messageId}/react", PostAddReaction)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter>()
            .Produces(200, typeof(ChatReactionDto))
            .Produces(400, typeof(InvalidEmojiError))
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatMessageNotFoundError));

        app.MapDelete("/chat/api/message/{messageId}/react", DeleteRemoveReaction)
            .RequireAuthorization()
            .Produces(204)
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatMessageNotFoundError));

        app.MapGet("/chat/api/message/{messageId}/reactions", GetReactions)
            .RequireAuthorization()
            .Produces(200, typeof(GetReactionsResponse))
            .Produces(403, typeof(ChatAccessDeniedError))
            .Produces(404, typeof(ChatMessageNotFoundError));
    }

    private async Task<IResult> PostAddReaction(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        ChatSseService sseService,
        Guid messageId,
        AddReactionRequest req)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var normalizedEmoji = EmojiValidator.NormalizeEmoji(req.Emoji);

        var message = await db.ChatMessages
            .Include(m => m.Channel)
            .ThenInclude(c => c!.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(m => m.Channel)
            .ThenInclude(c => c!.Event)
            .ThenInclude(e => e!.Subscribers)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return Results.NotFound(new ChatMessageNotFoundError());

        var channel = message.Channel!;

        var hasAccess = channel.Type switch
        {
            ChatChannelType.DirectMessage => channel.Participant1Id == userId || channel.Participant2Id == userId,
            ChatChannelType.Community => channel.Community!.Memberships.Any(m => m.UserId == userId),
            ChatChannelType.Event => channel.Event!.Subscribers.Any(s => s.Id == userId) || channel.Event.AuthorId == userId,
            _ => false
        };

        if (!hasAccess)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var existingReaction = await db.ChatReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == normalizedEmoji);

        if (existingReaction != null)
        {
            var user = await userManager.FindByIdAsync(userId);
            return Results.Ok(new ChatReactionDto(
                existingReaction.Id,
                existingReaction.MessageId,
                existingReaction.UserId,
                user?.UserName ?? "Unknown",
                existingReaction.Emoji,
                existingReaction.CreatedAt
            ));
        }

        var reaction = new ChatReaction
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            UserId = userId,
            Emoji = normalizedEmoji,
            CreatedAt = DateTime.UtcNow
        };

        db.ChatReactions.Add(reaction);
        await db.SaveChangesAsync();

        var memberIds = await GetChannelMemberIds(db, channel);
        foreach (var memberId in memberIds)
            await sseService.SendReactionAddedAsync(memberId, reaction.Id, reaction.MessageId, channel.Id, reaction.UserId, reaction.Emoji, reaction.CreatedAt);

        var reactUser = await userManager.FindByIdAsync(userId);
        return Results.Ok(new ChatReactionDto(
            reaction.Id,
            reaction.MessageId,
            reaction.UserId,
            reactUser?.UserName ?? "Unknown",
            reaction.Emoji,
            reaction.CreatedAt
        ));
    }

    private async Task<IResult> DeleteRemoveReaction(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        ChatSseService sseService,
        Guid messageId,
        string emoji)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var normalizedEmoji = EmojiValidator.NormalizeEmoji(emoji);

        var message = await db.ChatMessages
            .Include(m => m.Channel)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return Results.NotFound(new ChatMessageNotFoundError());

        var reaction = await db.ChatReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == normalizedEmoji);

        if (reaction == null)
            return Results.NoContent();

        var channel = message.Channel!;
        db.ChatReactions.Remove(reaction);
        await db.SaveChangesAsync();

        var memberIds = await GetChannelMemberIds(db, channel);
        foreach (var memberId in memberIds)
            await sseService.SendReactionRemovedAsync(memberId, reaction.Id, reaction.MessageId, channel.Id, reaction.UserId, reaction.Emoji);

        return Results.NoContent();
    }

    private async Task<IResult> GetReactions(
        CartesianDbContext db,
        UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal,
        Guid messageId,
        bool? groupByEmoji = false)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var message = await db.ChatMessages
            .Include(m => m.Channel)
            .ThenInclude(c => c!.Community)
            .ThenInclude(c => c!.Memberships)
            .Include(m => m.Channel)
            .ThenInclude(c => c!.Event)
            .ThenInclude(e => e!.Subscribers)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return Results.NotFound(new ChatMessageNotFoundError());

        var channel = message.Channel!;

        var hasAccess = channel.Type switch
        {
            ChatChannelType.DirectMessage => channel.Participant1Id == userId || channel.Participant2Id == userId,
            ChatChannelType.Community => channel.Community!.Memberships.Any(m => m.UserId == userId),
            ChatChannelType.Event => channel.Event!.Subscribers.Any(s => s.Id == userId) || channel.Event.AuthorId == userId,
            _ => false
        };

        if (!hasAccess)
            return Results.Json(new ChatAccessDeniedError(), statusCode: 403);

        var reactions = await db.ChatReactions
            .Include(r => r.User)
            .Where(r => r.MessageId == messageId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        if (groupByEmoji == true)
        {
            var grouped = reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionSummaryDto(
                    g.Key,
                    g.Count(),
                    g.Select(r => r.UserId).ToList(),
                    g.Any(r => r.UserId == userId)
                ))
                .ToList();

            return Results.Ok(new GetReactionsSummaryResponse(grouped));
        }

        var dtos = reactions.Select(r => new ChatReactionDto(
            r.Id,
            r.MessageId,
            r.UserId,
            r.User?.UserName ?? "Unknown",
            r.Emoji,
            r.CreatedAt
        )).ToList();

        return Results.Ok(new GetReactionsResponse(dtos));
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

    public record ChatReactionDto(
        Guid Id,
        Guid MessageId,
        string UserId,
        string Username,
        string Emoji,
        DateTime CreatedAt
    );

    public record GetReactionsResponse(
        List<ChatReactionDto> Reactions
    );

    public record GetReactionsSummaryResponse(
        List<ReactionSummaryDto> Summary
    );
}
