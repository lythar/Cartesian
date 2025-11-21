using Cartesian.Services.Account;

namespace Cartesian.Services.Content;

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
    
    public Guid? CommunityId { get; set; }
    public Guid? EventId { get; set; }
    public Guid? EventWindowId { get; set; }

    public MediaDto ToDto() => new(Id, BucketName, ObjectKey, FileName, ContentType, UploadedAt);
}
