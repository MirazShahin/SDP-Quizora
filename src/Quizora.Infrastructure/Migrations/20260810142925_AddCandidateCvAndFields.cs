using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quizora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateCvAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CvFileSize",
                table: "Candidates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvOriginalName",
                table: "Candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvStoredName",
                table: "Candidates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CvUploadedAt",
                table: "Candidates",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvFileSize",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CvOriginalName",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CvStoredName",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "CvUploadedAt",
                table: "Candidates");
        }
    }
}
