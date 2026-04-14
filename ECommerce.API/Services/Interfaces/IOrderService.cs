using ECommerce.API.DTOs;

namespace ECommerce.API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO> GetOrderByIdAsync(int id);
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto);
        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId);
    }
}
