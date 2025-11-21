using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Events;

public class EventWindowNotFoundError(string eventWindowId) : CartesianError($"Event window {eventWindowId} not found.")
{
    public string EventWindowId { get; set; } = eventWindowId;
}
