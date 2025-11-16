using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Content;

namespace Cartesian.Services.Events;

public class Event
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string AuthorId { get; set; } = null!;
    public required CartesianUser Author { get; set; }
    public Guid CommunityId { get; set; }
    public Community? Community { get; set; }
    public EventVisibility Visibility { get; set; }
    public EventTiming Timing { get; set; }
    public List<EventTag> Tags { get; set; } = [];
    public List<EventWindow> Windows { get; set; } = [];
    public List<CartesianUser> Subscribers { get; set; } = [];

    public EventDto ToDto() =>
        new(Id, Name, Description, Author.ToDto(), Community?.ToDto(), Visibility, Timing, Tags,
            Windows.Select(w => w.ToDto()));
}
