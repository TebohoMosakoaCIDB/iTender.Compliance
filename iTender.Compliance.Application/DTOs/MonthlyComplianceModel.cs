namespace iTender.Compliance.Application.DTOs
{
    public class MonthlyComplianceModel
    {
        public string Month { get; set; } = "";

        public int Compliant { get; set; }

        public int NonCompliant { get; set; }
    }
}
