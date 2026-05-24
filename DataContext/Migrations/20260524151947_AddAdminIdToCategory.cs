using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminIdToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminID",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_AdminID",
                table: "Categories",
                column: "AdminID");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Admins_AdminID",
                table: "Categories",
                column: "AdminID",
                principalTable: "Admins",
                principalColumn: "AdminID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Admins_AdminID",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_AdminID",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "AdminID",
                table: "Categories");
        }
    }
}
