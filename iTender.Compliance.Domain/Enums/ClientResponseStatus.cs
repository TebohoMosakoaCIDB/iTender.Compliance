namespace iTender.Compliance.Domain.Enums
{
    public enum ClientResponseStatus
    {
        /// <summary>
        /// A client submission has been received and is awaiting review.
        /// </summary>
        Received = 1,

        /// <summary>
        /// A Compliance Officer is reviewing the submission.
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// Additional information has been requested from the client.
        /// </summary>
        AdditionalInformationRequested = 3,

        /// <summary>
        /// The submission has been assessed as incomplete.
        /// </summary>
        Incomplete = 4,

        /// <summary>
        /// The client has submitted a further response following a request
        /// for additional information or an incomplete submission.
        /// </summary>
        Resubmitted = 5,

        /// <summary>
        /// The Compliance Officer has verified that the response is satisfactory.
        /// </summary>
        Accepted = 6,

        /// <summary>
        /// The Compliance Officer has determined that the response is unsatisfactory.
        /// </summary>
        Rejected = 7
    }
}