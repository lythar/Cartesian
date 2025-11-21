using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Content;

public class MediaNotFoundError(Guid mediaId) : CartesianError($"Media with ID {mediaId} not found");
