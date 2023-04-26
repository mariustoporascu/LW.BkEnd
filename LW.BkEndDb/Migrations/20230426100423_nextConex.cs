using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class nextConex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Documente_NextConexId",
                table: "Documente",
                column: "NextConexId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_ConexiuniConturi_NextConexId",
                table: "Documente",
                column: "NextConexId",
                principalTable: "ConexiuniConturi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documente_ConexiuniConturi_NextConexId",
                table: "Documente");

            migrationBuilder.DropIndex(
                name: "IX_Documente_NextConexId",
                table: "Documente");
        }
    }
}
