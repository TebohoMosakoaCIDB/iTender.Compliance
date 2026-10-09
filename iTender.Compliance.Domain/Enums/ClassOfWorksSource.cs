namespace iTender.Compliance.Domain.Enums
{
    public enum ClassOfWorksSource
    {
        NotSpecified,          // employer said nothing, we couldn't infer
        SpecifiedByAdvert,     // employer explicitly stated it in the advert
        InferredByCategory,    // category mapping produced it, description agrees
        InferredByKeywords,    // text resolver produced it
        UnresolvedConflict,    // category implies X, description disagrees
        ManuallySet            // CO overrode
    }
}
