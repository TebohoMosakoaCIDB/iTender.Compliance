using iTender.Compliance.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace iTender.Compliance.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole<Guid>>>();

            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<ApplicationUser>>();

            string[] roles =
            [
                Roles.Director,
            Roles.ComplianceManager,
            Roles.ComplianceOfficer,
            Roles.ComplianceAdministrator
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result =
                        await roleManager.CreateAsync(
                            new IdentityRole<Guid>(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            string.Join(
                                Environment.NewLine,
                                result.Errors.Select(x => x.Description)));
                    }
                }
            }

            const string email = "admin@itender.co.za";
            const string password = "Admin@123";

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            Environment.NewLine,
                            result.Errors.Select(x => x.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(
                    user,
                    Roles.ComplianceAdministrator))
            {
                await userManager.AddToRoleAsync(
                    user,
                    Roles.ComplianceAdministrator);
            }

            // The seeded account is the bootstrap/test account for the whole
            // system - give it every role so it isn't locked out of
            // policy-protected pages (e.g. "CaseManagement" requires
            // Director/ComplianceManager/ComplianceOfficer, none of which
            // ComplianceAdministrator alone satisfies).
            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}