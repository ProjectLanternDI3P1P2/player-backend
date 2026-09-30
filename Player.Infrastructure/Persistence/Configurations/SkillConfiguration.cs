using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skill");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
        builder.Property(x => x.ClassCode).HasColumnName("class_code").HasMaxLength(50);
        builder.Property(x => x.Label).HasColumnName("label").HasMaxLength(100);
        builder.Property(x => x.RequiredLevel).HasColumnName("required_level");
        builder.Property(x => x.TargetingType).HasColumnName("targeting_type").HasMaxLength(50);
        builder
            .HasOne(x => x.HeroClass)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.ClassCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
