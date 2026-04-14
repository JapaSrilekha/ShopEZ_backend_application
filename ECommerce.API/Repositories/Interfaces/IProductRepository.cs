using ECommerce.API.Models;

namespace ECommerce.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // Contract for product data access operations.

        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product?> UpdateAsync(int id, Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
