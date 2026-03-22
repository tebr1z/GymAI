using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameMemberRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """UPDATE users SET "Role" = 'User' WHERE "Role" = 'Member';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """UPDATE users SET "Role" = 'Member' WHERE "Role" = 'User';""");
        }
    }
}
