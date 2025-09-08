namespace TaskManager.Identity.Dtos
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Thêm property ConfirmPassword
        public string ConfirmPassword { get; set; } = string.Empty;

        // Thêm dòng này
        public string FullName { get; set; } = string.Empty;
    }
}
