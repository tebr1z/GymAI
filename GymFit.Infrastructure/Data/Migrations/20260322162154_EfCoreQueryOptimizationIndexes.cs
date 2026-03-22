using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EfCoreQueryOptimizationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainer_profiles_IsApproved",
                table: "trainer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_trainer_profiles_Rating",
                table: "trainer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_trainer_orders_CreatedAt",
                table: "trainer_orders");

            migrationBuilder.DropIndex(
                name: "IX_trainer_orders_TrainerProfileId",
                table: "trainer_orders");

            migrationBuilder.DropIndex(
                name: "IX_trainer_orders_UserId",
                table: "trainer_orders");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_UserId_Status",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_plans_CreatedAt",
                table: "plans");

            migrationBuilder.DropIndex(
                name: "IX_plans_UserId",
                table: "plans");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_profiles_IsApproved_PricePerMonth",
                table: "trainer_profiles",
                columns: new[] { "IsApproved", "PricePerMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_trainer_profiles_IsApproved_Rating",
                table: "trainer_profiles",
                columns: new[] { "IsApproved", "Rating" });

            migrationBuilder.CreateIndex(
                name: "IX_trainer_orders_TrainerProfileId_CreatedAt",
                table: "trainer_orders",
                columns: new[] { "TrainerProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainer_orders_UserId_CreatedAt",
                table: "trainer_orders",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainer_orders_UserId_TrainerProfileId_Status",
                table: "trainer_orders",
                columns: new[] { "UserId", "TrainerProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_Status_EndDate",
                table: "subscriptions",
                columns: new[] { "UserId", "Status", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_Status_StartDate",
                table: "subscriptions",
                columns: new[] { "UserId", "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_plans_UserId_CreatedAt",
                table: "plans",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_trainer_profiles_IsApproved_PricePerMonth",
                table: "trainer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_trainer_profiles_IsApproved_Rating",
                table: "trainer_profiles");

            migrationBuilder.DropIndex(
                name: "IX_trainer_orders_TrainerProfileId_CreatedAt",
                table: "trainer_orders");

            migrationBuilder.DropIndex(
                name: "IX_trainer_orders_UserId_CreatedAt",
                table: "trainer_orders");

            migrationBuilder.DropIndex(
                name: "IX_trainer_orders_UserId_TrainerProfileId_Status",
                table: "trainer_orders");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_UserId_Status_EndDate",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_UserId_Status_StartDate",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_plans_UserId_CreatedAt",
                table: "plans");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_profiles_IsApproved",
                table: "trainer_profiles",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_profiles_Rating",
                table: "trainer_profiles",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_orders_CreatedAt",
                table: "trainer_orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_orders_TrainerProfileId",
                table: "trainer_orders",
                column: "TrainerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trainer_orders_UserId",
                table: "trainer_orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_Status",
                table: "subscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_plans_CreatedAt",
                table: "plans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_plans_UserId",
                table: "plans",
                column: "UserId");
        }
    }
}
