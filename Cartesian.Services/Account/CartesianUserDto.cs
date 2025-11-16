using Cartesian.Services.Content;

namespace Cartesian.Services.Account;

public record CartesianUserDto(string Id, string Name, MediaDto? Avatar);
