using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyBudget.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramUsername",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramUsername",
                table: "Users");
        }
    }
}
