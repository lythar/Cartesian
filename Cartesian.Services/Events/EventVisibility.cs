using System.Text.Json.Serialization;

namespace Cartesian.Services.Events;

[JsonConverter(typeof(JsonStringEnumConverter<EventVisibility>))]
public enum EventVisibility
{
    Draft,
    Internal,
    Community,
    Public
}
