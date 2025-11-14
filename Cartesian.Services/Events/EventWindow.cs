using Cartesian.Services.Database;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events.Models;

public class EventWindow
{
    public Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Point Location { get; set; }
    public List<CartesianUser> Participants { get; set; } = [];
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
