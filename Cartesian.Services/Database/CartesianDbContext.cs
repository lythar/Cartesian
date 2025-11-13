using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Database;

public class CartesianDbContext : IdentityDbContext<CartesianUser>
{
    public CartesianDbContext()
    {
    }

    public CartesianDbContext(DbContextOptions<CartesianDbContext> options)
        : base(options)
    {
    }
}
