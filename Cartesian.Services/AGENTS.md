# Cartesian Backend Development Guide

## Project Structure

- **Cartesian.AppHost**: Aspire orchestration host
- **Cartesian.Services**: Main API service with domain logic
- **Cartesian.ServiceDefaults**: Shared Aspire configuration (telemetry, health checks)

## Tech Stack

- **.NET 10.0** with ASP.NET Core
- **Entity Framework Core 9.0** + PostgreSQL with PostGIS
- **ASP.NET Core Identity** for authentication
- **Aspire** for local development orchestration
- **Minio** for object storage
- **NetTopologySuite** for geospatial data
- **OpenAPI/Scalar** for API documentation

## Commands

- **Build**: `dotnet build Cartesian.slnx`
- **Run**: `dotnet run --project Cartesian.AppHost` (starts entire stack)
- **Migrations**: 
  - Add: `dotnet ef migrations add <Name> --project Cartesian.Services`
  - Apply: Auto-runs in development, manual in production
- **Format**: `dotnet format`

## Aspire Development Tools

When the Aspire AppHost is running, **ALWAYS** use the Aspire MCP tools for monitoring and managing resources:

### Available Aspire Tools

- **aspire-dashboard_list_resources**: List all resources (services, databases, containers) with their status, endpoints, and health
- **aspire-dashboard_list_console_logs**: View console output for a specific resource (stdout/stderr)
- **aspire-dashboard_list_structured_logs**: View structured logs for a specific resource or all resources
- **aspire-dashboard_list_traces**: View distributed traces across resources for performance monitoring
- **aspire-dashboard_list_trace_structured_logs**: View logs for a specific trace ID
- **aspire-dashboard_execute_resource_command**: Execute commands on resources (start, stop, restart)

### Common Scenarios

**Check service status:**
```
Use: aspire-dashboard_list_resources
Returns: All services with running state, endpoints, and health status
```

**Debug a failing service:**
```
1. Use: aspire-dashboard_list_console_logs with resourceName
2. Review stdout/stderr for exceptions or errors
3. Use: aspire-dashboard_list_structured_logs with resourceName for detailed logs
```

**Restart a service:**
```
Use: aspire-dashboard_execute_resource_command with resourceName and commandName: "resource-restart"
Common resources: "cartesian-services", "cartesian-frontend", "cartesian-db", "cartesian-minio"
```

**Investigate performance issues:**
```
1. Use: aspire-dashboard_list_traces (optionally filter by resourceName)
2. Identify slow traces by duration
3. Use: aspire-dashboard_list_trace_structured_logs with traceId for detailed analysis
```

### Critical Rules for Aspire Tools

1. **ALWAYS** use `aspire-dashboard_list_resources` first to verify resource names before using other tools
2. **ALWAYS** check console logs when a resource fails to start or crashes
3. **NEVER** use `dotnet run` or bash commands to restart services - use `aspire-dashboard_execute_resource_command` instead
4. **ALWAYS** use Aspire traces for investigating distributed system issues (cross-service calls, database queries)
5. **PREFER** structured logs over console logs for application-level debugging (more detailed, filterable)
6. **DO NOT** assume resource names - they may differ from project names (e.g., "cartesian-services" not "Cartesian.Services")

## Architecture Patterns

### Endpoint Pattern

All API endpoints MUST implement `IEndpoint`:

```csharp
public class CreateResourceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/resource/api/create", Handler)
            .RequireAuthorization()
            .Produces(200, typeof(ResourceDto))
            .Produces(400, typeof(SomeError));
    }
    
    async Task<IResult> Handler(Dependencies deps, Body body)
    {
        // Implementation
        return Results.Ok(result);
    }
    
    record Body(string Prop);
}
```

- Auto-registered via reflection in `Program.cs`
- Use `MapPost/Get/Put/Delete` with route pattern
- Return `IResult` (Results.Ok, Results.BadRequest, etc.)
- Use record types for request/response bodies

### Error Handling

Inherit from `CartesianError`:

```csharp
public class ResourceNotFoundError(string resourceId) 
    : CartesianError($"Resource {resourceId} not found")
{
    public string ResourceId { get; } = resourceId;
}
```

- Error code auto-derived from class name
- Extra properties automatically included in OpenAPI
- Return via `Results.BadRequest(new SomeError())`
- Use appropriate status codes (400/403/404/500)

### Entity & DTO Pattern

```csharp
public class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string Name { get; set; }
    
    // Navigation properties
    public List<Related> Related { get; set; } = [];
    
    public EntityDto ToDto() => new(Id, Name);
}

public record EntityDto(Guid Id, string Name);
```

- Use `required` for mandatory properties
- Initialize collections to empty lists
- `ToDto()` methods for mapping to DTOs
- DTOs use records for immutability

### Database Context

```csharp
public class CartesianDbContext : IdentityDbContext<CartesianUser>
{
    public DbSet<Entity> Entities { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Entity>(entity =>
        {
            entity.HasOne(e => e.Related)
                  .WithMany(r => r.Entities);
        });
    }
}
```

- Inherit from `IdentityDbContext<CartesianUser>`
- Configure relationships in `OnModelCreating`
- Use Fluent API, not data annotations

### Authorization

```csharp
var userId = userManager.GetUserId(principal);
if (userId == null) return Results.Unauthorized();

var membership = await dbContext.Memberships
    .WhereMember(userId, communityId)
    .FirstOrDefaultAsync();
    
if (membership.TryAssertPermission(Permissions.ManageEvents, out var error))
    return Results.Json(error, statusCode: 403);
```

- Use `UserManager<CartesianUser>` and `ClaimsPrincipal`
- Check permissions via `TryAssertPermission`
- Return 401 for unauthenticated, 403 for unauthorized

## Code Style & Conventions

- **C# 12+**: Use modern C# features (primary constructors, required, collection expressions)
- **Nullable**: Enabled. Use `null!` only for EF navigation properties
- **Naming**: PascalCase for public, camelCase for parameters
- **Indentation**: 4 spaces (C#), enforced by `.editorconfig`
- **Async**: All I/O operations must be async
- **No regions**: Avoid `#region`, organize with partial classes if needed
- **Records**: Use for DTOs and immutable data
- **LINQ**: Prefer query syntax for complex queries
- **File-scoped namespaces**: Use `namespace X;` not `namespace X { }`

## Database Migrations

1. Add migration: `dotnet ef migrations add <Name> --project Cartesian.Services`
2. Review generated migration in `Database/Migrations/`
3. Migrations auto-run in development (`AutoRunMigrations = true`)
4. In production, manually apply or use deployment pipeline

## OpenAPI Documentation

- Accessible at `/docs` in development (Scalar UI)
- Auto-generates schemas from endpoints and errors
- Cookie authentication scheme auto-configured
- Tags derived from namespace (Account, Communities, etc.)

## Testing

- No test runner currently configured
- Manual testing via Scalar UI at `/docs`
- Future: Add xUnit tests in separate test project

## Domain Organization

- **Account/**: Authentication, user management
- **Communities/**: Community CRUD, memberships, permissions
- **Events/**: Event management, windows, subscriptions
- **Content/**: Media uploads (Minio integration)
- **Database/**: DbContext, migrations
- **Endpoints/**: Shared endpoint infrastructure

## Common Patterns

### Dependency Injection
```csharp
async Task<IResult> Handler(
    CartesianDbContext dbContext,
    UserManager<CartesianUser> userManager,
    ClaimsPrincipal principal,
    ILogger<MyEndpoint> logger,
    Body body)
```

### Query Pattern
```csharp
var result = await dbContext.Entities
    .Include(e => e.Related)
    .Where(e => e.UserId == userId)
    .FirstOrDefaultAsync();
```

### Create Pattern
```csharp
var entity = new Entity
{
    Id = Guid.NewGuid(),
    Name = body.Name,
    // ...
};
await dbContext.AddAsync(entity);
await dbContext.SaveChangesAsync();
return Results.Ok(entity.ToDto());
```

## Critical Rules

1. **NEVER** use `any` or suppress nullability warnings unnecessarily
2. **ALWAYS** use async/await for database operations
3. **ALWAYS** validate user permissions before modifications
4. **ALWAYS** return appropriate HTTP status codes
5. **ALWAYS** use typed errors inheriting from `CartesianError`
6. **ALWAYS** use DTOs for API responses, never expose entities directly
7. **NEVER** bypass EF Core, use DbContext for all data access
8. **ALWAYS** use transactions for multi-step operations
9. **ALWAYS** log important operations with ILogger
10. **NEVER** store sensitive data in migrations or code

## Environment

- **Development**: Runs via Aspire, auto-migrations, CORS enabled
- **Production**: Manual migrations, HTTPS redirection, no CORS
- **Database**: PostgreSQL 18 with PostGIS (port 41021 in dev)
- **Storage**: Minio (port 41022 in dev)

## Validation

### FluentValidation

The project uses **FluentValidation** for request validation. Validators are automatically discovered and applied via an endpoint filter.

```csharp
public class CreateResourceBodyValidator : AbstractValidator<CreateResourceEndpoint.Body>
{
    public CreateResourceBodyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 100).WithMessage("Name must be between 3 and 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Count)
            .InclusiveBetween(1, 1000).WithMessage("Count must be between 1 and 1000");
    }
}
```

**Key Points:**
- Validators must inherit from `AbstractValidator<T>`
- Request body records should be `public` for validators to access them
- Validation errors return 400 with `ValidationError` containing a dictionary of field errors
- Add `.Produces(400, typeof(ValidationError))` to endpoint definitions

**Validation Error Format:**
```json
{
  "Code": "ValidationError",
  "Message": "Validation failed",
  "Errors": {
    "Name": ["Name must be between 3 and 100 characters"],
    "Email": ["Invalid email format", "Email must not exceed 255 characters"]
  }
}
```

**Reasonable Request Limits:**
- **Usernames**: 3-30 characters, alphanumeric + spaces only
- **Emails**: Valid email format, max 255 characters
- **Passwords**: 8-128 characters, must include uppercase, lowercase, digit, special character
- **Short text (titles)**: 3-100 characters
- **Long text (descriptions)**: max 2000 characters
- **Collections (tags, lists)**: max 10 items
- **Files**: max 10MB (configured in MediaService)


## API Features

### Community Member Listing

The `/community/api/{communityId}/members` endpoint supports flexible sorting:

**Sorting Options** (via `sortBy` query parameter):
- `JoinDate` (default): Sort by membership creation date (oldest first)
- `Authority`: Sort by number of permissions (most permissions first), then by join date

**Authority Sorting Logic:**
- Counts the number of permission flags set for each member
- Members with more permissions appear first
- Members with equal permissions are ordered by join date

**Example Requests:**
```
GET /community/api/{communityId}/members?sortBy=JoinDate&limit=50&skip=0
GET /community/api/{communityId}/members?sortBy=Authority&limit=25&skip=0
```

**Query Parameters:**
- `limit`: 1-100 (default: 50)
- `skip`: >= 0 (default: 0)
- `sortBy`: `JoinDate` or `Authority` (default: `JoinDate`)

