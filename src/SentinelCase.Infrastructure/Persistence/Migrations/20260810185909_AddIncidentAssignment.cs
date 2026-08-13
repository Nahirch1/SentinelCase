using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelCase.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AssignedAt",
                table: "SecurityIncidents",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "SecurityIncidents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_AssignedTo",
                table: "SecurityIncidents",
                column: "AssignedTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SecurityIncidents_AssignedTo",
                table: "SecurityIncidents");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "SecurityIncidents");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "SecurityIncidents");
        }
    }
}
