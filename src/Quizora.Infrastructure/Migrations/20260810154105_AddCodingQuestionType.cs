using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodingQuestionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SampleInput",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SampleOutput",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StarterCode",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "SampleInput",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SampleOutput",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "StarterCode",
                table: "Questions");
        }
    }
}
