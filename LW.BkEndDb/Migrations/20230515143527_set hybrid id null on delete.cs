using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class sethybrididnullondelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConexiuniConturi_Hybrid_HybridId",
                table: "ConexiuniConturi");

            migrationBuilder.AddForeignKey(
                name: "FK_ConexiuniConturi_Hybrid_HybridId",
                table: "ConexiuniConturi",
                column: "HybridId",
                principalTable: "Hybrid",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConexiuniConturi_Hybrid_HybridId",
                table: "ConexiuniConturi");

            migrationBuilder.AddForeignKey(
                name: "FK_ConexiuniConturi_Hybrid_HybridId",
                table: "ConexiuniConturi",
                column: "HybridId",
                principalTable: "Hybrid",
                principalColumn: "Id");
        }
    }
}
