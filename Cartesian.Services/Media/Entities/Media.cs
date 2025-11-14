using Cartesian.Services.Account.Entities;

namespace Cartesian.Services.Media.Entities;

public class Media
{
    public Guid Id { get; set; }
    public required string BucketName { get; set; }
    public required string ObjectKey { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public CartesianUser? Author { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
