using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShortAnswerSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "QuestionBanks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "QuestionBanks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SampleAnswer",
                table: "QuestionBanks",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedOptionId",
                table: "Answers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "AnswerText",
                table: "Answers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "SampleAnswer",
                table: "QuestionBanks");

            migrationBuilder.DropColumn(
                name: "AnswerText",
                table: "Answers");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedOptionId",
                table: "Answers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
