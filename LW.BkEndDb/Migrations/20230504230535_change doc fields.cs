using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class changedocfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocNumber",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "ExtractedBusinessAddress",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "ExtractedBusinessData",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Documente");

            migrationBuilder.RenameColumn(
                name: "ReceiptId",
                table: "Documente",
                newName: "OcrDataJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OcrDataJson",
                table: "Documente",
                newName: "ReceiptId");

            migrationBuilder.AddColumn<string>(
                name: "DocNumber",
                table: "Documente",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedBusinessAddress",
                table: "Documente",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedBusinessData",
                table: "Documente",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Documente",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
