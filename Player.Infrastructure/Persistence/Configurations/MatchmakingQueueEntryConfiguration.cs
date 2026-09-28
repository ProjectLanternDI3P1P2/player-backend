using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class MatchmakingQueueEntryConfiguration
    : IEntityTypeConfiguration<MatchmakingQueueEntry>
{
    public void Configure(EntityTypeBuilder<MatchmakingQueueEntry> builder)
    {
        builder.ToTable("matchmaking_queue_entry");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PlayerId).HasColumnName("player_id");
        builder.Property(x => x.HeroId).HasColumnName("hero_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
        builder
            .Property(x => x.NewGamePlusLevelAtEntry)
            .HasColumnName("new_game_plus_level_at_entry");
        builder.Property(x => x.EnteredAt).HasColumnName("entered_at");
        builder.Property(x => x.LeftAt).HasColumnName("left_at");
        builder
            .HasOne(x => x.Player)
            .WithMany(x => x.QueueEntries)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(x => x.Hero)
            .WithMany(x => x.QueueEntries)
            .HasForeignKey(x => x.HeroId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(x => x.Group)
            .WithMany(x => x.QueueEntries)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
