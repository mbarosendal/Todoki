using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    public partial class FixAllDurationColumnTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old int columns
            migrationBuilder.DropColumn(name: "PomodoroDurationMinutes", table: "UserSettings");
            migrationBuilder.DropColumn(name: "ShortBreakDurationMinutes", table: "UserSettings");
            migrationBuilder.DropColumn(name: "LongBreakDurationMinutes", table: "UserSettings");

            // Add new TimeSpan (time) columns
            migrationBuilder.AddColumn<TimeSpan>(
                name: "PomodoroDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.FromMinutes(25));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.FromMinutes(5));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "LongBreakDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.FromMinutes(15));
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
