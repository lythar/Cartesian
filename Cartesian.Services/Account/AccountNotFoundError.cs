using Cartesian.Services.Endpoints;

namespace Cartesian.Services.Account;

public class AccountNotFoundError(string accountId) : CartesianError($"Account {accountId} not found.")
{
    public string AccountId { get; set; } = accountId;
}
