using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContestCoding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodingSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodingProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SourceCode = table.Column<string>(type: "text", nullable: false),
                    Verdict = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PassedCount = table.Column<int>(type: "integer", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    MaxTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    CompileOutput = table.Column<string>(type: "text", nullable: true),
                    DetailJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodingSubmissions_CodingProblems_CodingProblemId",
                        column: x => x.CodingProblemId,
                        principalTable: "CodingProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CodingSubmissions_TestInvitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "TestInvitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCodingProblems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodingProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCodingProblems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCodingProblems_CodingProblems_CodingProblemId",
                        column: x => x.CodingProblemId,
                        principalTable: "CodingProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestCodingProblems_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodingSubmissions_CodingProblemId",
                table: "CodingSubmissions",
                column: "CodingProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_CodingSubmissions_InvitationId_CodingProblemId",
                table: "CodingSubmissions",
                columns: new[] { "InvitationId", "CodingProblemId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestCodingProblems_CodingProblemId",
                table: "TestCodingProblems",
                column: "CodingProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCodingProblems_TestId_CodingProblemId",
                table: "TestCodingProblems",
                columns: new[] { "TestId", "CodingProblemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodingSubmissions");

            migrationBuilder.DropTable(
                name: "TestCodingProblems");
        }
    }
}
