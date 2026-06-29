using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalochiAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookIdToUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "Units",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_BookId",
                table: "Units",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Books_BookId",
                table: "Units",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Books_BookId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_BookId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Units");
        }
    }
}
