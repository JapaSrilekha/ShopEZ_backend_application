using ECommerce.API.Models;

namespace ECommerce.API.Repositories.Interfaces
{
    // Defines all database operations related to User entity
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);

        // Checks if an email already exists in the database.
        Task<bool> EmailExistsAsync(string email);
    }
}
