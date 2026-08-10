using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MockTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionBanks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBanks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MockTestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    TimeTakenInSeconds = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockAttempts_MockTests_MockTestId",
                        column: x => x.MockTestId,
                        principalTable: "MockTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockTestQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MockTestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockTestQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockTestQuestions_MockTests_MockTestId",
                        column: x => x.MockTestId,
                        principalTable: "MockTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockTestQuestions_PracticeQuestions_PracticeQuestionId",
                        column: x => x.PracticeQuestionId,
                        principalTable: "PracticeQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionBankOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionBankOptions_QuestionBanks_QuestionBankId",
                        column: x => x.QuestionBankId,
                        principalTable: "QuestionBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockAttempts_MockTestId",
                table: "MockAttempts",
                column: "MockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MockAttempts_UserId",
                table: "MockAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestQuestions_MockTestId",
                table: "MockTestQuestions",
                column: "MockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestQuestions_PracticeQuestionId",
                table: "MockTestQuestions",
                column: "PracticeQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankOptions_QuestionBankId",
                table: "QuestionBankOptions",
                column: "QuestionBankId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockAttempts");

            migrationBuilder.DropTable(
                name: "MockTestQuestions");

            migrationBuilder.DropTable(
                name: "QuestionBankOptions");

            migrationBuilder.DropTable(
                name: "MockTests");

            migrationBuilder.DropTable(
                name: "QuestionBanks");
        }
    }
}
