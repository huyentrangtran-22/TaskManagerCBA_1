using TaskManager.Shared.Entities;
public class TaskEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? AssignedUserId { get; set; }
    public TaskManager.Shared.Entities.TaskStatus? Status { get; set; }

}
