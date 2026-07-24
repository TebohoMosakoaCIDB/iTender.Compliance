using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            AuditAction action,
            AuditEntity entity,
            Guid entityId,
            string description,
            Guid? userId = null,
            CancellationToken cancellationToken = default);
    }
}
