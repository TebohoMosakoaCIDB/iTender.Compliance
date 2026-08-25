using iTender.Compliance.Domain.Enums;

namespace iTender.Compliance.Application.Exceptions
{
    public class InvalidCaseTransitionException : Exception
    {
        public CaseStatus CurrentStatus { get; }
        public CaseStatus RequestedStatus { get; }

        public InvalidCaseTransitionException(
            CaseStatus currentStatus,
            CaseStatus requestedStatus)
            : base(
                $"Invalid compliance case transition from " +
                $"'{currentStatus}' to '{requestedStatus}'.")
        {
            CurrentStatus = currentStatus;
            RequestedStatus = requestedStatus;
        }
    }
}
