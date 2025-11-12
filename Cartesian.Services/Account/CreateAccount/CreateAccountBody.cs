namespace Cartesian.Services.Account.CreateAccount;

public class CreateAccountBody
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
