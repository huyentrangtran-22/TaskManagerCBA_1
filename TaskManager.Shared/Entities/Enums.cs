using System.ComponentModel.DataAnnotations;

namespace TaskManager.Shared.Entities
{
    public enum TaskStatus
    {
        [Display(Name = "🚧 In Progress")]
        InProgress,

        [Display(Name = "✅ Completed")]
        Completed
    }


}
