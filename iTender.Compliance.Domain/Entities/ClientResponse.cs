using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Domain.Entities
{
    public class ClientResponse : BaseEntity
    {
        public Guid ComplianceCaseId { get; set; }

        public Guid CaseLetterId { get; set; }

        /// <summary>
        /// Links a resubmission to the previous client response.
        /// Null indicates that this is the original submission.
        /// </summary>
        public Guid? PreviousResponseId { get; set; }

        /// <summary>
        /// Date and time the client submitted the response.
        /// </summary>
        public DateTime SubmittedOn { get; set; }

        /// <summary>
        /// Comments supplied by the client.
        /// </summary>
        public string? ClientComments { get; set; }

        public ClientResponseStatus Status { get; set; }
            = ClientResponseStatus.Received;

        /// <summary>
        /// Compliance Officer who reviewed the response.
        /// </summary>
        public Guid? ReviewedByAgentId { get; set; }

        public DateTime? ReviewedOn { get; set; }

        /// <summary>
        /// Internal review comments recorded by the Compliance Officer.
        /// </summary>
        public string? ReviewComments { get; set; }

        /// <summary>
        /// When additional information was requested.
        /// </summary>
        public DateTime? InformationRequestedOn { get; set; }

        /// <summary>
        /// Optional deadline for the client to provide additional information.
        /// </summary>
        public DateTime? InformationRequestDueOn { get; set; }

        // Navigation properties

        public virtual ComplianceCase ComplianceCase { get; set; } = null!;

        public virtual CaseLetter CaseLetter { get; set; } = null!;

        public virtual ClientResponse? PreviousResponse { get; set; }

        public virtual ICollection<ClientResponse> Resubmissions { get; set; }
            = new List<ClientResponse>();

        public virtual ICollection<ClientResponseDocument> Documents { get; set; }
            = new List<ClientResponseDocument>();
    }
}