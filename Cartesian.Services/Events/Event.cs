using Cartesian.Services.Communities;
using Cartesian.Services.Database;
using Cartesian.Services.Events.Models;

namespace Cartesian.Services.Events;

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
}
