using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class preflistchangesfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreferinteHybrid_Hybrid_HybridId",
                table: "PreferinteHybrid");

            migrationBuilder.DropIndex(
                name: "IX_PreferinteHybrid_HybridId",
                table: "PreferinteHybrid");

            migrationBuilder.DropColumn(
                name: "HybridId",
                table: "PreferinteHybrid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HybridId",
                table: "PreferinteHybrid",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreferinteHybrid_HybridId",
                table: "PreferinteHybrid",
                column: "HybridId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreferinteHybrid_Hybrid_HybridId",
                table: "PreferinteHybrid",
                column: "HybridId",
                principalTable: "Hybrid",
                principalColumn: "Id");
        }
    }
}
