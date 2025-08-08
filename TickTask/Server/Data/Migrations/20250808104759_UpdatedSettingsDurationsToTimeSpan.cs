using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    public partial class UpdatedSettingsDurationsToTimeSpan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings");

            migrationBuilder.AddColumn<int>(
                name: "ShortBreakDurationMinutes",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

    }
}
