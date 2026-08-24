using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class ProfileModel
    {
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string? JobTitle { get; set; }

        public AgentLevel Level { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime? LastLoginOn { get; set; }
    }

    public class UpdateMyProfileModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string? JobTitle { get; set; }
    }
}
