using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.DTOs;
using ECommerce.API.Models;
using ECommerce.API.Repositories.Interfaces;
using ECommerce.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.API.Services
{
    // Handles user authentication and registration logic.
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO dto)
        {
            // Prevent duplicate registrations
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

            // Hash password before storing
            var hashedPassword = HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Password = hashedPassword,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role.Trim()
            };

            var created = await _userRepository.CreateAsync(user);
            _logger.LogInformation("New user registered: {Email}", created.Email);

            var token = GenerateJwtToken(created);
            return BuildAuthResponse(created, token);
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {

            // Validate user credentials
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!VerifyPassword(dto.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            _logger.LogInformation("User logged in: {Email}", user.Email);

            var token = GenerateJwtToken(user);
            return BuildAuthResponse(user, token);
        }

        //---------  Generates JWT token with user claims. ---------- 
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey not configured."));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(
                    double.Parse(jwtSettings["ExpiresInHours"] ?? "24")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }


        // Hashes password before storing in DB
        private static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        // Verifies entered password with stored hash
        private static bool VerifyPassword(string plain, string hashed)
            => BCrypt.Net.BCrypt.Verify(plain, hashed);

        // Builds authentication response with token and user info.
        private static AuthResponseDTO BuildAuthResponse(User user, string token) => new()
        {
            Token = token,
            User = new UserDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            }
        };
    }
}
