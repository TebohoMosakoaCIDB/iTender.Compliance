using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Data
{
    public static class AgentSeeder
    {
        private const string AdminEmail = "admin@itender.co.za";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var context = serviceProvider
                .GetRequiredService<ComplianceDbContext>();

            var user = await userManager.FindByEmailAsync(AdminEmail);

            if (user == null)
                return;

            var agent = await context.Agents
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (agent == null)
            {
                context.Agents.Add(new Agent
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmployeeNumber = "SYSTEM-001",
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Department = "Regulatory Compliance",
                    Email = user.Email ?? AdminEmail,
                    PhoneNumber = string.Empty,
                    IsActive = true,
                    IsManager = true,
                    AutoAssignEnabled = false,
                    Level = AgentLevel.Senior,
                    JobTitle = "System Administrator",
                    CreatedOn = DateTime.UtcNow
                });

await context.SaveChangesAsync();
            }
            else if (!agent.IsManager)
               {
   agent.IsManager = true;
   
   await context.SaveChangesAsync();
               }
        }
    }
}
