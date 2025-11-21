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
    public DbSet<Membership> Memberships { get; set; } = null!;

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

        // Configure all DateTime properties to use UTC
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        property.ClrType == typeof(DateTime)
                            ? new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
                            : new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
                                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
                }
            }
        }

        builder.Entity<CartesianUser>(entity =>
        {
            entity.HasOne(e => e.Avatar)
                .WithOne()
                .HasForeignKey<CartesianUser>(e => e.AvatarId)
                .IsRequired(false);
            entity.HasMany(e => e.Memberships)
                .WithOne(e => e.User);
        });

        builder.Entity<Community>(entity =>
        {
            entity.HasOne(e => e.Avatar)
                .WithOne()
                .HasForeignKey<Community>(e => e.AvatarId)
                .IsRequired(false);
            entity.HasMany(e => e.Events)
                .WithOne(e => e.Community);
            entity.HasMany(e => e.Memberships)
                .WithOne(e => e.Community);
            entity.HasMany(e => e.Images)
                .WithOne()
                .HasForeignKey(m => m.CommunityId)
                .IsRequired(false);
        });

        builder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.Author);
            entity.HasOne(e => e.Community)
                .WithMany(e => e.Events)
                .IsRequired(false);
            entity.HasMany(e => e.Windows)
                .WithOne(e => e.Event);
            entity.HasMany(e => e.Subscribers)
                .WithMany(e => e.SubscribedEvents);
            entity.HasMany(e => e.Images)
                .WithOne()
                .HasForeignKey(m => m.EventId)
                .IsRequired(false);
        });

        builder.Entity<EventWindow>(entity =>
        {
            entity.HasOne(e => e.Event);
            entity.HasMany(e => e.Participants)
                .WithMany(e => e.ParticipatedWindows);
            entity.HasMany(e => e.Images)
                .WithOne()
                .HasForeignKey(m => m.EventWindowId)
                .IsRequired(false);
        });

        builder.Entity<Media>(entity =>
        {
            entity.HasOne(e => e.Author)
                .WithMany(e => e.Media)
                .IsRequired(false);
        });
    }
}
