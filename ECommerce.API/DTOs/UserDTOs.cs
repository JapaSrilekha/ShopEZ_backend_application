using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs
{
    
    // The User entity's Password field is NEVER included in any response DTO.
    // Used inside AuthResponseDTO after login/register.
    
    public class UserDTO
    {
        
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Input DTO — received from the client for POST /api/auth/register.
    // All validation rules are defined here (not in the User model).
    // The plain-text password received here is hashed in UserService before storage.
    public class RegisterUserDTO
    {
        
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;

        
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;


        // Optional — defaults to "Customer" if not provided.
        // Accepted values: "Admin" | "Customer".
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        [RegularExpression("^(Admin|Customer)$",ErrorMessage = "Role must be either 'Admin' or 'Customer'.")]
        public string Role { get; set; } = "Customer";
    }

    // Input DTO — received from the client for POST /api/auth/login.
    // Credentials are verified in UserService against the stored hashed password.

    public class LoginDTO
    {
        /// <summary>
        /// Registered email address of the user.
        /// Required. Must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password submitted by the user.
        /// Required. Compared against the stored hash in UserService.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

   
    // Output DTO — returned after a successful login or registration.
    // Contains the JWT bearer token and the safe user profile (UserDTO).
    
    public class AuthResponseDTO
    {
       
        // Signed JWT token.
        public string Token { get; set; } = string.Empty;

       
        // Safe profile of the authenticated user.
        // Password is never included here.
        public UserDTO User { get; set; } = null!;
    }
}
