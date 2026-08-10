using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyBudget.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeExternalLogins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderSubject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderUsername = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLogins_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO "Accounts" ("Id", "DisplayName", "CreatedAtUtc", "DisabledAtUtc")
                SELECT "Id", "Name", CURRENT_TIMESTAMP, NULL
                FROM "Users"
                ON CONFLICT ("Id") DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "ExternalLogins"
                    ("Id", "AccountId", "Provider", "ProviderSubject", "ProviderUsername", "CreatedAtUtc", "LastSeenAtUtc")
                SELECT
                    "Id",
                    "Id",
                    'Telegram',
                    CAST("TelegramId" AS text),
                    "TelegramUsername",
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                FROM "Users";
                """);

            migrationBuilder.DropIndex(
                name: "IX_Users_TelegramId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramUsername",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_AccountId_Provider",
                table: "ExternalLogins",
                columns: new[] { "AccountId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_Provider_ProviderSubject",
                table: "ExternalLogins",
                columns: new[] { "Provider", "ProviderSubject" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Accounts_Id",
                table: "Users",
                column: "Id",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Users" AS users
                        WHERE NOT EXISTS (
                            SELECT 1
                            FROM "ExternalLogins" AS logins
                            WHERE logins."AccountId" = users."Id"
                              AND logins."Provider" = 'Telegram')) THEN
                        RAISE EXCEPTION
                            'Cannot downgrade: every user must have a Telegram external login.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "ExternalLogins" AS logins
                        WHERE logins."Provider" = 'Telegram'
                          AND CASE
                              WHEN logins."ProviderSubject" ~ '^[0-9]+$'
                              THEN CAST(logins."ProviderSubject" AS numeric)
                                   NOT BETWEEN 1 AND 9223372036854775807
                              ELSE TRUE
                          END) THEN
                        RAISE EXCEPTION
                            'Cannot downgrade: a Telegram subject is not a valid positive bigint.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "ExternalLogins" AS logins
                        WHERE logins."Provider" = 'Telegram'
                          AND length(logins."ProviderUsername") > 32) THEN
                        RAISE EXCEPTION
                            'Cannot downgrade: a Telegram username exceeds the legacy 32-character limit.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Accounts_Id",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "TelegramId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramUsername",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Users" AS users
                SET
                    "TelegramId" = CAST(logins."ProviderSubject" AS bigint),
                    "TelegramUsername" = logins."ProviderUsername"
                FROM "ExternalLogins" AS logins
                WHERE logins."AccountId" = users."Id"
                  AND logins."Provider" = 'Telegram';
                """);

            migrationBuilder.AlterColumn<long>(
                name: "TelegramId",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                table: "Users",
                column: "TelegramId",
                unique: true);

            migrationBuilder.DropTable(
                name: "ExternalLogins");
        }
    }
}
