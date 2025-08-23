using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rollbackv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "Projects");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
