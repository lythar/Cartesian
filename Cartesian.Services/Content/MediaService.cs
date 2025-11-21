using Cartesian.Services.Database;
using Minio;
using Minio.DataModel.Args;
using Microsoft.EntityFrameworkCore;

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
