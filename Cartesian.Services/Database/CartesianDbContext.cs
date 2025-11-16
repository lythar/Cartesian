using Cartesian.Services.Account;
using Cartesian.Services.Communities;
using Cartesian.Services.Content;
using Cartesian.Services.Events;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cartesian.Services.Database;

public class CartesianDbContext : IdentityDbContext<CartesianUser>
{
    public DbSet<Community> Communities { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<EventWindow> EventWindows { get; set; } = null!;
    public DbSet<Media> Media { get; set; } = null!;

    public CartesianDbContext()
    {
    }

    public CartesianDbContext(DbContextOptions<CartesianDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CartesianUser>()
            .HasOne(e => e.Avatar);

        builder.Entity<CartesianUser>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.User);

        builder.Entity<Community>()
            .HasOne(e => e.Avatar);

        builder.Entity<Community>()
            .HasMany(e => e.Events)
            .WithOne(e => e.Community);

        builder.Entity<Community>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.Community);

        builder.Entity<Community>()
            .HasMany(e => e.Memberships)
            .WithOne(e => e.Community);

        builder.Entity<Event>()
            .HasOne(e => e.Author);

        builder.Entity<Event>()
            .HasOne(e => e.Community)
            .WithMany(e => e.Events)
            .IsRequired(false);

        builder.Entity<Event>()
            .HasMany(e => e.Windows)
            .WithOne(e => e.Event);

        builder.Entity<Event>()
            .HasMany(e => e.Subscribers)
            .WithMany(e => e.SubscribedEvents);

        builder.Entity<EventWindow>()
            .HasOne(e => e.Event);

        builder.Entity<EventWindow>()
            .HasMany(e => e.Participants)
            .WithMany(e => e.ParticipatedWindows);

        builder.Entity<Media>()
            .HasOne(e => e.Author)
            .WithMany(e => e.Media)
            .IsRequired(false);
    }
}
