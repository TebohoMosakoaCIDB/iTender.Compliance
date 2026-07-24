using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class UpdateUserModel
    {
        public Guid Id { get; set; }

        // Personal Information
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public AgentLevel Level { get; set; } = AgentLevel.Junior;

        // Account Status
        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public bool LockoutEnabled { get; set; }

        // Agent Details
        public string? EmployeeNumber { get; set; }

        public string? PhoneNumber { get; set; }

        // Read-only Information
        public int AccessFailedCount { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastLoginOn { get; set; }

        public string? HeaderImagePath { get; set; }

        public string? SignatureImagePath { get; set; }

        public string? FooterText { get; set; }
    }
}
