using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.API.Models
{
    // Represents a product  
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public int Stock { get; set; }

        //  Navigation Property
  
        // One Product - Many OrderItems
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
