using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Projects.Dtos;
using TaskManager.Projects.Interfaces;
using TaskManager.Projects.Services;
using TaskManager.Shared.Entities;

namespace TaskManager.Projects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ProjectMembershipService _membership;






        public ProjectsController(
            IProjectService projectService,
            ProjectMembershipService membership)
        {
            _projectService = projectService;
            _membership = membership;
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
            //var result = await _projectService.UpdateAsync(id, dto);
            //return result ? Ok() : NotFound();
            var updated = await _projectService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _projectService.DeleteAsync(id);
            return result ? Ok() : NotFound();
        }

        //[HttpPost("{id}/members/{userId}")]
        //public async Task<IActionResult> AddMember(int id, string userId)
        //{
        //    var result = await _projectService.AddMemberAsync(id, userId);
        //    return result ? Ok() : NotFound();
        //    //var actorUserId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "system";
        //    //await _membership.AddMemberAndNotifyAsync(projectId, userId, actorUserId, ct);
        //    //return RedirectToAction("Detail", new { id = projectId });
        //}

        [HttpPost("{id:int}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMemberToProject(
            [FromRoute] int id,
            [FromRoute] string userId,
            CancellationToken ct)
        {
            var actorUserId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "system";
            await _membership.AddMemberAndNotifyAsync(id, userId, actorUserId, ct);
            return NoContent();
        }


        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            var result = await _projectService.RemoveMemberAsync(id, userId);
            return result ? Ok() : NotFound();
        }
    }
}
