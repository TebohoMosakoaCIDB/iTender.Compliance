using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Services
{
    internal class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ComplianceDbContext _dbContext;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            ComplianceDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<ProfileModel?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(
                userId.ToString());

            if (user == null)
                return null;

            var agent = await _dbContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);

            return new ProfileModel
            {
                UserId = user.Id,

                FirstName = user.FirstName,
                LastName = user.LastName,

                Email = user.Email ?? string.Empty,

                PhoneNumber = user.PhoneNumber ?? string.Empty,

                EmployeeNumber = agent?.EmployeeNumber ?? string.Empty,

                Department = agent?.Department ?? string.Empty,

                JobTitle = agent?.JobTitle,

                Level = agent?.Level ?? default,

                Role = roles.FirstOrDefault() ?? string.Empty,

                IsActive = user.IsActive,

                LastLoginOn = user.LastLoginOn
            };
        }

        public async Task<bool> UpdateAsync(
            Guid userId,
            UpdateMyProfileModel model,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(
                userId.ToString());

            if (user == null)
                return false;

            var agent = await _dbContext.Agents
                .FirstOrDefaultAsync(
                    x => x.Email == user.Email,
                    cancellationToken);

            if (agent != null)
            {
                agent.EmployeeNumber = model.EmployeeNumber.Trim();
                agent.Department = model.Department.Trim();
                agent.JobTitle = string.IsNullOrWhiteSpace(model.JobTitle)
                    ? null
                    : model.JobTitle.Trim();

                agent.ModifiedOn = DateTime.UtcNow;

                _dbContext.Agents.Update(agent);
                await _dbContext.SaveChangesAsync();
            }

            // Identity information
            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim();            

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return false;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}