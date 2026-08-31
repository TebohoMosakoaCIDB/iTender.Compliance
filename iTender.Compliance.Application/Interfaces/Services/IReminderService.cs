using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IReminderService
    {
        /// <summary>Sends a reminder letter for every case whose Instruction Letter has gone unanswered
        /// past the configured delay. Returns how many reminders were sent.</summary>
        Task<int> ProcessRemindersAsync(
            CancellationToken cancellationToken = default);
    }
}