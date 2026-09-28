using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class GameSessionTransitionConfiguration
    : IEntityTypeConfiguration<GameSessionTransition>
{
    public void Configure(EntityTypeBuilder<GameSessionTransition> builder)
    {
        builder.ToTable("game_session_transition");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SessionId).HasColumnName("session_id");
        builder.Property(x => x.SourceStatus).HasColumnName("source_status").HasMaxLength(32);
        builder.Property(x => x.TargetStatus).HasColumnName("target_status").HasMaxLength(32);
        builder.Property(x => x.Cause).HasColumnName("cause").HasMaxLength(100);
        builder.Property(x => x.Actor).HasColumnName("actor").HasMaxLength(100);
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder
            .HasOne(x => x.GameSession)
            .WithMany(x => x.Transitions)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
