using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddParentToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagCategory_Categories_CategoryID",
                table: "TagCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_TagCategory_Tags_TagID",
                table: "TagCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TagCategory",
                table: "TagCategory");

            migrationBuilder.RenameTable(
                name: "TagCategory",
                newName: "TagCategories");

            migrationBuilder.RenameIndex(
                name: "IX_TagCategory_TagID",
                table: "TagCategories",
                newName: "IX_TagCategories_TagID");

            migrationBuilder.RenameIndex(
                name: "IX_TagCategory_CategoryID",
                table: "TagCategories",
                newName: "IX_TagCategories_CategoryID");

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryID",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TagCategories",
                table: "TagCategories",
                column: "TagCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_TagCategories_Categories_CategoryID",
                table: "TagCategories",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagCategories_Tags_TagID",
                table: "TagCategories",
                column: "TagID",
                principalTable: "Tags",
                principalColumn: "TagID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagCategories_Categories_CategoryID",
                table: "TagCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_TagCategories_Tags_TagID",
                table: "TagCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TagCategories",
                table: "TagCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryID",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "TagCategories",
                newName: "TagCategory");

            migrationBuilder.RenameIndex(
                name: "IX_TagCategories_TagID",
                table: "TagCategory",
                newName: "IX_TagCategory_TagID");

            migrationBuilder.RenameIndex(
                name: "IX_TagCategories_CategoryID",
                table: "TagCategory",
                newName: "IX_TagCategory_CategoryID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TagCategory",
                table: "TagCategory",
                column: "TagCategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_TagCategory_Categories_CategoryID",
                table: "TagCategory",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagCategory_Tags_TagID",
                table: "TagCategory",
                column: "TagID",
                principalTable: "Tags",
                principalColumn: "TagID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
