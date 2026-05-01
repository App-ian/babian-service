using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Babian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurrenceAndParentIdToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrenceRule",
                table: "market_events");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentEventId",
                table: "market_events",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Recurrence",
                table: "market_events",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentEventId",
                table: "market_events");

            migrationBuilder.DropColumn(
                name: "Recurrence",
                table: "market_events");

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceRule",
                table: "market_events",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
