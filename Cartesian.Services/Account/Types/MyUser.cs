using Cartesian.Services.Database;

namespace Cartesian.Services.Account.Types;

public class MyUser
{
    public string Id { get; }
    public string Name { get; }
    public string Email { get; }
    public bool EmailConfirmed { get; }

    public MyUser(CartesianUser user)
    {
        Id = user.Id;
        Name = user.UserName!;
        Email = user.Email!;
        EmailConfirmed = user.EmailConfirmed;
    }
}
