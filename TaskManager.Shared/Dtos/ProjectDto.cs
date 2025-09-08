namespace TaskManager.Shared.Dtos
{
    public record ProjectDto(int Id, string Name, string? Description, DateTime CreatedAt);
    public record CreateProjectDto(string Name, string? Description); 
}
