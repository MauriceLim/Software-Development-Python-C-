using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonteCarloSimulatorAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "CurrentPrice",
                table: "Trades",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "CurrentPrice",
                table: "Trades",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
