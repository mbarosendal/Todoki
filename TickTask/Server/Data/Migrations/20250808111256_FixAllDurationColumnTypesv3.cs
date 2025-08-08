using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    public partial class FixAllDurationColumnTypesv3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old int columns
            migrationBuilder.DropColumn(
                name: "PomodoroDurationMinutes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "LongBreakDurationMinutes",
                table: "UserSettings");

            // Add new columns with type 'time' (mapped to TimeSpan)
            migrationBuilder.AddColumn<TimeSpan>(
                name: "PomodoroDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.Zero);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.Zero);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "LongBreakDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.Zero);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the TimeSpan columns
            migrationBuilder.DropColumn(
                name: "PomodoroDurationMinutes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "LongBreakDurationMinutes",
                table: "UserSettings");

            // Recreate the old int columns
            migrationBuilder.AddColumn<int>(
                name: "PomodoroDurationMinutes",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LongBreakDurationMinutes",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
