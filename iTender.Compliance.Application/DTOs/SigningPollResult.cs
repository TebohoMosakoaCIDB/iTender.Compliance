namespace iTender.Compliance.Application.DTOs
{
    public class SigningPollResult
    {
        /// <summary>CaseLetterIds whose Manager has now signed - ready to actually deliver to the client.</summary>
        public List<Guid> CompletedCaseLetterIds { get; set; } = [];

        /// <summary>CaseLetterIds the Manager declined, with the reason - the case should route back to the officer.</summary>
        public List<RejectedLetter> RejectedLetters { get; set; } = [];
    }

    public class RejectedLetter
    {
        public Guid CaseLetterId { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}