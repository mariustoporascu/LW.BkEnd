using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class setnullccondelusr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConexiuniConturi_AspNetUsers_UserId",
                table: "ConexiuniConturi");

            migrationBuilder.AddForeignKey(
                name: "FK_ConexiuniConturi_AspNetUsers_UserId",
                table: "ConexiuniConturi",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConexiuniConturi_AspNetUsers_UserId",
                table: "ConexiuniConturi");

            migrationBuilder.AddForeignKey(
                name: "FK_ConexiuniConturi_AspNetUsers_UserId",
                table: "ConexiuniConturi",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
