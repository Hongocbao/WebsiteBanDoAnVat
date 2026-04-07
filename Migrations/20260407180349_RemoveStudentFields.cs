using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanDoAnVat.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSinhVien",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MaSinhVien",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSinhVien",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaSinhVien",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
