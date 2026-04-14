using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs
{
    
    // Represents a single item in the cart submitted by the client.
    public class CartItemDTO
    {

        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
        public int ProductId { get; set; }

        // Required. Must be at least 1.
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    // Contains list of cart items to be ordered.
    public class CreateOrderDTO
    {

        // List of products and quantities in the cart.
        
        [Required(ErrorMessage = "Cart items are required.")]
        [MinLength(1, ErrorMessage = "At least one cart item is required.")]
        public List<CartItemDTO> CartItems { get; set; } = new();
    }

    // Returned as part of OrderDTO
    public class OrderItemResponseDTO
    {
        
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }

        // via navigation property (Product.Name) in the service mapper
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    // Represents the full order including all line items.
    // No validation attributes needed (this is outbound data, not user input).
   
    public class OrderDTO
    {
        
        public int OrderId { get; set; }

        //ID of the user who placed the order
        public int UserId { get; set; }

        // Full name of the user who placed the order.
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponseDTO> OrderItems { get; set; } = new();
    }
}
