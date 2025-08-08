using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    public partial class RenamedLabelToTextInUserSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortBreakLabel",
                table: "UserSettings",
                newName: "ShortBreakText");

            migrationBuilder.RenameColumn(
                name: "PomodoroLabel",
                table: "UserSettings",
                newName: "PomodoroText");

            migrationBuilder.RenameColumn(
                name: "LongBreakLabel",
                table: "UserSettings",
                newName: "LongBreakText");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortBreakText",
                table: "UserSettings",
                newName: "ShortBreakLabel");

            migrationBuilder.RenameColumn(
                name: "PomodoroText",
                table: "UserSettings",
                newName: "PomodoroLabel");

            migrationBuilder.RenameColumn(
                name: "LongBreakText",
                table: "UserSettings",
                newName: "LongBreakLabel");
        }
    }
}
