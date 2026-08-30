using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContestAndTestCodingProblem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContestId",
                table: "CodingSubmissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContestRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestRegistrations_Tests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestRegistrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestRegistrations_ContestId_UserId",
                table: "ContestRegistrations",
                columns: new[] { "ContestId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestRegistrations_UserId",
                table: "ContestRegistrations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContestRegistrations");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "CodingSubmissions");
        }
    }
}
