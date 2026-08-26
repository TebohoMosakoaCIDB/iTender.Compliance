
using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<DocumentResult> GenerateInstructionLetterAsync(
            SendInstructionLetterModel model,
            CancellationToken cancellationToken = default);

        Task<DocumentResult> GenerateReminderLetterAsync(
            SendReminderLetterModel model,
            CancellationToken cancellationToken = default);

        Task<DocumentResult> GenerateContraventionNoticeAsync(
            SendContraventionNoticeModel model,
            CancellationToken cancellationToken = default);

        Task<DocumentResult> GenerateAgsaReferralDocumentAsync(
            AgsaReferralDocumentModel model,
            CancellationToken cancellationToken = default);
    }
}