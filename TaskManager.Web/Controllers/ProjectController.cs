using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManager.Identity.Entities;
using TaskManager.Projects.Dtos;
using TaskManager.Projects.Interfaces;
using TaskManager.Shared.Data;
using TaskManager.Shared.Utils; // chứa ServiceResult

namespace TaskManager.Web.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(IProjectService projectService, AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _projectService = projectService;
            _context = context;
            _userManager = userManager;

        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            var projectDtos = await _projectService.GetAllAsync(); // Lấy toàn bộ danh sách dự án

            // Không nhóm theo trạng thái nữa, truyền thẳng danh sách vào View
            return View(projectDtos);


        }

        // GET: /Project/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project); // Truyền ProjectDto vào view
        }


        // GET: /Project/Create
        public async Task<IActionResult> Create()
        {
            return View(new ProjectCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectCreateDto projectDto)
        {
            if (!ModelState.IsValid)
            {
                // Nếu có lỗi, hiển thị lại chuỗi username đã nhập
                projectDto.MemberUsername = string.Join(", ", projectDto.MemberUsernames);
                return View(projectDto);
            }

            var createdProject = await _projectService.CreateAsync(projectDto);

            if (createdProject == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể tạo dự án");
                projectDto.MemberUsername = string.Join(", ", projectDto.MemberUsernames);
                return View(projectDto);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Project/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null) return NotFound();

            var projectDto = new ProjectUpdateDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                MemberUsernames = project.MemberUsernames ?? new List<string>() // đảm bảo không null
            };

            return View(projectDto);
        }

        // POST: /Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectUpdateDto projectDto)
        {
            if (id != projectDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(projectDto);

            // Gọi service UpdateAsync, trả về ProjectDto hoặc null nếu update thất bại
            var updatedProject = await _projectService.UpdateAsync(id, projectDto);

            if (updatedProject == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot update project");
                return View(projectDto);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Project/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: /Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deletedProject = await _projectService.DeleteAsync(id);

            if (deletedProject == null)
            {
                TempData["Error"] = "Cannot delete project";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
