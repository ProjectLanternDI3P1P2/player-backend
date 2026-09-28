using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerEntity = Player.Domain.Entities.Player;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<PlayerEntity>
{
    public void Configure(EntityTypeBuilder<PlayerEntity> builder)
    {
        builder.ToTable("player");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100);
        builder.Property(x => x.AccountStatus).HasColumnName("account_status").HasMaxLength(32);
        builder.Property(x => x.NewGamePlusLevel).HasColumnName("new_game_plus_level");
        builder.Property(x => x.NewGamePlusUpdatedAt).HasColumnName("new_game_plus_updated_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.AnonymizedAt).HasColumnName("anonymized_at");
    }
}
