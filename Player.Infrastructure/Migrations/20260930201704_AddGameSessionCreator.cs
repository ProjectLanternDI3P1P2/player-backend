using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "creator_player_id",
                table: "game_session",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_game_session_creator_player_id",
                table: "game_session",
                column: "creator_player_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_game_session_creator_player_id",
                table: "game_session"
            );

            migrationBuilder.DropColumn(name: "creator_player_id", table: "game_session");
        }
    }
}
