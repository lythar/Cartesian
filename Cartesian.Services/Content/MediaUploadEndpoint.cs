using System.Security.Claims;
using Cartesian.Services.Account;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Content;

public class MediaUploadEndpoint : IEndpoint
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/media/api/upload", PostUpload)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces(200, typeof(MediaDto))
            .Produces(400, typeof(InvalidMediaTypeError))
            .Produces(401);

        app.MapGet("/media/api/{mediaId}", GetMedia)
            .AllowAnonymous()
            .Produces(200)
            .Produces(404, typeof(MediaNotFoundError));
    }

    async Task<IResult> PostUpload(IFormFile file, MediaService mediaService, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            return Results.BadRequest(new InvalidMediaTypeError("File size must be between 1 byte and 5MB"));

        if (!AllowedContentTypes.Contains(file.ContentType))
            return Results.BadRequest(new InvalidMediaTypeError(file.ContentType));

        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        await using var stream = file.OpenReadStream();
        var media = await mediaService.UploadMedia(stream, file.FileName, file.ContentType, userId);

        return Results.Ok(media.ToDto());
    }

    async Task<IResult> GetMedia(Guid mediaId, MediaService mediaService)
    {
        var result = await mediaService.GetMedia(mediaId);
        if (result == null)
            return Results.NotFound(new MediaNotFoundError(mediaId));

        var (stream, contentType) = result.Value;
        return Results.File(stream, contentType);
    }
}
