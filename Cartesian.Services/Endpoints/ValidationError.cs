namespace Cartesian.Services.Endpoints;

public class ValidationError : CartesianError
{
    public ValidationError(Dictionary<string, string[]> errors) 
        : base("Validation failed")
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}
