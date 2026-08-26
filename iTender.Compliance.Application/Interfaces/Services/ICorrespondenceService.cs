using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICorrespondenceService
    {
        Task SendInstructionLetterAsync(
           SendInstructionLetterModel model,
           CancellationToken cancellationToken = default);

        Task SendReminderLetterAsync(
            SendReminderLetterModel model,
            CancellationToken cancellationToken = default);

        Task<Guid> SendContraventionNoticeAsync(
            SendContraventionNoticeModel model,
            CancellationToken cancellationToken = default);

        Task CaptureResponseAsync(
            CaptureResponseModel model,
            CancellationToken cancellationToken = default);

        Task RecordResponseAsync(
            Guid caseLetterId,
            bool accepted,
            string? comments,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
