using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;

namespace iTender.Compliance.Infrastructure.Services.SigningHub
{
    public class DocumentSigningService : IDocumentSigningService
    {
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly ISigningRequestRepository _signingRequestRepository;
        private readonly ISigningRequestService _signingRequestService;
        private readonly ISigningHubService _signingHubService;

        public DocumentSigningService(
            ICaseLetterRepository caseLetterRepository,
            IComplianceCaseRepository caseRepository,
            IAgentRepository agentRepository,
            ISigningRequestRepository signingRequestRepository,
            ISigningRequestService signingRequestService,
            ISigningHubService signingHubService)
        {
            _caseLetterRepository = caseLetterRepository;
            _caseRepository = caseRepository;
            _agentRepository = agentRepository;
            _signingRequestRepository = signingRequestRepository;
            _signingRequestService = signingRequestService;
            _signingHubService = signingHubService;
        }

        public async Task<Guid> PrepareInstructionLetterForSigningAsync(
    Guid caseLetterId,
    CancellationToken cancellationToken = default)
        {
            var letter = await _caseLetterRepository.GetByIdAsync(
                caseLetterId,
                cancellationToken);

            if (letter == null)
                throw new InvalidOperationException("Case letter not found.");

            var complianceCase = await _caseRepository.GetByIdAsync(
                letter.ComplianceCaseId,
                cancellationToken);

            if (complianceCase == null)
                throw new InvalidOperationException("Compliance case not found.");

            if (complianceCase.AgentId == null)
                throw new InvalidOperationException("Case has not been assigned.");

            var agent = await _agentRepository.GetByIdAsync(
                complianceCase.AgentId.Value,
                cancellationToken);

            if (agent == null)
                throw new InvalidOperationException("Assigned agent not found.");

            return await _signingRequestService.CreateAsync(
                caseLetterId,
                agent.Id,
                agent.FullName,
                agent.Email,
                cancellationToken);
        }

        public Task<Guid> PrepareReminderLetterForSigningAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default)
        {
            return PrepareInstructionLetterForSigningAsync(
                caseLetterId,
                cancellationToken);
        }

        public async Task UploadPendingDocumentsAsync(
            CancellationToken cancellationToken = default)
        {
            var pendingRequests =
                await _signingRequestRepository.GetPendingAsync(
                    cancellationToken);

            foreach (var request in pendingRequests)
            {
                // OAuth

                // Upload PDF

                // Create Workflow

                // Assign Signer

                // MarkUploaded(...)
            }
        }

        public async Task RefreshSigningStatusesAsync(CancellationToken cancellationToken = default)
        {
            // Poll SigningHub

            // Update statuses

            // Download signed documents

            await Task.CompletedTask;
        }
    }
}
