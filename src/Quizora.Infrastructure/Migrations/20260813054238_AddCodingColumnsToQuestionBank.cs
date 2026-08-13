using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodingColumnsToQuestionBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PassingPercent",
                table: "Tests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassingScore",
                table: "Tests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SampleInput",
                table: "QuestionBanks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SampleOutput",
                table: "QuestionBanks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StarterCode",
                table: "QuestionBanks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingPercent",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "SampleInput",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "SampleOutput",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "StarterCode",
                table: "QuestionBanks");
        }
    }
}
