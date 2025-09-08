namespace TaskManager.Projects.Dtos
{
    public class ProjectCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // 👉 Nhập danh sách username dưới dạng chuỗi
        public string MemberUsername { get; set; } = string.Empty;

        // 👉 Chuyển chuỗi thành danh sách username đã xử lý
        public List<string> MemberUsernames =>
            string.IsNullOrWhiteSpace(MemberUsername)
                ? new List<string>()
                : MemberUsername.Split(',')
                           .Select(u => u.Trim())
                           .Where(u => !string.IsNullOrEmpty(u))
                           .ToList();
    }
}