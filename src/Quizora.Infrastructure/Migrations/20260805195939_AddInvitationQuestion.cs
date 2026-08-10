using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsageType",
                table: "QuestionBanks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InvitationQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationQuestions_QuestionBanks_QuestionBankId",
                        column: x => x.QuestionBankId,
                        principalTable: "QuestionBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvitationQuestions_TestInvitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "TestInvitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvitationQuestions_InvitationId",
                table: "InvitationQuestions",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationQuestions_QuestionBankId",
                table: "InvitationQuestions",
                column: "QuestionBankId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvitationQuestions");

            migrationBuilder.DropColumn(
                name: "UsageType",
                table: "QuestionBanks");
        }
    }
}
