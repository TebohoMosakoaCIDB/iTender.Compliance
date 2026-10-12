namespace iTender.Compliance.Application.Interfaces.Services
{
    // iTender.Compliance.Application/Interfaces/Services/IOfficerScopeService.cs
    public interface IOfficerScopeService
    {
        /// <summary>
        /// Returns the current user's AgentId if they are a Compliance Officer.
        /// Returns null for Managers and Administrators (who see everything).
        /// Returns null if the user has no linked Agent record.
        /// </summary>
        Task<Guid?> GetCurrentOfficerScopeAsync(CancellationToken ct = default);
    }
}
