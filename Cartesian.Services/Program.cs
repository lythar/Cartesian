using Cartesian.Services.Account;
using Cartesian.Services.Content;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddMinioClient("cartesian-storage");
builder.AddNpgsqlDbContext<CartesianDbContext>("cartesian",
    configureDbContextOptions: options => options.UseNpgsql(o => o.UseNetTopologySuite()));
builder.AddServiceDefaults();

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

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<CartesianDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddValidation();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
    app.UseCors(x => x.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
