using System.Security.Claims;
using ECommerce.API.Authorization;
using ECommerce.API.DTOs;
using ECommerce.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthRoles = ECommerce.API.Authorization.Roles;

namespace ECommerce.API.Controllers
{
    /*
     *  Role Access:
     *    POST /api/orders               — Admin + Customer (own account only for Customer)
     *    GET  /api/orders               — Admin only
     *    GET  /api/orders/{id}          — Admin (any) | Customer (own orders only)
     *    GET  /api/orders/user/{userId} — Admin only
     *
     *  All service exceptions are handled centrally by ExceptionHandlingMiddleware.
     *  Controllers do NOT wrap service calls in try/catch.
     */

    [ApiController]
    [Route("api/orders")]
    [Produces("application/json")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger       = logger;
        }

        // POST /api/orders — Admin + Customer
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OrderDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));
            
            // Service throws are handled by ExceptionHandlingMiddleware
            var order = await _orderService.CreateOrderAsync(dto);

            _logger.LogInformation("Order {OrderId} created for UserId {UserId}.", order.OrderId, order.UserId);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                ApiResponse<OrderDTO>.Ok(order, "Order placed successfully."));
        }

        // GET /api/orders — Admin only
        [HttpGet]
        [Authorize(Roles = AuthRoles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<IEnumerable<OrderDTO>>.Ok(orders, "Orders retrieved successfully."));
        }

        // GET /api/orders/{id} — Admin (any order) | Customer (own orders only)
        [HttpGet("{id:int}")]
        [Authorize(Roles = AuthRoles.AdminOrCustomer)]
        [ProducesResponseType(typeof(ApiResponse<OrderDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Fail("Order ID must be a positive integer."));

            // Service throws KeyNotFoundException if not found → global middleware returns 404
            var order = await _orderService.GetOrderByIdAsync(id);

            // Customer can only view their own order
            if (!IsAdmin() && !IsOwner(order.UserId))
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail("You are not authorized to view this order."));

            return Ok(ApiResponse<OrderDTO>.Ok(order, $"Order with ID {id} retrieved successfully."));
        }

        // GET /api/orders/user/{userId} — Admin only
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = AuthRoles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<OrderDTO>>.Ok(
                orders, $"Orders retrieved for user {userId}."));
        }

        //  Helpers 

        private bool IsAdmin() => User.IsInRole(AuthRoles.Admin);

        
        // Returns true when the JWT sub claim matches the given userId.
        // Used to enforce that a Customer can only access their own resources.
        
        private bool IsOwner(int resourceUserId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var tokenUserId) && tokenUserId == resourceUserId;
        }

        private string GetModelErrors() =>
            string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
