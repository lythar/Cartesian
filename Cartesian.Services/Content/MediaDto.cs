using Cartesian.Services.Account;

namespace Cartesian.Services.Content;

public record MediaDto(
    Guid Id,
    string BucketName,
    string ObjectKey,
    string FileName,
    string ContentType,
    CartesianUserDto? Author,
    DateTime UploadedAt);
