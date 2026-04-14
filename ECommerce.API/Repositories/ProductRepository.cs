using ECommerce.API.Data;
using ECommerce.API.Models;
using ECommerce.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves all products
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // Retrieves a product by ID.
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        // Adds a new product and saves changes.
        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // Updates an existing product if found.
        public async Task<Product?> UpdateAsync(int id, Product product)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing is null) return null;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.ImageUrl = product.ImageUrl;
            existing.Stock = product.Stock;

            await _context.SaveChangesAsync();
            return existing;
        }

        // Deletes a product by ID if it exists.
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // Checks if a product exists by ID.
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == id);
        }
    }
}
