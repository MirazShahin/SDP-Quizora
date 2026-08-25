using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCodingProblem_CodingProblems_CodingProblemId",
                table: "TestCodingProblem");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCodingProblem_Tests_TestId",
                table: "TestCodingProblem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCodingProblem",
                table: "TestCodingProblem");

            migrationBuilder.RenameTable(
                name: "TestCodingProblem",
                newName: "TestCodingProblems");

            migrationBuilder.RenameIndex(
                name: "IX_TestCodingProblem_TestId_CodingProblemId",
                table: "TestCodingProblems",
                newName: "IX_TestCodingProblems_TestId_CodingProblemId");

            migrationBuilder.RenameIndex(
                name: "IX_TestCodingProblem_CodingProblemId",
                table: "TestCodingProblems",
                newName: "IX_TestCodingProblems_CodingProblemId");

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContestEndAt",
                table: "Tests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContestStartAt",
                table: "Tests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsContest",
                table: "Tests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Tests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCodingProblems",
                table: "TestCodingProblems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCodingProblems_CodingProblems_CodingProblemId",
                table: "TestCodingProblems",
                column: "CodingProblemId",
                principalTable: "CodingProblems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestCodingProblems_Tests_TestId",
                table: "TestCodingProblems",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCodingProblems_CodingProblems_CodingProblemId",
                table: "TestCodingProblems");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCodingProblems_Tests_TestId",
                table: "TestCodingProblems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCodingProblems",
                table: "TestCodingProblems");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContestEndAt",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "ContestStartAt",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "IsContest",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Tests");

            migrationBuilder.RenameTable(
                name: "TestCodingProblems",
                newName: "TestCodingProblem");

            migrationBuilder.RenameIndex(
                name: "IX_TestCodingProblems_TestId_CodingProblemId",
                table: "TestCodingProblem",
                newName: "IX_TestCodingProblem_TestId_CodingProblemId");

            migrationBuilder.RenameIndex(
                name: "IX_TestCodingProblems_CodingProblemId",
                table: "TestCodingProblem",
                newName: "IX_TestCodingProblem_CodingProblemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCodingProblem",
                table: "TestCodingProblem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCodingProblem_CodingProblems_CodingProblemId",
                table: "TestCodingProblem",
                column: "CodingProblemId",
                principalTable: "CodingProblems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestCodingProblem_Tests_TestId",
                table: "TestCodingProblem",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
