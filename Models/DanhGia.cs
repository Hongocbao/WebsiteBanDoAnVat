namespace WebsiteBanDoAnVat.Models
{
    public class DanhGia
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public int SoSao { get; set; } // 1 - 5 sao
        public DateTime NgayDang { get; set; } = DateTime.Now;
        public int MonAnId { get; set; } // Liên kết với món ăn

        public string? HinhAnh { get; set; }
    }
}
