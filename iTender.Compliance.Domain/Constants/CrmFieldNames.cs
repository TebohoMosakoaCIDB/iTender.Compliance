using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Domain.Constants
{
    public class CrmFieldNames
    {
        public static class AccountFields
        {
            public const string Id = "accountid";
            public const string PrimaryContactId = "_primarycontactid_value";
            public const string CrsNumber = "nv_crsnumber";
            public const string Name = "name";
            public const string TradingAs = "nv_tradingas";
            public const string Telephone = "telephone1";
            public const string Email = "emailaddress1";
            public const string CsdNumber = "nv_csdnumber";
            public const string StatusCode = "statuscode";
            public const string Grade = "_nv_currentgradeid_value";
            public const string Enterprise = "_nv_enterprisetypeid_value";
            public const string ProvinceId = "_nv_provinceid_value";
            public const string EnterpriseRegistrationNumber = "nv_enterpriseregistrationnumber";
            public const string AnnualUpdateDueDate = "nv_annualupdateduedate";
            public const string RenewalDueDate = "nv_renewalduedate";
            public const string CurrentContractorGradingDesignation = "nv_currentcontractorgradingdesignation";
            public const string CurrentContractorGrade = "nv_currentcontractorgrade";
            public const string DateEnterpriseRegistered = "nv_dateenterpriseregistered";
            public const string DateOperationsStarted = "nv_dateoperationsstarted";
            public const string SuspensionReasonAnnualUpdate = "nv_suspensionreason_annualupdate";
            public const string Type = "nv_type";
            public const string EnterpriseType = "nv_enterprisetype";
        }

        public static class ApplicationFields
        {
            public const string Id = "nv_applicationid";
            public const string ApplicationNumber = "nv_applicationnumber";
            public const string Type = "nv_applicationtype";
            public const string Status = "statuscode";
            public const string ContractorId = "_nv_contractorid_value";
            public const string CreatedOn = "createdon";
            public const string ActivationDate = "nv_activationdate";
            public const string StateCode = "statecode";
            public const string ContractorPotentiallyEmerging = "nv_hdi_contractorpotentiallyemerging";

            public const int StatusCodeCreateComplete = 100000001;
            public const int StatusCodeCaptureInProgress = 100000003;
        }

        public static class ContactFields
        {
            public const string Id = "contactid";
            public const string FirstName = "firstname";
            public const string LastName = "lastname";
            public const string Email = "emailaddress1";
            public const string Telephone = "telephone1";
            public const string Title = "nv_title";
            public const string IdNumber = "nv_idnumber";
            public const string ContactType = "nv_contact_type";
            public const string GenderCode = "gendercode";
            public const string RSACitizen = "nv_rsacitizen";
            public const string IsBlack = "nv_black";
            public const string CredentialsId = "_nv_credentialsid_value";
            public const string CredentialsRequested = "nv_credentialsrequested";
            public const string Initials = "nv_initials";
            public const string Designation = "nv_designation";
            public const string MobilePhone = "mobilephone";
            public const string FaxNumber = "faxnumber";
            public const string Employer = "nv_employerid";
            public const string ExternalSystemAccess = "nv_externalsystemaccesstype";
            public const string AccountEnabled = "nv_accountenabled";
            public const string Role = "nv_role";
            public const string TenderID = "nv_tenderid";
        }

        public static class ContactPermissionFields
        {
            public const string Id = "nv_PermissionId";
            public const string ContactPermissionId = "nv_contact_permissionId";
            public const string Name = "nv_name";
            public const string IndividualId = "nv_individualid";
        }

        public static class Contractfields
        {
            public const string ContractId = "nv_contractid";
            public const string Name = "nv_name";
            public const string TenderId = "nv_tenderid";
            public const string Employer_contract = "nv_employer_contract_id";
            public const string ProvinceID = "nv_provinceid";
            public const string Province = "";
            public const string MetroDistrictMunicipalityID = "nv_metrodistrictid";
            public const string MunicipalityId = "nv_municipalityid";
            public const string ClassOfConstructionWorks = "nv_classofconstructionworks";
            public const string ContractDescription = "nv_descriptionofproject";
            public const string ContractTitle = "nv_title";
            public const string ContractNumber = "nv_contractnumber";
            public const string TypeOfContract = "nv_typeofcontract";
            public const string TenderValueRage = "nv_tendervaluerange";
            public const string ContractAwardDate = "nv_contractawarddate";
            public const string EmergingEnterpriseSupport = "nv_emergingenterprisesupport";
            public const string FinancialManagement = "nv_natureoffinancialormanagement";
            public const string FinancialManagementOtherDescription = "nv_othernatureoffinancialormanagement";
            public const string Satisfied = "nv_satisfied_contractor_has_potential";
            public const string UnderExtremeConditions = "nv_underextremeconditions";
            public const string ExpandedPublicWorksProgramme = "nv_expandedpublicworksprogramme";
            public const string ContractorDevelopmentProgramme = "nv_contractordevelopmentprogramme";
            public const string NationalContractorDevelopmentProgramme = "nv_nationalcontractordevelopmentprogramme";
            public const string PublicPrivatePartnership = "nv_publicprivatepartnership";
            public const string MunicipalWorks = "nv_municipalworksinvolved";
            public const string EvaluateResponses = "nv_proceduretoevaluateresponses";
            public const string Years = "nv_contractperiodyears";
            public const string Months = "nv_contractperiodmonths";
            public const string Days = "nv_contractperioddays";
            public const string IsTermContract = "nv_termcontract";
            public const string TenderEvaluation = "nv_tenderevaluationmethod";
            public const string IsJV = "nv_isajv";
            public const string GPSCoordinateType = "nv_gpscoordinatestype";
            public const string NumberOfContractors = "nv_numberofcontractors";
            public const string AverageAnnualContract = "nv_averageannualcontract";
            public const string UndisputedInvoicesPaid = "nv_undisputedinvoicespaidwithin30daysofrecei";
            public const string DisputedInvoices = "nv_wheretheredisputedinvoices";
            public const string VariationsMade = "nv_werethereanyvariationsmadeinclvat";
            public const string PriceAdjustmentsMade = "nv_werethereanypriceadjustmentmade";
            public const string PenaltiesApplied = "nv_wherethereanypenaltiesapplied";
            public const string CompletedOnTime = "nv_contractcompletedontime";
            public const string DisputesReferred = "nv_disputesrefferedtodisputeresolutioncontra";
            public const string DisputePending = "nv_istheoutcomeofthedisputestillpending";
            public const string InformCidb = "nv_informedcidboflitigationarbitration";
            public const string ReceiptOfNotification = "nv_didyoureceivereceiptofthenotification";
            public const string CompletedWithInBudget = "nv_wasthecontractcompletedwithinbudget";
            public const string CompletedToRequiredQuality = "nv_wasthecontractcompletedtotherequiredquali";
            public const string ReasonForTermination = "nv_reasonfortermination";
            public const string EmployerContractID = "nv_employer_contract_id";
            public const string DateOfPracticalCompletion = "nv_dateofpracticalcompletion";
            public const string CIDBContractNumber = "nv_cidbcontractnumber";
            public const string ChangeRequestStatus = "nv_changerequeststatus";
            public const string TerminationDate = "nv_terminationdate";
            public const string ContractEndDate = "nv_contractenddate";
            public const string SolicitationType = "nv_solicitation_type";
            public const string FinancialPointsTotal = "nv_financialpoints";
            public const string PreferencePointsTotal = "nv_preferencepoints";
            public const string FinancialPoints = "nv_financialpointsoutof";
            public const string PreferencePoints = "nv_preferencepointsoutof";
            public const string Grade = "nv_gradecombinedgrade";
            public const string TotalContractValueIncludingVat = "nv_totalcontractvalueinclvat";
            public const string ContractTotalInvoicedAmountInclVat = "nv_contracttotalinvoicedamountinclvat_base";
            public const string ContractPeriodYears = "nv_contractperiodyears";
            public const string ContractPeriodMonths = "nv_contractperiodmonths";
            public const string ContractPeriodDays = "nv_contractperioddays";
            public const string ContractPeriod = "nv_contractperiod";
            public const string StateCode = "statecode";
            public const string StatusCode = "statuscode";
        }

        public static class CredentialFields
        {
            public const string Id = "nv_credentialsid";
            public const string Name = "nv_name";
            public const string Password = "nv_password";
            public const string LastLogin = "nv_lastlogin";
            public const string IsLocked = "nv_islocked";
            public const string MFAEnabled = "cidb_mfaenabled";
            public const string PreferredMfa = "cidb_preferredmfamethod";
            public const string ForcePasswordChange = "nv_forcepasswordchange";
            public const string IncorrectLoginCount = "nv_incorrectlogincount";
            public const string StatusCode = "statuscode";
        }

        public static class CityFields
        {
            public const string Id = "nv_cityid";
            public const string Name = "nv_name";
            public const string ProvinceId = "nv_provinceid";
        }

        public static class ClassOfConstructionWorkFields
        {
            public const string Id = "nv_classofworktypeid";
            public const string Name = "nv_name";
            public const string Description = "nv_description";
            public const string SourceId = "nv_class_of_worktype_sourceid";
        }

        public static class ClassOfConstructionWorkSubCategoryFields
        {
            public const string Id = "nv_classofworktypesubcategoryid";
            public const string ClassOfWorkTypeId = "nv_classofworktypeid";
            public const string Name = "nv_name";
            public const string Description = "nv_description";
        }

        public static class ConstructionContractContractorFields
        {
            public const string Id = "nv_constructioncontractcontractorsid";
            public const string ConstructionContractId = "nv_constructioncontractid";
            public const string ContractorId = "nv_contractorid";
            public const string ValidCidbRegistration = "nv_validcidbregistration";
            public const string EnterpriseName = "nv_enterprisename";
        }

        public static class ContractorDevelopmentProgrammeFields
        {
            public const string Id = "nv_contractordevelopmentprogrammeid";
            public const string EmployerId = "nv_employer_cdpid";
            public const string PrimaryFocus = "nv_primaryfocus";
            public const string CDPNumber = "nv_cdpnumber";
            public const string Name = "nv_name";
            public const string TotalBudget = "nv_totalbudget";
        }

        public static class CdpSubmissionFields
        {
            public const string ExchangeRate = "exchangerate";
            public const string CdpSubmissionId = "nv_cdpsubmissionid";
            public const string OrganizationId = "organizationid";
            public const string ContractorCdpSubmissionId = "nv_contractor_cdpsubmissionid";
            public const string CompetencyAssessmentPerformed = "nv_competencyassessmentperformed";
            public const string RemovedFromCdp = "nv_removedfromcdp";
            public const string ContractorGradingAtEntry = "nv_contractorgradingatentry";
            public const string CDPCdpSubmission = "nv_cdp_cdpsubmissionid";
            public const string TrainingRequirements = "nv_trainingrequirements";
            public const string TargetClassOfWork = "nv_targetclassofwork";
        }

        public static class ContractorFields
        {
            public const string Id = "accountid";
            public const string Name = "name";
            public const string TradingAs = "nv_tradingas";
            public const string CrsNumber = "nv_crsnumber";
            public const string CSDNumber = "nv_csdnumber";
            public const string ProvinceId = "nv_provinceid";
            public const string PrimaryContactId = "primarycontactid";
            public const string PotentiallyEmerging = "nv_hdi_potentiallyemerging";
            public const string StatusCode = "statuscode";
            public const string EnterpriseType = "nv_enterprisetype";
            public const string Type = "nv_type";
            public const string DateOfPracticalCompletion = "nv_dateofpracticalcompletion";
            public const string ActivationDate = "nv_activationdate";
            public const string RenewalDueDate = "nv_renewalduedate";
            public const string EnterpriseRegistrationNumber = "nv_enterpriseregistrationnumber";
            public const string BBBEEEStatus = "nv_hdi_bbbeestatus";
            public const string BBBEEEStatusTest = "nv_hdi_bbbeestatus";

            // Physical Address (Address1)
            public const string AddressLine1 = "address1_line1";
            public const string AddressLine2 = "address1_line2";
            public const string AddressLine3 = "address1_line3";
            public const string AddressCity = "address1_city";
            public const string AddressProvince = "address1_stateorprovince";
            public const string AddressPostalCode = "address1_postalcode";

            // Postal Address (Address2)
            public const string PostalAddressLine1 = "address2_line1";
            public const string PostalAddressLine2 = "address2_line2";
            public const string PostalAddressLine3 = "address2_line3";
            public const string PostalAddressCity = "address2_city";
            public const string PostalAddressProvince = "address2_stateorprovince";
            public const string PostalAddressPostalCode = "address2_postalcode";

            // Banking
            public const string BankName = "nv_bankingdetails_bankname";
            public const string BankAccountNumber = "nv_bankingdetails_accountnumber";
            public const string BankAccountName = "nv_bankingdetails_accountname";
            public const string BranchCode = "nv_bankingdetails_branchcode";

            // Contact
            public const string Email = "emailaddress1";
            public const string Phone = "telephone1";

            // Flags
            public const string IsSanctioned = "nv_sanctioned";
            public const string PreviouslySanctioned = "nv_previouslysanctioned";
            public const string Moratorium = "nv_moratoriumcontractor";

            // Metadata
            public const string CreatedOn = "createdon";
            public const string ModifiedOn = "modifiedon";
            public const string CurrentContractorGradingDesignation = "nv_currentcontractorgradingdesignation";
            public const string CurrentContractorGrade = "nv_currentcontractorgrade";
            public const string HdiExpiryDate = "nv_hdi_expirydate";
        }

        public static class ContractorGradeFields
        {
            public const string Id = "nv_classofworkid";
            public const string Name = "nv_name";
            public const string ContractorId = "nv_contractorid";
            public const string ClassOfWorkTypeId = "nv_classofworktypeid";
            public const string ApprovedGrade = "nv_approved_grade";
            public const string ElectricalLicense = "nv_electricallicense";
            public const string CreatedOn = "createdon";
            public const string ModifiedOn = "modifiedon";
            public const string StateCode = "statecode";
        }

        public static class MetroDistrictFields
        {
            public const string Id = "nv_metrodistrictid";
            public const string ProvinceId = "nv_provinceid";
            public const string StateCode = "statecode";
            public const string Name = "nv_name";
        }

        public static class GradeFields
        {
            public const string Name = "nv_name";
            public const string AnnualFee = "nv_annualfee";
            public const string AdminFee = "nv_adminfee";
        }

        public static class FinancialStatementFields
        {
            public const string Id = "nv_financialstatementid";
            public const string ContractorId = "nv_contractorid";
            public const string Year = "nv_year";
            public const string FyEnd = "nv_fyend";
            public const string CreatedOn = "createdon";
            public const string ApplicationId = "nv_applicationid";
            public const string TurnoverInclVat = "nv_turnoverinclvat";
            public const string Turnover = "nv_turnover";
            public const string NetAssetValue = "nv_netassetvalue";
            public const string MeetsRegulations = "nv_meetsnecessaryregulations";
        }

        public static class ProvinceFields
        {
            public const string Id = "nv_provinceid";
            public const string Name = "nv_name";
        }

        public static class OwnershipFields
        {
            public const string Name = "nv_name";
            public const string SharesHeld = "nv_percentage_sharesheld";
            public const string PercentageDevotedToEnterprise = "nv_percentage_devotedtoenterprise";
            public const string PercentageVotingRights = "nv_percentage_votingrights";
            public const string ManageAssets = "nv_mc_manageassets";
            public const string FinancialAuthority = "nv_mc_financialauthority";
            public const string ManageDailyOperations = "nv_mc_authoritymanagedailyopsenterprise";
            public const string ManagementOfCompanyPolicies = "nv_mc_authoritydeterminemanagementcomppolicy";
        }

        public static class TenderFields
        {
            public const string Id = "nv_tenderid";
            public const string EmployerTenderNumber = "nv_employertendernumber";
            public const string Title = "nv_tendertitle";
            public const string EmployerId = "nv_employer_tender_id";
            public const string ProvinceId = "nv_provinceid";
            public const string BatchReferenceNumber = "nv_batchreferencenumber";
            public const string MetroDistrictId = "nv_metrodistrictmunicipalityid";
            public const string ClassOfWork = "nv_classofconstructionworkid";
            public const string AltClassOfWork = "nv_classofconstructionworksaltid";
            public const string SubCategory = "nv_classofconstructionworkssubcategory";
            public const string AltSubCategory = "nv_alternateclassofconstructionworkssubcateg";
            public const string TenderValueRange = "nv_tendervaluerange";
            public const string DocsAvailableFrom = "nv_dateandtimedocumentsavailablefrom";
            public const string ClosingDate = "nv_closingdateandtime";
            public const string ClarificationMeeting = "nv_clarificationmeeting";
            public const string ClarificationMeetingPlace = "nv_clarificationmeetingplace";
            public const string ClarificationMeetingDateAndTime = "nv_clarificationmeetingdateandtime";
            public const string ClarificationMeetingCompulsory = "nv_clarificationmeetingcompulsory";
            public const string AdditionalClarificationMeeting = "nv_addclarificationmeeting";
            public const string AdditionalClarificationMeetingPlace = "nv_addclarificationmeetingplace";
            public const string AdditionalClarificationMeetingDateAndTime = "nv_addclarificationmeetingdateandtime";
            public const string AdditionalClarificationMeetingCompulsory = "nv_addclarificationmeetingcompulsory";
            public const string BatchId = "nv_advertisebatch";
            public const string IsClosed = "nv_isclosed";
            public const string IsEOI = "nv_iseoiortender";
            public const string StatusCode = "statuscode";
            public const string StateCode = "statecode";
            public const string IsEmergency = "nv_isemergencytender";
            public const string IsTerm = "nv_termcontract";
            public const string EPWP = "nv_contractpartofepwp";
            public const string PPP = "nv_partofpublicprivatepartnership";
            public const string MunicipalWorks = "nv_municipalworksinvolved";
            public const string TypeOfContract = "nv_typeofcontract";
            public const string EmergingEnterpriseSupport = "nv_emergingenterprisesupport";
            public const string NationalContractorDevelopmentProgramme = "nv_nationalcontractordevelopmentprogramme";
            public const string PublicPrivatePartnership = "nv_partofpublicprivatepartnership";
            public const string MunicipalWorksInvolved = "nv_municipalworksinvolved";
            public const string Awarded = "nv_awarded";
            public const string ParentTender = "nv_parenttender";
            public const string ChangeRequestStatus = "nv_changerequeststatus";
            public const string CidbReferencenNumber = "nv_cidbreferencenumber";
            public const string Name = "nv_name";
            public const string DateAdvertised = "nv_dateadvertised";
            public const string PreferencesOffered = "nv_preferencesoffered";
            public const string TendersInvitedFor = "nv_tendersinvitedfor";
            public const string EligibilityCriteria = "nv_eligibilitycriteria";
            public const string MetroDistrictMunicipalityId = "nv_MetroDistrictMunicipalityId";
            public const string LocalMunicipalityId = "nv_municipality";
            public const string AltClassOfConstructionWorksSubId = "nv_alternateclassofconstructionworkssubcateg";
            public const string PrimaryAddressLine1 = "nv_address1_line1";
            public const string PrimaryAddressLine2 = "nv_address1_line2";
            public const string PrimaryAddressLine3 = "nv_address1_line3";
            public const string PrimaryAddressLine4 = "nv_address1_line4";
            public const string SecondaryAddressLine1 = "nv_address2_line1";
            public const string SecondaryAddressLine2 = "nv_address2_line2";
            public const string SecondaryAddressLine3 = "nv_address2_line3";
            public const string SecondaryAddressLine4 = "nv_address2_line4";
            public const string DepositAmount = "nv_depositamount";
            public const string NotAcceptTelegraphic = "nv_notacceptedtelegraphic";
            public const string NotAcceptTelephonic = "nv_notacceptedtelephonic";
            public const string NotAcceptTelex = "nv_notacceptedtelex";
            public const string NotAcceptFacsimile = "nv_notacceptedfacsimile";
            public const string NameOfTargetedDevelopmentProgramme = "nv_nameoftargeteddevelopmentprogramme";
            public const string IsTermContract = "nv_termcontract";
            public const string MethodOfPaymentBankGuaranteedCheque = "new_methodofpaymentbankguaranteedcheque";
            public const string MethodOfPaymentCash = "new_methodofpaymentcash";
            public const string MethodOfPaymentProofOfDeposit = "new_methodofpaymentproofofdeposit";
            public const string FurtherPaymentAndCollectionInformation = "nv_additionalpaymentandcollectiondetails";
        }
    }
}
