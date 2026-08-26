using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IObjectionService
    {
        Task<Guid> RecordObjectionAsync(
            RecordObjectionModel model,
            CancellationToken cancellationToken = default);

        Task ResolveObjectionAsync(
            ResolveObjectionModel model,
            Guid resolvedByAgentId,
            CancellationToken cancellationToken = default);
    }
}
