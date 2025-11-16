using Cartesian.Services.Account;
using Cartesian.Services.Communities;

namespace Cartesian.Services.Events;

public record EventDto(
    Guid Id,
    string Name,
    string Description,
    CartesianUserDto Author,
    CommunityDto? Community,
    EventVisibility Visibility,
    EventTiming Timing,
    IEnumerable<EventTag> Tags,
    IEnumerable<EventWindowDto> Windows);
