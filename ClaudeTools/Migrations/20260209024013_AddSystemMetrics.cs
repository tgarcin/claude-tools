using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaudeTools.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CpuPercent = table.Column<double>(type: "REAL", nullable: false),
                    MemUsedMb = table.Column<double>(type: "REAL", nullable: false),
                    MemTotalMb = table.Column<double>(type: "REAL", nullable: false),
                    DiskUsedPercent = table.Column<double>(type: "REAL", nullable: false),
                    NetRxBytesPerSec = table.Column<double>(type: "REAL", nullable: false),
                    NetTxBytesPerSec = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_Timestamp",
                table: "SystemMetrics",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemMetrics");
        }
    }
}
