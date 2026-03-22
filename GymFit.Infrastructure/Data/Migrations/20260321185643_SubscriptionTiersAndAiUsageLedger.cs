using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionTiersAndAiUsageLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscriptions_UserId_PlanType",
                table: "subscriptions");

            migrationBuilder.RenameColumn(
                name: "PlanType",
                table: "subscriptions",
                newName: "Tier");

            migrationBuilder.AddColumn<string>(
                name: "ExternalCustomerId",
                table: "subscriptions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalProvider",
                table: "subscriptions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSubscriptionId",
                table: "subscriptions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "subscriptions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.Sql("""
                UPDATE subscriptions SET "Tier" = CASE
                    WHEN LOWER("Tier") LIKE '%premium%' THEN 'Premium'
                    WHEN LOWER("Tier") LIKE '%pro%' THEN 'Pro'
                    ELSE 'Free' END
                """);

            migrationBuilder.CreateTable(
                name: "ai_usage_ledgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodKey = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    RequestCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_usage_ledgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_usage_ledgers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_ExternalProvider_ExternalSubscriptionId",
                table: "subscriptions",
                columns: new[] { "ExternalProvider", "ExternalSubscriptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_Status",
                table: "subscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_ledgers_UserId",
                table: "ai_usage_ledgers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_ledgers_UserId_PeriodKey",
                table: "ai_usage_ledgers",
                columns: new[] { "UserId", "PeriodKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_usage_ledgers");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_ExternalProvider_ExternalSubscriptionId",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_UserId_Status",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ExternalCustomerId",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ExternalProvider",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "ExternalSubscriptionId",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "subscriptions");

            migrationBuilder.RenameColumn(
                name: "Tier",
                table: "subscriptions",
                newName: "PlanType");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_PlanType",
                table: "subscriptions",
                columns: new[] { "UserId", "PlanType" });
        }
    }
}
