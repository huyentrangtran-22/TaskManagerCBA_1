using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Services;
using TaskManager.Projects.Interfaces;
using TaskManager.Shared.Data;
using TaskManager.Shared.Dtos;
using TaskManager.Shared.Entities;
using TaskManager.Tasks.Dtos;
using SharedTaskStatus = TaskManager.Shared.Entities.TaskStatus;

namespace TaskManager.Tasks.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IProjectService _projectService;

        public TaskService(AppDbContext context, IProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
        }


        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return null;

            var userName = task.AssignedUserId != null
                ? await _context.Users
                    .Where(u => u.Id == task.AssignedUserId)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync()
                : "Chưa phân công";

            return new TaskDto
            {
                Id = task.Id,
                Name = task.Title,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = userName ?? "Không rõ",
                Status = (SharedTaskStatus)task.Status,
                ProjectId = task.ProjectId,
                Project = task.Project
            };
        }
        public async Task<TaskDto?> CreateAsync(TaskCreateDto dto)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
            if (!projectExists)
                throw new Exception($"ProjectId {dto.ProjectId} không tồn tại.");

            var task = new TaskItem
            {
                Title = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = (DateTime)dto.EndDate,
                AssignedUserId = string.IsNullOrWhiteSpace(dto.AssignedUserId) ? null : dto.AssignedUserId,
                Status = (Shared.Entities.TaskStatus)(int)dto.Status,
                ProjectId = dto.ProjectId
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            // ✅ Kiểm tra và cập nhật trạng thái dự án
            await _projectService.UpdateProjectStatusIfCompletedAsync(task.ProjectId);

            return new TaskDto
            {
                Id = task.Id,
                Name = task.Title,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                AssignedUserId = string.IsNullOrWhiteSpace(task.AssignedUserId) ? "Chưa phân công" : task.AssignedUserId,
                Status = (Shared.Entities.TaskStatus)(int)task.Status,
                ProjectId = task.ProjectId
            };
        }

        public async Task<TaskDto?> UpdateAsync(int id, TaskUpdateDto dto)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return null;

            task.Title = dto.Name;
            task.Description = dto.Description;
            task.StartDate = dto.StartDate;
            task.EndDate = dto.EndDate;
            task.AssignedUserId = string.IsNullOrWhiteSpace(dto.AssignedUserId) ? null : dto.AssignedUserId;
            task.Status = (Shared.Entities.TaskStatus)dto.Status;

            await _context.SaveChangesAsync();

            // ✅ Kiểm tra và cập nhật trạng thái dự án
            await _projectService.UpdateProjectStatusIfCompletedAsync(task.ProjectId);

            return new TaskDto
            {
                Id = task.Id,
                Name = task.Title,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                AssignedUserId = string.IsNullOrWhiteSpace(task.AssignedUserId) ? "Chưa phân công" : task.AssignedUserId,
                Status = (Shared.Entities.TaskStatus)task.Status,
                ProjectId = task.ProjectId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null)
            {
                Console.WriteLine($"Không tìm thấy task với ID: {id}");
                return false;
            }

            try
            {
                _context.TaskItems.Remove(task);
                await _context.SaveChangesAsync();

                await _projectService.UpdateProjectStatusIfCompletedAsync(task.ProjectId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa task: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AssignUserAsync(int taskId, string userId)
        {
            var task = await _context.TaskItems.FindAsync(taskId);
            if (task == null) return false;

            // ✅ Kiểm tra xem userId có phải là thành viên của dự án không
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

            if (!isMember)
            {
                throw new Exception("Người dùng không thuộc dự án này, không thể phân công.");
            }

            task.AssignedUserId = string.IsNullOrWhiteSpace(userId) ? null : userId;
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<TaskReportDto> GetTaskReportAsync()
        {
            var completedCount = await _context.TaskItems
                .CountAsync(t => t.Status == Shared.Entities.TaskStatus.Completed);

            var inProgressCount = await _context.TaskItems
                .CountAsync(t => t.Status == Shared.Entities.TaskStatus.InProgress);

            return new TaskReportDto
            {
                Completed = completedCount,
                InProgress = inProgressCount
            };

        }

        public async Task<List<TaskDto>> GetAllAsync()
        {
            // Truy vấn tất cả user và đưa vào dictionary để tra nhanh
            var userDict = await _context.Users
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            // Truy vấn tất cả task kèm theo thông tin project
            var tasks = await _context.TaskItems
                .Include(t => t.Project)
                .ToListAsync();

            // Ánh xạ sang DTO
            var result = tasks.Select(task =>
            {
                var userName = task.AssignedUserId != null && userDict.TryGetValue(task.AssignedUserId, out var name)
                    ? name
                    : "Chưa phân công";

                return new TaskDto
                {
                    Id = task.Id,
                    Name = task.Title,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    AssignedUserId = task.AssignedUserId,
                    AssignedUserName = string.IsNullOrWhiteSpace(userName) ? "Không rõ" : userName,
                    Status = (SharedTaskStatus)task.Status,
                    ProjectId = task.ProjectId,
                    Project = task.Project
                };
            }).ToList();

            return result;
        }
    }
}