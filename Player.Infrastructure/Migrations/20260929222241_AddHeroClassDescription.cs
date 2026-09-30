using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddHeroClassDescription : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "description",
            table: "hero_class",
            type: "character varying(240)",
            maxLength: 240,
            nullable: false,
            defaultValue: ""
        );

        migrationBuilder.Sql(
            """
            UPDATE hero_class
            SET description = CASE code
                WHEN 'warrior' THEN 'Front-line fighter. Takes the hits and protects the team.'
                WHEN 'shaman' THEN 'Spirit wielder. Uses totems and resilience to support the party.'
                WHEN 'mage' THEN 'Master of the arcane. Ranged area damage, but fragile.'
                ELSE description
            END;
            """
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "description", table: "hero_class");
    }
}
