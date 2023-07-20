using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class doccascadeandaddedfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisiereDocumente_Documente_DocumenteId",
                table: "FisiereDocumente");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tranzactii",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameAnaf",
                table: "FirmaDiscount",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FisiereDocumente_Documente_DocumenteId",
                table: "FisiereDocumente",
                column: "DocumenteId",
                principalTable: "Documente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisiereDocumente_Documente_DocumenteId",
                table: "FisiereDocumente");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tranzactii");

            migrationBuilder.DropColumn(
                name: "NameAnaf",
                table: "FirmaDiscount");

            migrationBuilder.AddForeignKey(
                name: "FK_FisiereDocumente_Documente_DocumenteId",
                table: "FisiereDocumente",
                column: "DocumenteId",
                principalTable: "Documente",
                principalColumn: "Id");
        }
    }
}
