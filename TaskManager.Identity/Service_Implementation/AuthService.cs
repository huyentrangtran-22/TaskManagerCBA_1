using Microsoft.AspNetCore.Identity;
using TaskManager.Identity.DTOs;
using TaskManager.Identity.Entities;

namespace TaskManager.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Đăng ký
        public async Task<AuthResponse> RegisterAsync(Dtos.RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = string.Join("; ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponse { Success = true, Message = "Đăng ký thành công" };
        }

        // Đăng nhập
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return new AuthResponse { Success = false, Message = "Không tìm thấy tài khoản với email đã nhập" };
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, false);

            if (!result.Succeeded)
            {
                return new AuthResponse { Success = false, Message = "Sai tài khoản hoặc mật khẩu" };
            }

            return new AuthResponse { Success = true, Message = "Đăng nhập thành công" };
        }


        // Đổi mật khẩu
        public async Task<AuthResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return new AuthResponse { Success = false, Message = "Không tìm thấy user" };

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = string.Join("; ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponse { Success = true, Message = "Đổi mật khẩu thành công" };
        }

        // Đăng xuất
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
