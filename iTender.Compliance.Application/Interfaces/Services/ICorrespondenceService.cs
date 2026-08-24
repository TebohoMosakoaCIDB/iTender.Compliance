namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICorrespondenceService
    {
        Task<byte[]> GenerateInstructionalLetterAsync(
            Guid complianceCaseId);

        Task<byte[]> GenerateContraventionNoticeAsync(
            Guid complianceCaseId);

        Task<byte[]> GenerateErratumAsync(Guid complianceCaseId);
    }
}
