namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IWorkClassificationValidator
    {
        Task<bool> ValidateAsync(string classOfWorks, string projectDescription, CancellationToken cancellationToken = default);
    }
}
