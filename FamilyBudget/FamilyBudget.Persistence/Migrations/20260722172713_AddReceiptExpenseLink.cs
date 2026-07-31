using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyBudget.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptExpenseLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseId",
                table: "Receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ExpenseId",
                table: "Receipts",
                column: "ExpenseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Expenses_ExpenseId",
                table: "Receipts",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Expenses_ExpenseId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_ExpenseId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "Receipts");
        }
    }
}
