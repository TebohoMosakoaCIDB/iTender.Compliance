using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    /// <summary>
    /// Orchestrates the manager sign-off gate: sends a generated but not-yet-sent
    /// letter to SigningHub for the assigned Manager to approve, then, once
    /// signed, hands it back to <see cref="ICorrespondenceService"/> so the
    /// letter can actually be emailed to the client.
    /// </summary>
    public interface IDocumentSigningService
    {
        /// <summary>Creates a SigningRequest for the given letter and pushes it to SigningHub for the manager to sign.
        /// Does not throw on SigningHub failure - the request is marked Failed and the caller can retry/fall back.</summary>
        Task RequestApprovalAsync(
            CaseLetter letter,
            Agent manager,
            CancellationToken cancellationToken = default);

        /// <summary>Checks every outstanding SigningRequest against SigningHub. Completed ones are downloaded
        /// and marked Signed; rejected ones are marked Failed. Returns the case letters that changed state so
        /// the caller (which owns ICorrespondenceService) can move each case forward without a circular
        /// dependency between the two services.</summary>
        Task<SigningPollResult> PollAndCompleteAsync(
            CancellationToken cancellationToken = default);
    }
}