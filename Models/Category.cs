using System.ComponentModel.DataAnnotations;
namespace WebsiteBanDoAnVat.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Snack>? Snacks { get; set; }
    }
}