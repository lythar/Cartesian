using Cartesian.Services.Account;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events;

public class EventWindow
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Point Location { get; set; }
    public List<CartesianUser> Participants { get; set; } = [];
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public EventWindowDto ToDto() =>
        new(Id, EventId, Title, Description, Location, Participants.Select(p => p.ToDto()), StartTime, EndTime);
}
