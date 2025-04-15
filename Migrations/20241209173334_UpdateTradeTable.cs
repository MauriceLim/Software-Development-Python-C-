using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonteCarloSimulatorAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTradeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Trades",
                newName: "Vega");

            migrationBuilder.AddColumn<double>(
                name: "CurrentPrice",
                table: "Trades",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Delta",
                table: "Trades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Gamma",
                table: "Trades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Theta",
                table: "Trades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TradePrice",
                table: "Trades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Delta",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Gamma",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Theta",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "TradePrice",
                table: "Trades");

            migrationBuilder.RenameColumn(
                name: "Vega",
                table: "Trades",
                newName: "Price");
        }
    }
}
