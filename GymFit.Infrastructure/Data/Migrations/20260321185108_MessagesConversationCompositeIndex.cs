using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MessagesConversationCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_messages_SenderId_ReceiverId_CreatedAt",
                table: "messages",
                columns: new[] { "SenderId", "ReceiverId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_SenderId_ReceiverId_CreatedAt",
                table: "messages");
        }
    }
}
