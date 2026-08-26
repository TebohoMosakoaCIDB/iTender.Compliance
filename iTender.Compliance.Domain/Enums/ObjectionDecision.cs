namespace iTender.Compliance.Domain.Enums
{
    public enum ObjectionDecision
    {
        /// <summary>Objection accepted - the client's position stands.</summary>
        Upheld = 1,

        /// <summary>Objection rejected - the case proceeds through the workflow.</summary>
        Overruled = 2
    }
}
