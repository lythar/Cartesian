using Cartesian.Services.Content;

namespace Cartesian.Services.Account;

public record MyUserDto(string Id, string Name, string Email, bool EmailConfirmed, MediaDto? Avatar);
