namespace iTender.Compliance.Application.Interfaces.Services
{
    public interface IEscalationService
    {
        /// <summary>Instruction/Reminder letters past their due date with no response -&gt; issue a Contravention Notice.</summary>
        Task<int> EscalateOverdueInstructionLettersAsync(
            CancellationToken cancellationToken = default);

        /// <summary>Contravention Notices past their due date with no response -&gt; refer for enforcement (AGSA).</summary>
        Task<int> EscalateOverdueContraventionNoticesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>Runs both escalation passes. Intended to be called on a schedule.</summary>
        Task RunEscalationCycleAsync(
            CancellationToken cancellationToken = default);
    }
}
