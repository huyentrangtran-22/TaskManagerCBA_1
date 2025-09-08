using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Models;

namespace TaskManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var hour = DateTime.Now.Hour;

                string timeGreeting = hour switch
                {
                    < 12 => "Good morning",
                    < 18 => "Good afternoon",
                    _ => "Good evening"
                };

                // Kiểm tra kỹ để tránh chuỗi rỗng hoặc null
                string rawName = User.Identity.Name;
                string userName = string.IsNullOrWhiteSpace(rawName) ? "Meowster" : rawName;

                ViewBag.WelcomeMessage = $"{timeGreeting}, {userName}! 👋";
            }

            return View();

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}