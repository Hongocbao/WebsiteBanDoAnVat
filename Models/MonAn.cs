using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBanDoAnVat.Models
{
    public class MonAn
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món không được để trống")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Liên kết với loại món (Category)
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}