using Cartesian.Services.Account.Entities;
using Cartesian.Services.Communities.Entities;
using Cartesian.Services.Events.Data;

namespace Cartesian.Services.Events.Entities;

public class Event
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required CartesianUser Owner { get; set; }
    public Community? Community { get; set; }
    public EventVisibility Visibility { get; set; }
    public EventTiming Timing { get; set; }
    public List<EventTag> Tags { get; set; } = [];
    public List<EventWindow> Windows { get; set; } = [];
    public List<CartesianUser> Subscribers { get; set; } = [];

    public EventDto ToDto() =>
        new(Id, Name, Description, Owner.ToDto(), Community?.ToDto(), Visibility, Timing, Tags,
            Windows.Select(w => w.ToDto()), Subscribers.Select(s => s.ToDto()));
}
