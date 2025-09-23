using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Application.Services;
using TaskManager.Projects.Interfaces;
using TaskManager.Tasks.Dtos;
using TaskManager.Tasks.Services;
using TaskManager.Shared.Entities;

namespace TaskManager.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;

        public TaskController(ITaskService taskService, IProjectService projectService)
        {
            _taskService = taskService;
            _projectService = projectService;
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

            var dto = new TaskCreateDto
            {
                Status = status ?? Shared.Entities.TaskStatus.InProgress
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
                return View(taskDto);
            }

            var createdTask = await _taskService.CreateAsync(taskDto);

            if (createdTask == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể tạo nhiệm vụ");
                await LoadProjectsAsync();
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
                Status = task.Status
            };

            await LoadProjectsAsync();
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
                return View(taskDto);
            }

            var updatedTask = await _taskService.UpdateAsync(id, taskDto);

            if (updatedTask == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật nhiệm vụ");
                await LoadProjectsAsync();
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _taskService.DeleteAsync(id);

            if (!success)
                return BadRequest("Không thể xóa nhiệm vụ.");

            return RedirectToAction(nameof(Index));
        }

        // 🔧 Helper để load danh sách project
        private async Task LoadProjectsAsync()
        {
            var projects = await _projectService.GetAllAsync();
            ViewBag.ProjectList = new SelectList(projects, "Id", "Name");
        }



    }
}