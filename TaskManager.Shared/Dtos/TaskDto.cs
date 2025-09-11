using TaskManager.Shared.Dtos;
using TaskManager.Shared.Entities; // để dùng TaskStatus
namespace TaskManager.Tasks.Dtos
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; } = "Chưa phân công"; // 👈 Thêm dòng này
        public Shared.Entities.TaskStatus Status { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; } 

        public TaskDto() { }

        public TaskDto(int id, string name, string? description, DateTime start, DateTime end, string? userId, Shared.Entities.TaskStatus status)
        {
            Id = id;
            Name = name;
            Description = description;
            StartDate = start;
            EndDate = end;
            AssignedUserId = userId;
            Status = status;
        }
    }

    public class TaskCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? AssignedUserId { get; set; }
        public Shared.Entities.TaskStatus Status { get; set; }

        public int ProjectId { get; set; } // ✅ để liên kết với Project

    }

    public class TaskUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ProjectId { get; set; }
        public string? AssignedUserId { get; set; }
        public Shared.Entities.TaskStatus Status { get; set; } 
    }
}
