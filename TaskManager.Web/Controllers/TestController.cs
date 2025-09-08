using Microsoft.AspNetCore.Mvc;
using TaskManager.Projects.Data;

namespace TaskManager.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly ProjectDbContext _context;

        public TestController(ProjectDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var projects = _context.Projects.ToList();
            return Ok(projects);
        }
    }
}