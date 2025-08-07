using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TickTask.Server.Data.Migrations
{
    public partial class FixUserSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PomodoroDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ShortBreakDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    LongBreakDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    PomodoroLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortBreakLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LongBreakLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HideTasks = table.Column<bool>(type: "bit", nullable: false),
                    HideActiveTask = table.Column<bool>(type: "bit", nullable: false),
                    IsAutoStart = table.Column<bool>(type: "bit", nullable: false),
                    IsAutoStartAfterRestart = table.Column<bool>(type: "bit", nullable: false),
                    AutomaticallyMarkDoneTasks = table.Column<bool>(type: "bit", nullable: false),
                    AutomaticallyProceedToNextTaskAfterDone = table.Column<bool>(type: "bit", nullable: false),
                    AutomaticallyClearDoneTasks = table.Column<bool>(type: "bit", nullable: false),
                    EnableNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfPomodorosRun = table.Column<int>(type: "int", nullable: false),
                    RunsBeforeLongBreak = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.UserSettingsId);
                    table.ForeignKey(
                        name: "FK_UserSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
