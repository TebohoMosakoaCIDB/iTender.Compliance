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

        public async Task<bool> CanAssignAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await IsDirectorAsync(user)
                || await IsRegulatoryComplianceManagerAsync(user);
        }

        public async Task<bool> CanCloseAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await IsDirectorAsync(user)
                || await IsRegulatoryComplianceManagerAsync(user);
        }

        public async Task<bool> CanEditAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            // Director can edit any case
            if (await IsDirectorAsync(user))
                return true;

            // Regulatory Compliance Manager can edit any case
            if (await IsRegulatoryComplianceManagerAsync(user))
                return true;

            // Compliance Officer can edit cases assigned to them
            var agent = await GetCurrentAgentAsync(cancellationToken);

            if (agent == null)
                return false;

            var complianceCase =
                await _caseRepository.GetByIdAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase == null)
                return false;

            return complianceCase.AgentId == agent.Id;
        }

        public async Task<bool> CanViewAsync(
            Guid caseId,
            CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            // Director can view all cases
            if (await IsDirectorAsync(user))
                return true;

            // Regulatory Compliance Manager can view all cases
            if (await IsRegulatoryComplianceManagerAsync(user))
                return true;

            // Compliance Officer can view assigned cases
            var agent = await GetCurrentAgentAsync(cancellationToken);

            if (agent == null)
                return false;

            var complianceCase =
                await _caseRepository.GetByIdAsync(
                    caseId,
                    cancellationToken);

            if (complianceCase == null)
                return false;

            return complianceCase.AgentId == agent.Id;
        }

        public async Task<bool> CanViewAllCasesAsync()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return false;

            return await IsDirectorAsync(user)
                || await IsRegulatoryComplianceManagerAsync(user);
        }

        public async Task<Guid?> GetCurrentAgentIdAsync()
        {
            var agent = await GetCurrentAgentAsync(
                CancellationToken.None);

            return agent?.Id;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            if (_currentUser.UserId == null)
                return null;

            return await _userManager.FindByIdAsync(
                _currentUser.UserId.Value.ToString());
        }

        private async Task<bool> IsDirectorAsync(
            ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(
                user,
                Roles.Director);
        }

        private async Task<bool> IsRegulatoryComplianceManagerAsync(
            ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(
                user,
                Roles.ComplianceManager);
        }

        private async Task<bool> IsComplianceOfficerAsync(
            ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(
                user,
                Roles.ComplianceOfficer);
        }

        private async Task<bool> IsComplianceAdministratorAsync(
            ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(
                user,
                Roles.ComplianceAdministrator);
        }

        private async Task<Agent?> GetCurrentAgentAsync(
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                return null;

            return await _agentRepository.GetByUserIdAsync(
                _currentUser.UserId.Value,
                cancellationToken);
        }
    }
}
