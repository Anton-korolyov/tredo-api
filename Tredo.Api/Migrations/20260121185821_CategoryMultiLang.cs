using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tredo.Api.Migrations
{
    /// <inheritdoc />
    public partial class CategoryMultiLang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "NameRu");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameHe",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameHe",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "NameRu",
                table: "Categories",
                newName: "Name");
        }
    }
}
