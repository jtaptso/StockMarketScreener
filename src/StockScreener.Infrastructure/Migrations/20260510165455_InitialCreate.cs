using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockScreener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilterPresets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilterJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exchange = table.Column<int>(type: "int", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DayHigh = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DayLow = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Week52High = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Week52Low = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Volume = table.Column<long>(type: "bigint", nullable: true),
                    AvgVolume = table.Column<long>(type: "bigint", nullable: true),
                    Beta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Watchlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watchlists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fundamentals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    PE_Ratio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PB_Ratio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PS_Ratio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EPS = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DividendYield = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExDividendDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DebtToEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QuickRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ROE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProfitMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OperatingMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrossMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RevenueGrowth = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EPSGrowth = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FiscalYearEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fundamentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fundamentals_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OpenPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HighPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LowPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistory_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalIndicators",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RSI_14 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SMA_20 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SMA_50 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SMA_200 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MACD = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MACD_Signal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MACD_Histogram = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ATR_14 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BB_Upper = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BB_Middle = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BB_Lower = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Volume = table.Column<long>(type: "bigint", nullable: true),
                    AvgVolume_20 = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalIndicators_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WatchlistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    SharesOwned = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Watchlists_WatchlistId",
                        column: x => x.WatchlistId,
                        principalTable: "Watchlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fundamentals_StockId",
                table: "Fundamentals",
                column: "StockId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_StockId",
                table: "PriceHistory",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalIndicators_StockId",
                table: "TechnicalIndicators",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_StockId",
                table: "WatchlistItems",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_WatchlistId",
                table: "WatchlistItems",
                column: "WatchlistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterPresets");

            migrationBuilder.DropTable(
                name: "Fundamentals");

            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "TechnicalIndicators");

            migrationBuilder.DropTable(
                name: "WatchlistItems");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Watchlists");
        }
    }
}
