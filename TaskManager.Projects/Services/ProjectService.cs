using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using TaskManager.Identity.Entities;
using TaskManager.Projects.Dtos;
using TaskManager.Projects.DTOs;
using TaskManager.Projects.Interfaces;
using TaskManager.Shared.Data;
using TaskManager.Shared.Dtos;
using TaskManager.Shared.Entities;
using TaskManager.Tasks.Dtos;

namespace TaskManager.Projects.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<Dtos.ProjectDto>> GetAllAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Members)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var projectDtos = projects.Select(p => new Dtos.ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                MemberUsernames = p.Members
                    .Join(users, m => m.UserId, u => u.Id, (m, u) => u.UserName ?? string.Empty)
                    .Where(username => !string.IsNullOrWhiteSpace(username))
                    .ToList()
            }).ToList();

            return projectDtos;
        }

        public async Task<Dtos.ProjectDto?> GetByIdAsync(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            var users = await _userManager.Users.ToListAsync();

            var memberUsernames = project.Members
                .Join(users, m => m.UserId, u => u.Id, (m, u) => u.UserName)
                .ToList();

            var taskDtos = project.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Title,
                Description = t.Description,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                AssignedUserId = t.AssignedUserId,
                Status = t.Status,
                ProjectId = t.ProjectId
            }).ToList();

            return new Dtos.ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = project.Status,
                MemberUsernames = memberUsernames,
                Tasks = taskDtos
            };
        }

        public async Task<Dtos.ProjectDto> CreateAsync(ProjectCreateDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                StartDate = dto.StartDate != default ? dto.StartDate : DateTime.UtcNow,
                EndDate = dto.EndDate,
                Status = ProjectStatus.InProgress
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var addedUsernames = new List<string>();
            var failedUsernames = new List<string>();

            if (dto.MemberUsernames != null && dto.MemberUsernames.Any())
            {
                foreach (var username in dto.MemberUsernames)
                {
                    var user = await _userManager.FindByNameAsync(username);
                    if (user == null)
                    {
                        failedUsernames.Add(username);
                        continue;
                    }

                    var exists = await _context.ProjectMembers
                        .AnyAsync(pm => pm.ProjectId == project.Id && pm.UserId == user.Id);

                    if (exists) continue;

                    var member = new ProjectMember
                    {
                        ProjectId = project.Id,
                        UserId = user.Id
                    };

                    _context.ProjectMembers.Add(member);
                    addedUsernames.Add(username);
                }

                await _context.SaveChangesAsync();
            }

            var message = "Tạo dự án thành công.";
            if (failedUsernames.Any())
                message += $" Không tìm thấy các username: {string.Join(", ", failedUsernames)}.";

            return new Dtos.ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Status = project.Status,
                MemberUsernames = addedUsernames,
                Success = true,
                Message = message
            };
        }

        public async Task<bool> UpdateAsync(int id, ProjectUpdateDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            if (dto.MemberUsernames != null && dto.MemberUsernames.Any(u => !string.IsNullOrWhiteSpace(u)))
            {
                var validUsernames = dto.MemberUsernames
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .ToList();

                await UpdateMembersAsync(id, validUsernames);
            }

            await UpdateProjectStatusIfCompletedAsync(id);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddMemberAsync(int projectId, string userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            var user = await _userManager.FindByIdAsync(userId);

            if (project == null || user == null)
                return false;

            var exists = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (exists) return false;

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (member == null) return false;

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateMembersAsync(int projectId, List<string> newUsernames)
        {
            var existingMembers = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();

            _context.ProjectMembers.RemoveRange(existingMembers);

            foreach (var username in newUsernames.Distinct())
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    _context.ProjectMembers.Add(new ProjectMember
                    {
                        ProjectId = projectId,
                        UserId = user.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateProjectStatusIfCompletedAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.Tasks == null || !project.Tasks.Any())
                return;

            bool allTasksCompleted = project.Tasks.All(t => t.Status == Shared.Entities.TaskStatus.Completed);
            var newStatus = allTasksCompleted ? ProjectStatus.Completed : ProjectStatus.InProgress;

            if (project.Status != newStatus)
            {
                project.Status = newStatus;
                _context.Projects.Update(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ProjectReportDto> GetProjectReportAsync(int projectId)
        {
            var completedCount = await _context.Projects
                .CountAsync(p => p.Status == ProjectStatus.Completed);

            var inProgressCount = await _context.Projects
                .CountAsync(p => p.Status == ProjectStatus.InProgress);

            return new ProjectReportDto
            {
                Completed = completedCount,
                InProgress = inProgressCount
            };
        }

        async Task<List<ProjectProgressDto>> IProjectService.GetAllProjectsProgressAsync()
        {
            var projects = await _context.Projects
         .Include(p => p.Tasks) // load luôn danh sách task
         .ToListAsync();

            return projects.Select(project =>
            {
                int totalTasks = project.Tasks?.Count ?? 0;
                int completedTasks = project.Tasks?.Count(t => t.Status == Shared.Entities.TaskStatus.Completed) ?? 0;

                int progress = totalTasks > 0
                    ? (int)((completedTasks / (double)totalTasks) * 100)
                    : 0;

                return new ProjectProgressDto
                {
                    Name = project.Name,
                    ProgressPercent = progress
                };
            }).ToList();
        }

        public async Task<IEnumerable> GetProjectMembersAsync(int projectId)
        {
            var members = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .ToListAsync(); // lấy danh sách tất cả user trong project

            var result = members.Select(pm => new ProjectMemberDto
            {
                UserId = pm.UserId,
                UserName = pm.User?.UserName ?? "Không rõ"
            }).ToList();

            return result;

        }

    }
}