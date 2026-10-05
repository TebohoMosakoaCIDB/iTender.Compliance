using iTender.Compliance.Domain.Entities;

namespace iTender.Compliance.Application.Interfaces.Services
{
    /// <summary>
    /// Tries to determine a tender's CIDB class of works when the source (eTenders/OCDS) didn't
    /// supply one directly - which is always, since OCDS only carries a generic works/goods/services
    /// category, never a CIDB-specific class code. Searches the advert's own text against CIDB's
    /// real class list from the CRM instead.
    /// </summary>
    public interface IClassOfWorksResolver
    {
        /// <summary>Returns the matched CIDB class name, or null if nothing in the advert text
        /// matched a known class closely enough to be confident about it.</summary>
        Task<string?> ResolveAsync(
            Tender tender,
            CancellationToken cancellationToken = default);
    }
}
