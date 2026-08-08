using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultMeetingLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultMeetingLink",
                table: "TutorProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultMeetingLink",
                table: "TutorProfiles");
        }
    }
}
