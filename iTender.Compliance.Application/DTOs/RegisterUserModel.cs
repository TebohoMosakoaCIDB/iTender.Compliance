using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.DTOs
{
    public class RegisterUserModel
    {
        // Identity
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        // Agent Details
        public AgentLevel Level { get; set; } = AgentLevel.Junior;

        public string? EmployeeNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; } = true;

        // Correspondence Branding
        public string? HeaderImagePath { get; set; }

        public string? SignatureImagePath { get; set; }

        public string? FooterText { get; set; }
    }
}
