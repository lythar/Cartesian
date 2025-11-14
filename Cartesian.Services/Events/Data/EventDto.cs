using Cartesian.Services.Account.Data;
using Cartesian.Services.Communities.Data;

namespace Cartesian.Services.Events.Data;

public record EventDto(
    Guid Id,
    string Name,
    string Description,
    CartesianUserDto Owner,
    CommunityDto? Community,
    EventVisibility Visibility,
    EventTiming Timing,
    IEnumerable<EventTag> Tags,
    IEnumerable<EventWindowDto> Windows,
    IEnumerable<CartesianUserDto> Subscribers);
