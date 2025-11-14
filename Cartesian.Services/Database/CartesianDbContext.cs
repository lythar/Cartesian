using Cartesian.Services.Events;
using Cartesian.Services.Events.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Database;

public class CartesianDbContext : IdentityDbContext<CartesianUser>
{
    // public DbSet<Event> Events { get; set; } = null!;
    // public DbSet<EventWindow> EventWindows { get; set; } = null!;

    public CartesianDbContext()
    {
    }

    public CartesianDbContext(DbContextOptions<CartesianDbContext> options)
        : base(options)
    {
    }
}
