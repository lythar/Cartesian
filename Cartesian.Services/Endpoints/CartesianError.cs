namespace Cartesian.Services.Endpoints;

public class CartesianError(string code, string message)
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    public CartesianError(Type type, string message) : this(type.Name, message)
    {
    }
    
    public CartesianError(string message) : this("CartesianError", message)
    {
        Code = GetType().Name;
    }
}
