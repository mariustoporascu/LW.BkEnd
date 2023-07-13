using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class preflistchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MyConexId",
                table: "PreferinteHybrid",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreferinteHybrid_MyConexId",
                table: "PreferinteHybrid",
                column: "MyConexId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreferinteHybrid_ConexiuniConturi_MyConexId",
                table: "PreferinteHybrid",
                column: "MyConexId",
                principalTable: "ConexiuniConturi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreferinteHybrid_ConexiuniConturi_MyConexId",
                table: "PreferinteHybrid");

            migrationBuilder.DropIndex(
                name: "IX_PreferinteHybrid_MyConexId",
                table: "PreferinteHybrid");

            migrationBuilder.DropColumn(
                name: "MyConexId",
                table: "PreferinteHybrid");
        }
    }
}
