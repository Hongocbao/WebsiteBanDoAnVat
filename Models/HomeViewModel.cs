namespace WebsiteBanDoAnVat.Models
{
    public class HomeViewModel
    {
        public List<MonAn> BestSellers { get; set; } = new List<MonAn>();
        public List<MonAn> NewProducts { get; set; } = new List<MonAn>();
    }
}
