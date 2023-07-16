using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class newdiscfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveSecondary",
                table: "FirmaDiscount",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActiveSecondary",
                table: "FirmaDiscount");
        }
    }
}
