using FluentValidation;

namespace Cartesian.Services.Endpoints;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        
        return services;
    }
}

public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue;
            
            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;
            
            if (validator is null) continue;
            
            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            
            if (!validationResult.IsValid)
            {
                var errorDict = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => ToCamelCase(g.Key),
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                    
                return Results.BadRequest(new ValidationError(errorDict));
            }
        }
        
        return await next(context);
    }
    
    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;
            
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
