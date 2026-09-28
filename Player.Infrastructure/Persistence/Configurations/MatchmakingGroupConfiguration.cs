using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain.Entities;

namespace Player.Infrastructure.Persistence.Configurations;

public sealed class MatchmakingGroupConfiguration : IEntityTypeConfiguration<MatchmakingGroup>
{
    public void Configure(EntityTypeBuilder<MatchmakingGroup> builder)
    {
        builder.ToTable(
            "matchmaking_group",
            table =>
                table.HasCheckConstraint(
                    "ck_matchmaking_group_level_range",
                    "min_new_game_plus_level <= max_new_game_plus_level"
                )
        );
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
        builder.Property(x => x.MinNewGamePlusLevel).HasColumnName("min_new_game_plus_level");
        builder.Property(x => x.MaxNewGamePlusLevel).HasColumnName("max_new_game_plus_level");
        builder.Property(x => x.FormedAt).HasColumnName("formed_at");
    }
}
