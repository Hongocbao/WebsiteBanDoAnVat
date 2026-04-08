using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanDoAnVat.Migrations
{
    /// <inheritdoc />
    public partial class DanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonAnId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGias_MonAns_MonAnId",
                        column: x => x.MonAnId,
                        principalTable: "MonAns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_MonAnId",
                table: "DanhGias",
                column: "MonAnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGias");
        }
    }
}
