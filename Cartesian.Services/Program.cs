using Cartesian.Services.Account;
using Cartesian.Services.Chat;
using Cartesian.Services.Content;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddMinioClient("storage");
builder.AddNpgsqlDbContext<CartesianDbContext>("database",
    configureDbContextOptions: options => options.UseNpgsql(o => o.UseNetTopologySuite()));
builder.Services.AddDbContextFactory<CartesianDbContext>();
builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new GeoJsonConverterFactory());
    options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
    options.SerializerOptions.Converters.Add(new NullableUtcDateTimeConverter());
});
builder.Services.AddCors();
builder.Services.AddEndpoints();
builder.Services.AddOpenApi(options => options.AddCartesian());
builder.Services.AddIdentity<CartesianUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<CartesianDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddValidation();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return context.Response.WriteAsJsonAsync(new AuthorizationFailedError(context.Request.Path));
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;

        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<ClaimsService>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddSingleton<ChatSseService>();

var app = builder.Build();

var autoRunMigrations = builder.Environment.IsDevelopment() || bool.Parse(builder.Configuration["AutoRunMigrations"] ?? "false");

if (autoRunMigrations)
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<CartesianDbContext>();
    db.Database.Migrate();

    var mediaService = scope.ServiceProvider.GetRequiredService<MediaService>();
    await mediaService.EnsureBucketExists();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
    app.UseCors(x => x.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
