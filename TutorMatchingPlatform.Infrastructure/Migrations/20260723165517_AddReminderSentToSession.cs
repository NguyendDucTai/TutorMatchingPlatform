using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorMatchingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderSentToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Sessions");
        }
    }
}
