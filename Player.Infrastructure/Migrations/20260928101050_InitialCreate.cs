using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_class",
                columns: table => new
                {
                    code = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    label = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    base_health = table.Column<int>(type: "integer", nullable: false),
                    base_mana = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_class", x => x.code);
                }
            );

            migrationBuilder.CreateTable(
                name: "matchmaking_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    min_new_game_plus_level = table.Column<int>(type: "integer", nullable: false),
                    max_new_game_plus_level = table.Column<int>(type: "integer", nullable: false),
                    formed_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchmaking_group", x => x.id);
                    table.CheckConstraint(
                        "ck_matchmaking_group_level_range",
                        "min_new_game_plus_level <= max_new_game_plus_level"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "player",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    account_status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    new_game_plus_level = table.Column<int>(type: "integer", nullable: false),
                    new_game_plus_updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    anonymized_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "skill",
                columns: table => new
                {
                    code = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    class_code = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    label = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    required_level = table.Column<int>(type: "integer", nullable: false),
                    targeting_type = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill", x => x.code);
                    table.ForeignKey(
                        name: "FK_skill_hero_class_class_code",
                        column: x => x.class_code,
                        principalTable: "hero_class",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "game_session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    matchmaking_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    mode = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    dungeon_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dungeon_seed = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    progression_recorded = table.Column<bool>(type: "boolean", nullable: false),
                    termination_reason = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    started_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_active_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ended_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_session_matchmaking_group_matchmaking_group_id",
                        column: x => x.matchmaking_group_id,
                        principalTable: "matchmaking_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "hero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_code = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    level = table.Column<int>(type: "integer", nullable: false),
                    strength = table.Column<int>(type: "integer", nullable: false),
                    endurance = table.Column<int>(type: "integer", nullable: false),
                    agility = table.Column<int>(type: "integer", nullable: false),
                    intelligence = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    deleted_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero", x => x.id);
                    table.ForeignKey(
                        name: "FK_hero_hero_class_class_code",
                        column: x => x.class_code,
                        principalTable: "hero_class",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_hero_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "idempotency_key",
                columns: table => new
                {
                    key = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    request_fingerprint = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: false
                    ),
                    produced_resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_key", x => x.key);
                    table.ForeignKey(
                        name: "FK_idempotency_key_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "game_session_transition",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    target_status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    cause = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    actor = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    occurred_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_session_transition", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_session_transition_game_session_session_id",
                        column: x => x.session_id,
                        principalTable: "game_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "game_session_member",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    joined_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    left_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_game_session_member",
                        x => new { x.session_id, x.hero_id }
                    );
                    table.ForeignKey(
                        name: "FK_game_session_member_game_session_session_id",
                        column: x => x.session_id,
                        principalTable: "game_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_game_session_member_hero_hero_id",
                        column: x => x.hero_id,
                        principalTable: "hero",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "hero_skill",
                columns: table => new
                {
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_code = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    unlocked_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_skill", x => new { x.hero_id, x.skill_code });
                    table.ForeignKey(
                        name: "FK_hero_skill_hero_hero_id",
                        column: x => x.hero_id,
                        principalTable: "hero",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_hero_skill_skill_skill_code",
                        column: x => x.skill_code,
                        principalTable: "skill",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "matchmaking_queue_entry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    new_game_plus_level_at_entry = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    entered_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    left_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchmaking_queue_entry", x => x.id);
                    table.ForeignKey(
                        name: "FK_matchmaking_queue_entry_hero_hero_id",
                        column: x => x.hero_id,
                        principalTable: "hero",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_matchmaking_queue_entry_matchmaking_group_group_id",
                        column: x => x.group_id,
                        principalTable: "matchmaking_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_matchmaking_queue_entry_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_game_session_matchmaking_group_id",
                table: "game_session",
                column: "matchmaking_group_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_game_session_member_hero_id",
                table: "game_session_member",
                column: "hero_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_game_session_transition_session_id",
                table: "game_session_transition",
                column: "session_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_hero_class_code",
                table: "hero",
                column: "class_code"
            );

            migrationBuilder.CreateIndex(
                name: "IX_hero_player_id_name",
                table: "hero",
                columns: new[] { "player_id", "name" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_hero_skill_skill_code",
                table: "hero_skill",
                column: "skill_code"
            );

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_key_player_id_scope_key",
                table: "idempotency_key",
                columns: new[] { "player_id", "scope", "key" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entry_group_id",
                table: "matchmaking_queue_entry",
                column: "group_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entry_hero_id",
                table: "matchmaking_queue_entry",
                column: "hero_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_queue_entry_player_id",
                table: "matchmaking_queue_entry",
                column: "player_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_skill_class_code",
                table: "skill",
                column: "class_code"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "game_session_member");

            migrationBuilder.DropTable(name: "game_session_transition");

            migrationBuilder.DropTable(name: "hero_skill");

            migrationBuilder.DropTable(name: "idempotency_key");

            migrationBuilder.DropTable(name: "matchmaking_queue_entry");

            migrationBuilder.DropTable(name: "game_session");

            migrationBuilder.DropTable(name: "skill");

            migrationBuilder.DropTable(name: "hero");

            migrationBuilder.DropTable(name: "matchmaking_group");

            migrationBuilder.DropTable(name: "hero_class");

            migrationBuilder.DropTable(name: "player");
        }
    }
}
