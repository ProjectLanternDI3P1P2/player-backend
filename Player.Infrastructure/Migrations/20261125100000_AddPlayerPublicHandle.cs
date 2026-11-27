using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations
{
    /// <summary>
    /// US-PLAYER-06: public handle of the server-rendered public profile. Existing players are
    /// backfilled with the rule of <c>PlayerPublicHandle.Create</c>, written in SQL.
    /// </summary>
    public partial class AddPlayerPublicHandle : Migration
    {
        // Same rule as PlayerPublicHandle.Create: NFD, drop combining accents (U+0300-U+036F),
        // lowercase, one dash per run of other characters, no edge dash, at most 23 characters,
        // "player" when empty, then a dash and the first 8 hex digits of the id.
        private const string BackfillPublicHandleSql = """
            UPDATE player
            SET public_handle =
                COALESCE(
                    NULLIF(
                        btrim(
                            left(
                                btrim(
                                    regexp_replace(
                                        regexp_replace(
                                            lower(normalize(display_name, NFD)),
                                            '[' || chr(768) || '-' || chr(879) || ']',
                                            '',
                                            'g'
                                        ),
                                        '[^a-z0-9]+',
                                        '-',
                                        'g'
                                    ),
                                    '-'
                                ),
                                23
                            ),
                            '-'
                        ),
                        ''
                    ),
                    'player'
                )
                || '-'
                || left(replace(id::text, '-', ''), 8);
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "public_handle",
                table: "player",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true
            );

            migrationBuilder.Sql(BackfillPublicHandleSql);

            migrationBuilder.AlterColumn<string>(
                name: "public_handle",
                table: "player",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_player_public_handle",
                table: "player",
                column: "public_handle",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_player_public_handle", table: "player");

            migrationBuilder.DropColumn(name: "public_handle", table: "player");
        }
    }
}
