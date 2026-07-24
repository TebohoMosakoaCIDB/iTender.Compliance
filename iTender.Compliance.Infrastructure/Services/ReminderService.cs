using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Infrastructure.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IComplianceCaseRepository _complianceCaseRepository;
        private readonly ISystemSettingService _systemSettingService;
        //private readonly ILe _letterService;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public ReminderService(
            IComplianceCaseRepository complianceCaseRepository, ISystemSettingService systemSettingService,
            IAuditService auditService, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _complianceCaseRepository = complianceCaseRepository;
            _systemSettingService = systemSettingService;
            _auditService = auditService;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessRemindersAsync(
            CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAsync();

            var cases = await _complianceCaseRepository
                .GetCasesAwaitingReminderAsync(
                    settings.ReminderDelayHours,
                    cancellationToken);

            foreach (var complianceCase in cases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    //await _letterService.GenerateReminderLetterAsync(
                    //    complianceCase.Id,
                    //    cancellationToken);

                    await _auditService.LogAsync(
                        AuditAction.Updated,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        "Automatic reminder letter generated.",
                        _currentUser.UserId,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    await _auditService.LogAsync(
                        AuditAction.Error,
                        AuditEntity.ComplianceCase,
                        complianceCase.Id,
                        $"Failed to generate reminder letter. {ex.Message}",
                        _currentUser.UserId,
                        cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
