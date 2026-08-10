using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeAndInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PracticeCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewQAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewQAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewQAs_InterviewTopics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "InterviewTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeAttempts_PracticeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PracticeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticeAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeQuestions_PracticeCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PracticeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeOptions_PracticeQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "PracticeQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewQAs_TopicId",
                table: "InterviewQAs",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeAttempts_CategoryId",
                table: "PracticeAttempts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeAttempts_UserId",
                table: "PracticeAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeOptions_QuestionId",
                table: "PracticeOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeQuestions_CategoryId",
                table: "PracticeQuestions",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewQAs");

            migrationBuilder.DropTable(
                name: "PracticeAttempts");

            migrationBuilder.DropTable(
                name: "PracticeOptions");

            migrationBuilder.DropTable(
                name: "InterviewTopics");

            migrationBuilder.DropTable(
                name: "PracticeQuestions");

            migrationBuilder.DropTable(
                name: "PracticeCategories");
        }
    }
}
