using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveHourlyCreditsToTutorSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyCredits",
                table: "TutorProfiles");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyCredits",
                table: "TutorSubjects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyCredits",
                table: "TutorSubjects");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyCredits",
                table: "TutorProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
