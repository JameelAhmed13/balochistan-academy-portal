using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalochiAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintMessages_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMessages_ComplaintId",
                table: "ComplaintMessages",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMessages_SenderId",
                table: "ComplaintMessages",
                column: "SenderId");

            // Backfill: every existing single AdminReply becomes the first admin message in that
            // complaint's new thread, before the column is dropped below. Skips the handful of
            // theoretically-possible rows with neither a handler nor an author to attribute it to.
            migrationBuilder.Sql(@"
                INSERT INTO [ComplaintMessages] ([ComplaintId], [SenderId], [IsAdmin], [Message], [CreatedAt])
                SELECT [Id], COALESCE([HandledById], [UserId]), 1, [AdminReply], COALESCE([ResolvedAt], [CreatedAt])
                FROM [Complaints]
                WHERE [AdminReply] IS NOT NULL AND [AdminReply] <> ''
                  AND (COALESCE([HandledById], [UserId])) IS NOT NULL
            ");

            migrationBuilder.DropColumn(
                name: "AdminReply",
                table: "Complaints");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintMessages");

            migrationBuilder.AddColumn<string>(
                name: "AdminReply",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
