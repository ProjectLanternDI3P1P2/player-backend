using PlayerEntity = global::Player.Domain.Entities.Player;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Player.Infrastructure.Persistence.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<PlayerEntity>
{
    public void Configure(EntityTypeBuilder<PlayerEntity> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).IsRequired().HasColumnName("PlayerId");

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100).HasColumnName("PlayerName");

        builder.Property(p => p.Health).IsRequired().HasColumnName("PlayerHealth");

        builder.Property(p => p.MaxHealth).IsRequired().HasColumnName("PlayerMaxHealth");

        builder.Property(p => p.Attack).IsRequired().HasColumnName("PlayerAttack");
    }
}
