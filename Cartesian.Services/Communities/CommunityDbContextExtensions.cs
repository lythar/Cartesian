namespace Cartesian.Services.Communities;

public static class CommunityDbContextExtensions
{
    public static IQueryable<Membership> WhereMember(this IQueryable<Membership> query, string userId,
        Guid communityId) => query.Where(c => c.CommunityId == communityId && c.UserId == userId);
}
