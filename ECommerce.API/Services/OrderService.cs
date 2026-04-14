using ECommerce.API.DTOs;
using ECommerce.API.Models;
using ECommerce.API.Repositories.Interfaces;
using ECommerce.API.Services.Interfaces;

namespace ECommerce.API.Services
{
    // Handles order processing, validation, stock management, and business logic.

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OrderService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            ILogger<OrderService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository   = orderRepository;
            _productRepository = productRepository;
            _userRepository    = userRepository;
            _logger            = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // Retrieves all orders across all users 
        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDTO);
        }

        // Retrieves a single order by ID.
        public async Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Order ID must be a positive integer.");

            var order = await _orderRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            return MapToDTO(order);
        }

        // Retrieves all orders for a specific user.
        // Returns an empty list if the user has placed no orders 

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive integer.");

            //  Check if user exists
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            //  Fetch orders
            var orders = await _orderRepository.GetByUserIdAsync(userId);

            //  Empty list is valid if user exists but has no orders
            return orders.Select(MapToDTO);
        }


        // Creates a new order from cart items.

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto)
        {
            // Validate request 
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Order data cannot be null.");

            //  1. Get logged-in user from JWT token 
            //  extract userId from authenticated user's claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Invalid or missing authentication token.");

            var userId = int.Parse(userIdClaim);

            // 2. Validate user exists 
            // Ensures the user in token is still valid in DB
            _ = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            //  3. Validate cart 
            if (dto.CartItems == null || !dto.CartItems.Any())
                throw new ArgumentException("Cart must contain at least one item.");

            var orderItems = new List<OrderItem>();

            // Cache products to avoid multiple DB calls (performance optimization)
            var productMap = new Dictionary<int, Product>();

            //  4. Validate each cart item 
            foreach (var cartItem in dto.CartItems)
            {
                // Quantity must be positive
                if (cartItem.Quantity <= 0)
                    throw new ArgumentException(
                        $"Quantity for ProductId {cartItem.ProductId} must be greater than zero.");

                // Check if product exists
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId)
                    ?? throw new KeyNotFoundException($"Product with ID {cartItem.ProductId} not found.");

                // Check stock availability
                if (product.Stock < cartItem.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Available: {product.Stock}, Requested: {cartItem.Quantity}.");

                // Add item to order
                orderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = product.Price // store price at time of purchase
                });

                // Store product in cache for stock deduction
                productMap[product.ProductId] = product;
            }

            //  5. Calculate total amount 
            decimal totalAmount = orderItems.Sum(i => i.Price * i.Quantity);

            //  6. Deduct stock (inventory update) 
            foreach (var item in orderItems)
            {
                var product = productMap[item.ProductId];

                product.Stock -= item.Quantity;

                // Persist updated stock
                await _productRepository.UpdateAsync(product.ProductId, product);
            }

            //  7. Create order entity 
            var order = new Order
            {
                UserId = userId, //  Always from token (secure)
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };

            //  8. Save order to database 
            var created = await _orderRepository.CreateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} created for UserId {UserId}, Total: {Total}",
                created.OrderId, created.UserId, created.TotalAmount);

            //  9. Return response DTO 
            return MapToDTO(created);
        }

        // Maps Order entity to OrderDTO.
        private static OrderDTO MapToDTO(Order o) => new()
        {
            OrderId     = o.OrderId,
            UserId      = o.UserId,
            UserName    = o.User?.Name ?? string.Empty,
            OrderDate   = o.OrderDate,
            TotalAmount = o.TotalAmount,
            OrderItems  = o.OrderItems.Select(oi => new OrderItemResponseDTO
            {
                OrderItemId = oi.OrderItemId,
                ProductId   = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity    = oi.Quantity,
                Price       = oi.Price
            }).ToList()
        };
    }
}
