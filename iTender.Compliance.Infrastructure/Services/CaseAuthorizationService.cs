using iTender.Compliance.Application.Common;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CaseAuthorizationService : ICaseAuthorizationService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IAgentRepository _agentRepository;

        public CaseAuthorizationService(
                ICurrentUserService currentUser,
                UserManager<ApplicationUser> userManager,
                IComplianceCaseRepository caseRepository,
                IAgentRepository agentRepository)
        {
            _currentUser = currentUser;
            _userManager = userManager;
            _caseRepository = caseRepository;
            _agentRepository = agentRepository;
        }

        public async Task<bool> CanAssignAsync(Guid caseId, CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await IsAdministratorAsync(user)
                || await IsSupervisorAsync(user);
        }

        public async Task<bool> CanCloseAsync(Guid caseId, CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await IsAdministratorAsync(user)
                || await IsSupervisorAsync(user);
        }
        public Task<bool> CanEditAsync(Guid caseId, CancellationToken cancellationToken = default)
        {
            return CanViewAsync(caseId, cancellationToken);
        }

        public async Task<bool> CanViewAsync(Guid caseId, CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            if (await IsAdministratorAsync(user))
                return true;

            if (await IsSupervisorAsync(user))
                return true;

            var agent = await GetCurrentAgentAsync(cancellationToken);

            if (agent == null)
                return false;

            var complianceCase =
                await _caseRepository.GetByIdAsync(caseId, cancellationToken);

            return complianceCase.AgentId == agent.Id;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            if (_currentUser.UserId == null)
                return null;

            return await _userManager.FindByIdAsync(_currentUser.UserId.ToString()!);
        }

        private async Task<bool> IsAdministratorAsync(ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(
                user,
                Roles.Administrator);
        }

        private async Task<bool> IsSupervisorAsync(ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(
                user,
                Roles.Supervisor);
        }

        private async Task<Agent?> GetCurrentAgentAsync(CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return null;

            return await _agentRepository.GetByUserIdAsync(
                _currentUser.UserId.Value,
                cancellationToken);
        }

        public async Task<bool> CanViewAllCasesAsync()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await IsAdministratorAsync(user)
                || await IsSupervisorAsync(user);
        }

        public async Task<Guid?> GetCurrentAgentIdAsync()
        {
            var agent = await GetCurrentAgentAsync(CancellationToken.None);

            return agent?.Id;
        }
    }
}
