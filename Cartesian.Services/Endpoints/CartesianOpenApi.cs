using System.Reflection;
using Cartesian.Services.Account;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Cartesian.Services.Endpoints;

public static class CartesianOpenApi
{
    public static OpenApiOptions AddCartesian(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

            if (!document.Components.SecuritySchemes.ContainsKey("Cookie"))
            {
                document.Components.SecuritySchemes.Add("Cookie",
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Cookie,
                        Name = ".AspNetCore.Identity.Application",
                        Description = "ASP.NET Core Identity cookie authentication"
                    });
            }

            var errorTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CartesianError)) && !t.IsAbstract);

            foreach (var errorType in errorTypes)
            {
                var errorName = errorType.Name;

                var properties = new Dictionary<string, IOpenApiSchema>
                {
                    ["code"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Const = errorName
                    },
                    ["message"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Description = "Error message describing the failure"
                    }
                };

                var required = new HashSet<string> { "code", "message" };

                var typeProperties = errorType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.DeclaringType == errorType && p.Name != "Code" && p.Name != "Message");

                foreach (var prop in typeProperties)
                {
                    var propName = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);

                    properties[propName] = new OpenApiSchema
                    {
                        Type = GetJsonSchemaType(prop.PropertyType),
                        Description = $"{prop.Name} property"
                    };
                    required.Add(propName);
                }

                document.Components.Schemas[errorName] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    Properties = properties,
                    Required = required
                };
            }

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, ct) =>
        {
            var endpointType = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<MethodInfo>()
                .FirstOrDefault()
                ?.DeclaringType;

            if (endpointType != null)
            {
                var namespaceParts = endpointType.Namespace?.Split('.') ?? [];

                var tag = namespaceParts.Length > 0 ? namespaceParts[^1] : "Default";

                operation.Tags = new HashSet<OpenApiTagReference> { new(tag) };
            }

            var metadata = context.Description.ActionDescriptor.EndpointMetadata;
            var requiresAuth = metadata.Any(m => m is Microsoft.AspNetCore.Authorization.AuthorizeAttribute);
            var allowsAnonymous = metadata.Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

            if (!requiresAuth || allowsAnonymous) return Task.CompletedTask;

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            var securityRequirement = new OpenApiSecurityRequirement();
            securityRequirement.Add(
                new OpenApiSecuritySchemeReference("Cookie"),
                new List<string>()
            );
            operation.Security.Add(securityRequirement);

            operation.Responses ??= new OpenApiResponses();
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized - Authentication required",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Schema = new OpenApiSchemaReference(nameof(AuthorizationFailedError))
                        }
                    }
                });
            }

            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Forbidden - Insufficient permissions"
                });
            }

            return Task.CompletedTask;
        });

        return options;
    }

    private static JsonSchemaType GetJsonSchemaType(Type type)
    {
        if (type == typeof(string))
            return JsonSchemaType.String;
        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
            return JsonSchemaType.Integer;
        if (type == typeof(bool))
            return JsonSchemaType.Boolean;
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            return JsonSchemaType.Number;
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            return JsonSchemaType.Array;

        return JsonSchemaType.Object;
    }
}
