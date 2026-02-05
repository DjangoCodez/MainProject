using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    #region Enums

    // Summery needs on EmployeeChangeRowIODTO aswell in order for Swagger to display the description. Can not be set on the enum (which would have been great...)

    /// <summary>
    /// Defines the type of change to apply for an Employee.<br/>
    /// Active = 1 : (bool) Activate or deactive the employee and user (format: 1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// FirstName = 2 : (string, max 100) Set or change first name (max: 100)<br/>
    /// LastName = 3 : (string, max 100) Set or change Last name (max: 100)<br/>
    /// SocialSec = 4 : (string) Set or change social security number (will be formatted to yyyyMMdd-xxxx). For bypassing validation set optionalExternalCode to "force"<br/>
    /// DisbursementMethod = 5: (int) Set or change the payroll payment method, (numeric value: 1 = Cash deposit, 2 = Personal account(SE), 3 = Account deposit(SE))<br/>
    /// DisbursementAccountNr = (string) 6: Set or change the bank account nr inlcuding clearing nr. Use '#' as separator (format: "ClearingNr#AccountNr")<br/>
    /// ExternalCode = 7: (string, max 100) Add or change employee external code (max: 100)<br/>
    /// DontValidateDisbursementAccountNr = 8: (bool) Set if the DisbursementAccountNr should not be validated<br/>
    /// EmployeeTemplateId = 11: If created from an employee template<br/>
    /// Vacant = 12. (bool) Set if the Employee is vacant (not a real person yet)(format: 1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// HierarchicalAccount = 50 : (string) Set or change hierarchicalaccount (externalcode, number or name). Employee can have multuple accounts. Dates are optional and set thru FromDate and ToDate. Use optionalExternalCode set more information <br/>
    /// '2000|true#false' means connect to parent accountNr 2000 and set main allocation to true and default(show colleges) to false. So to set only main allocation to true, use '|true#' and to set only default use '|#true'. <br/>
    /// Use sort to sort levels, if you want to set a tree with 3 levels, three EmployeeChangeRowIODTOs are needed. First with sort 1, second with sort 2 and third with sort 3. <br/>
    /// AccountNrSieDim = 70 : (string) Employment field. Changes affect whole employment. Set or change account on employee, use account number as key and optionalexternalcode to identify SIE-level. Valid optionalexternalcodes (1,2,6,7,8,9,10,30,40,50)<br/>
    /// Email = 100 : (string) Set or change email (max: 100)<br/>
    /// PhoneHome = 101 : (string, max 100) Set or change cell phone number (max: 100)<br/>
    /// PhoneMobile = 102 : (string, max 100) Set or change office phone number (max: 100)<br/>
    /// PhoneJob = 103 : (string, max 100) Set or change office phone number (max: 100)<br/>
    /// ClosestRelativeNr = 104 : (string, max 100) Set or change number to closest relative (max: 100)<br/>
    /// ClosestRelativeName = 105 : (string, max 100) Set or change name of closest relative (max: 512 for Name and Relation combined)<br/>
    /// ClosestRelativeRelation = 106 : (string, max 100) Set or change type of relation (for example mother, wife, husband, sibling) on closest relative (max: 512 for Name and Relation combined)<br/>
    /// ClosestRelativeHidden = 107 : (bool) Make information on closest relative secret to everyone except closest executive (format: 1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// ClosestRelativeNr2 = 108 : (string, max 100) Set or change number to closest relative (max: 100)<br/>
    /// ClosestRelativeName2 = 109 : (string, max 100) Set or change name of closest relative (max: 512 for Name2 and Relation2 combined)<br/>
    /// ClosestRelativeRelation2 = 110 : (string, max 100) Set or change type of relation (for exemple mother, wife, husband, sibling) on closest relative (max: 512 for Name2 and Relation2 combined)<br/>
    /// ClosestRelativeHidden2 = 111 : (bool) Make information on closest relative secret to everyone except closest executive (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// Address = 112 : (string, max 100) Set or change the delivery address (max: 100)<br/>
    /// AddressCO = 113 : (string, max 100) Set or change the address co (max: 100)<br/>
    /// AddressPostCode = 114 : (string, max 100) Set or change the postal code (max: 100)<br/>
    /// AddressPostalAddress = 115 : (string, max 100) Set or change the postal address (max: 100)<br/>
    /// AddressCountry = 116 : (string, max 100) Set or change the address country (max: 100)<br/>
    /// AddressHidden = 117 : (bool) Make adress secret to everyone except closest executive (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// ExtraFieldEmployee = 118 : (string) Set or change an extra field, use optionalExternalCode to set which field. Use value according to datatype of extrafield<br/>
    /// ExcludeFromPayroll = 119: (bool) Set if the Employee should be exclude from payrollcalculation and export<br/>
    /// WantsExtraShifts = 121: (bool) Set if the Employee wants extra shifts<br/>
    /// EmploymentStartDateChange = 200 : (date) Add or change start date of employment (format: yyyyMMdd)<br/>
    /// EmploymentStopDateChange = 201 : (date) Add or change end date of employment (format: yyyyMMdd)<br/>
    /// EmployeeGroup = 202 :  (string) Employment field with changes thru FromDate (mandatory) and ToDate. Add or change employeegroup (Time agreement) on employee (externalcode or name)<br/>
    /// PayrollGroup = 203 : (string) Employment field with changes thru FromDate (mandatory) and ToDate. Add or change payroll group employee (externalcode or name)<br/>
    /// VacationGroup = 204 : (string) Employment field with changes thru FromDate (mandatory) and ToDate. Add or change vaction group employee (externalcode or name)<br/>
    /// WorkTimeWeekMinutes = 205 : (decimal) Employment field with changes thru FromDate and ToDate. Number of minutes in a week according to employment percent. Choose to send WorkTimeWeekMinutes OR EmploymentPercent <br/>
    /// EmploymentPercent = 206 : (decimal) Employment field with changes thru FromDate and ToDate. Add or change employment percent. Choose to send WorkTimeWeekMinutes OR EmploymentPercent<br/>
    /// EmploymentExternalCode = 207 : (string) Employment field. Add or change employment external code. (max: 50)<br/>
    /// EmploymentType = 208 : (string) Employment field with changes thru FromDate and ToDate. Add or change employment type on employee (code, name or type)<br/>
    /// EmployeePosition = 209 : (string) Add or change employee position (code, name or type)<br/>
    /// EmployeePositionDefault = 210 (bool) Set if the EmployeePosition should be default<br/>
    /// TaxRate = 211 : (int) Set taxrate table on new employee.<br/>
    /// IsSecondaryEmployment = 212 : (bool) Employment field with changes thru FromDate and ToDate. Set if employment is secondary. (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// ExperienceMonths = 220 : (int) Employment field with changes thru FromDate and ToDate. Set number of experience months.<br/>
    /// ExperienceAgreedOrEstablished = 221 : (bool) Employment field with changes thru FromDate and ToDate. Set if i experience is agreed (true) or Established (false) (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// WorkTasks = 223 : (string) Employment field with changes thru FromDate and ToDate. Set WorkTasks on employment<br/>
    /// SpecialConditions = 224 : (string) Employment field with changes thru FromDate and ToDate. Set special conditions on employment<br/>
    /// WorkPlace = 225 : (string, max 100) Employment field with changes thru FromDate and ToDate. Set work place on employment<br/>
    /// SubstituteFor = 226 : (string) Employment field with changes thru FromDate and ToDate. Set "substitue for" place on employment<br/>
    /// SubstituteForDueTo = 227 : (string) Employment field with changes thru FromDate and ToDate. Set why the "SubstituteFor" needed to be substituted<br/>
    /// EmploymentEndReason = 228 : (string) Employment field. Set why the employment ended. Use OptionalEmploymentDate to point to the employment to change. The employment must have stopDate or have stopDate (201) in current batch.<br/>
    /// BaseWorkTimeWeek = 229 (int) Employment field with changes thru FromDate and ToDate. Set base work time week minutes<br/>
    /// EmploymentStopDateKeepScheduleHierarchicalAccountAndAttestRole = 251 : (bool) If 201 is also sent and schedule, hierarchical account and attestrole shouldnt be closed on that date. Use only if another employment is created directly afterwards and that employment ends same date or later than the closed employment<br/>
    /// ClearScheduleFrom = 252 : (DateTime) Set this to clear all active schedules after set date.
    /// ChangeToNotTemporaryPrimary = 253 : (bool) Set this to true to change the temporary primary employment to ordinary from the date passed as FromDate. The new ordinary employment will inherit the settings from temporary primary employments last date. ToDate is an optional stop date of the new ordinary employment.
    /// FullTimeWorkTimeWeekMinutes = 254 : (decimal) Employment field with changes thru FromDate and ToDate. Number of fulltime minutes in a week according. If empty fulltime workminutes on employee group vill we used <br/>
    /// DoNotValidateAccount = 261 : (bool) Set this to true if bankaccount shouldnt be validated <br/>
    /// DisbursementCountryCode = 262 : (string, max 10) Set or change bank account country code (max: 10)<br/>
    /// DisbursementBIC = 263 : (string, max 50) Set or change bank account BIC (max: 50)<br/>
    /// DisbursementIBAN = 264 : (string, max 100) Set or change IBAN bank account (max: 100)<br/>
    /// EmploymentPriceType = 300 : (decimal, date and string) Employmentpricetype is salarytype on employee. Send value as Value and code/number of pricetyp as optional external code and startDate as fromdate. To set a stopdate, set pricetype with amount 0 from the stopdate<br/>
    /// UserRole = 400 : (string) Add or change UserRole (external code or name). User can have multuple roles. Dates are optional and set thru FromDate and ToDate.<br/>
    /// AttestRole = 401 : (string) Add or change attest role. If attestrole is connected to account use account number in optionalexternalcode. (OptinalExternalCode: external code or name). User can have multuple roles. Dates are optional and set thru FromDate and ToDate.<br/>
    /// BlockedFromDate = 402 : (date) Sets a date on user that disables the possibility to login (format: yyyyMMdd or empty to reset)<br/>
    /// DefaultUserRole = 403 : Sets the Role to default for the User. Note that if you change Default Role, and the earlier Default Role should be removed, then 400 with flag Delete must be used also. Othwerise the earlier role still exists for the user, but not as default. (external code or name)<br/>
    /// ExternalAuthId = 500 : (string) Code for SSO-solution. Identity in external provider. (external provider must be set on license)<br/>
    /// PayrollStatisticsPersonalCategory = 601 : (int) Set or change field "SCB och Näringsliv: Personalkategori" (0 = Inget, 1 = Arbetare, 2 = Övriga (t.ex. tjänstemän) för Stål- och metallförbundet, 3 = Arbetare röda avtalet, 4 = Arbetare blå avtalet, 5 = Arbetare elavtalet, 6 = Medarbetare gröna avtalet, 7 = SIF, 8 = CF, 9 = LEDARNA)<br/>
    /// PayrollStatisticsWorkTimeCategory = 602 : (int) Set or change field "SCB och Näringsliv: Arbetstidart" (0 = Inget, 1 = Dagarbete, 2 = 2-skift, 3 = Intermittent 3-skift, 4 = Kontinuerligt 3-skift, 5 = Kontinuerligt skift med storhelgsdrift, arbete under jord. 6 = Ständigt nattskift, 7 = Deltidsarbete, 8 = Deltidspensionär)<br/>
    /// PayrollStatisticsSalaryType = 603 : (int) Set or change field "SCB och Näringsliv: Anställningsform" (0 = Inget, 1 = Månadslön, 2 = Veckolön, 3 = Timlön)<br/>
    /// PayrollStatisticsWorkPlaceNumber = 604 : (int) Set or change field "SCB och Näringsliv: Arbetsplatsnummer"<br/>
    /// PayrollStatisticsCFARNumber = 605 : (int) Set or change field "SCB och Näringsliv: CFAR-nummer"<br/>
    /// ControlTaskWorkPlacSCB = 611 : (string, 100) Set or change field "Kontrolluppgift: Arbetsställe SCB"<br/>
    /// ControlTaskPartnerInCloseCompany = 612 : (bool) Set or change field "Kontrolluppgift: Delägare i fåmansföretag"<br/>
    /// ControlTaskBenefitAsPension = 613 : (bool) Set or change field "Kontrolluppgift: Förmån som pension"<br/>
    /// AFACategory = 621 : (int) Set or change field "AFA (FORA): Kategori" (0 = Inget, 1 = Arbetare, 2 = Tjänsteman/anställd VD, 3 = Undantas helt)<br/>
    /// AFASpecialAgreement = 622 : (int) Set or change field "AFA (FORA): Speciellt avtal" (0 = Inget, 1 = Eget avtal Tjänstemän)<br/>
    /// AFAWorkplaceNr = 623 : (string, max 100) Set or change field "AFA (FORA): Arbetsplatsnummer"<br/>
    /// AFAParttimePensionCode = 624 : (bool) Set or change field "Kontrolluppgift: Kod för deltidspension"<br/>
    /// CollectumITPPlan = 631 : (int) Set or change field "Collectum: ITP-plan" (0 = Inget, 1 = ITP1, 2 = ITP2)<br/>
    /// CollectumAgreedOnProduct = 632 : (string, max 100) Set or change field "Collectum: Avtalad produkt"<br/>
    /// CollectumCostPlace = 633 : (string, max 100) Set or change field "Collectum: Kostnadsställe"<br/>
    /// CollectumCancellationDate = 634 :  (date) Set or change field "Collectum: Avanmäld datum" (format: yyyyMMdd)<br/>
    /// CollectumCancellationDateIsLeaveOfAbsence = 635 : (bool) Set or change field "Collectum: Tjänsteledig"<br/>
    /// KPARetirementAge = 641 : (int) Set or change field "KPA: Pensionsålder"<br/>
    /// KPABelonging = 642 :  (int) Set or change field "KPA: Tillhörighet" (0 = Inget, 1 = BEA, 2 = PAN, 3 = Medstud)<br/>
    /// KPAEndCode = 643 : (int) Set or change field "KPA: Avslutskod" (1 = U1, 2 = U3, 3 = US, 4 = UD)<br/>
    /// KPAAgreementType = 644 : (int) Set or change field "KPA: Typ av avtal" (0 = Inget, 1 = PFA01, 2 = PFA98, 3 = KAP_KL, 4 = AKAP_KL)<br/>
    /// BygglosenAgreementArea = 651 :  (string, max 10) Set or change field "Bygglösen: Bygglösen avtalsomårde"<br/>
    /// BygglosenAllocationNumber = 652 : (string, max 10) Set or change field "Bygglösen: Bygglösen fördelningstal"<br/>
    /// BygglosenSalaryFormula = 653 : (string, max 10) Set or change formula for field "Bygglösen: Bygglösen avtalad lön" (name)<br/>
    /// BygglosenMunicipalCode = 654 : (string, max 10) Set or change field "Bygglösen: Bygglösen Bygglösen kommunkod"<br/>
    /// BygglosenProfessionCategory = 655, (string, max 10) Set or change field "Bygglösen: Bygglösen yrkeskategori"<br/>
    /// BygglosenSalaryType = 656, (int) Set or change field "Bygglösen: SalaryType" (0 = Inget, 1 = Tidlön, 2 = Prestationslön, 3 = Prestationslön TIA och Plåt & Vent )<br/>
    /// BygglosenWorkPlaceNumber = 657, (string, max 100) Set or change field "Bygglösen: Bygglösen arbetsplatsnummer"<br/>
    /// BygglosenLendedToOrgNr = 658, (string, max 10) Set or change field "Bygglösen: Bygglösen utlånad till orgnr"<br/>
    /// BygglosenAgreedHourlyPayLevel = 659 (Decinal) Set or change field "Bygglösen: Bygglösen utbetalningsnivå per timme"<br/>
    /// GTPAgreementNumber = 661 : (string) Folksam pension agreement number<br/>
    /// GTPExcluded = 662 : (bool) Exclude from GTP (Folksam) (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// AGIPlaceOfEmploymentAddress = 671 : (string, max 100) Set or change field AGI "Place of employment address"<br/>
    /// AGIPlaceOfEmploymentCity = 672 :  (string, max 100) Set or change field AGI "Place of employment city"<br/>
    /// AGIPlaceOfEmploymentIgnore = 673 : (bool) Set or change field AGI "Leave empty in AGI-file" (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
    /// VacationDaysPaid = 700. (decimal) Number of paid day of vacation remaining (only new employee)<br/>
    /// VacationDaysUnPaid = 701, (decimal) Number of unpaid day of vacation remaining (only new employee)<br/>
    /// VacationDaysAdvance = 702,  (decimal) Number of advance paid day of vacation remaining (only new employee)<br/>
    /// ParentEmployeeNr = 717 : (string) Set or change Parent EmployeeNr on employee (This is not the parent in the biologic sense, Employee and Parent is the same Person (Only needed for certain specific scenarios) <br/>
    /// EmployerRegistrationNr = 718 : (string, max 50) Set or change organisation on employee. Value is organisation number and FromDate (mandatory) and ToDateOnly. (Only needed for certain specific scenarios (max: 50) <br/>
    /// NewEmployments = 1002: Use the list of NewEmploymentRowIO. This can also be used for importing old employments on startup.
    /// </summary>
    public enum EmployeeChangeType
    {
        None = 0,
        Active = 1,
        FirstName = 2,
        LastName = 3,
        SocialSec = 4,
        DisbursementMethod = 5,
        DisbursementAccountNr = 6,
        ExternalCode = 7,
        DontValidateDisbursementAccountNr = 8,
        EmployeeTemplateId = 11,
        Vacant = 12,
        HierarchicalAccount = 50,
        AccountNrSieDim = 70,
        Email = 100,
        PhoneHome = 101,
        PhoneMobile = 102,
        PhoneJob = 103,
        ClosestRelativeNr = 104,
        ClosestRelativeName = 105,
        ClosestRelativeRelation = 106,
        ClosestRelativeHidden = 107,
        ClosestRelativeNr2 = 108,
        ClosestRelativeName2 = 109,
        ClosestRelativeRelation2 = 110,
        ClosestRelativeHidden2 = 111,
        Address = 112,
        AddressCO = 113,
        AddressPostCode = 114,
        AddressPostalAddress = 115,
        AddressCountry = 116,
        AddressHidden = 117,
        ExtraFieldEmployee = 118,
        ExcludeFromPayroll = 119,
        TimeWorkAccount = 120,
        WantsExtraShifts = 121,
        EmploymentStartDateChange = 200,
        EmploymentStopDateChange = 201,
        EmployeeGroup = 202,
        PayrollGroup = 203,
        VacationGroup = 204,
        WorkTimeWeekMinutes = 205,
        EmploymentPercent = 206,
        EmploymentExternalCode = 207,
        EmploymentType = 208,
        EmployeePosition = 209,
        EmployeePositionDefault = 210,
        TaxRate = 211,
        IsSecondaryEmployment = 212,
        ExperienceMonths = 220,
        ExperienceAgreedOrEstablished = 221,
        WorkTasks = 223,
        SpecialConditions = 224,
        WorkPlace = 225,
        SubstituteFor = 226,
        SubstituteForDueTo = 227,
        EmploymentEndReason = 228,
        BaseWorkTimeWeek = 229,
        EmploymentStopDateKeepScheduleHierarchicalAccountAndAttestRole = 251,
        ClearScheduleFrom = 252,
        ChangeToNotTemporaryPrimary = 253,
        FullTimeWorkTimeWeekMinutes = 254,
        DoNotValidateAccount = 261,
        DisbursementCountryCode = 262,
        DisbursementBIC = 263,
        DisbursementIBAN = 264,
        EmploymentPriceType = 300,
        UserRole = 400,
        AttestRole = 401,
        BlockedFromDate = 402,
        DefaultUserRole = 403,
        ExternalAuthId = 500,
        PayrollStatisticsPersonalCategory = 601,
        PayrollStatisticsWorkTimeCategory = 602,
        PayrollStatisticsSalaryType = 603,
        PayrollStatisticsWorkPlaceNumber = 604,
        PayrollStatisticsCFARNumber = 605,
        ControlTaskWorkPlaceSCB = 611,
        ControlTaskPartnerInCloseCompany = 612,
        ControlTaskBenefitAsPension = 613,
        AFACategory = 621,
        AFASpecialAgreement = 622,
        AFAWorkplaceNr = 623,
        AFAParttimePensionCode = 624,
        CollectumITPPlan = 631,
        CollectumAgreedOnProduct = 632,
        CollectumCostPlace = 633,
        CollectumCancellationDate = 634,
        CollectumCancellationDateIsLeaveOfAbsence = 635,
        KPARetirementAge = 641,
        KPABelonging = 642,
        KPAEndCode = 643,
        KPAAgreementType = 644,
        BygglosenAgreementArea = 651,
        BygglosenAllocationNumber = 652,
        BygglosenSalaryFormula = 653,
        BygglosenMunicipalCode = 654,
        BygglosenProfessionCategory = 655,
        BygglosenSalaryType = 656,
        BygglosenWorkPlaceNumber = 657,
        BygglosenLendedToOrgNr = 658,
        BygglosenAgreedHourlyPayLevel = 659,
        GTPAgreementNumber = 661,
        GTPExcluded = 662,
        AGIPlaceOfEmploymentAddress = 671,
        AGIPlaceOfEmploymentCity = 672,
        AGIPlaceOfEmploymentIgnore = 673,
        IFAssociationNumber = 681,
        IFPaymentCode = 682,
        IFWorkPlace = 683,
        VacationDaysPaid = 700,
        VacationDaysUnPaid = 701,
        VacationDaysAdvance = 702,
        TaxTinNumber = 710,
        TaxCountryCode = 711,
        TaxBirthPlace = 712,
        TaxCountryCodeBirthPlace = 713,
        TaxCountryCodeCitizen = 714,
        ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = 715,
        AnnualLeaveGroup = 716,
        ParentEmployeeNr = 717,
        EmployerRegistrationNr = 718,

        //Internal
        NewEmployment = 1001,
        NewEmployments = 1002,
    }

    public enum EmploymentChangeValidationError
    {
        /// <summary>
        /// Contains no changes for the Employee
        /// </summary>
        NoChanges = 1,
        /// <summary>
        /// Save failed
        /// </summary>
        SaveFailed = 2,

        /// <summary>
        /// Value is mandatory when not delete
        /// </summary>
        InvalidValueMustHaveValueWhenNotDelete = 3,
        /// <summary>
        /// EmployeeChangeType not supported
        /// </summary>
        InvalidChange = 4,
        /// <summary>
        /// Data not found
        /// </summary>        
        DataNotFound = 5,

        /// <summary>
        /// OptionalExternalCode missing.
        /// </summary>
        OptionalExternalCodeMissing = 7,
        /// <summary>
        /// Value is mandatory when not delete
        /// </summary>
        InvalidValueMustHaveValue = 8,
        /// <summary>
        /// Permission for the EmployeeChangeType is missing
        /// </summary>
        PermissionMissing = 9,

        /// <summary>
        /// Value is not a valid int
        /// </summary>
        InvalidInt = 11,
        /// <summary>
        /// Value is not a valid decimal
        /// </summary>
        InvalidDecimal = 12,
        /// <summary>
        /// Value is not a valid bool
        /// </summary>
        InvalidBool = 13,
        /// <summary>
        /// Value is not a valid date
        /// </summary>
        InvalidDate = 14,
        /// <summary>
        /// Date cannot be null
        /// </summary>
        InvalidDateCannotBeNull = 15,
        /// <summary>
        /// Int value is not valid for type
        /// </summary>
        InvalidEnum = 16,
        /// <summary>
        /// String length exceeds max length
        /// </summary>
        StringLengthExceedsMaxLength = 17,
        /// <summary>
        /// Invalid value. Active state could not be set
        /// </summary>
        InvalidValueActiveStateNotFound = 18,
        /// <summary>
        /// Phone number is mandatory
        /// </summary>
        InvalidPhoneNumberIsMandatory = 19,
        /// <summary>
        /// Value is mandatory when adding closest relative
        /// </summary>
        InvalidValueIsMandatoryClosestRelative = 20,
        /// <summary>
        /// Invalid social security number or incorrectly formatted
        /// </summary>
        InvalidSocialSec = 21,
        /// <summary>
        /// Invalid Social Security Number. Value is to short. Must be at least 8 characters
        /// </summary>
        InvalidSocialSecToToShort = 22,
        /// <summary>
        /// EmploymentDate is too small or too large
        /// </summary>
        InvalidEmploymentDate = 23,
        /// <summary>
        /// EmploymentDate is mandatory
        /// </summary>
        InvalidEmploymentDateCannotBeNull = 24,
        /// <summary>
        /// OptionalExternalCode is mandatory in this context
        /// </summary>
        InvalidOptionExternalCodeCannotBeEmpty = 25,
        /// <summary>
        /// Disbursement method is invalid
        /// </summary>
        InvalidDisbursementMethodInvalidValue = 26,
        /// <summary>
        /// Disbursement accountnr is invalid
        /// </summary>
        InvalidDisbursementAccountNr = 27,
        /// <summary>
        /// Attestrole is invalid
        /// </summary>
        InvalidAttestRole = 28,
        /// <summary>
        /// Existing role could not be found
        /// </summary>
        InvalidDefaultUserRole = 29,
        /// <summary>
        /// Email is invalid
        /// </summary>
        InvalidEmail = 30,
        /// <summary>
        /// Email cannot be empty
        /// </summary>
        InvalidEmailCannotBeEmpty = 31,
        /// <summary>
        /// Employment cannot be both temporary primary and secondary
        /// </summary>
        EmploymentCannotBeTemporaryPrimaryAndSecondary = 32,
        /// <summary>
        /// Temporary primary employment must have DateFrom and DateTo
        /// </summary>
        TemporaryPrimaryEmploymentMustHaveDateFromAndDateTo = 33,
        /// <summary>
        /// Temporary primary employment must have Employment to hibernate whole interval
        /// </summary>
        TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval = 34,
        /// <summary>
        /// Already exists temporary primary employment in interval
        /// </summary>
        TemporaryPrimaryAlreadyExistsInInterval = 35,
        /// <summary>
        /// The employment startdate could not be parsed
        /// </summary>
        InvalidEmploymentStartDateCannotBeParsed = 36,
        /// <summary>
        /// The employment stopdate could not be parsed
        /// </summary>
        InvalidEmploymentStopDateCannotBeParsed = 37,
        /// <summary>
        /// Value contains no date. If Employment dont have a stopdate OptionalEmploymentDate must contain a date
        /// </summary>
        InvalidEmploymentIfEmploymentDontHaveStopDateOptionaEmploymentdateMustContainDate = 38,
        /// <summary>
        /// Employee can only have one Employment without stopdate
        /// </summary>
        InvalidEmploymentEmpoyeeCanHaveOnlyOneEmploymentWithoutStopDate = 39,
        /// <summary>
        /// Already exists identical employment
        /// </summary>
        InvalidEmploymentAlreadyExistsIdenticalEmployment = 40,
        /// <summary>
        /// Employment not found
        /// </summary>
        InvalidEmploymentNotFound = 41,
        /// <summary>
        /// Employment not found. Endreason could not be set
        /// </summary>
        InvalidEmploymentNotFoundEndReasonNotSet = 42,
        /// <summary>
        /// TemporaryPrimary employment must have stop date
        /// </summary>
        InvalidEmploymentTemporaryPrimaryMustHaveStopDate = 44,
        /// <summary>
        /// TemporaryPrimary employment not found or invalid to be changed to not primary
        /// </summary>
        InvalidEmploymentTemporaryPrimaryNotValidToChangeToNotPrimary = 45,
        /// <summary>
        /// The employment could not be created or updated
        /// </summary>
        InvalidEmploymentCouldNotBeCreatedOrUpdated = 46,

        /// <summary>
        /// Invalid value. Account could not be mapped to existing externalcode, number or name
        /// </summary>
        InvalidValueAccountNotFound = 101,
        /// <summary>
        /// Invalid value. Position could not be mapped to an existing code, description or name
        /// </summary>
        InvalidValuePositionNotFound = 102,
        /// <summary>
        /// Invalid value. Role could not be mapped to an existing code, description or name
        /// </summary>
        InvalidValueRoleNotFound = 103,
        /// <summary>
        /// Invalid value. AttestRole could not be mapped to an existing code, description or name
        /// </summary>
        InvalidValueAttestRoleNotFound = 104,
        /// <summary>
        /// Invalid value. The EmployeeGroup could not be mapped to existing externalcode or name
        /// </summary>
        InvalidValueEmployeeGroupNotFound = 105,
        /// <summary>
        /// Invalid value. The PayrollGroup could not be mapped to existing externalcode or name
        /// </summary>
        InvalidValuePayrollGroupNotFound = 106,
        /// <summary>
        /// Invalid value. The VacationGroup could not be mapped to existing externalcode or name
        /// </summary>
        InvalidValueVacationGroupNotFound = 107,
        /// <summary>
        /// Invalid value. The EmploymentType could not be mapped to existing externalcode or name
        /// </summary>
        InvalidValueEmploymentTypeNotFound = 108,
        /// <summary>
        /// Invalid value. The EndReason could not be mapped to existing code, name or system id
        /// </summary>
        InvalidValueEndReasonNotFound = 109,
        /// <summary>
        /// Invalid value. The AbsenceCause could not be mapped to existing externalcode or name
        /// </summary>
        InvalidValueAbsenceCauseNotFound = 110,
        /// <summary>
        /// Invalid value. PayrollPriceType could not be parsed
        /// </summary>
        InvalidValuePayrollPriceTypeCannotBeParsed = 111,
        /// <summary>
        /// Invalid value. TimeWorkAccount could not be mapped to an existing name
        /// </summary>
        InvalidValueTimeWorkAccountNotFound = 112,
        /// <summary>
        /// Invalid value. TimeWorkAccount cannot overlap another TimeWorkAccount
        /// </summary>
        InvalidValueTimeWorkOverlapping = 113,
        /// <summary>
        /// Invalid value. The AnnualLeaveGroup could not be mapped to existing name
        /// </summary>
        InvalidValueAnnualLeaveGroupNotFound = 114,

        /// <summary>
        /// Invalid OptionalExternalCode. Account could not be mapped to existing externalcode, number or name
        /// </summary>
        InvalidOptionalExternalCodeAccountNotFound = 151,
        /// <summary>
        /// Invalid OptionalExternalCode. Parent account could not be mapped to existing externalcode, number or name
        /// </summary>
        InvalidOptionalExternalCodeParentAccountNotFound = 152,
        /// <summary>
        /// Invalid OptionalExternalCode. PayrollPriceType could not be mapped to existing externalcode
        /// </summary>
        InvalidValuePayrollPriceTypeNotFound = 153,
        /// <summary>
        /// Invalid OptionalExternalCode. PayrollLevel could not be mapped to existing externalcode
        /// </summary>
        InvalidOptionalExternalCodePayrollLevelNotFound = 154,
        /// <summary>
        /// Invalid OptionalExternalCode. The accounting could not be parsed
        /// </summary>
        InvalidOptionalExternalCodeAccountCannotBeParsed = 155,

    }

    #endregion

    /// <summary>
    /// Model containing information about the result of the import.
    /// The input data is also included in the result, with additional validation information.
    /// </summary>
    public class EmployeeChangeResult
    {
        /// <summary>
        /// If invalid the request could not be handled. See InvalidMessage for further information
        /// </summary>
        public bool Invalid { get; set; }
        /// <summary>
        /// Nr of Employees sent to the API
        /// </summary>
        /// /// <summary>
        /// Message if the request not could be handled. Example wrong API-key or invalid token
        /// </summary>
        public string InvalidMessage { get; set; }
        /// <summary>
        /// Number of employees sent to the endpoint
        /// </summary>
        public int NrOfReceivedEmployees { get; set; }
        /// <summary>
        /// Number of Employees where all changes was committed
        /// </summary>
        public int NrOfCommittedEmployees { get; set; }
        /// <summary>
        /// Number of Employees where at least one change was committed and at least one change was not committed
        /// </summary>
        public int NrOfPartlyCommittedEmployees { get; set; }
        /// <summary>
        /// Number of Employees where no changes was committed
        /// </summary>
        public int NrOfUnCommittedEmployees { get; set; }
        /// <summary>
        /// Original employees and their changes sent to the endpoint with added error validation
        /// </summary>
        public List<EmployeeChangeIODTO> Employees { get; set; }

        public EmployeeChangeResult()
        {
            this.Employees = new List<EmployeeChangeIODTO>();
        }

        public EmployeeChangeResult(List<EmployeeChangeIODTO> employees)
        {
            this.Employees = employees;
            this.NrOfReceivedEmployees = this.Employees.GetNrOfReceivedEmployees();
            this.NrOfCommittedEmployees = this.Employees.GetNrOfCommittedEmployees();
            this.NrOfPartlyCommittedEmployees = this.Employees.GetNrOfPartlyCommittedEmployees();
            this.NrOfUnCommittedEmployees = this.Employees.GetNrOfUnCommittedEmployees();
        }

        public EmployeeChangeResult(string invalidMessage)
        {
            this.Invalid = true;
            this.InvalidMessage = invalidMessage;
            this.Employees = new List<EmployeeChangeIODTO>();
        }
    }

    /// <summary>
    /// Model containing information about changes on an employee. This makes it possible to send delta changes on employee. It is also possible to create an employee if enough information are sent at the same request.
    /// </summary>
    public class EmployeeChangeIODTO
    {
        #region Properties

        /// <summary>
        /// EmployeeNr is the key to find the right employee. If employeeNr is not found the systems assumes that this is a new employee.
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Employee External Code is the alternative key to find the right employee. If externalCode is not found the systems assumes that this is a new employee. ExternalCode need to be set in the rows.
        public string EmployeeExternalCode { get; set; }
        /// <summary>
        /// OrganisationNr is the key to find the right organisation. If there are only one organisation, this field is not mandatory.
        /// </summary>
        public string OrganisationNr { get; set; }
        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<EmployeeChangeRowValidation> ValidationErrors
        {
            get
            {
                List<EmployeeChangeRowValidation> rowValidationErrors = this.EmployeeChangeRowIOs?.Where(i => !i.IsValid).SelectMany(i => i.ValidationErrors).ToList() ?? new List<EmployeeChangeRowValidation>();
                if (!headValidationErrors.IsNullOrEmpty())
                    rowValidationErrors.AddRange(headValidationErrors);
                return rowValidationErrors;
            }
            set
            {
                headValidationErrors = value;
            }
        }
        private List<EmployeeChangeRowValidation> headValidationErrors = null;
        /// <summary>
        /// True if atleast one change has been committed
        /// </summary>
        public bool HasChanges { get; set; }
        /// <summary>
        /// All changes on the employee.
        /// </summary>
        public List<EmployeeChangeRowIODTO> EmployeeChangeRowIOs { get; set; }

        #endregion

        #region Public methods

        public void AddMandatoryFields()
        {
            if (this.EmployeeChangeRowIOs == null)
                this.EmployeeChangeRowIOs = new List<EmployeeChangeRowIODTO>();

            if (!this.EmployeeChangeRowIOs.Any(i => i.EmployeeChangeType == EmployeeChangeType.FirstName))
                this.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO { EmployeeChangeType = EmployeeChangeType.FirstName, Value = string.Empty });
            if (!this.EmployeeChangeRowIOs.Any(i => i.EmployeeChangeType == EmployeeChangeType.LastName))
                this.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO { EmployeeChangeType = EmployeeChangeType.LastName, Value = string.Empty });
        }

        public bool DoLoadContactAddresses() => this.ContainsAnyType(GetContactAddressInformationTypes());
        public bool DoLoadEmploymentAccounts() => this.ContainsAnyType(EmployeeChangeType.AccountNrSieDim);
        public bool DoLoadEmploymentPriceTypes() => this.ContainsAnyType(EmployeeChangeType.EmploymentPriceType);
        public bool DoLoadEmployeePositions() => this.ContainsAnyType(EmployeeChangeType.EmployeePosition);
        public bool DoLoadExtraFieldRecords() => this.ContainsAnyType(EmployeeChangeType.ExtraFieldEmployee);
        public bool DoLoadEmployeeTimeWorkAccounts() => this.ContainsAnyType(EmployeeChangeType.TimeWorkAccount);
        public bool DoLoadRoles() => this.ContainsAnyType(GetRoleTypes());
        public bool DoLoadUserExternalAuth() => this.ContainsAnyType(EmployeeChangeType.ExternalAuthId);

        public List<EmployeeChangeRowValidation> Validate(List<GenericType> terms, Dictionary<EmployeeChangeType, bool> permissions, TermGroup_Country companyCountry)
        {
            if (this.EmployeeChangeRowIOs.IsNullOrEmpty())
            {
                this.ValidationErrors = new List<EmployeeChangeRowValidation>
                {
                    new EmployeeChangeRowValidation(EmployeeChangeType.None, EmploymentChangeValidationError.NoChanges, null, terms?.FirstOrDefault(t => t.Id == (int)EmploymentChangeValidationError.NoChanges)?.Name)
                };
            }
            else
            {
                foreach (EmployeeChangeRowIODTO row in this.EmployeeChangeRowIOs)
                {
                    row.FromDate = row.FromDate?.Date ?? row.FromDate;
                    row.ToDate = row.ToDate?.Date ?? row.ToDate;
                    row.OptionalEmploymentDate = row.OptionalEmploymentDate.Date;
                    row.Validate(terms, HasPermission(permissions, row.EmployeeChangeType), companyCountry);
                }
            }
            return this.ValidationErrors;
        }

        public List<string> GetValidationErrorStrings()
        {
            return this.ValidationErrors?.Select(i => $"{this.EmployeeNr}/{(int)i.Error} {i.Message}").ToList() ?? new List<string>();
        }

        public void SaveFailed(string errorMessage)
        {
            if (this.EmployeeChangeRowIOs == null)
                this.EmployeeChangeRowIOs = new List<EmployeeChangeRowIODTO>();

            this.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO
            {
                EmployeeChangeType = EmployeeChangeType.None,
                Value = errorMessage,
                ValidationErrors = new List<EmployeeChangeRowValidation>
                {
                    new EmployeeChangeRowValidation(EmployeeChangeType.None, EmploymentChangeValidationError.SaveFailed, null, errorMessage)
                }
            });
        }

        #endregion

        #region Private methods

        private bool HasPermission(Dictionary<EmployeeChangeType, bool> permissions, EmployeeChangeType type)
        {
            if (permissions == null || !permissions.ContainsKey(type))
                return false;

            return permissions[type];
        }

        private List<EmployeeChangeType> GetContactAddressInformationTypes()
        {
            return new List<EmployeeChangeType>()
            {
                EmployeeChangeType.ClosestRelativeName,
                EmployeeChangeType.ClosestRelativeName,
                EmployeeChangeType.ClosestRelativeNr,
                EmployeeChangeType.ClosestRelativeRelation,
                EmployeeChangeType.ClosestRelativeHidden,
                EmployeeChangeType.ClosestRelativeName2,
                EmployeeChangeType.ClosestRelativeNr2,
                EmployeeChangeType.ClosestRelativeRelation2,
                EmployeeChangeType.ClosestRelativeHidden2,
                EmployeeChangeType.PhoneHome,
                EmployeeChangeType.PhoneMobile,
                EmployeeChangeType.PhoneJob,
                EmployeeChangeType.Email,
                EmployeeChangeType.Address,
                EmployeeChangeType.AddressCO,
                EmployeeChangeType.AddressPostCode,
                EmployeeChangeType.AddressPostalAddress,
                EmployeeChangeType.AddressCountry,
                EmployeeChangeType.AddressHidden,
            };
        }

        private List<EmployeeChangeType> GetRoleTypes()
        {
            return new List<EmployeeChangeType>()
            {
                EmployeeChangeType.UserRole,
                EmployeeChangeType.DefaultUserRole,
                EmployeeChangeType.AttestRole,
                EmployeeChangeType.EmploymentStopDateChange,
            };
        }

        #endregion
    }

    /// <summary>
    /// If new or update value can not be empty or null. If delete then set Delete as true (Value will be ignored). OptionalCode is mandatory on lists of value on Employee.
    /// </summary>
    public class EmployeeChangeRowIODTO
    {
        #region Properties
        // Summery needs to be here in order for Swagger to display the description. Can not be set on the enum (which would have been great...)

        /// <summary>
        /// Active = 1 : (bool) Activate or deactive the employee and user (format: 1/true/yes/on/ja or 0/false/no/off/nej)<br/>
        /// FirstName = 2 : (string, max 100) Set or change first name (max: 100)<br/>
        /// LastName = 3 : (string, max 100) Set or change Last name (max: 100)<br/>
        /// SocialSec = 4 : (string) Set or change social security number (will be formatted to yyyyMMdd-xxxx). For bypassing validation set optionalExternalCode to "force"
        /// DisbursementMethod = 5: (int) Set or change the payroll payment method, (numeric value: 1 = Cash deposit, 2 = Personal account(SE), 3 = Account deposit(SE))
        /// DisbursementAccountNr = (string) 6: Set or change the bank account nr inlcuding clearing nr. Use '#' as separator (format: "ClearingNr#AccountNr")
        /// ExternalCode = 7: (string, max 100) Add or change employee external code (max: 100)
        /// DontValidateDisbursementAccountNr = 8: (bool) Set if the DisbursementAccountNr should not be validated<br/>
        /// EmployeeTemplateId = 11: If created from an employee template
        /// Vacant = 12: (bool) Set if the employee is vacant (1/true/yes/on/ja or 0/false/no/off/nej)
        /// HierarchicalAccount = 50 : (string) Set or change hierarchicalaccount (externalcode, number or name). Employee can have multuple accounts. Dates are optional and set thru FromDate and ToDate. Use optionalExternalCode set more information <br/>
        /// '2000|true#false' means connect to parent accountNr 2000 and set main allocation to true and default(show colleges) to false. So to set only main allocation to true, use '|true#' and to set only default use '|#true'. <br/>
        /// Use sort to sort levels, if you want to set a tree with 3 levels, three EmployeeChangeRowIODTOs are needed. First with sort 1, second with sort 2 and third with sort 3. <br/>
        /// AccountNrSieDim = 70 : (string) Employment field. Changes affect whole employment. Set or change account on employee, use account number as key and optionalexternalcode to identify SIE-level. Valid optionalexternalcodes (1,2,6,7,8,9,10,30,40,50)<br/>
        /// Email = 100 : (string) Set or change email (max: 100)
        /// PhoneHome = 101 : (string, max 100) Set or change cell phone number (max: 100)
        /// PhoneMobile = 102 : (string, max 100) Set or change office phone number (max: 100)
        /// PhoneJob = 103 : (string, max 100) Set or change office phone number (max: 100)
        /// ClosestRelativeNr = 104 : (string, max 100) Set or change number to closest relative (max: 100)
        /// ClosestRelativeName = 105 : (string, max 100) Set or change name of closest relative (max: 512 for Name and Relation combined)
        /// ClosestRelativeRelation = 106 : (string, max 100) Set or change type of relation (for example mother, wife, husband, sibling) on closest relative (max: 512 for Name and Relation combined)
        /// ClosestRelativeHidden = 107 : (bool) Make information on closest relative secret to everyone except closest executive (format: 1/true/yes/on/ja or 0/false/no/off/nej)
        /// ClosestRelativeNr2 = 108 : (string, max 100) Set or change number to closest relative (max: 100)
        /// ClosestRelativeName2 = 109 : (string, max 100) Set or change name of closest relative (max: 512 for Name2 and Relation2 combined)
        /// ClosestRelativeRelation2 = 110 : (string, max 100) Set or change type of relation (for exemple mother, wife, husband, sibling) on closest relative (max: 512 for Name2 and Relation2 combined)
        /// ClosestRelativeHidden2 = 111 : (bool) Make information on closest relative secret to everyone except closest executive (1/true/yes/on/ja or 0/false/no/off/nej)
        /// Address = 112 : (string, max 100) Set or change the delivery address (max: 100)
        /// AddressCO = 113 : (string, max 100) Set or change the address co (max: 100)
        /// AddressPostCode = 114 : (string, max 100) Set or change the postal code (max: 100)
        /// AddressPostalAddress = 115 : (string, max 100) Set or change the postal address (max: 100)
        /// AddressCountry = 116 : (string, max 100) Set or change the address country (max: 100)
        /// AddressHidden = 117 : (bool) Make adress secret to everyone except closest executive (1/true/yes/on/ja or 0/false/no/off/nej)
        /// ExtraFieldEmployee = 118 : (string) Set or change an extra field, use optionalExternalCode to set which field. Use value according to datatype of extrafield
        /// ExcludeFromPayroll = 119: (bool) Set if the Employee should be exclude from payrollcalculation and export
        /// WantsExtraShifts = 121: (bool) Set if the Employee wants extra shifts
        /// EmploymentStartDateChange = 200 : (date) Add or change start date of employment (format: yyyyMMdd)
        /// EmploymentStopDateChange = 201 : (date) Add or change end date of employment (format: yyyyMMdd)
        /// EmployeeGroup = 202 :  (string) Employment field with changes thru FromDate (mandatory) and ToDate. Add or change employeegroup (Time agreement) on employee (externalcode or name)<br/>
        /// PayrollGroup = 203 : (string) Employment field with changes thru FromDate (mandatory) and ToDate. Add or change payroll group employee (externalcode or name)<br/>
        /// VacationGroup = 204 : (string) Employment field with changes thru FromDate (mandatory) and ToDate. Add or change vaction group employee (externalcode or name)<br/>
        /// WorkTimeWeekMinutes = 205 : (decimal) Employment field with changes thru FromDate and ToDate. Number of minutes in a week according to employment percent. Choose to send WorkTimeWeekMinutes OR EmploymentPercent <br/>
        /// EmploymentPercent = 206 : (decimal) Employment field with changes thru FromDate and ToDate. Add or change employment percent. Choose to send WorkTimeWeekMinutes OR EmploymentPercent<br/>
        /// EmploymentExternalCode = 207 : (string) Employment field. Add or change employment external code. (max: 50)<br/>
        /// EmploymentType = 208 : (string) Employment field with changes thru FromDate and ToDate. Add or change employment type on employee (code, name or type)<br/>
        /// EmployeePosition = 209 : (string) Add or change employee position (code, name or type)
        /// EmployeePositionDefault = 210 (bool) Set if the EmployeePosition should be default
        /// TaxRate = 211 : (int) Set taxrate table on new employee.
        /// IsSecondaryEmployment = 212 : (bool) Employment field with changes thru FromDate and ToDate. Set if employment is secondary. (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
        /// ExperienceMonths = 220 : (int) Employment field with changes thru FromDate and ToDate. Set number of experience months.<br/>
        /// ExperienceAgreedOrEstablished = 221 : (bool) Employment field with changes thru FromDate and ToDate. Set if i experience is agreed (true) or Established (false) (1/true/yes/on/ja or 0/false/no/off/nej)<br/>
        /// WorkTasks = 223 : (string) Employment field with changes thru FromDate and ToDate. Set WorkTasks on employment<br/>
        /// SpecialConditions = 224 : (string) Employment field with changes thru FromDate and ToDate. Set special conditions on employment<br/>
        /// WorkPlace = 225 : (string, max 100) Employment field with changes thru FromDate and ToDate. Set work place on employment<br/>
        /// SubstituteFor = 226 : (string) Employment field with changes thru FromDate and ToDate. Set "substitue for" place on employment<br/>
        /// SubstituteForDueTo = 227 : (string) Employment field with changes thru FromDate and ToDate. Set why the "SubstituteFor" needed to be substituted<br/>
        /// EmploymentEndReason = 228 : (string) Employment field. Set why the employment ended. Use OptionalEmploymentDate to point to the employment to change. The employment must have stopDate or have stopDate (201) in current batch.<br/>
        /// BaseWorkTimeWeek = 229 (int) Employment field with changes thru FromDate and ToDate. Set base work time week minutes<br/>
        /// EmploymentStopDateKeepScheduleHierarchicalAccountAndAttestRole = 251 : (bool) If 201 is also sent and schedule, hierarchical account and attestrole shouldnt be closed on that date. Use only if another employment is created directly afterwards and that employment ends same date or later than the closed employment<br/>
        /// ClearScheduleFrom = 252 : (DateTime) Set this to clear all active schedules after set date.
        /// ChangeToNotTemporaryPrimary = 253 : (bool) Set this to true to change the temporary primary employment to ordinary from the date passed as FromDate. The new ordinary employment will inherit the settings from temporary primary employments last date. ToDate is an optional stop date of the new ordinary employment.
        /// WorkTimeWeekMinutes = 254 : (decimal) Employment field with changes thru FromDate and ToDate. Number of fulltime minutes in a week according. If empty fulltime workminutes on employee group vill we used <br/>
        /// DoNotValidateAccount = 261 : (bool) Set this to true if bankaccount shouldnt be validated <br/>
        /// DisbursementCountryCode = 262 : (string, max 10) Set or change bank account country code (max: 10)<br/>
        /// DisbursementBIC = 263 : (string, max 50) Set or change bank account BIC (max: 50)<br/>
        /// DisbursementIBAN = 264 : (string, max 100) Set or change IBAN bank account (max: 100)<br/>
        /// EmploymentPriceType = 300 : (decimal, date and string) Employmentpricetype is salyartype on employee. Send value as Value and code/number of pricetyp as optional external code and startDate as fromdate
        /// UserRole = 400 : (string) Add or change UserRole (external code or name). User can have multuple roles. Dates are optional and set thru FromDate and ToDate.<br/>
        /// AttestRole = 401 : (string) Add or change attest role. If attestrole is connected to account use account number in optionalexternalcode. (OptinalExternalCode: external code or name). User can have multuple roles. Dates are optional and set thru FromDate and ToDate.<br/>
        /// BlockedFromDate = 402 : (date) Sets a date on user that disables the possibility to login (format: yyyyMMdd or empty to reset)
        /// DefaultUserRole = 403 : Sets the Role to default for the User. Note that if you change Default Role, and the earlier Default Role should be removed, then 400 with flag Delete must be used also. Othwerise the earlier role still exists for the user, but not as default. (external code or name)
        /// ExternalAuthId = 500 : (string) Code for SSO-solution. Identity in external provider. (external provider must be set on license). Set Delete to delete existing value.
        /// PayrollStatisticsPersonalCategory = 601 : (int) Set or change field "SCB och Näringsliv: Personalkategori" (0 = Inget, 1 = Arbetare, 2 = Övriga (t.ex. tjänstemän) för Stål- och metallförbundet, 3 = Arbetare röda avtalet, 4 = Arbetare blå avtalet, 5 = Arbetare elavtalet, 6 = Medarbetare gröna avtalet, 7 = SIF, 8 = CF, 9 = LEDARNA)
        /// PayrollStatisticsWorkTimeCategory = 602 : (int) Set or change field "SCB och Näringsliv: Arbetstidart" (0 = Inget, 1 = Dagarbete, 2 = 2-skift, 3 = Intermittent 3-skift, 4 = Kontinuerligt 3-skift, 5 = Kontinuerligt skift med storhelgsdrift, arbete under jord. 6 = Ständigt nattskift, 7 = Deltidsarbete, 8 = Deltidspensionär)
        /// PayrollStatisticsSalaryType = 603 : (int) Set or change field "SCB och Näringsliv: Anställningsform" (0 = Inget, 1 = Månadslön, 2 = Veckolön, 3 = Timlön)
        /// PayrollStatisticsWorkPlaceNumber = 604 : (int) Set or change field "SCB och Näringsliv: Arbetsplatsnummer"
        /// PayrollStatisticsCFARNumber = 605 : (int) Set or change field "SCB och Näringsliv: CFAR-nummer"
        /// ControlTaskWorkPlacSCB = 611 : (string, 100) Set or change field "Kontrolluppgift: Arbetsställe SCB"
        /// ControlTaskPartnerInCloseCompany = 612 : (bool) Set or change field "Kontrolluppgift: Delägare i fåmansföretag"
        /// ControlTaskBenefitAsPension = 613 : (bool) Set or change field "Kontrolluppgift: Förmån som pension"
        /// AFACategory = 621 : (int) Set or change field "AFA (FORA): Kategori" (0 = Inget, 1 = Arbetare, 2 = Tjänsteman/anställd VD, 3 = Undantas helt)
        /// AFASpecialAgreement = 622 : (int) Set or change field "AFA (FORA): Speciellt avtal" (0 = Inget, 1 = Eget avtal Tjänstemän)
        /// AFAWorkplaceNr = 623 : (string, max 100) Set or change field "AFA (FORA): Arbetsplatsnummer"
        /// AFAParttimePensionCode = 624 : (bool) Set or change field "Kontrolluppgift: Kod för deltidspension"
        /// CollectumITPPlan = 631 : (int) Set or change field "Collectum: ITP-plan" (0 = Inget, 1 = ITP1, 2 = ITP2)        
        /// CollectumAgreedOnProduct = 632 : (string, max 100) Set or change field "Collectum: Avtalad produkt"
        /// CollectumCostPlace = 633 : (string, max 100) Set or change field "Collectum: Kostnadsställe"
        /// CollectumCancellationDate = 634 :  (date) Set or change field "Collectum: Avanmäld datum" (format: yyyyMMdd)
        /// CollectumCancellationDateIsLeaveOfAbsence = 635 : (bool) Set or change field "Collectum: Tjänsteledig"
        /// KPARetirementAge = 641 : (int) Set or change field "KPA: Pensionsålder"
        /// KPABelonging = 642 :  (int) Set or change field "KPA: Tillhörighet" (0 = Inget, 1 = BEA, 2 = PAN, 3 = Medstud)
        /// KPAEndCode = 643 : (int) Set or change field "KPA: Avslutskod" (1 = U1, 2 = U3, 3 = US, 4 = UD)
        /// KPAAgreementType = 644 : (int) Set or change field "KPA: Typ av avtal" (0 = Inget, 1 = PFA01, 2 = PFA98, 3 = KAP_KL, 4 = AKAP_KL)
        /// BygglosenAgreementArea = 651 :  (string, max 10) Set or change field "Bygglösen: Bygglösen avtalsomårde"
        /// BygglosenAllocationNumber = 652 : (string, max 10) Set or change field "Bygglösen: Bygglösen fördelningstal"
        /// BygglosenSalaryFormula = 653 : (string, max 10) Set or change formula for field "Bygglösen: Bygglösen avtalad lön" (name)
        /// BygglosenMunicipalCode = 654 : (string, max 10) Set or change field "Bygglösen: Bygglösen Bygglösen kommunkod"
        /// GTPAgreementNumber = 661 : (string) Folksam pension agreement number
        /// GTPExcluded = 662 : (bool) Exclude from GTP (Folksam) (1/true/yes/on/ja or 0/false/no/off/nej)
        /// AGIPlaceOfEmploymentAddress = 671 : (string, max 100) Set or change field AGI "Place of employment address"
        /// AGIPlaceOfEmploymentCity = 672 : (string, max 100) Set or change field AGI "Place of employment city"
        /// AGIPlaceOfEmploymentIgnore = 673 : (bool) Set or change field AGI "Leave empty in AGI-file" (1/true/yes/on/ja or 0/false/no/off/nej)
        /// VacationDaysPaid = 700 : (decimal) Number of paid day of vacation remaining (only new employee)
        /// VacationDaysUnPaid = 701 : (decimal) Number of unpaid day of vacation remaining (only new employee)
        /// VacationDaysAdvance = 702 : (decimal) Number of advance paid day of vacation remaining (only new employee)
        /// TaxTinNumber = 710 : (string, max 50) Employee tax, abroad, TIN Number
        /// TaxCountryCode = 711 : (string, max 5) Employee tax, abroad, TIN Country Code
        /// TaxBirthPlace = 712 : (string, max 50) Employee tax, abroad, Birth place
        /// TaxCountryCodeBirthPlace = 713 : (string, max 50) Employee tax, Country Code birth place
        /// TaxCountryCodeCitizen = 714 : (string, max 50) Employee tax, abroad, Country Code citizen
        /// ParentEmployeeNr = 717 : (string) Set or change Parent EmployeeNr on employee (This is not the parent in the biologic sense, Employee and Parent is the same Person (Only needed for certain specific scenarios) 
        /// EmployerRegistrationNr = 718 : (string, max 50) Set or change organisation on employee. Value is organisation number and FromDate (mandatory) and ToDateOnly. (Only needed for certain specific scenarios (max: 50) 
        /// NewEmployments = 1002: Use the list of NewEmploymentRowIO. This can also be used for importing old employments on startup.
        /// </summary>
        public EmployeeChangeType EmployeeChangeType { get; set; }
        /// <summary>
        /// Set the value of the change. For exemple if the value is decimal, set "1.1" or "1,1". The correct type will be parsed.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Use fromDate where it is applicable. Changes to simple values such as first name do not have any date functionality
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Use toDate where it is applicable. Changes to simple values such as first name do not have any date functionality
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// OptionalExternalCode is used where additional information is needed in order where to change value. (HierachicalAccount and AttestRole)
        /// </summary>
        public string OptionalExternalCode { get; set; }
        /// <summary> 
        /// When CRUD on employment OptinalEmploymentDate is mandatory
        /// </summary>
        public DateTime OptionalEmploymentDate { get; set; }
        /// <summary> 
        /// When CRUD on employment OptionalEmploymentExternalCode is mandatory if multiple employments exist at the same date with different codes
        /// </summary>
        public string OptionalEmploymentExternalCode { get; set; }
        /// <summary>
        /// Use sort to set order of execution on same EmployeeChangeType
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// Delete is used to delete connection between employee and connected role(AttestRole and UserRole). Also used for delete ExternalAuthId
        /// </summary>
        public bool Delete { get; set; }
        /// <summary>
        /// Optional information for tracking - No other logic
        /// </summary>
        public string Trace { get; set; }
        /// <summary>
        /// Optional value. Is used to find a existing value (when multiple can be found)
        /// </summary>
        public string PreviousValue { get; set; }
        /// <summary>
        /// Used to create new employements.
        /// </summary>
        public List<NewEmploymentRowIO> NewEmploymentRows { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<EmployeeChangeRowValidation> ValidationErrors { get; set; }
        /// <summary>
        /// Isvalid is based on no ValidationErrors
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.ValidationErrors.IsNullOrEmpty();
            }
            set //NOSONAR
            {
                //Not supported, but needed for serialization
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Use this to Validate model
        /// </summary>
        /// <returns></returns>
        public bool Validate(List<GenericType> terms, bool hasPermission, TermGroup_Country companyCountry)
        {
            if (!hasPermission)
                AddValidationError(EmploymentChangeValidationError.PermissionMissing);

            switch (this.EmployeeChangeType)
            {
                #region ValidateValueAsBool

                case EmployeeChangeType.Active:
                case EmployeeChangeType.Vacant:
                case EmployeeChangeType.ExcludeFromPayroll:
                case EmployeeChangeType.WantsExtraShifts:
                case EmployeeChangeType.AddressHidden:
                case EmployeeChangeType.ClosestRelativeHidden:
                case EmployeeChangeType.ClosestRelativeHidden2:
                case EmployeeChangeType.EmploymentStopDateKeepScheduleHierarchicalAccountAndAttestRole:
                case EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence:
                case EmployeeChangeType.ControlTaskPartnerInCloseCompany:
                case EmployeeChangeType.ControlTaskBenefitAsPension:
                case EmployeeChangeType.GTPExcluded:
                case EmployeeChangeType.AGIPlaceOfEmploymentIgnore:
                case EmployeeChangeType.DoNotValidateAccount:
                    ValidateValueAsBool();
                    break;

                #endregion

                #region ValidateValueAsDate / ValidateFromDate / ValidateToDate

                case EmployeeChangeType.AccountNrSieDim:
                    ValidateValueAsStringIfNotDelete();
                    ValidateOptionalExternalCode();
                    break;
                case EmployeeChangeType.BlockedFromDate:
                case EmployeeChangeType.CollectumCancellationDate:
                case EmployeeChangeType.ClearScheduleFrom:
                    ValidateValueAsDate();
                    break;
                case EmployeeChangeType.EmploymentType:
                case EmployeeChangeType.EmploymentExternalCode:
                case EmployeeChangeType.WorkTasks:
                case EmployeeChangeType.WorkPlace:
                case EmployeeChangeType.SpecialConditions:
                case EmployeeChangeType.SubstituteFor:
                case EmployeeChangeType.SubstituteForDueTo:
                    ValidateValueAsString();
                    ValidateFromDate();
                    ValidateToDate();
                    break;
                case EmployeeChangeType.EmploymentStartDateChange:
                    ValidateValueAsDate(acceptNull: false);
                    ValidateFromDate();
                    ValidateOptionalEmploymentDate();
                    break;
                case EmployeeChangeType.EmploymentStopDateChange:
                    ValidateValueAsDate(acceptNull: true);
                    ValidateOptionalEmploymentDate();
                    break;
                case EmployeeChangeType.EmployeeGroup:
                case EmployeeChangeType.PayrollGroup:
                case EmployeeChangeType.VacationGroup:
                case EmployeeChangeType.AnnualLeaveGroup:
                case EmployeeChangeType.EmployerRegistrationNr:
                    ValidateFromDate(acceptNull: false);
                    ValidateToDate();
                    break;
                case EmployeeChangeType.HierarchicalAccount:
                    ValidateValueAsString();
                    var acceptNull = !string.IsNullOrEmpty(this.PreviousValue) && this.PreviousValue.Contains("|");
                    ValidateFromDate(acceptNull: acceptNull, defaultDateTimeSameAsNull: false);
                    ValidateToDate();
                    break;
                case EmployeeChangeType.WorkTimeWeekMinutes:
                case EmployeeChangeType.FullTimeWorkTimeWeekMinutes:
                case EmployeeChangeType.BaseWorkTimeWeek:
                case EmployeeChangeType.ExperienceMonths:
                    ValidateFromDate();
                    ValidateToDate();
                    ValidateValueAsInt();
                    break;
                case EmployeeChangeType.EmploymentPercent:
                    ValidateFromDate();
                    ValidateToDate();
                    break;
                case EmployeeChangeType.EmploymentPriceType:
                    ValidateFromDate();
                    ValidateValueAsDecimal();
                    ValidateOptionalExternalCode();
                    break;
                case EmployeeChangeType.TimeWorkAccount:
                case EmployeeChangeType.AttestRole:
                case EmployeeChangeType.UserRole:
                    ValidateValueAsStringIfNotDelete();
                    ValidateFromDate();
                    ValidateToDate();
                    break;
                case EmployeeChangeType.ExperienceAgreedOrEstablished:
                case EmployeeChangeType.IsSecondaryEmployment:
                case EmployeeChangeType.ChangeToNotTemporaryPrimary:
                    ValidateValueAsBool();
                    ValidateFromDate();
                    ValidateToDate();
                    break;
                #endregion

                #region ValidateValueAsDecimal

                case EmployeeChangeType.VacationDaysPaid:
                case EmployeeChangeType.VacationDaysUnPaid:
                case EmployeeChangeType.VacationDaysAdvance:
                    ValidateValueAsDecimal();
                    break;

                #endregion

                #region ValidateValueAsDecimalIfNotDelete

                case EmployeeChangeType.BygglosenAgreedHourlyPayLevel:
                    ValidateValueAsDecimalIfNotDelete();
                    break;

                #endregion

                #region ValidateValueAsEnum

                case EmployeeChangeType.AFACategory:
                    ValidateValueAsEnum(typeof(TermGroup_AfaCategory));
                    break;
                case EmployeeChangeType.AFASpecialAgreement:
                    ValidateValueAsEnum(typeof(TermGroup_AfaSpecialAgreement));
                    break;
                case EmployeeChangeType.CollectumITPPlan:
                    ValidateValueAsEnum(typeof(TermGroup_PayrollReportsCollectumITPplan));
                    break;
                case EmployeeChangeType.KPABelonging:
                    ValidateValueAsEnum(typeof(KpaBelonging));
                    break;
                case EmployeeChangeType.KPAEndCode:
                    ValidateValueAsEnum(typeof(KpaEndCode));
                    break;
                case EmployeeChangeType.KPAAgreementType:
                    ValidateValueAsEnum(typeof(KpaAgreementType));
                    break;
                case EmployeeChangeType.PayrollStatisticsPersonalCategory:
                    ValidateValueAsEnum(typeof(TermGroup_PayrollExportPersonalCategory));
                    break;
                case EmployeeChangeType.PayrollStatisticsWorkTimeCategory:
                    ValidateValueAsEnum(typeof(TermGroup_PayrollExportWorkTimeCategory));
                    break;
                case EmployeeChangeType.PayrollStatisticsSalaryType:
                    ValidateValueAsEnum(typeof(TermGroup_PayrollExportSalaryType));
                    break;
                case EmployeeChangeType.GTPAgreementNumber:
                    ValidateValueAsEnum(typeof(TermGroup_GTPAgreementNumber));
                    break;
                case EmployeeChangeType.BygglosenSalaryType:
                    ValidateValueAsEnum(typeof(TermGroup_BygglosenSalaryType));
                    break;
                #endregion

                #region ValidateValueAsInt

                case EmployeeChangeType.EmployeeTemplateId:
                case EmployeeChangeType.KPARetirementAge:
                case EmployeeChangeType.IFAssociationNumber:
                case EmployeeChangeType.IFPaymentCode:
                case EmployeeChangeType.TaxRate:
                case EmployeeChangeType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                    ValidateValueAsInt();
                    break;

                #endregion

                #region ValidateValueAsString

                case EmployeeChangeType.TaxCountryCode:
                    ValidateValueAsString(5);
                    break;
                case EmployeeChangeType.DisbursementCountryCode:
                    ValidateValueAsString(10);
                    break;
                case EmployeeChangeType.DisbursementBIC:
                case EmployeeChangeType.TaxTinNumber:
                case EmployeeChangeType.TaxBirthPlace:
                case EmployeeChangeType.TaxCountryCodeBirthPlace:
                case EmployeeChangeType.TaxCountryCodeCitizen:
                case EmployeeChangeType.ParentEmployeeNr:
                    ValidateValueAsString(50);
                    break;
                case EmployeeChangeType.Address:
                case EmployeeChangeType.AddressCO:
                case EmployeeChangeType.AddressPostCode:
                case EmployeeChangeType.AddressPostalAddress:
                case EmployeeChangeType.AddressCountry:
                case EmployeeChangeType.ClosestRelativeNr:
                case EmployeeChangeType.ClosestRelativeName:
                case EmployeeChangeType.ClosestRelativeRelation:
                case EmployeeChangeType.ClosestRelativeNr2:
                case EmployeeChangeType.ClosestRelativeName2:
                case EmployeeChangeType.ClosestRelativeRelation2:
                case EmployeeChangeType.ExternalCode:
                case EmployeeChangeType.FirstName:
                case EmployeeChangeType.LastName:
                case EmployeeChangeType.PhoneHome:
                case EmployeeChangeType.PhoneMobile:
                case EmployeeChangeType.PhoneJob:
                case EmployeeChangeType.DisbursementIBAN:
                    ValidateValueAsString(100);
                    break;
                case EmployeeChangeType.DefaultUserRole:
                case EmployeeChangeType.EmployeePositionDefault:
                    ValidateValueAsString();
                    break;

                #endregion

                #region ValidateValueAsStringIfNotDelete

                case EmployeeChangeType.BygglosenAgreementArea:
                case EmployeeChangeType.BygglosenAllocationNumber:
                case EmployeeChangeType.BygglosenMunicipalCode:
                case EmployeeChangeType.BygglosenProfessionCategory:
                case EmployeeChangeType.BygglosenLendedToOrgNr:
                case EmployeeChangeType.IFWorkPlace:
                    ValidateValueAsStringIfNotDelete(10);
                    break;
                case EmployeeChangeType.AFAWorkplaceNr:
                case EmployeeChangeType.AGIPlaceOfEmploymentCity:
                case EmployeeChangeType.AGIPlaceOfEmploymentAddress:
                case EmployeeChangeType.ControlTaskWorkPlaceSCB:
                case EmployeeChangeType.CollectumAgreedOnProduct:
                case EmployeeChangeType.CollectumCostPlace:
                    ValidateValueAsStringIfNotDelete(100);
                    break;
                case EmployeeChangeType.BygglosenSalaryFormula:
                case EmployeeChangeType.BygglosenWorkPlaceNumber:
                case EmployeeChangeType.AFAParttimePensionCode:
                case EmployeeChangeType.EmployeePosition:
                case EmployeeChangeType.PayrollStatisticsWorkPlaceNumber:
                case EmployeeChangeType.PayrollStatisticsCFARNumber:
                case EmployeeChangeType.ExternalAuthId:
                    ValidateValueAsStringIfNotDelete();
                    break;

                #endregion

                #region ValidateSocialSec

                case EmployeeChangeType.SocialSec:
                    ValidateSocialSec(this.Value, companyCountry);
                    break;

                #endregion

                #region ValidateDisbursementMethod / ValidateDisbursementClearingAndAccountNr

                case EmployeeChangeType.DisbursementMethod:
                    ValidateDisbursementMethod(this.Value);
                    break;
                case EmployeeChangeType.DisbursementAccountNr:
                    ValidateDisbursementClearingAndAccountNr(this.Value);
                    break;

                #endregion

                #region Email

                case EmployeeChangeType.Email:
                    ValidateEmail(this.Value);
                    break;

                #endregion

                #region EmploymentEndReason

                case EmployeeChangeType.EmploymentEndReason:
                    ValidateValueAsString();
                    ValidateOptionalEmploymentDate();
                    break;

                #endregion

                #region ExtraFieldEmployee

                case EmployeeChangeType.ExtraFieldEmployee:
                    ValidateValueAsString();
                    ValidateOptionalExternalCode();
                    break;

                #endregion

                #region NewEmploymentRows
                case EmployeeChangeType.NewEmployments:
                    ValidateNewEmploymentRows(this.NewEmploymentRows);
                    break;
                #endregion

                default:
                    AddValidationError(EmploymentChangeValidationError.InvalidChange, ((int)this.EmployeeChangeType).ToString());
                    break;
            }

            if (!this.ValidationErrors.IsNullOrEmpty() && !terms.IsNullOrEmpty())
            {
                foreach (EmployeeChangeRowValidation validationError in this.ValidationErrors)
                {
                    validationError.SetMessage(terms.FirstOrDefault(t => t.Id == (int)validationError.Error)?.Name);
                }
            }

            return this.IsValid;
        }

        #endregion

        #region Private methods

        private void ValidateValueAsString(int? maxLength = null)
        {
            string source = this.Value;

            if (string.IsNullOrEmpty(source))
                AddValidationError(EmploymentChangeValidationError.InvalidValueMustHaveValue);
            if (maxLength.HasValue)
                ValidateStringLength(maxLength.Value);
        }

        private void ValidateValueAsStringIfNotDelete(int? maxLength = null)
        {
            string source = this.Value;

            if (string.IsNullOrEmpty(source) && !this.Delete)
                AddValidationError(EmploymentChangeValidationError.InvalidValueMustHaveValueWhenNotDelete);
            if (maxLength.HasValue)
                ValidateStringLength(maxLength.Value);
        }

        private void ValidateStringLength(int maxLength)
        {
            if (this.Value != null && this.Value.Length > maxLength)
                AddValidationError(EmploymentChangeValidationError.StringLengthExceedsMaxLength, maxLength);
        }

        private void ValidateValueAsBool()
        {
            string source = this.Value;

            if (!StringUtility.IsValidBool(source))
                AddValidationError(EmploymentChangeValidationError.InvalidBool);
        }

        private void ValidateValueAsDecimal()
        {
            string source = this.Value;

            if (decimal.TryParse(source, out _))
                return;
            if (!string.IsNullOrEmpty(source) && source.Contains(",") && decimal.TryParse(source.Replace(",", "."), out _))
                return;
            if (!string.IsNullOrEmpty(source) && source.Contains(".") && decimal.TryParse(source.Replace(".", ","), out _))
                return;

            AddValidationError(EmploymentChangeValidationError.InvalidDecimal);
        }

        private void ValidateValueAsDecimalIfNotDelete()
        {
            string source = this.Value;

            if (string.IsNullOrEmpty(source) && !this.Delete)
                AddValidationError(EmploymentChangeValidationError.InvalidValueMustHaveValueWhenNotDelete);
        }

        private void ValidateValueAsInt()
        {
            if (this.Delete)
                return;

            if (!string.IsNullOrEmpty(this.Value))
            {
                string source = this.Value;
                if (int.TryParse(source, out _))
                    return;

                if (decimal.TryParse(source.Replace(".", ","), out Decimal decimalValue))
                {
                    decimalValue = Math.Round(decimalValue, 0);
                    this.Value = Convert.ToInt32(decimalValue).ToString();
                    return;
                }
            }
            AddValidationError(EmploymentChangeValidationError.InvalidInt);
        }

        private void ValidateValueAsEnum(Type enumType)
        {
            ValidateValueAsStringIfNotDelete();

            if (this.Delete)
                return;

            if (!string.IsNullOrEmpty(this.Value))
            {
                string source = this.Value;
                if (int.TryParse(source, out int id) && Enum.IsDefined(enumType, id))
                    return;
            }
            AddValidationError(EmploymentChangeValidationError.InvalidEnum);
        }

        private void ValidateDate(DateTime? source, bool acceptNull = true, bool defaultDateTimeSameAsNull = true)
        {
            bool isEmpty = !source.HasValue || source.Value == DateTime.MinValue || (defaultDateTimeSameAsNull && source.Value == CalendarUtility.DATETIME_DEFAULT);
            if (isEmpty && acceptNull)
                return;

            if (isEmpty)
                AddValidationError(EmploymentChangeValidationError.InvalidDateCannotBeNull);
            if (source < DateTime.Now.AddYears(-200) || source > DateTime.Now.AddYears(200))
                AddValidationError(EmploymentChangeValidationError.InvalidDate);
        }

        private void ValidateValueAsDate(bool acceptNull = true)
        {
            if (string.IsNullOrEmpty(this.Value) && acceptNull)
                return;

            if (DateTime.TryParse(this.Value, out DateTime date))
                ValidateDate(date, acceptNull);
            else
                AddValidationError(EmploymentChangeValidationError.InvalidDate);
        }

        private void ValidateFromDate(bool acceptNull = true, bool defaultDateTimeSameAsNull = true)
        {
            ValidateDate(this.FromDate, acceptNull, defaultDateTimeSameAsNull);
        }

        private void ValidateToDate(bool acceptNull = true)
        {
            ValidateDate(this.ToDate, acceptNull);
        }

        private void ValidateOptionalExternalCode()
        {
            string source = this.OptionalExternalCode;

            if (string.IsNullOrEmpty(source) && !this.Delete)
                AddValidationError(EmploymentChangeValidationError.InvalidOptionExternalCodeCannotBeEmpty);
        }

        private void ValidateOptionalEmploymentDate(bool acceptNull = true)
        {
            DateTime source = this.OptionalEmploymentDate;

            bool isEmpty = source == DateTime.MinValue;
            if (isEmpty && acceptNull)
                return;

            if (isEmpty)
                AddValidationError(EmploymentChangeValidationError.InvalidEmploymentDateCannotBeNull);
            if (source < DateTime.Now.AddYears(-200) || source > DateTime.Now.AddYears(200))
                AddValidationError(EmploymentChangeValidationError.InvalidEmploymentDate);
        }

        private void ValidateEmail(string source)
        {
            ValidateValueAsString();

            if (!Validator.ValidateEmail(source))
                AddValidationError(EmploymentChangeValidationError.InvalidEmail);
        }

        private void ValidateSocialSec(string source, TermGroup_Country companyCountry)
        {
            ValidateValueAsString();

            if (!string.IsNullOrEmpty(this.OptionalExternalCode) && this.OptionalExternalCode.Equals("force", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(source))
                    AddValidationError(EmploymentChangeValidationError.InvalidSocialSecToToShort);

                return;
            }
            else
            {
                if (!string.IsNullOrEmpty(source) && source.Length < 8)
                    AddValidationError(EmploymentChangeValidationError.InvalidSocialSecToToShort);
            }

            string socialSec = (companyCountry == TermGroup_Country.SE) ? StringUtility.SocialSecYYYYMMDD_Dash_XXXX(source) : source;
            if (!CalendarUtility.IsValidSocialSecurityNumber(companyCountry, socialSec, true, true, true))
                AddValidationError(EmploymentChangeValidationError.InvalidSocialSec);
        }

        private void ValidateDisbursementMethod(string source)
        {
            ValidateValueAsInt();

            if (int.TryParse(source, out int intValue) && (intValue <= 0 || !Enum.IsDefined(typeof(TermGroup_EmployeeDisbursementMethod), intValue)))
                AddValidationError(EmploymentChangeValidationError.InvalidDisbursementMethodInvalidValue);
        }

        private void ValidateNewEmploymentRows(List<NewEmploymentRowIO> source)
        {
            if (source.IsNullOrEmpty())
                AddValidationError(EmploymentChangeValidationError.DataNotFound);
        }

        private void ValidateDisbursementClearingAndAccountNr(string source)
        {
            ValidateValueAsString();

            if (!string.IsNullOrEmpty(source))
            {
                string[] values = StringUtility.Split(new char[1] { '#' }, source);

                // No validation
                if (values.Count() > 2 && StringUtility.IsValidBool(values[2]) && StringUtility.GetBool(values[2]))
                    return;

                if (values.Count() < 2 || !Validator.IsValidBankNumberSE(null, values[0], values[1]))
                    AddValidationError(EmploymentChangeValidationError.InvalidDisbursementAccountNr);
            }
        }

        private void AddValidationError(EmploymentChangeValidationError error, object value = null)
        {
            AddValidationError(new EmployeeChangeRowValidation(this.EmployeeChangeType, error, value ?? this.Value));
        }

        public void AddValidationError(EmployeeChangeRowValidation validationError)
        {
            if (validationError == null)
                return;

            if (this.ValidationErrors == null)
                this.ValidationErrors = new List<EmployeeChangeRowValidation>();
            if (!this.ValidationErrors.Any(i => i.Error == validationError.Error))
                this.ValidationErrors.Add(validationError);
        }

        #endregion
    }

    public class EmployeeChangeRowValidation
    {
        /// <summary>
        /// The type of type of change that failed
        /// </summary>
        public EmployeeChangeType Type { get; private set; }
        /// <summary>
        /// The validation error
        /// </summary>
        public EmploymentChangeValidationError Error { get; private set; }
        /// <summary>
        /// The description of the error
        /// </summary>
        public object Value { get; private set; }
        /// <summary>
        /// The description of the error
        /// </summary>
        public string Message { get; private set; }

        public EmployeeChangeRowValidation(EmployeeChangeType type, EmploymentChangeValidationError error, object value, string message = null)
        {
            this.Type = type;
            this.Error = error;
            this.Value = value;
            this.SetMessage(message);
        }

        public void SetMessage(string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{(int)this.Type}/{(int)this.Error}");
            if (!message.IsNullOrEmpty())
            {
                sb.Append($": {message}");
                if (this.Value != null && !string.IsNullOrEmpty(this.Value.ToString()))
                    sb.Append($" ({this.Value})");
            }
            this.Message = sb.ToString();
        }
    }

    public class NewEmploymentRowIO
    {
        public NewEmploymentRowIO()
        {
            IsSecondaryEmployment = false;
        }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        /// <summary>
        /// Use Code to match
        /// </summary>
        public string EmploymentTypeCode { get; set; }
        /// <summary>
        /// WorkTimeWeek is i minutes.
        /// </summary>
        public int WorkTimeWeek { get; set; }
        /// <summary>
        /// FullTimeWorkTimeWeek is i minutes.
        /// </summary>
        public int FullTimeWorkTimeWeek { get; set; }
        /// <summary>
        /// EmploymentPercent is in percent.
        /// </summary>
        public decimal EmploymentPercent { get; set; }
        /// <summary>
        /// Set as startvalue
        /// </summary>
        public int ExperienceMonths { get; set; }
        /// <summary>
        /// Set to true from Agreed
        /// </summary>
        public bool ExperienceAgreedOrEstablished { get; set; }
        /// <summary>
        /// Set workplace
        /// </summary>
        public string WorkPlace { get; set; }
        /// <summary>
        /// Set special conditions
        /// </summary>
        public string SpecialConditions { get; set; }
        /// <summary>
        /// Set comment
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Set base WorkTimeWeek. In minutes.
        /// </summary>
        public int BaseWorkTimeWeek { get; set; }
        /// <summary>
        /// Set name of person that this persons substitutes for.
        /// </summary>
        public string SubstituteFor { get; set; }
        /// <summary>
        /// Set reason for the first person to be substituted.
        /// </summary>
        public string SubstituteForDueTo { get; set; }
        /// <summary>
        /// Matches on externalcode, and if no match it tries on name
        /// </summary>
        public string EmployeeGroupCode { get; set; }
        /// <summary>
        /// Matches on externalcode, and if no match it tries on name
        /// </summary>
        public string PayrollGroupCode { get; set; }
        /// <summary>
        /// Matches on externalcode, and if no match it tries on name
        /// </summary>
        public string VacationGroupCode { get; set; }
        /// <summary>
        /// Set work tasks.
        /// </summary>
        public string WorkTasks { get; set; }
        /// <summary>
        /// Set employment external code
        /// </summary>
        public string ExternalCode { get; set; }
        /// <summary>
        /// Set to true if employment is secondary
        /// </summary>
        public bool IsSecondaryEmployment { get; set; }
        /// <summary>
        /// Set to true if employment is temporary primary and existing employment should be hibernating during that period
        /// </summary>
        public bool IsTemporaryPrimary { get; set; }
        /// <summary>
        /// The code for the absencecause of the hibernating employment if the new employment is temporary primary
        /// </summary>
        public string HibernatingEmploymentAbsenceCause { get; set; }
        /// <summary>
        /// Set if the previous employment should have an end date. System will search for a valid employment on set date. If now employment is found, nothing happens.
        /// </summary>
        public DateTime? ToDateOnPreviousEmploymentIfExists { get; set; }
        /// <summary>
        /// Set if there could be more than one valid employment on ToDateOnPreviousEmploymentIfExists
        /// </summary>
        public string OptionalEmploymentExternalCodeOnPreviousEmployment { get; set; }
        /// <summary>
        /// Null = Use employment type setting, True = Yes, False = No
        /// </summary>
        public bool? ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment { get; set; }
    }

    public class ExistingEmployeeIO
    {
        public ExistingEmployeeIO()
        {
            ExistingEmployments = new List<ExistingEmploymentIO>();
            ExistingEmployeeAccounts = new List<ExistingEmployeeAccountIO>();
            ExistingEmployeeEmployers = new List<ExistingEmployeeEmployerIO>();
        }
        public string EmployeeNr { get; set; }
        public string EmployeeId { get; set; }
        public string ExternalCode { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdateKey { get; set; }
        public string ParentEmployeeNr { get; set; }
        public List<ExistingEmploymentIO> ExistingEmployments { get; set; }
        public List<ExistingEmployeeAccountIO> ExistingEmployeeAccounts { get; set; }
        public List<ExistingEmployeeEmployerIO> ExistingEmployeeEmployers { get; set; }
    }

    public class ExistingEmployeeEmployerIO
    {
        public string EmployerRegistrationNumber { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? StopDate { get; set; }
    }

    public class ExistingEmploymentIO
    {
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string ExternalCode { get; set; }
        public List<string> EmployeeGroupExternalCodes { get; set; }
        public List<string> PayrollGroupExternalCodes { get; set; }
        public List<string> EmploymentTypeExternalCodes { get; set; }
    }

    public class ExistingEmployeeAccountIO
    {
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public string AccountNr { get; set; }
        public string ParentAccountNr { get; set; }
        public List<string> AccountExternalCodes { get; set; }
        public bool IsDefault { get; set; }
    }
}


