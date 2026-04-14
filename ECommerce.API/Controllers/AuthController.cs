using ECommerce.API.DTOs;
using ECommerce.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    /*
     *  All auth endpoints are public ([AllowAnonymous]).
     *  Exceptions thrown by UserService are handled by ExceptionHandlingMiddleware:
     *    InvalidOperationException  - 409 Conflict  (duplicate email)
     *    UnauthorizedAccessException - 401 Unauthorized (bad credentials)
     */

    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    [AllowAnonymous]   // Both register and login are public
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger      = logger;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            // InvalidOperationException (duplicate email) 
            var result = await _userService.RegisterAsync(dto);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthResponseDTO>.Ok(result, "Registration successful."));
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(GetModelErrors()));

            var result = await _userService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDTO>.Ok(result, "Login successful."));
        }

        private string GetModelErrors() =>
            string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
    }
}
