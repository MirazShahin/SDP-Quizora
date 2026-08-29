using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttemptAntiCheatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CopyAttempts",
                table: "TestAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FocusLost",
                table: "TestAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PasteAttempts",
                table: "TestAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TabSwitches",
                table: "TestAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CopyAttempts",
                table: "TestAttempts");

            migrationBuilder.DropColumn(
                name: "FocusLost",
                table: "TestAttempts");

            migrationBuilder.DropColumn(
                name: "PasteAttempts",
                table: "TestAttempts");

            migrationBuilder.DropColumn(
                name: "TabSwitches",
                table: "TestAttempts");
        }
    }
}
