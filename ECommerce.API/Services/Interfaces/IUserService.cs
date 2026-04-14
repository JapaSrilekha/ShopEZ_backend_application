using ECommerce.API.DTOs;

namespace ECommerce.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterUserDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
    }
}
