using System.ComponentModel.DataAnnotations;

namespace WebsiteBanDoAnVat.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

      
        public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
    }
}