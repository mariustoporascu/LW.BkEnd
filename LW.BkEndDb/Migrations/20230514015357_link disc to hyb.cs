using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class linkdisctohyb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FirmaDiscountId",
                table: "Hybrid",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hybrid_FirmaDiscountId",
                table: "Hybrid",
                column: "FirmaDiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hybrid_FirmaDiscount_FirmaDiscountId",
                table: "Hybrid",
                column: "FirmaDiscountId",
                principalTable: "FirmaDiscount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hybrid_FirmaDiscount_FirmaDiscountId",
                table: "Hybrid");

            migrationBuilder.DropIndex(
                name: "IX_Hybrid_FirmaDiscountId",
                table: "Hybrid");

            migrationBuilder.DropColumn(
                name: "FirmaDiscountId",
                table: "Hybrid");
        }
    }
}
