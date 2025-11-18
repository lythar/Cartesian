using System.Text.Json.Serialization;

namespace Cartesian.Services.Events;

[JsonConverter(typeof(JsonStringEnumConverter<EventWindowStatus>))]
public enum EventWindowStatus
{
    Ready,
    Cancelled
}
