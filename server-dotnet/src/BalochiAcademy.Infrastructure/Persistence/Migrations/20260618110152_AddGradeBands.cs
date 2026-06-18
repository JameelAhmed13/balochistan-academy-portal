using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalochiAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeBands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradeBands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeBands", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GradeBands_Name",
                table: "GradeBands",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GradeBands");
        }
    }
}
