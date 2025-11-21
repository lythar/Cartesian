using Cartesian.Services.Database;
using Minio;
using Minio.DataModel.Args;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Cartesian.Services.Content;

public class MediaService(CartesianDbContext dbContext, IMinioClient minioClient)
{
    private const string DefaultBucket = "cartesian-media";

    public async Task<Media> UploadMedia(Stream stream, string fileName, string contentType, string? authorId, CancellationToken ct = default)
    {
        var objectKey = $"{Guid.NewGuid()}/{fileName}";
        
        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(DefaultBucket)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType), ct);

        var media = new Media
        {
            Id = Guid.NewGuid(),
            BucketName = DefaultBucket,
            ObjectKey = objectKey,
            FileName = fileName,
            ContentType = contentType
        };

        if (authorId != null)
        {
            var author = await dbContext.Users.FindAsync([authorId], ct);
            media.Author = author;
        }

        dbContext.Media.Add(media);
        await dbContext.SaveChangesAsync(ct);

        return media;
    }

    public async Task<Media> UploadAvatar(Stream stream, string fileName, string? authorId, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(stream, ct);
        
        // Calculate center crop dimensions
        var size = Math.Min(image.Width, image.Height);
        var x = (image.Width - size) / 2;
        var y = (image.Height - size) / 2;

        // Process: crop to center square, resize to 512x512, convert to JPEG
        image.Mutate(ctx => ctx
            .Crop(new Rectangle(x, y, size, size))
            .Resize(512, 512));

        // Save as JPEG to memory stream
        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, new JpegEncoder { Quality = 90 }, ct);
        outputStream.Position = 0;

        // Upload processed image with standardized filename
        return await UploadMedia(outputStream, "avatar.jpg", "image/jpeg", authorId, ct);
    }

    public Task<Media> UploadCommunityImage(Stream stream, string fileName, string contentType, string? authorId, CancellationToken ct = default)
        => ProcessAndUploadWebp(stream, $"community-{fileName}", authorId, ct);

    public Task<Media> UploadEventImage(Stream stream, string fileName, string contentType, string? authorId, CancellationToken ct = default)
        => ProcessAndUploadWebp(stream, $"event-{fileName}", authorId, ct);

    public Task<Media> UploadEventWindowImage(Stream stream, string fileName, string contentType, string? authorId, CancellationToken ct = default)
        => ProcessAndUploadWebp(stream, $"event-window-{fileName}", authorId, ct);

    public Task<Media> UploadGeneralImage(Stream stream, string fileName, string contentType, string? authorId, CancellationToken ct = default)
        => ProcessAndUploadWebp(stream, $"general-{fileName}", authorId, ct);

    private async Task<Media> ProcessAndUploadWebp(Stream stream, string fileName, string? authorId, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(stream, ct);
        
        using var outputStream = new MemoryStream();
        await image.SaveAsWebpAsync(outputStream, ct);
        outputStream.Position = 0;

        var webpFileName = Path.GetFileNameWithoutExtension(fileName) + ".webp";
        return await UploadMedia(outputStream, webpFileName, "image/webp", authorId, ct);
    }

    public async Task<(Stream Stream, string ContentType)?> GetMedia(Guid id, CancellationToken ct = default)
    {
        var media = await dbContext.Media
            .Where(m => m.Id == id && !m.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (media == null)
            return null;

        var stream = new MemoryStream();
        await minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(media.BucketName)
            .WithObject(media.ObjectKey)
            .WithCallbackStream(s => s.CopyTo(stream)), ct);

        stream.Position = 0;
        return (stream, media.ContentType);
    }

    public async Task<bool> DeleteMedia(Guid id, CancellationToken ct = default)
    {
        var media = await dbContext.Media.FindAsync([id], ct);
        if (media == null || media.IsDeleted)
            return false;

        media.IsDeleted = true;
        media.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task EnsureBucketExists(CancellationToken ct = default)
    {
        var exists = await minioClient.BucketExistsAsync(new BucketExistsArgs()
            .WithBucket(DefaultBucket), ct);

        if (!exists)
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs()
                .WithBucket(DefaultBucket), ct);
        }
    }
}
