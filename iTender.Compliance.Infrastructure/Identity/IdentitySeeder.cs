using iTender.Compliance.Application.Common;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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

            var dbContext =
                serviceProvider.GetRequiredService<ComplianceDbContext>();

            string[] roles =
            [
                Roles.Director,
        Roles.ComplianceManager,
        Roles.ComplianceOfficer,
        Roles.ComplianceAdministrator
            ];

            // ---------------------------------------------------------
            // Seed Roles
            // ---------------------------------------------------------

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

            // ---------------------------------------------------------
            // Seed Bootstrap Administrator
            // ---------------------------------------------------------

            const string adminEmail = "admin@itender.co.za";
            const string adminPassword = "Admin@123";

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(
                            Environment.NewLine,
                            result.Errors.Select(x => x.Description)));
                }
            }

            // Give the bootstrap administrator every role
            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(adminUser, role))
                {
                    var result =
                        await userManager.AddToRoleAsync(
                            adminUser,
                            role);

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            string.Join(
                                Environment.NewLine,
                                result.Errors.Select(x => x.Description)));
                    }
                }
            }

            // ---------------------------------------------------------
            // Users to Seed
            // ---------------------------------------------------------

            var usersToSeed = new[]
            {
        // Directors
        new
        {
            Email = "director1@itender.co.za",
            FirstName = "Director",
            LastName = "One",
            EmployeeNumber = "DIR001",
            Role = Roles.Director
        },
        new
        {
            Email = "director2@itender.co.za",
            FirstName = "Director",
            LastName = "Two",
            EmployeeNumber = "DIR002",
            Role = Roles.Director
        },
        new
        {
            Email = "director3@itender.co.za",
            FirstName = "Director",
            LastName = "Three",
            EmployeeNumber = "DIR003",
            Role = Roles.Director
        },

        // Compliance Managers
        new
        {
            Email = "manager1@itender.co.za",
            FirstName = "Compliance",
            LastName = "Manager One",
            EmployeeNumber = "MGR001",
            Role = Roles.ComplianceManager
        },
        new
        {
            Email = "manager2@itender.co.za",
            FirstName = "Compliance",
            LastName = "Manager Two",
            EmployeeNumber = "MGR002",
            Role = Roles.ComplianceManager
        },
        new
        {
            Email = "manager3@itender.co.za",
            FirstName = "Compliance",
            LastName = "Manager Three",
            EmployeeNumber = "MGR003",
            Role = Roles.ComplianceManager
        },

        // Compliance Officers
        new
        {
            Email = "officer1@itender.co.za",
            FirstName = "Compliance",
            LastName = "Officer One",
            EmployeeNumber = "OFF001",
            Role = Roles.ComplianceOfficer
        },
        new
        {
            Email = "officer2@itender.co.za",
            FirstName = "Compliance",
            LastName = "Officer Two",
            EmployeeNumber = "OFF002",
            Role = Roles.ComplianceOfficer
        },
        new
        {
            Email = "officer3@itender.co.za",
            FirstName = "Compliance",
            LastName = "Officer Three",
            EmployeeNumber = "OFF003",
            Role = Roles.ComplianceOfficer
        }
    };

            const string seededPassword = "Test@123";

            // ---------------------------------------------------------
            // Seed Users
            // ---------------------------------------------------------

            foreach (var seedData in usersToSeed)
            {
                var user =
                    await userManager.FindByEmailAsync(
                        seedData.Email);

                // Create user if it doesn't exist
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = seedData.Email,
                        Email = seedData.Email,
                        FirstName = seedData.FirstName,
                        LastName = seedData.LastName,
                        EmailConfirmed = true
                    };

                    var result =
                        await userManager.CreateAsync(
                            user,
                            seededPassword);

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            string.Join(
                                Environment.NewLine,
                                result.Errors.Select(x => x.Description)));
                    }
                }

                // -----------------------------------------------------
                // Assign Role
                // -----------------------------------------------------

                if (!await userManager.IsInRoleAsync(
                        user,
                        seedData.Role))
                {
                    var result =
                        await userManager.AddToRoleAsync(
                            user,
                            seedData.Role);

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            string.Join(
                                Environment.NewLine,
                                result.Errors.Select(x => x.Description)));
                    }
                }

                // -----------------------------------------------------
                // Create Agent for Compliance Officers
                // -----------------------------------------------------

                if (seedData.Role == Roles.ComplianceOfficer)
                {
                    var agent =
                        await dbContext.Agents
                            .FirstOrDefaultAsync(
                                x => x.UserId == user.Id);

                    if (agent == null)
                    {
                        agent = new Agent
                        {
                            Id = Guid.NewGuid(),

                            UserId = user.Id,

                            EmployeeNumber =
                                seedData.EmployeeNumber,

                            FullName =
                                $"{user.FirstName} {user.LastName}",

                            Department = "Compliance",

                            Email =
                                user.Email ?? seedData.Email,

                            PhoneNumber = "0000000000",

                            IsActive = true,

                            IsManager = false,

                            AutoAssignEnabled = true,

                            MaximumOpenCases = 10,

                            DisplayOrder = 1,

                            Level = AgentLevel.Senior,

                            JobTitle = "Compliance Officer",

                            Notes = "Seeded test agent."
                        };

                        await dbContext.Agents.AddAsync(agent);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}