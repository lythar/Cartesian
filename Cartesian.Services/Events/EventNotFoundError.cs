using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Events;

public class EventNotFoundError(string eventId) : CartesianError($"Event {eventId} not found.")
{
    public string EventId { get; set; } = eventId;
}
