using System.Diagnostics.CodeAnalysis;

namespace Cartesian.Services.Communities;

public static class MembershipExtension
{
    public static bool TryAssertPermission(this Membership membership, Permissions required,
        [NotNullWhen(true)] out MissingPermissionError? error)
    {
        if (membership.Permissions.HasFlag(required))
        {
            error = null;
            return false;
        }

        error = new MissingPermissionError(required, membership.CommunityId);
        return true;
    }
}
