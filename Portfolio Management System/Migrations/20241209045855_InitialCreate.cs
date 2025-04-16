using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonteCarloSimulatorAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Curves",
                columns: table => new
                {
                    CurveID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curves", x => x.CurveID);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    ExchangeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.ExchangeID);
                });

            migrationBuilder.CreateTable(
                name: "Rates",
                columns: table => new
                {
                    RateID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurveID = table.Column<int>(type: "integer", nullable: false),
                    Tenor = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => x.RateID);
                    table.ForeignKey(
                        name: "FK_Rates_Curves_CurveID",
                        column: x => x.CurveID,
                        principalTable: "Curves",
                        principalColumn: "CurveID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    MarketID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExchangeID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.MarketID);
                    table.ForeignKey(
                        name: "FK_Markets_Exchanges_ExchangeID",
                        column: x => x.ExchangeID,
                        principalTable: "Exchanges",
                        principalColumn: "ExchangeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Underlyings",
                columns: table => new
                {
                    UnderlyingID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketID = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Underlyings", x => x.UnderlyingID);
                    table.ForeignKey(
                        name: "FK_Underlyings_Markets_MarketID",
                        column: x => x.MarketID,
                        principalTable: "Markets",
                        principalColumn: "MarketID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Derivatives",
                columns: table => new
                {
                    DerivativeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnderlyingID = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    StrikePrice = table.Column<double>(type: "double precision", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCall = table.Column<bool>(type: "boolean", nullable: true),
                    BarrierLevel = table.Column<double>(type: "double precision", nullable: true),
                    BarrierType = table.Column<string>(type: "text", nullable: true),
                    Payout = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Derivatives", x => x.DerivativeID);
                    table.ForeignKey(
                        name: "FK_Derivatives_Underlyings_UnderlyingID",
                        column: x => x.UnderlyingID,
                        principalTable: "Underlyings",
                        principalColumn: "UnderlyingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    PriceID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UnderlyingID = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpenPrice = table.Column<double>(type: "double precision", nullable: false),
                    ClosePrice = table.Column<double>(type: "double precision", nullable: false),
                    HighPrice = table.Column<double>(type: "double precision", nullable: false),
                    LowPrice = table.Column<double>(type: "double precision", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.PriceID);
                    table.ForeignKey(
                        name: "FK_Prices_Underlyings_UnderlyingID",
                        column: x => x.UnderlyingID,
                        principalTable: "Underlyings",
                        principalColumn: "UnderlyingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    TradeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DerivativeID = table.Column<int>(type: "integer", nullable: true),
                    UnderlyingID = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.TradeID);
                    table.ForeignKey(
                        name: "FK_Trades_Derivatives_DerivativeID",
                        column: x => x.DerivativeID,
                        principalTable: "Derivatives",
                        principalColumn: "DerivativeID");
                    table.ForeignKey(
                        name: "FK_Trades_Underlyings_UnderlyingID",
                        column: x => x.UnderlyingID,
                        principalTable: "Underlyings",
                        principalColumn: "UnderlyingID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Derivatives_UnderlyingID",
                table: "Derivatives",
                column: "UnderlyingID");

            migrationBuilder.CreateIndex(
                name: "IX_Markets_ExchangeID",
                table: "Markets",
                column: "ExchangeID");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_UnderlyingID",
                table: "Prices",
                column: "UnderlyingID");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_CurveID",
                table: "Rates",
                column: "CurveID");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_DerivativeID",
                table: "Trades",
                column: "DerivativeID");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_UnderlyingID",
                table: "Trades",
                column: "UnderlyingID");

            migrationBuilder.CreateIndex(
                name: "IX_Underlyings_MarketID",
                table: "Underlyings",
                column: "MarketID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "Rates");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Curves");

            migrationBuilder.DropTable(
                name: "Derivatives");

            migrationBuilder.DropTable(
                name: "Underlyings");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropTable(
                name: "Exchanges");
        }
    }
}
