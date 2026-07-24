using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReminderService
    {
        Task ProcessRemindersAsync(
            CancellationToken cancellationToken = default);
    }
}
