using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Application.Services;
using TaskManager.Projects.Interfaces;
using TaskManager.Shared.Dtos;
using TaskManager.Web.Models;

namespace TaskManager.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IProjectService _projectService;

        public DashboardController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            // 🔹 Báo cáo tổng hợp (hoàn thành, đang làm)
            var reportProjectDto = await _projectService.GetProjectReportAsync(1);

            // 🔹 Lấy danh sách tiến độ tất cả dự án (nếu null thì tạo list rỗng)
            var projectProgress = await _projectService.GetAllProjectsProgressAsync()
                                 ?? new List<ProjectProgressDto>();

            // 🔹 Chuẩn bị dữ liệu cho ViewModel                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
            var viewModel = new ProjectReportViewModel
            {
                Completed = reportProjectDto?.Completed ?? 0,
                InProgress = reportProjectDto?.InProgress ?? 0,
                ProjectNames = projectProgress.Select(p => p.Name).ToList(),
                ProgressValues = projectProgress.Select(p => p.ProgressPercent).ToList()
            };

            return View(viewModel);
        }
    }
}
