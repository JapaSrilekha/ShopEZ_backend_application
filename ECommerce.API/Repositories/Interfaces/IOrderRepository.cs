using ECommerce.API.Models;

namespace ECommerce.API.Repositories.Interfaces
{
    // Contract for order data access operations.

    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);

        // Retrieves all orders for a specific user.
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

        Task<Order> CreateAsync(Order order);
    }
}
