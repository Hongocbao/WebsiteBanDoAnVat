namespace WebsiteBanDoAnVat.Models
{
    public class DoanhThuViewModel
    {
        public decimal TongDoanhThu { get; set; }
        public int TongHoaDon { get; set; }
        public decimal LoiNhuanUocTinh { get; set; }
        public List<Order> LichSuGiaoDich { get; set; } = new List<Order>();
    }
}
