using Cartesian.Services.Account;

namespace Cartesian.Services.Account;

public class UserBlock
{
    public required Guid Id { get; init; }
    public required string BlockerId { get; init; }
    public CartesianUser Blocker { get; init; } = null!;
    public required string BlockedId { get; init; }
    public CartesianUser Blocked { get; init; } = null!;
    public required DateTime CreatedAt { get; init; }
}

public record UserBlockDto(Guid Id, string BlockedId, CartesianUserDto BlockedUser, DateTime CreatedAt);
