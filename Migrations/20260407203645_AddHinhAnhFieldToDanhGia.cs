using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanDoAnVat.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhFieldToDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "DanhGias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "DanhGias");
        }
    }
}
