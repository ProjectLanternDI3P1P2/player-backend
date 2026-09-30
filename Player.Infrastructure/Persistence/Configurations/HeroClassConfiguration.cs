using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class HeroClassConfiguration : IEntityTypeConfiguration<HeroClass>
{
    public void Configure(EntityTypeBuilder<HeroClass> builder)
    {
        builder.ToTable("hero_class");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
        builder.Property(x => x.Label).HasColumnName("label").HasMaxLength(100);
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(240);
        builder.Property(x => x.BaseHealth).HasColumnName("base_health");
        builder.Property(x => x.BaseMana).HasColumnName("base_mana");
    }
}
