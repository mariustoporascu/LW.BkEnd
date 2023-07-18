using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    /// <inheritdoc />
    public partial class removestaticstatusnameandtranzactionname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeName",
                table: "Tranzactii");

            migrationBuilder.DropColumn(
                name: "StatusName",
                table: "Documente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypeName",
                table: "Tranzactii",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "Documente",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
