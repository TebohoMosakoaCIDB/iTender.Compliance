using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;

namespace iTender.Compliance.Infrastructure.Services
{
    public class OfficerScopeService : IOfficerScopeService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IAgentRepository _agentRepository;
        private readonly IUserService _userService;

        public OfficerScopeService(
            ICurrentUserService currentUser,
            IAgentRepository agentRepository,
            IUserService userService)
        {
            _currentUser = currentUser;
            _agentRepository = agentRepository;
            _userService = userService;
        }

        public async Task<Guid?> GetCurrentOfficerScopeAsync(CancellationToken ct = default)
        {
            if (!_currentUser.UserId.HasValue)
                return null;

            var userId = _currentUser.UserId.Value;

            // Managers and Admins see everything.
            if (await _userService.IsInRoleAsync(userId, "Manager", ct) ||
                await _userService.IsInRoleAsync(userId, "Administrator", ct))
                return null;

            // Otherwise, return the user's AgentId (may be null if unconfigured).
            return await _agentRepository.GetAgentIdByUserIdAsync(userId, ct);
        }
    }
}
