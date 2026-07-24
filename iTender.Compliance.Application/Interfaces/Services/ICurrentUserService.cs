namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? FullName { get; }
    }
}
