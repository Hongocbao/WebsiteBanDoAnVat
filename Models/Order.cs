using System.ComponentModel.DataAnnotations;

namespace WebsiteBanDoAnVat.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = ""; 
        public string PhoneNumber { get; set; } = ""; 
        public string Address { get; set; } = "";    
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Chờ duyệt";

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}