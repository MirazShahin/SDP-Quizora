using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CodingStandalone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodingSubmissions_TestInvitations_InvitationId",
                table: "CodingSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCodingProblems_CodingProblems_CodingProblemId",
                table: "TestCodingProblems");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCodingProblems_Tests_TestId",
                table: "TestCodingProblems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestCodingProblems",
                table: "TestCodingProblems");

            migrationBuilder.RenameTable(
                name: "TestCodingProblems",
                newName: "TestCodingProblem");

            migrationBuilder.RenameColumn(
                name: "InvitationId",
                table: "CodingSubmissions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CodingSubmissions_InvitationId_CodingProblemId",
                table: "CodingSubmissions",
                newName: "IX_CodingSubmissions_UserId_CodingProblemId");

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
                name: "FK_CodingSubmissions_Users_UserId",
                table: "CodingSubmissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodingSubmissions_Users_UserId",
                table: "CodingSubmissions");

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

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CodingSubmissions",
                newName: "InvitationId");

            migrationBuilder.RenameIndex(
                name: "IX_CodingSubmissions_UserId_CodingProblemId",
                table: "CodingSubmissions",
                newName: "IX_CodingSubmissions_InvitationId_CodingProblemId");

            migrationBuilder.RenameIndex(
                name: "IX_TestCodingProblem_TestId_CodingProblemId",
                table: "TestCodingProblems",
                newName: "IX_TestCodingProblems_TestId_CodingProblemId");

            migrationBuilder.RenameIndex(
                name: "IX_TestCodingProblem_CodingProblemId",
                table: "TestCodingProblems",
                newName: "IX_TestCodingProblems_CodingProblemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestCodingProblems",
                table: "TestCodingProblems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodingSubmissions_TestInvitations_InvitationId",
                table: "CodingSubmissions",
                column: "InvitationId",
                principalTable: "TestInvitations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
