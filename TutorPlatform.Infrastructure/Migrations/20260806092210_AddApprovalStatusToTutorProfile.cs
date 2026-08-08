using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStatusToTutorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "TutorProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "TutorProfiles");
        }
    }
}
