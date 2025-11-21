using System.Security.Claims;
using Cartesian.Services.Content;
using Cartesian.Services.Database;
using Cartesian.Services.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Account;

public class UserAvatarEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/account/api/me/avatar", PutSetAvatar)
            .RequireAuthorization()
            .Produces(200, typeof(MyUserDto))
            .Produces(400, typeof(AccountNotFoundError))
            .Produces(404, typeof(MediaNotFoundError));

        app.MapDelete("/account/api/me/avatar", DeleteRemoveAvatar)
            .RequireAuthorization()
            .Produces(200, typeof(MyUserDto))
            .Produces(400, typeof(AccountNotFoundError));
    }

    async Task<IResult> PutSetAvatar(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal, SetAvatarBody body)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await dbContext.Users
            .Include(u => u.Avatar)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        var media = await dbContext.Media.FindAsync(body.MediaId);
        if (media == null || media.IsDeleted)
            return Results.NotFound(new MediaNotFoundError(body.MediaId));

        user.AvatarId = body.MediaId;
        await dbContext.SaveChangesAsync();

        return Results.Ok(user.ToMyUserDto());
    }

    async Task<IResult> DeleteRemoveAvatar(CartesianDbContext dbContext, UserManager<CartesianUser> userManager,
        ClaimsPrincipal principal)
    {
        var userId = userManager.GetUserId(principal);
        if (userId == null) return Results.Unauthorized();

        var user = await dbContext.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null) return Results.BadRequest(new AccountNotFoundError(userId));

        user.AvatarId = null;
        user.Avatar = null;
        await dbContext.SaveChangesAsync();

        return Results.Ok(user.ToMyUserDto());
    }

    record SetAvatarBody(Guid MediaId);
}
