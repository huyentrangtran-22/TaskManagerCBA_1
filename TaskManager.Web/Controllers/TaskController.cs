using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Services;
using TaskManager.Projects.Interfaces;
using TaskManager.Shared.Data;
using TaskManager.Shared.Entities;
using TaskManager.Tasks.Dtos;
using TaskManager.Tasks.Services;

namespace TaskManager.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly AppDbContext _context;

        public TaskController(ITaskService taskService, IProjectService projectService, AppDbContext context)
        {
            _taskService = taskService;
            _projectService = projectService;
            _context = context;
        }

        // GET: /Task
        public async Task<IActionResult> Index()
        {
            var tasks = await _taskService.GetAllAsync();
            return View(tasks);
        }

        // GET: /Task/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound();

            return View(task); // Trả về TaskDto
        }

        // GET: /Task/Create
        public async Task<IActionResult> Create(Shared.Entities.TaskStatus? status)
        {
            await LoadProjectsAsync();

            // 👉 Lấy ProjectId mặc định (nếu chưa có)
            var defaultProjectId = await _context.Projects
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            await LoadProjectMembersAsync(defaultProjectId);

            var dto = new TaskCreateDto
            {
                Status = status ?? Shared.Entities.TaskStatus.InProgress,
                ProjectId = defaultProjectId // 👈 Gán vào DTO để giữ giá trị
            };

            return View(dto);

        }


        // POST: /Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskCreateDto taskDto)
        {
            if (!ModelState.IsValid)
            {
                await LoadProjectsAsync();
                await LoadProjectMembersAsync(taskDto.ProjectId); // 👈 Load đúng thành viên theo dự án đã chọn
                return View(taskDto);
            }

            var createdTask = await _taskService.CreateAsync(taskDto);

            if (createdTask == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể tạo nhiệm vụ");
                await LoadProjectsAsync();
                await LoadProjectMembersAsync(taskDto.ProjectId);
                return View(taskDto);
            }

            return RedirectToAction(nameof(Index));

        }

        // GET: /Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound();

            var dto = new TaskUpdateDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                AssignedUserId = task.AssignedUserId,
                Status = task.Status,
                ProjectId = task.ProjectId // 👈 cần có để load members
            };

            await LoadProjectsAsync();
            if (dto.ProjectId > 0) // 👈 kiểm tra có project
            {
                await LoadProjectMembersAsync(dto.ProjectId.Value);
            }

            return View(dto);
        }

        // POST: /Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskUpdateDto taskDto)
        {
            if (id != taskDto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadProjectsAsync();
                if (taskDto.ProjectId > 0)
                {
                    await LoadProjectMembersAsync(taskDto.ProjectId.Value);
                }
                return View(taskDto);
            }

            var updatedTask = await _taskService.UpdateAsync(id, taskDto);

            if (updatedTask == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật nhiệm vụ");
                await LoadProjectsAsync();
                if (taskDto.ProjectId > 0)
                {
                    await LoadProjectMembersAsync(taskDto.ProjectId.Value);
                }
                return View(taskDto);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: /Task/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound();

            return View(task); // Hiển thị trang xác nhận xóa
        }

        // POST: /Task/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _taskService.DeleteAsync(id);

                if (!success)
                    return BadRequest("Không thể xóa nhiệm vụ. Có thể do dữ liệu liên quan chưa được cấu hình xóa tự động.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest($"Không thể xóa nhiệm vụ. Chi tiết: {ex.Message}");
            }

        }

        // 🔧 Helper để load danh sách project
        private async Task LoadProjectsAsync()
        {
            var projects = await _projectService.GetAllAsync();
            ViewBag.ProjectList = new SelectList(projects, "Id", "Name");
        }
        private async Task LoadProjectMembersAsync(int projectId)
        {
            var members = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .Select(pm => new
            {
                UserId = pm.UserId, // 👈 viết hoa đúng
                UserName = pm.User.UserName ?? "Không rõ" // 👈 xử lý null
            })
            .ToListAsync();

                ViewBag.ProjectMembers = new SelectList(members, "UserId", "UserName");
        }

    }
}