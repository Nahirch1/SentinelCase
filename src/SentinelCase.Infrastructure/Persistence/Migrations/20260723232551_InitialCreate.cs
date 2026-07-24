using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelCase.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_CreatedAt",
                table: "SecurityIncidents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Severity",
                table: "SecurityIncidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Status",
                table: "SecurityIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Title",
                table: "SecurityIncidents",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityIncidents");
        }
    }
}
