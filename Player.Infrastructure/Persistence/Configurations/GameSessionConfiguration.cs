using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("game_session");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MatchmakingGroupId).HasColumnName("matchmaking_group_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
        builder.Property(x => x.Mode).HasColumnName("mode").HasMaxLength(32);
        builder.Property(x => x.DungeonRunId).HasColumnName("dungeon_run_id");
        builder.Property(x => x.DungeonSeed).HasColumnName("dungeon_seed").HasMaxLength(128);
        builder.Property(x => x.ProgressionRecorded).HasColumnName("progression_recorded");
        builder
            .Property(x => x.TerminationReason)
            .HasColumnName("termination_reason")
            .HasMaxLength(100);
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.LastActiveAt).HasColumnName("last_active_at");
        builder.Property(x => x.EndedAt).HasColumnName("ended_at");
        builder.HasIndex(x => x.MatchmakingGroupId).IsUnique();
        builder
            .HasOne(x => x.MatchmakingGroup)
            .WithOne(x => x.GameSession)
            .HasForeignKey<GameSession>(x => x.MatchmakingGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
