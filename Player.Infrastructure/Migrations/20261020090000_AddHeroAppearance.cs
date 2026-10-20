using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddHeroAppearance : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Existing heroes receive the default appearance: skin tone 1, bob hair, hair color 1.
        migrationBuilder.AddColumn<int>(
            name: "skin_tone",
            table: "hero",
            type: "integer",
            nullable: false,
            defaultValue: 1
        );

        migrationBuilder.AddColumn<string>(
            name: "hair_style",
            table: "hero",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "bob1"
        );

        migrationBuilder.AddColumn<int>(
            name: "hair_color",
            table: "hero",
            type: "integer",
            nullable: true,
            defaultValue: 1
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "skin_tone", table: "hero");
        migrationBuilder.DropColumn(name: "hair_style", table: "hero");
        migrationBuilder.DropColumn(name: "hair_color", table: "hero");
    }
}
