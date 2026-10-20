using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations;

/// <summary>
/// US-PLAYER-10: a soft-deleted hero frees its name, so uniqueness only applies to active heroes.
/// </summary>
public partial class FilterHeroNameUniquenessOnActiveHeroes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_hero_player_id_name", table: "hero");

        migrationBuilder.CreateIndex(
            name: "IX_hero_player_id_name",
            table: "hero",
            columns: new[] { "player_id", "name" },
            unique: true,
            filter: "NOT is_deleted"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_hero_player_id_name", table: "hero");

        migrationBuilder.CreateIndex(
            name: "IX_hero_player_id_name",
            table: "hero",
            columns: new[] { "player_id", "name" },
            unique: true
        );
    }
}
