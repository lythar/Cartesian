using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Account.CreateAccount;

public class UserAlreadyExistsError() : CartesianError("This e-mail or username is already in use.")
{
}
