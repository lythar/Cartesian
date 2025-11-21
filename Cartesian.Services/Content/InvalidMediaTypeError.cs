using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Content;

public class InvalidMediaTypeError(string contentType) : CartesianError($"Invalid media type: {contentType}. Only images are allowed.");
