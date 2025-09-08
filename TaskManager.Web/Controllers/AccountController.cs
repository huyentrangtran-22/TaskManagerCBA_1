using Microsoft.AspNetCore.Mvc;
using TaskManager.Identity.Services;
using TaskManager.Identity.Dtos;
using TaskManager.Identity.DTOs;

namespace TaskManager.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            AuthResponse result = await _authService.LoginAsync(request);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View(request);
            }

            // TODO: Lưu token / cookie từ result.Token nếu dùng JWT
            // Example: HttpContext.Session.SetString("Token", result.Token);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            AuthResponse result = await _authService.RegisterAsync(request);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View(request);
            }

            return RedirectToAction("Login");
        }
    }
}
