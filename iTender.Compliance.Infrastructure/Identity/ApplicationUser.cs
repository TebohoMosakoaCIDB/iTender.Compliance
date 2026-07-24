using Microsoft.AspNetCore.Identity;

namespace iTender.Compliance.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginOn { get; set; }
    }
}
