using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs
{
    public class ProductDTO
    {
      
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    // All validation rules are defined here (not in the Product model).
    public class CreateProductDTO
    {
        
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        // description of the product.
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        // Selling price of the product.
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        
        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string ImageUrl { get; set; } = string.Empty;


        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
    }

    public class UpdateProductDTO
    {
        
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        
        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string ImageUrl { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
    }
}
