namespace iTender.Application.DTOs
{
    public class ContractModel
    {
        public Guid ContractId { get; set; }
        public Guid ParentContractId { get; set; }
        public Guid EmployerId { get; set; }
        public Guid TenderId { get; set; }
        public String TenderName { get; set; }
        public string EmployerName { get; set; }
        public string EmployerNumber { get; set; }
        public string CidbContractNumber { get; set; }
        public int FinancialPointsTotalString { get; set; }
        public int PreferencePointsOutString { get; set; }
        public string ContractStatus { get; set; }
        public string PracticalCompletionDate { get; set; }
        public string CreatedDate { get; set; }
        public Guid CreatedByExternalUserId { get; set; }
        public Guid LastModifiedByExternalUserId { get; set; }
        public string ContractEndDate { get; set; }
        public int ChangeRequestStatusId { get; set; }
        public string ChangeRequestStatus { get; set; }
        public List<string> contractors { get; set; }
        public string ContractNumber { get; set; }
        public string ContractDescription { get; set; }
        public int TypeOfContractId { get; set; }
        public string TypeOfContractName { get; set; }
        public string isPublicPrivatePartnership { get; set; }
        public int ContractPeriodYears { get; set; }
        public int ContractPeriodMonths { get; set; }
        public int ContractPeriodDays { get; set; }
        public decimal FinancialPointsValue { get; set; }
        public int FinancialPointsTotal { get; set; }
        public decimal PreferencePointsValue { get; set; }
        public int PreferencePointsTotal { get; set; }
        public string isExpandedPublicWorksProgramme { get; set; }
        public Guid ClassOfConstructionWorkId { get; set; }
        public string ClassOfConstructionWorkName { get; set; }
        public int TenderValueRangeId { get; set; }
        public string ProgrammeName { get; set; }
        public int EmergingEnterpriseSupportId { get; set; }
        public int FinancialManagementId { get; set; }
        public string OtherFinancialManagement { get; set; }
        public int ExpandedPublicWorksProgrammeId { get; set; }
        public Guid ContractorDevelopmentProgrammeId { get; set; }
        public int NationalContractorDevelopmentProgrammeId { get; set; }
        public int PublicPrivatePartnershipId { get; set; }
        public Guid ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string SubCategoryName { get; set; }
        public Guid MetroDistrictMunicipalityId { get; set; }
        public string MetroDistrictMunicipalityName { get; set; }
        public Guid LocalMunicipalityId { get; set; }
        public string LocalMunicipalityName { get; set; }
        public int GPSCoordinatesTypeId { get; set; }
        public string GPSLatDegrees { get; set; }
        public string GPSLatMinutes { get; set; }
        public string GPSLatSeconds { get; set; }
        public string GPSLonDegrees { get; set; }
        public string GPSLonMinutes { get; set; }
        public string GPSLonSeconds { get; set; }
        public string GPSLatitude { get; set; }

        public string GPSLongitude { get; set; }

        // Token: 0x17009162 RID: 37218
        // (get) Token: 0x06010DF6 RID: 69110 RVA: 0x00250592 File Offset: 0x0024E792
        // (set) Token: 0x06010DF7 RID: 69111 RVA: 0x0025059A File Offset: 0x0024E79A
        public string ContractValueRands
        {
            get
            {
                return this.m_Rands;
            }
            set
            {
                this.m_Rands = value;
            }
        }

        public string ContractValueCents
        {
            get
            {
                return this.m_Cents;
            }
            set
            {
                this.m_Cents = value;
            }
        }

        public string ContractInvoicedAmountRands
        {
            get
            {
                return this.m_InvoiceRands;
            }
            set
            {
                this.m_InvoiceRands = value;
            }
        }

        public string ContractInvoicedAmountCents
        {
            get
            {
                return this.m_InvoiceCents;
            }
            set
            {
                this.m_InvoiceCents = value;
            }
        }

        public decimal ContractValue { get; set; }
        public decimal ContractInvoicedAmount { get; set; }
        public int YearId
        {
            get
            {
                return this.m_Years;
            }
            set
            {
                this.m_Years = value;
            }
        }
        public int MonthId
        {
            get
            {
                return this.m_Months;
            }
            set
            {
                this.m_Months = value;
            }
        }
        public int DayId
        {
            get
            {
                return this.m_Days;
            }
            set
            {
                this.m_Days = value;
            }
        }
        public int ContractPeriod { get; set; }
        public int IsTermContractId { get; set; }
        public string DateAward { get; set; }
        public bool JBCC { get; set; }
        public bool GCC { get; set; }
        public bool FIDIC { get; set; }
        public bool NEC3 { get; set; }
        public bool OtherFormOfContract { get; set; }
        public string OtherFormOfContractReason { get; set; }
        public Decimal FinancialPoints { get; set; }
        public int FinancialPointsOutOfId { get; set; }
        public decimal PreferencePoints { get; set; }
        public int PreferencePointsOutOfId { get; set; }
        public int JobOpportunitiesOnAward { get; set; }
        public string BillsIndividualName { get; set; }
        public string BillsCompany { get; set; }
        public string BillsTelephone { get; set; }
        public string BillsEmail { get; set; }
        public int IsJV { get; set; }
        public int? JVShare { get; set; }
        public int NumberOfContractorsId { get; set; }
        public bool IsNonComplaint { get; set; }
        public bool NonComplaintChecked { get; set; }
        public bool Reasonable { get; set; }
        public bool NoUndueRisk { get; set; }
        public bool ComplyWithRegulation { get; set; }
        public int AverageAnnualContractId { get; set; }
        public string ContractValueInWords { get; set; }
        public int SolicitationTypeId { get; set; }
        public string ContractTitle { get; set; }
        public string Grade { get; set; }
        public string RequiredGrade { get; set; }
        public string ContractPeriodString { get; set; }
        public string NCDPLink
        {
            get
            {
                return "http://www.cidb.org.za/contractor/publications/ncdp_framework/default.aspx";
            }
        }

        // Token: 0x040010A2 RID: 4258
        private const string m_EmergingEnterpriseSupport = "Is this award being made in terms of regulation 25.(8) to an emerging enterprise who has a contractor grading designation lower than that required for the contract as contemplated in Part IV of the cidb Regulations? *";

        // Token: 0x040010A3 RID: 4259
        private const string m_FinancialManagement = "What is the nature of the support that will be provided to the contract concerned? *";

        // Token: 0x040010A4 RID: 4260
        private const string m_Satisfied = "Are you satisfied that the contractor has the potential to develop and qualify to be registered in that higher grade? *";

        // Token: 0x040010A5 RID: 4261
        private const string m_UnderExtremeConditions = "Does this Tender constitute procurement of goods or services under extreme conditions? *";

        // Token: 0x040010A6 RID: 4262
        private const string m_EmergencyDescription = "Provide a brief description of the emergency in terms of the definition provided in the PFMA or MFMA (no more than 100 words)";

        // Token: 0x040010A7 RID: 4263
        private const string m_EvaluateResponses = "Which of the following procedures were used to solicit bids prior to the contract being awarded?";

        // Token: 0x040010A8 RID: 4264
        private const string m_Reasonable = "The margin with which the tenderer exceeded his or her tender value range contemplated in the cidb regulation 17, is reasonable; (Less than 20% of the lower level of the required grade where the contractor is 1 grade lower)";

        // Token: 0x040010A9 RID: 4265
        private const string m_NoUndueRisk = "The award of the contract does not pose undue risk to the organ of state;";

        // Token: 0x040010AA RID: 4266
        private const string m_ComplyWithRegulation = "The tender offer in all other aspects comply with these Regulations";

        // Token: 0x040010AB RID: 4267
        private const string m_AverageAnnualContract = "Was the contract awarded using Regulation 25(1b) where the estimated tender value is based on the average annual contract value?";

        // Token: 0x040010AC RID: 4268
        private const string m_NationalContractorDevelopmentProgramme = "Is this targeted development programme in line with the National Contractor Development Programme (NCDP)as promoted by the cidb, national and provincial public works and other stakeholders?:";

        // Token: 0x040010AD RID: 4269
        private const string m_ContractorDevelopmentProgramme = "Contractor Development Programme: * (Note: Please ensure the CDP has been captured if it does not appear on the list.)";

        // Token: 0x040010AE RID: 4270
        private string m_Rands = "0";

        // Token: 0x040010AF RID: 4271
        private string m_Cents = "00";

        // Token: 0x040010B0 RID: 4272
        private string m_InvoiceRands = "0";

        // Token: 0x040010B1 RID: 4273
        private string m_InvoiceCents = "00";

        // Token: 0x040010B2 RID: 4274
        private int m_Years = 100000000;

        // Token: 0x040010B3 RID: 4275
        private int m_Months = 100000000;

        // Token: 0x040010B4 RID: 4276
        private int m_Days = 100000000;
    }
}
