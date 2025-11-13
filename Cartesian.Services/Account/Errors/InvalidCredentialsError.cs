using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Account.Errors;

public class InvalidCredentialsError()
    : CartesianError("Your e-mail and/or password are incorrect. Please check them and try again.")
{
}
