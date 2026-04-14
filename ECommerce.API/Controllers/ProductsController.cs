using ECommerce.API.Authorization;
using AppRoles = ECommerce.API.Authorization.Roles;
using ECommerce.API.DTOs;
using ECommerce.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /*
     *  Role Access:
     *    GET  /api/products          — Public (no token required)
     *    GET  /api/products/{id}     — Public (no token required)
     *    POST /api/products          — Admin only
     *    PUT  /api/products/{id}     — Admin only
     *    DELETE /api/products/{id}   — Admin only
     */

    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET /api/products — Public
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductDTO>>.Ok(products, "Products retrieved successfully."));
        }

        // GET /api/products/{id} — Public
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Fail("Product ID must be a positive integer."));

            // Service throws KeyNotFoundException if not found - handled by global middleware
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(ApiResponse<ProductDTO>.Ok(product!,  $"Product with ID {id} retrieved successfully."));
        }

        // POST /api/products — Admin only
        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var created = await _productService.CreateProductAsync(dto);
            _logger.LogInformation("Admin '{User}' created Product {ProductId}.", User.Identity?.Name, created.ProductId);

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = created.ProductId },
                ApiResponse<ProductDTO>.Ok(created, "Product created successfully."));
        }

        // PUT /api/products/{id} — Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO dto)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Fail("Product ID must be a positive integer."));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            // Service throws KeyNotFoundException if not found - handled by global middleware
            var updated = await _productService.UpdateProductAsync(id, dto);
            _logger.LogInformation("Admin '{User}' updated Product {ProductId}.", User.Identity?.Name, id);

            return Ok(ApiResponse<ProductDTO>.Ok(updated!, "Product updated successfully."));
        }

        // DELETE /api/products/{id} — Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Fail("Product ID must be a positive integer."));

            // Service throws KeyNotFoundException if not found - handled by global middleware
            await _productService.DeleteProductAsync(id);
            _logger.LogInformation("Admin '{User}' deleted Product {ProductId}.", User.Identity?.Name, id);

            return Ok(ApiResponse<object>.Ok(new { }, "Product deleted successfully."));
        }

        private string GetModelErrors() =>
            string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
