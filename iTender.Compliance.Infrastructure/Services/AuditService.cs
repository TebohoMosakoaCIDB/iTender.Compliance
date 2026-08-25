using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(
            AuditAction action,
            AuditEntity entity,
            Guid entityId,
            string description,
            Guid? userId = null,
            CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Description = description,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
