using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalochiAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastPaperIdToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PastPaperId",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PastPaperId",
                table: "Questions",
                column: "PastPaperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_PastPapers_PastPaperId",
                table: "Questions",
                column: "PastPaperId",
                principalTable: "PastPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_PastPapers_PastPaperId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_PastPaperId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PastPaperId",
                table: "Questions");
        }
    }
}
