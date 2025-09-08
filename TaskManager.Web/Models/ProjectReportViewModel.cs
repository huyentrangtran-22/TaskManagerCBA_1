using TaskManager.Shared.Dtos;

namespace TaskManager.Web.Models
{
    public class ProjectReportViewModel
    {
        public int Completed { get; set; }
        public int InProgress { get; set; }

        // 🔹 Dùng cho biểu đồ cột
        // 🔹 Khởi tạo mặc định để tránh null
        public List<string> ProjectNames { get; set; } = new List<string>();
        public List<int> ProgressValues { get; set; } = new List<int>();
        // Thêm dòng này để dùng cho Bar Chart
        public List<ProjectProgressDto>? ProjectProgresses { get; set; }
    }
}
