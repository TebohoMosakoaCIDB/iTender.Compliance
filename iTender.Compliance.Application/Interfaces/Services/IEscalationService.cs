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

        /// <summary>CIDB finalized rule: any open case not resolved within 30 calendar days of being allocated
        /// to a Compliance Officer is referred for enforcement, regardless of where it sits in the IL/CN cycle.
        /// A safety net alongside (not a replacement for) the CN-overdue trigger above.</summary>
        Task<int> EscalateStaleCasesToAgsaAsync(
            CancellationToken cancellationToken = default);

        /// <summary>Runs all escalation passes. Intended to be called on a schedule.</summary>
        Task RunEscalationCycleAsync(
            CancellationToken cancellationToken = default);
    }
}