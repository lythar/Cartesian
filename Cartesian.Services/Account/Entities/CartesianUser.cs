using Cartesian.Services.Account.Data;
using Microsoft.AspNetCore.Identity;

namespace Cartesian.Services.Account.Entities;

public class CartesianUser : IdentityUser
{
    public CartesianUserDto ToDto() => new(Id, UserName!);
    public MyUserDto ToMyUserDto() => new(Id, UserName!, Email!, EmailConfirmed);
}
