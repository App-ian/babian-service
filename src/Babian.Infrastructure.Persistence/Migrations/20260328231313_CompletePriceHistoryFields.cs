using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Babian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompletePriceHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cycle_number",
                table: "price_histories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "variation",
                table: "price_histories",
                type: "numeric(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cycle_number",
                table: "price_histories");

            migrationBuilder.DropColumn(
                name: "variation",
                table: "price_histories");
        }
    }
}
