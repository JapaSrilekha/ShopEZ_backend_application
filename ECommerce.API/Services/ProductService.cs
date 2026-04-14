using ECommerce.API.DTOs;
using ECommerce.API.Models;
using ECommerce.API.Repositories.Interfaces;
using ECommerce.API.Services.Interfaces;

namespace ECommerce.API.Services
{

    // Handles all business logic for product operations.
    // Input validation is handled by DTOs before reaching this layer.
    
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        // Retrieves all products ordered by name.
        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDTO);
        }

        // Retrieves a product by ID. 
        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

            return MapToDTO(product);
        }

        // Creates a new product from the provided DTO.
        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Product data cannot be null.");

            var product = new Product
            {
                Name        = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price       = dto.Price,
                ImageUrl    = dto.ImageUrl.Trim(),
                Stock       = dto.Stock
            };

            var created = await _productRepository.CreateAsync(product);
            _logger.LogInformation("Product created: {ProductId} - {Name}", created.ProductId, created.Name);
            return MapToDTO(created);
        }

        // Updates an existing product.
        public async Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto)
        {
            var product = new Product
            {
                Name        = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price       = dto.Price,
                ImageUrl    = dto.ImageUrl.Trim(),
                Stock       = dto.Stock
            };

            var updated = await _productRepository.UpdateAsync(id, product);

            if (updated is null)
            {
                _logger.LogWarning("Update attempted on non-existent product: {ProductId}", id);
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            _logger.LogInformation("Product updated: {ProductId}", id);
            return MapToDTO(updated);
        }

        // Deletes a product by ID.
        public async Task<bool> DeleteProductAsync(int id)
        {
            var result = await _productRepository.DeleteAsync(id);

            if (!result)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            _logger.LogInformation("Product deleted: {ProductId}", id);
            return true;
        }

        // Maps Product entity to ProductDTO 
        private static ProductDTO MapToDTO(Product p) => new()
        {
            ProductId   = p.ProductId,
            Name        = p.Name,
            Description = p.Description,
            Price       = p.Price,
            ImageUrl    = p.ImageUrl,
            Stock       = p.Stock
        };
    }
}
