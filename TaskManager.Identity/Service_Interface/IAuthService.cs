using TaskManager.Identity.DTOs;

namespace TaskManager.Identity.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(Dtos.RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> ChangePasswordAsync(ChangePasswordRequest request);
        Task LogoutAsync();
    }
}
