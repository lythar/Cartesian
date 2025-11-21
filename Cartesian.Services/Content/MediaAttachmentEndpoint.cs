using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Cartesian.Services.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Content;

public class MediaAttachmentEndpoint : IEndpoint
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/community/api/{communityId}/images", PostCommunityImage)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces(200, typeof(MediaDto))
            .Produces(400, typeof(InvalidMediaTypeError))
            .Produces(403, typeof(MissingPermissionError))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapGet("/community/api/{communityId}/images", GetCommunityImages)
            .AllowAnonymous()
            .Produces(200, typeof(IEnumerable<MediaDto>))
            .Produces(404, typeof(CommunityNotFoundError));

        app.MapPost("/event/api/{eventId}/images", PostEventImage)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces(200, typeof(MediaDto))
            .Produces(400, typeof(InvalidMediaTypeError))
            .Produces(404, typeof(EventNotFoundError));

        app.MapGet("/event/api/{eventId}/images", GetEventImages)
            .AllowAnonymous()
            .Produces(200, typeof(IEnumerable<MediaDto>))
            .Produces(404, typeof(EventNotFoundError));

        app.MapPost("/event/api/window/{windowId}/images", PostEventWindowImage)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces(200, typeof(MediaDto))
            .Produces(400, typeof(InvalidMediaTypeError))
            .Produces(404, typeof(EventWindowNotFoundError));

        app.MapGet("/event/api/window/{windowId}/images", GetEventWindowImages)
            .AllowAnonymous()
            .Produces(200, typeof(IEnumerable<MediaDto>))
            .Produces(404, typeof(EventWindowNotFoundError));
    }

    async Task<IResult> PostCommunityImage(IFormFile file, Guid communityId, CartesianDbContext dbContext,
        MediaService mediaService, UserManager<CartesianUser> userManager, ClaimsPrincipal principal)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            return Results.BadRequest(new InvalidMediaTypeError("File size must be between 1 byte and 5MB"));

        if (!AllowedContentTypes.Contains(file.ContentType))
            return Results.BadRequest(new InvalidMediaTypeError(file.ContentType));

        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var community = await dbContext.Communities
            .Where(c => c.Id == communityId)
            .FirstOrDefaultAsync();

        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var membership = await dbContext.Memberships
            .WhereMember(userId, communityId)
            .FirstOrDefaultAsync();

        if (membership == null)
            return Results.NotFound(new MembershipNotFoundError(userId, communityId.ToString()));

        if (membership.TryAssertPermission(Permissions.Member, out var error))
            return Results.Json(error, statusCode: 403);

        await using var stream = file.OpenReadStream();
        var media = await mediaService.UploadCommunityImage(stream, file.FileName, file.ContentType, userId);
        
        media.CommunityId = communityId;
        await dbContext.SaveChangesAsync();

        return Results.Ok(media.ToDto());
    }

    async Task<IResult> GetCommunityImages(Guid communityId, CartesianDbContext dbContext,
        [AsParameters] GetImagesRequest req)
    {
        var community = await dbContext.Communities
            .Where(c => c.Id == communityId)
            .FirstOrDefaultAsync();

        if (community == null)
            return Results.NotFound(new CommunityNotFoundError(communityId.ToString()));

        var images = await dbContext.Media
            .Where(m => m.CommunityId == communityId && !m.IsDeleted)
            .OrderByDescending(m => m.UploadedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .ToListAsync();

        return Results.Ok(images.Select(m => m.ToDto()));
    }

    async Task<IResult> PostEventImage(IFormFile file, Guid eventId, CartesianDbContext dbContext,
        MediaService mediaService, UserManager<CartesianUser> userManager, ClaimsPrincipal principal)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            return Results.BadRequest(new InvalidMediaTypeError("File size must be between 1 byte and 5MB"));

        if (!AllowedContentTypes.Contains(file.ContentType))
            return Results.BadRequest(new InvalidMediaTypeError(file.ContentType));

        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var eventEntity = await dbContext.Events
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
            return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        await using var stream = file.OpenReadStream();
        var media = await mediaService.UploadEventImage(stream, file.FileName, file.ContentType, userId);
        
        media.EventId = eventId;
        await dbContext.SaveChangesAsync();

        return Results.Ok(media.ToDto());
    }

    async Task<IResult> GetEventImages(Guid eventId, CartesianDbContext dbContext,
        [AsParameters] GetImagesRequest req)
    {
        var eventEntity = await dbContext.Events
            .Where(e => e.Id == eventId)
            .FirstOrDefaultAsync();

        if (eventEntity == null)
            return Results.NotFound(new EventNotFoundError(eventId.ToString()));

        var images = await dbContext.Media
            .Where(m => m.EventId == eventId && !m.IsDeleted)
            .OrderByDescending(m => m.UploadedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .ToListAsync();

        return Results.Ok(images.Select(m => m.ToDto()));
    }

    async Task<IResult> PostEventWindowImage(IFormFile file, Guid windowId, CartesianDbContext dbContext,
        MediaService mediaService, UserManager<CartesianUser> userManager, ClaimsPrincipal principal)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            return Results.BadRequest(new InvalidMediaTypeError("File size must be between 1 byte and 5MB"));

        if (!AllowedContentTypes.Contains(file.ContentType))
            return Results.BadRequest(new InvalidMediaTypeError(file.ContentType));

        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var window = await dbContext.EventWindows
            .Where(w => w.Id == windowId)
            .FirstOrDefaultAsync();

        if (window == null)
            return Results.NotFound(new EventWindowNotFoundError(windowId.ToString()));

        await using var stream = file.OpenReadStream();
        var media = await mediaService.UploadEventWindowImage(stream, file.FileName, file.ContentType, userId);
        
        media.EventWindowId = windowId;
        await dbContext.SaveChangesAsync();

        return Results.Ok(media.ToDto());
    }

    async Task<IResult> GetEventWindowImages(Guid windowId, CartesianDbContext dbContext,
        [AsParameters] GetImagesRequest req)
    {
        var window = await dbContext.EventWindows
            .Where(w => w.Id == windowId)
            .FirstOrDefaultAsync();

        if (window == null)
            return Results.NotFound(new EventWindowNotFoundError(windowId.ToString()));

        var images = await dbContext.Media
            .Where(m => m.EventWindowId == windowId && !m.IsDeleted)
            .OrderByDescending(m => m.UploadedAt)
            .Skip(req.Skip)
            .Take(req.Limit)
            .ToListAsync();

        return Results.Ok(images.Select(m => m.ToDto()));
    }

    record GetImagesRequest(
        [FromQuery(Name = "limit")] int Limit = 50,
        [FromQuery(Name = "skip")] int Skip = 0);
}
