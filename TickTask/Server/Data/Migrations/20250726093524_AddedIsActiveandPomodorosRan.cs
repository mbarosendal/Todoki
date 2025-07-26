using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    public partial class AddedIsActiveandPomodorosRan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isDone",
                table: "TaskItems",
                newName: "IsDone");

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveTask",
                table: "TaskItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PomodorosRanOnTask",
                table: "TaskItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActiveTask",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "PomodorosRanOnTask",
                table: "TaskItems");

            migrationBuilder.RenameColumn(
                name: "IsDone",
                table: "TaskItems",
                newName: "isDone");
        }
    }
}
