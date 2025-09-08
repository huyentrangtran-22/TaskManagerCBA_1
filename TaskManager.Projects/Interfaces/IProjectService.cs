using TaskManager.Projects.Dtos;
using TaskManager.Shared.Dtos;

namespace TaskManager.Projects.Interfaces
{
    public interface IProjectService
    {
        Task<List<Dtos.ProjectDto>> GetAllAsync();
        Task<Dtos.ProjectDto?> GetByIdAsync(int id);
        Task<Dtos.ProjectDto> CreateAsync(ProjectCreateDto dto);
        Task<bool> UpdateAsync(int id, ProjectUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddMemberAsync(int projectId, string userId);
        Task<bool> RemoveMemberAsync(int projectId, string userId);
        Task UpdateProjectStatusIfCompletedAsync(int projectId);
        Task<Shared.Dtos.ProjectReportDto> GetProjectReportAsync(int projectId);
        Task<List<ProjectProgressDto>> GetAllProjectsProgressAsync();
    }
}
