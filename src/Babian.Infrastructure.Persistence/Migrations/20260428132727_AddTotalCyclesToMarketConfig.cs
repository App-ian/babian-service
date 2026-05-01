using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Babian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalCyclesToMarketConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalCycles",
                table: "market_configs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCycles",
                table: "market_configs");
        }
    }
}
