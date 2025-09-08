namespace TaskManager.Projects.Dtos
{
    public class ProjectUpdateDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? EndDate { get; set; }
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public List<string> MemberUsernames { get; set; } = new();
    }
}
