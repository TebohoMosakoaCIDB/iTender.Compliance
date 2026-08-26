using iTender.Compliance.Application.DTOs;

namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(
            EmailMessageModel message,
            CancellationToken cancellationToken = default);
    }
}
