using Cartesian.Services.Account;
using NetTopologySuite.Geometries;

namespace Cartesian.Services.Events;

public record EventWindowDto(
    Guid Id,
    Guid EventId,
    string Title,
    string Description,
    Point Location,
    DateTime? StartTime,
    DateTime? EndTime);
