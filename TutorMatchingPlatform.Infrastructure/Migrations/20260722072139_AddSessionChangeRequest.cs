using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorMatchingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionChangeRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionChangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProposedStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProposedEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionChangeRequests_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionChangeRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionChangeRequests_RequesterId",
                table: "SessionChangeRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionChangeRequests_SessionId",
                table: "SessionChangeRequests",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionChangeRequests");
        }
    }
}
