using Cartesian.Services.Account;
using Cartesian.Services.Chat;
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
    public DbSet<ChatChannel> ChatChannels { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<ChatMention> ChatMentions { get; set; } = null!;
    public DbSet<ChatMute> ChatMutes { get; set; } = null!;
    public DbSet<ChatUserSettings> ChatUserSettings { get; set; } = null!;
    public DbSet<ChatPinnedMessage> ChatPinnedMessages { get; set; } = null!;
    public DbSet<ChatReaction> ChatReactions { get; set; } = null!;

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
            entity.HasMany(e => e.FavoritedBy)
                .WithMany(e => e.FavoritedEvents)
                .UsingEntity(j => j.ToTable("EventFavorites"));
            entity.HasMany(e => e.Participants)
                .WithMany(e => e.ParticipatedEvents);
            entity.HasMany(e => e.Images)
                .WithOne()
                .HasForeignKey(m => m.EventId)
                .IsRequired(false);
        });

        builder.Entity<EventWindow>(entity =>
        {
            entity.HasOne(e => e.Event);
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

        builder.Entity<ChatChannel>(entity =>
        {
            entity.HasOne(e => e.Participant1)
                .WithMany()
                .HasForeignKey(e => e.Participant1Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Participant2)
                .WithMany()
                .HasForeignKey(e => e.Participant2Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Community)
                .WithMany()
                .HasForeignKey(e => e.CommunityId)
                .IsRequired(false);
            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .IsRequired(false);
            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Channel);
            entity.HasMany(e => e.Mutes)
                .WithOne(e => e.Channel);
        });

        builder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(e => e.Channel)
                .WithMany(e => e.Messages);
            entity.HasOne(e => e.Author)
                .WithMany();
            entity.HasMany(e => e.Mentions)
                .WithOne(e => e.Message);
            entity.HasMany(e => e.Attachments)
                .WithMany();
        });

        builder.Entity<ChatMention>(entity =>
        {
            entity.HasOne(e => e.Message)
                .WithMany(e => e.Mentions);
            entity.HasOne(e => e.User)
                .WithMany();
        });

        builder.Entity<ChatMute>(entity =>
        {
            entity.HasOne(e => e.Channel)
                .WithMany(e => e.Mutes);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.MutedBy)
                .WithMany()
                .HasForeignKey(e => e.MutedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ChatUserSettings>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany();
        });

        builder.Entity<ChatPinnedMessage>(entity =>
        {
            entity.HasOne(e => e.Channel)
                .WithMany(c => c.PinnedMessages)
                .HasForeignKey(e => e.ChannelId);
            entity.HasOne(e => e.Message)
                .WithMany(m => m.PinnedIn)
                .HasForeignKey(e => e.MessageId);
            entity.HasOne(e => e.PinnedBy)
                .WithMany()
                .HasForeignKey(e => e.PinnedById);
            entity.HasIndex(e => new { e.ChannelId, e.MessageId }).IsUnique();
        });

        builder.Entity<ChatReaction>(entity =>
        {
            entity.HasOne(e => e.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Emoji)
                .HasMaxLength(20)
                .IsUnicode(true);
            entity.HasIndex(e => new { e.MessageId, e.UserId, e.Emoji }).IsUnique();
        });
    }
}
