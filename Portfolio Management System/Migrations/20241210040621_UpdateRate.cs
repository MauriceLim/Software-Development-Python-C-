using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonteCarloSimulatorAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Tenor",
                table: "Rates",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Tenor",
                table: "Rates",
                type: "text",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
