using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillCombatCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mana_cost",
                table: "skill",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "power",
                table: "skill",
                type: "integer",
                nullable: false,
                defaultValue: 100
            );

            // Balance of the skills that already exist (docs/player/combatant_snapshot.en.md).
            migrationBuilder.Sql(
                """
                UPDATE skill SET mana_cost = 4, power = 150 WHERE code = 'warrior-strike';
                UPDATE skill SET mana_cost = 6, power = 170 WHERE code = 'mage-bolt';
                UPDATE skill SET mana_cost = 5, power = 140 WHERE code = 'shaman-totem';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "mana_cost", table: "skill");

            migrationBuilder.DropColumn(name: "power", table: "skill");
        }
    }
}
