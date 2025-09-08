using Microsoft.AspNetCore.Mvc;
using TaskManager.Projects.Dtos;
using TaskManager.Projects.Interfaces;

namespace TaskManager.Projects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _projectService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            return project == null ? NotFound() : Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectCreateDto dto)
        {
            var project = await _projectService.CreateAsync(dto);
            return Ok(project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProjectUpdateDto dto)
        {
            var result = await _projectService.UpdateAsync(id, dto);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _projectService.DeleteAsync(id);
            return result ? Ok() : NotFound();
        }

        [HttpPost("{id}/members/{userId}")]
        public async Task<IActionResult> AddMember(int id, string userId)
        {
            var result = await _projectService.AddMemberAsync(id, userId);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            var result = await _projectService.RemoveMemberAsync(id, userId);
            return result ? Ok() : NotFound();
        }
    }
}
