using System.Text.Json.Serialization;

namespace Cartesian.Services.Events;

[JsonConverter(typeof(JsonStringEnumConverter<EventTiming>))]
public enum EventTiming
{
    Recurring,
    Seasonal,
    OneTime,
    Unknown
}
