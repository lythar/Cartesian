using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events;

public record EventDto(
    Guid Id,
    string Name,
    string Description,
    Point Location,
    CartesianUserDto Author,
    CommunityDto? Community,
    EventVisibility Visibility,
    EventTiming Timing,
    IEnumerable<EventTag> Tags,
    IEnumerable<EventWindowDto> Windows,
    IEnumerable<CartesianUserDto> Participants);
