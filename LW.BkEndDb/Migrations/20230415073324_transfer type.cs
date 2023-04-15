using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class transfertype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isWithdraw",
                table: "Tranzactii");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Tranzactii",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TypeName",
                table: "Tranzactii",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tranzactii");

            migrationBuilder.DropColumn(
                name: "TypeName",
                table: "Tranzactii");

            migrationBuilder.AddColumn<bool>(
                name: "isWithdraw",
                table: "Tranzactii",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
