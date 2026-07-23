using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorMatchingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionHeldAmountAndMilestoneCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HeldAmount",
                table: "Sessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CompletionPercentage",
                table: "LearningMilestones",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeldAmount",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CompletionPercentage",
                table: "LearningMilestones");
        }
    }
}
