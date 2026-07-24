using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services.SigningHub
{
    public class SigningRequestService : ISigningRequestService
    {
        private readonly ISigningRequestRepository _repository;
        private readonly ICaseLetterRepository _caseLetterRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SigningRequestService(
            ISigningRequestRepository repository,
            ICaseLetterRepository caseLetterRepository,
            IAgentRepository agentRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _caseLetterRepository = caseLetterRepository;
            _agentRepository = agentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateAsync(
            Guid caseLetterId,
            Guid signerId,
            string signerName,
            string signerEmail,
            CancellationToken cancellationToken = default)
        {
            var letter = await _caseLetterRepository.GetByIdAsync(
                caseLetterId,
                cancellationToken);

            if (letter == null)
                throw new InvalidOperationException("Case letter not found.");

            var request = new SigningRequest
            {
                CaseLetterId = caseLetterId,

                FileName = letter.FileName,

                OriginalDocumentPath = letter.FilePath,

                Status = SigningRequestStatus.Draft,

                SignerId = signerId,
                SignerName = signerName,
                SignerEmail = signerEmail
            };

            await _repository.AddAsync(request, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return request.Id;
        }

        public Task<SigningRequest?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task MarkUploadedAsync(
            Guid signingRequestId,
            string workflowId,
            string documentId,
            CancellationToken cancellationToken = default)
        {
            var request = await _repository.GetByIdAsync(
                signingRequestId,
                cancellationToken);

            if (request == null)
                return;

            request.WorkflowId = workflowId;
            request.DocumentId = documentId;
            request.Status = SigningRequestStatus.PendingSignature;
            request.SentOn = DateTime.UtcNow;

            await _repository.UpdateAsync(request, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkSignedAsync(
            Guid signingRequestId,
            string signedDocumentPath,
            CancellationToken cancellationToken = default)
        {
            var request = await _repository.GetByIdAsync(
                signingRequestId,
                cancellationToken);

            if (request == null)
                return;

            request.Status = SigningRequestStatus.Signed;
            request.SignedDocumentPath = signedDocumentPath;
            request.SignedOn = DateTime.UtcNow;

            await _repository.UpdateAsync(request, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkFailedAsync(
            Guid signingRequestId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var request = await _repository.GetByIdAsync(
                signingRequestId,
                cancellationToken);

            if (request == null)
                return;

            request.Status = SigningRequestStatus.Failed;
            request.FailureReason = reason;

            await _repository.UpdateAsync(request, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
