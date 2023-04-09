using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class updateef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DataProcDocsId",
                table: "FisiereDocumente",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DataProcDocs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsInvoice = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ReceiptId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtractedBusinessData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtractedBusinessAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uploaded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirmaDiscountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConexId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcDocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcDocs_ConexiuniConturi_ConexId",
                        column: x => x.ConexId,
                        principalTable: "ConexiuniConturi",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataProcDocs_FirmaDiscount_FirmaDiscountId",
                        column: x => x.FirmaDiscountId,
                        principalTable: "FirmaDiscount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FisiereDocumente_DataProcDocsId",
                table: "FisiereDocumente",
                column: "DataProcDocsId",
                unique: true,
                filter: "[DataProcDocsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcDocs_ConexId",
                table: "DataProcDocs",
                column: "ConexId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcDocs_FirmaDiscountId",
                table: "DataProcDocs",
                column: "FirmaDiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_FisiereDocumente_DataProcDocs_DataProcDocsId",
                table: "FisiereDocumente",
                column: "DataProcDocsId",
                principalTable: "DataProcDocs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisiereDocumente_DataProcDocs_DataProcDocsId",
                table: "FisiereDocumente");

            migrationBuilder.DropTable(
                name: "DataProcDocs");

            migrationBuilder.DropIndex(
                name: "IX_FisiereDocumente_DataProcDocsId",
                table: "FisiereDocumente");

            migrationBuilder.DropColumn(
                name: "DataProcDocsId",
                table: "FisiereDocumente");
        }
    }
}
