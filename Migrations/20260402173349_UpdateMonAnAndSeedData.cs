using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebsiteBanDoAnVat.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMonAnAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBestSeller",
                table: "MonAns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "MonAns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "MonAns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Khô gà, khô bò các loại", "Đồ Khô" },
                    { 2, "Bánh tráng trộn, nướng", "Bánh Tráng" },
                    { 3, "Kẹo, bánh ngọt, trái cây sấy", "Ăn Vặt Ngọt" }
                });

            migrationBuilder.InsertData(
                table: "MonAns",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "IsAvailable", "IsBestSeller", "Name", "Price", "StockQuantity", "ViewCount" },
                values: new object[,]
                {
                    { 1, 1, null, "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", true, true, "Khô Bò Giòn Cay Hồng Ngự", 380000m, 50, 0 },
                    { 2, 2, null, "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400", true, true, "Bánh Tráng Muối Bò Premium", 110000m, 100, 0 },
                    { 3, 2, null, "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", true, true, "Bánh Tráng Tóp Mỡ", 130000m, 80, 0 },
                    { 4, 1, null, "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400", true, true, "Khô Bò Vụn", 320000m, 30, 0 },
                    { 5, 3, null, "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", true, false, "Chùm Ruột Muối Tắc", 120000m, 200, 0 },
                    { 6, 3, null, "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400", true, false, "Kẹo Dừa Sáp Bọc Xíu", 110000m, 150, 0 },
                    { 7, 1, null, "https://images.unsplash.com/photo-1621236322951-f073527a4411?w=400", true, false, "Mực Rim Me Sấy Mè", 150000m, 40, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MonAns",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "IsBestSeller",
                table: "MonAns");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "MonAns");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "MonAns");
        }
    }
}
