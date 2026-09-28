using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class GameSessionMemberConfiguration : IEntityTypeConfiguration<GameSessionMember>
{
    public void Configure(EntityTypeBuilder<GameSessionMember> builder)
    {
        builder.ToTable("game_session_member");
        builder.HasKey(x => new { x.SessionId, x.HeroId });
        builder.Property(x => x.SessionId).HasColumnName("session_id");
        builder.Property(x => x.HeroId).HasColumnName("hero_id");
        builder.Property(x => x.MemberStatus).HasColumnName("member_status").HasMaxLength(32);
        builder.Property(x => x.JoinedAt).HasColumnName("joined_at");
        builder.Property(x => x.LeftAt).HasColumnName("left_at");
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
