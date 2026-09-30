using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class HeroSkillConfiguration : IEntityTypeConfiguration<HeroSkill>
{
    public void Configure(EntityTypeBuilder<HeroSkill> builder)
    {
        builder.ToTable("hero_skill");
        builder.HasKey(x => new { x.HeroId, x.SkillCode });
        builder.Property(x => x.HeroId).HasColumnName("hero_id");
        builder.Property(x => x.SkillCode).HasColumnName("skill_code").HasMaxLength(50);
        builder.Property(x => x.UnlockedAt).HasColumnName("unlocked_at");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder
            .HasOne(x => x.Hero)
            .WithMany(x => x.HeroSkills)
            .HasForeignKey(x => x.HeroId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.Skill)
            .WithMany(x => x.HeroSkills)
            .HasForeignKey(x => x.SkillCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
