using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.API.Models
{

    // Represents a customer
    public class Order
    {

        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        //  Navigation Properties 

        // Many Orders - One User.
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // One Order - Many OrderItems.
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
