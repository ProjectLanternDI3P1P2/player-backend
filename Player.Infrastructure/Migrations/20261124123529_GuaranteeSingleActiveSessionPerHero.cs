using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations;

/// <summary>
/// Guarantees a single active session per hero with a persisted constraint: a hero can be an
/// active member of at most one session. Leaving a session (terminal state) releases the hero.
/// </summary>
public partial class GuaranteeSingleActiveSessionPerHero : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Repair: heroes still marked active in an already terminated session would block the
        // new constraint (and any future session) forever, so release them first.
        migrationBuilder.Sql(
            """
            UPDATE game_session_member AS member
            SET member_status = 'Left', left_at = COALESCE(member.left_at, session.ended_at, now())
            FROM game_session AS session
            WHERE member.session_id = session.id
              AND member.member_status = 'Active'
              AND session.status IN ('Failed', 'Completed', 'Abandoned');
            """
        );

        migrationBuilder.DropIndex(
            name: "IX_game_session_member_hero_id",
            table: "game_session_member"
        );

        migrationBuilder.CreateIndex(
            name: "IX_game_session_member_hero_id_active",
            table: "game_session_member",
            column: "hero_id",
            unique: true,
            filter: "member_status = 'Active'"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_game_session_member_hero_id_active",
            table: "game_session_member"
        );

        migrationBuilder.CreateIndex(
            name: "IX_game_session_member_hero_id",
            table: "game_session_member",
            column: "hero_id"
        );
    }
}
