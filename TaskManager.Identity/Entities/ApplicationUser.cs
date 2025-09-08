using Microsoft.AspNetCore.Identity;

namespace TaskManager.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Bạn có thể thêm các thuộc tính mở rộng cho User
        public string FullName { get; set; }
    }
}
