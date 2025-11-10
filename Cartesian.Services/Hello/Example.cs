using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Hello;

public class Example : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/example", () => "Hello, World!");
    }
}