using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.API.Models
{
   
    // Represents a registered user
    public class User
    {

        [Key]
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Default role is "Customer" — set at the service layer.
        public string Role { get; set; } = "Customer";

        //  Navigation Property 

        // One User - Many Orders.
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
