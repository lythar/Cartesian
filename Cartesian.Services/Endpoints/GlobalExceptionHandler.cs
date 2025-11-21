using Microsoft.AspNetCore.Diagnostics;

namespace Cartesian.Services.Endpoints;

public sealed class GlobalExceptionHandler(IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = new InternalServerError(
            exception.GetType().Name,
            exception.Message,
            environment.IsDevelopment() ? exception.StackTrace : null);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}

public sealed class InternalServerError : CartesianError
{
    public InternalServerError(string exceptionType, string message, string? stackTrace = null) : base(message)
    {
        ExceptionType = exceptionType;
        StackTrace = stackTrace;
    }

    public string ExceptionType { get; }
    public string? StackTrace { get; }
}
