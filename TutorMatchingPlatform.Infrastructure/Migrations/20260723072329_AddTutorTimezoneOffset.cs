using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorMatchingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorTimezoneOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimezoneOffset",
                table: "TutorProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimezoneOffset",
                table: "TutorProfiles");
        }
    }
}
