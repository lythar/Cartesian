using Cartesian.Services.Account.Data;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events.Data;

public record EventWindowDto(
    Guid Id,
    Guid EventId,
    string Title,
    string Description,
    Point Location,
    IEnumerable<CartesianUserDto> Participants,
    DateTime? StartTime,
    DateTime? EndTime);
