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

        /// <summary>Called once a Manager has signed off on a letter via SigningHub - moves the case
        /// forward and actually emails the letter to the client. Not intended to be called directly
        /// for letters that don't require approval; those are delivered immediately when sent.</summary>
        Task CompleteApprovedLetterAsync(
            Guid caseLetterId,
            CancellationToken cancellationToken = default);

        /// <summary>Called when a Manager rejects a letter via SigningHub - routes the case back
        /// to the assigned officer instead of leaving it stuck awaiting approval.</summary>
        Task HandleRejectedLetterAsync(
            Guid caseLetterId,
            string reason,
            CancellationToken cancellationToken = default);
    }
}