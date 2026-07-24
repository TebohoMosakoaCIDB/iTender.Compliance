namespace iTender.Compliance.Application.DTOs
{
    public class UserListModel
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public bool HasAgent { get; set; }
    }
}
