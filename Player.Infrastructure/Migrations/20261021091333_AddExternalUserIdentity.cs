using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalUserIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_user_id",
                table: "player",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_player_external_user_id",
                table: "player",
                column: "external_user_id",
                unique: true,
                filter: "external_user_id IS NOT NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_player_external_user_id", table: "player");

            migrationBuilder.DropColumn(name: "external_user_id", table: "player");
        }
    }
}
