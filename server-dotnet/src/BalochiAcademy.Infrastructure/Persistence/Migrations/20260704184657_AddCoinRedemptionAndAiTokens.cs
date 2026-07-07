using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalochiAcademy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinRedemptionAndAiTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiTokenQuota",
                table: "UserSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiTokensUsedThisPeriod",
                table: "UserSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BonusAiTokens",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AiTokenQuota",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CoinRedemptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: true),
                    CoinsSpent = table.Column<int>(type: "int", nullable: false),
                    TokensGranted = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinRedemptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CoinRedemptions_UserSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "UserSubscriptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CoinRedemptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoinRedemptions_CreatedAt",
                table: "CoinRedemptions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CoinRedemptions_PlanId",
                table: "CoinRedemptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinRedemptions_SubscriptionId",
                table: "CoinRedemptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinRedemptions_UserId",
                table: "CoinRedemptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoinRedemptions");

            migrationBuilder.DropColumn(
                name: "AiTokenQuota",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "AiTokensUsedThisPeriod",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "BonusAiTokens",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AiTokenQuota",
                table: "SubscriptionPlans");
        }
    }
}
