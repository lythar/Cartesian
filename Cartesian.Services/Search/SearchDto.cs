using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Events;

namespace Cartesian.Services.Search;

public record SearchResultDto(
    string Type,
    EventDto? Event,
    CommunityDto? Community,
    CartesianUserDto? User);

public record MergedSearchResultsDto(
    IEnumerable<EventDto> Events,
    IEnumerable<CommunityDto> Communities,
    IEnumerable<CartesianUserDto> Users);
