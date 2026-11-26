using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class GameSessionMemberConfiguration : IEntityTypeConfiguration<GameSessionMember>
{
    /// <summary>Unique index guaranteeing a single active session per hero.</summary>
    public const string ActiveHeroIndexName = "IX_game_session_member_hero_id_active";

    public void Configure(EntityTypeBuilder<GameSessionMember> builder)
    {
        builder.ToTable("game_session_member");
        builder.HasKey(x => new { x.SessionId, x.HeroId });
        builder.Property(x => x.SessionId).HasColumnName("session_id");
        builder.Property(x => x.HeroId).HasColumnName("hero_id");
        builder
            .Property(x => x.MemberStatus)
            .HasConversion<string>()
            .HasColumnName("member_status")
            .HasMaxLength(32);
        builder.Property(x => x.JoinedAt).HasColumnName("joined_at");
        builder.Property(x => x.LeftAt).HasColumnName("left_at");
        // Persisted invariant: a hero is an active member of at most one session. Leaving a
        // session (any terminal state) sets the status to Left, which releases the hero.
        builder
            .HasIndex(x => x.HeroId)
            .IsUnique()
            .HasDatabaseName(ActiveHeroIndexName)
            .HasFilter("member_status = 'Active'");
        builder
            .HasOne(x => x.GameSession)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.Hero)
            .WithMany(x => x.SessionMembers)
            .HasForeignKey(x => x.HeroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
