using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ClientResponseService : IClientResponseService
    {
        private readonly IClientResponseRepository _responseRepository;
        private readonly IComplianceCaseRepository _caseRepository;
        private readonly ICaseLetterRepository _letterRepository;
        private readonly IComplianceFindingRepository _findingRepository;
        private readonly ICaseAuthorizationService _authorizationService;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;

        public ClientResponseService(
            IComplianceFindingRepository findingRepository,
            IClientResponseRepository responseRepository,
            IComplianceCaseRepository caseRepository,
            ICaseLetterRepository letterRepository,
            ICaseAuthorizationService authorizationService,
            IAuditService auditService,
            ICurrentUserService currentUser)
        {
            _responseRepository = responseRepository;
            _caseRepository = caseRepository;
            _letterRepository = letterRepository;
            _authorizationService = authorizationService;
            _findingRepository = findingRepository;
            _auditService = auditService;
            _currentUser = currentUser;
        }

        public async Task<Guid> SubmitAsync(
            Guid complianceCaseId,
            Guid caseLetterId,
            string? clientComments,
            CancellationToken cancellationToken = default)
        {
            var complianceCase = await GetCaseAsync(
                complianceCaseId,
                cancellationToken);

            var letter = await GetLetterAsync(
                caseLetterId,
                cancellationToken);

            if (letter.ComplianceCaseId != complianceCase.Id)
                throw new InvalidOperationException(
                    "The selected letter does not belong to this compliance case.");

            if (complianceCase.Status == CaseStatus.Closed ||
                complianceCase.Status == CaseStatus.ReferredForEnforcement)
            {
                throw new InvalidOperationException(
                    "A response cannot be submitted against a closed or referred case.");
            }

            var hasAnyResponse = await _responseRepository.HasAnyResponseAsync(caseLetterId, cancellationToken);

            if (hasAnyResponse)
            {
                throw new InvalidOperationException(
                    "A response has already been submitted for this letter. " +
                    "If additional information was requested, use the resubmission process.");
            }

            if (await _responseRepository.HasPendingReviewAsync(
                caseLetterId,
                cancellationToken))
            {
                throw new InvalidOperationException(
                    "This letter already has a response awaiting review.");
            }

            var now = DateTime.UtcNow;

            var response = new ClientResponse
            {
                ComplianceCaseId = complianceCaseId,
                CaseLetterId = caseLetterId,
                SubmittedOn = now,
                ClientComments = clientComments,
                Status = ClientResponseStatus.Received,
                CreatedOn = now
            };

            await _responseRepository.AddAsync(
                response,
                cancellationToken);

            if (!letter.RespondedOn.HasValue)
            {
                letter.RespondedOn = now;
                letter.ResponseComments = clientComments;

                await _letterRepository.UpdateAsync(
                    letter,
                    cancellationToken);
            }

            // A response is now awaiting officer review.
            // Do not close the case or mark it compliant here.
            complianceCase.ModifiedOn = now;

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ResponseReceived,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Client response {response.Id} received for {letter.Type} letter #{letter.LetterNumber}. Awaiting officer review.",
                cancellationToken: cancellationToken);

            return response.Id;
        }

        public async Task BeginReviewAsync(
            Guid responseId,
            CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(
                responseId,
                cancellationToken);

            await AuthorizeOfficerAsync(
                response.ComplianceCaseId,
                cancellationToken);

            if (response.Status != ClientResponseStatus.Received &&
                response.Status != ClientResponseStatus.Resubmitted)
            {
                throw new InvalidOperationException(
                    "Only newly received or resubmitted responses can be reviewed.");
            }

            response.Status = ClientResponseStatus.UnderReview;
            response.ReviewedByAgentId =
                await _authorizationService.GetCurrentAgentIdAsync();
            response.ReviewedOn = DateTime.UtcNow;

            await _responseRepository.UpdateAsync(
                response,
                cancellationToken);

            await LogReviewActionAsync(
                response,
                "Client response review started.",
                cancellationToken);
        }

        public async Task RequestAdditionalInformationAsync(
            Guid responseId,
            string comments,
            DateTime? dueOn = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(comments))
                throw new ArgumentException(
                    "A reason for requesting additional information is required.",
                    nameof(comments));

            var response = await GetResponseAsync(
                responseId,
                cancellationToken);

            await AuthorizeOfficerAsync(
                response.ComplianceCaseId,
                cancellationToken);

            EnsureUnderReview(response);

            var now = DateTime.UtcNow;

            response.Status =
                ClientResponseStatus.AdditionalInformationRequested;

            response.ReviewComments = comments;
            response.ReviewedByAgentId =
                await _authorizationService.GetCurrentAgentIdAsync();
            response.ReviewedOn = now;
            response.InformationRequestedOn = now;
            response.InformationRequestDueOn = dueOn;

            await _responseRepository.UpdateAsync(
                response,
                cancellationToken);

            await LogReviewActionAsync(
                response,
                $"Additional information requested: {comments}",
                cancellationToken);
        }

        public async Task MarkIncompleteAsync(
            Guid responseId,
            string comments,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(comments))
                throw new ArgumentException(
                    "An explanation is required when marking a response incomplete.",
                    nameof(comments));

            var response = await GetResponseAsync(
                responseId,
                cancellationToken);

            await AuthorizeOfficerAsync(
                response.ComplianceCaseId,
                cancellationToken);

            EnsureUnderReview(response);

            response.Status = ClientResponseStatus.Incomplete;
            response.ReviewComments = comments;
            response.ReviewedByAgentId =
                await _authorizationService.GetCurrentAgentIdAsync();
            response.ReviewedOn = DateTime.UtcNow;

            await _responseRepository.UpdateAsync(
                response,
                cancellationToken);

            await LogReviewActionAsync(
                response,
                $"Response marked incomplete: {comments}",
                cancellationToken);
        }



        public async Task AcceptAsync(
            Guid responseId,
            string? comments,
            CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(
                responseId,
                cancellationToken);

            await AuthorizeOfficerAsync(
                response.ComplianceCaseId,
                cancellationToken);

            EnsureUnderReview(response);

            var complianceCase = await GetCaseAsync(
                response.ComplianceCaseId,
                cancellationToken);

            if (complianceCase.Status == CaseStatus.Closed ||
                complianceCase.Status == CaseStatus.ReferredForEnforcement)
            {
                throw new InvalidOperationException(
                    "A response cannot be accepted for a closed or referred case.");
            }

            var letter = await GetLetterAsync(
                response.CaseLetterId,
                cancellationToken);

            var now = DateTime.UtcNow;
            var agentId = await _authorizationService.GetCurrentAgentIdAsync();

            response.Status = ClientResponseStatus.Accepted;
            response.ReviewComments = comments;
            response.ReviewedByAgentId = agentId;
            response.ReviewedOn = now;

            letter.RespondedOn ??= response.SubmittedOn;
            letter.Accepted = true;
            letter.ResponseComments = response.ClientComments;

            // Resolve the finding associated with this letter, if one exists.
            if (letter.ComplianceFindingId.HasValue)
            {
                var finding = await _findingRepository.GetByIdAsync(
                    letter.ComplianceFindingId.Value,
                    cancellationToken);

                if (finding != null &&
                    finding.ComplianceCaseId == complianceCase.Id)
                {
                    finding.IsResolved = true;
                    finding.ResolvedOn = now;
                    finding.VerificationStatus = VerificationStatus.ManualPassed;

                    await _findingRepository.UpdateAsync(
                        finding,
                        cancellationToken);
                }
            }

            // Determine whether all findings have been resolved.
            var findings = await _findingRepository.GetByCaseIdAsync(
                complianceCase.Id,
                cancellationToken);

            var findingsList = findings.ToList();

            var canCloseCase =
                findingsList.Any() &&
                findingsList.All(x => x.IsResolved);

            complianceCase.Comments = comments;
            complianceCase.ModifiedOn = now;

            if (canCloseCase)
            {
                complianceCase.Status = CaseStatus.Closed;
                complianceCase.Outcome = ComplianceOutcome.Compliant;
                complianceCase.ClosedDate = now;
                complianceCase.ClosureReason = CaseClosureReason.ClientComplied;
            }

            await _responseRepository.UpdateAsync(
                response,
                cancellationToken);

            await _letterRepository.UpdateAsync(
                letter,
                cancellationToken);

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            if (canCloseCase)
            {
                await _auditService.LogAsync(
                    AuditAction.CaseClosed,
                    AuditEntity.ComplianceCase,
                    complianceCase.Id,
                    "Case closed after all compliance findings were resolved through accepted client responses.",
                    _currentUser.UserId,
                    cancellationToken);
            }
            else
            {
                await _auditService.LogAsync(
                    AuditAction.Updated,
                    AuditEntity.ComplianceCase,
                    complianceCase.Id,
                    "Client response accepted. Case remains open because other compliance findings are outstanding or no findings are linked.",
                    _currentUser.UserId,
                    cancellationToken);
            }
        }

        public async Task RejectAsync(
            Guid responseId,
            string comments,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(comments))
                throw new ArgumentException(
                    "A reason for rejecting the response is required.",
                    nameof(comments));

            var response = await GetResponseAsync(
                responseId,
                cancellationToken);

            await AuthorizeOfficerAsync(
                response.ComplianceCaseId,
                cancellationToken);

            EnsureUnderReview(response);

            var complianceCase = await GetCaseAsync(
                response.ComplianceCaseId,
                cancellationToken);

            var letter = await GetLetterAsync(
                response.CaseLetterId,
                cancellationToken);

            var now = DateTime.UtcNow;

            response.Status = ClientResponseStatus.Rejected;
            response.ReviewComments = comments;
            response.ReviewedByAgentId =
                await _authorizationService.GetCurrentAgentIdAsync();
            response.ReviewedOn = now;

            letter.RespondedOn = response.SubmittedOn;
            letter.Accepted = false;
            letter.ResponseComments = comments;

            // Preserve the existing case lifecycle status.
            // Rejection is recorded against the response and letter.
            // The workflow/basket service will determine the next action.

            complianceCase.ModifiedOn = now;

            await _responseRepository.UpdateAsync(
                response,
                cancellationToken);

            await _letterRepository.UpdateAsync(
                letter,
                cancellationToken);

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await LogReviewActionAsync(
                response,
                $"Client response rejected: {comments}",
                cancellationToken);
        }

        private async Task<ClientResponse> GetResponseAsync(
            Guid responseId,
            CancellationToken cancellationToken)
        {
            return await _responseRepository.GetByIdAsync(
                responseId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Client response not found.");
        }

        private async Task<ComplianceCase> GetCaseAsync(
            Guid caseId,
            CancellationToken cancellationToken)
        {
            return await _caseRepository.GetByIdAsync(
                caseId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Compliance case not found.");
        }

        public async Task<Guid> ResubmitAsync(
    Guid previousResponseId,
    string? clientComments,
    CancellationToken cancellationToken = default)
        {
            // Retrieve the original response.
            var previousResponse = await GetResponseAsync(
                previousResponseId,
                cancellationToken);

            // Only responses returned for correction can be resubmitted.
            if (previousResponse.Status != ClientResponseStatus.AdditionalInformationRequested &&
                previousResponse.Status != ClientResponseStatus.Incomplete)
            {
                throw new InvalidOperationException(
                    "This response is not eligible for resubmission.");
            }

            // Prevent multiple submissions against the same previous response.
            var alreadyResubmitted = await _responseRepository.HasResubmissionAsync(
                previousResponseId,
                cancellationToken);

            if (alreadyResubmitted)
            {
                throw new InvalidOperationException(
                    "A resubmission has already been received for this response.");
            }

            // Retrieve and validate the associated case.
            var complianceCase = await GetCaseAsync(
                previousResponse.ComplianceCaseId,
                cancellationToken);

            if (complianceCase.Status == CaseStatus.Closed ||
                complianceCase.Status == CaseStatus.ReferredForEnforcement)
            {
                throw new InvalidOperationException(
                    "A response cannot be resubmitted for a closed or referred case.");
            }

            // Retrieve the associated letter.
            var letter = await GetLetterAsync(
                previousResponse.CaseLetterId,
                cancellationToken);

            if (letter.ComplianceCaseId != complianceCase.Id)
            {
                throw new InvalidOperationException(
                    "The response does not belong to the specified compliance case.");
            }

            // Prevent another response from being submitted while one is awaiting review.
            var pendingReview = await _responseRepository.HasPendingReviewAsync(
                previousResponse.CaseLetterId,
                cancellationToken);

            if (pendingReview)
            {
                throw new InvalidOperationException(
                    "There is already a response awaiting review for this letter.");
            }

            var now = DateTime.UtcNow;

            var resubmission = new ClientResponse
            {
                ComplianceCaseId = previousResponse.ComplianceCaseId,
                CaseLetterId = previousResponse.CaseLetterId,
                PreviousResponseId = previousResponse.Id,
                SubmittedOn = now,
                ClientComments = clientComments,
                Status = ClientResponseStatus.Resubmitted
            };

            await _responseRepository.AddAsync(
                resubmission,
                cancellationToken);

            // Update the case modification timestamp without changing its lifecycle status.
            complianceCase.ModifiedOn = now;

            await _caseRepository.UpdateAsync(
                complianceCase,
                cancellationToken);

            await _auditService.LogAsync(
                AuditAction.ResponseReceived,
                AuditEntity.ComplianceCase,
                complianceCase.Id,
                $"Client resubmitted a response for letter {letter.LetterNumber}. Previous response ID: {previousResponse.Id}.",
                _currentUser.UserId,
                cancellationToken);

            return resubmission.Id;
        }

        private async Task<CaseLetter> GetLetterAsync(
            Guid letterId,
            CancellationToken cancellationToken)
        {
            return await _letterRepository.GetByIdAsync(
                letterId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Case letter not found.");
        }

        private async Task AuthorizeOfficerAsync(
            Guid caseId,
            CancellationToken cancellationToken)
        {
            if (!await _authorizationService.CanEditAsync(
                caseId,
                cancellationToken))
            {
                throw new UnauthorizedAccessException(
                    "You are not authorised to review this compliance case.");
            }
        }

        private static void EnsureUnderReview(ClientResponse response)
        {
            if (response.Status != ClientResponseStatus.UnderReview)
            {
                throw new InvalidOperationException(
                    "The response must be under review before this action can be performed.");
            }
        }

        private async Task LogReviewActionAsync(
            ClientResponse response,
            string description,
            CancellationToken cancellationToken)
        {
            await _auditService.LogAsync(
                AuditAction.Updated,
                AuditEntity.ComplianceCase,
                response.ComplianceCaseId,
                description,
                _currentUser.UserId,
                cancellationToken);
        }
    }
}