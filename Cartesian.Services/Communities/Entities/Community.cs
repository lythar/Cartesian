using Cartesian.Services.Communities.Data;

namespace Cartesian.Services.Communities.Entities;

public class Community
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public CommunityDto ToDto() => new();
}
