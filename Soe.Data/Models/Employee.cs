using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Employee : IEmployee, ICreatedModified, IState
    {
        public string NumberAndName
        {
            get
            {
                return $"({this.EmployeeNr}) {this.Name}";
            }
        }
        public string NameOrNumber
        {
            get
            {
                return !String.IsNullOrEmpty(this.Name) ? this.Name : this.EmployeeNr;
            }
        }
        public string FirstName
        {
            get
            {
                return this.ContactPerson?.FirstName ?? String.Empty;
            }
        }
        public string LastName
        {
            get
            {
                return this.ContactPerson?.LastName ?? String.Empty;
            }
        }
        public string Name
        {
            get
            {
                return this.ContactPerson?.Name ?? String.Empty;
            }
        }
        public string SocialSec
        {
            get
            {
                return this.ContactPerson?.SocialSec ?? String.Empty;
            }
        }
        public TermGroup_Sex Sex
        {
            get
            {
                return (TermGroup_Sex)(this.ContactPerson?.Sex ?? (int)TermGroup_Sex.Unknown);
            }
        }
        public string EmployeeNrSort
        {
            get
            {
                return this.EmployeeNr?.PadLeft(50, '0') ?? string.Empty;
            }
        }
        public string EmployeeNrAndName
        {
            get
            {
                return $"({this.EmployeeNr}) {this.ContactPerson?.Name ?? string.Empty}";
            }
        }
        public string HibernatingText { get; set; }
        public string ActiveString { get; set; }
        public int Age { get; set; }
        public List<int> AdditionalOnAccountIds { get; set; }
        public bool IsAttested { get; set; }

        //EmployeeGroup

        private List<EmployeeGroup> _employeeGroups { get; set; }

        private List<EmployeeGroup> GetEmployeeGroups(List<EmployeeGroup> employeeGroups)
        {
            if (employeeGroups != null)
                _employeeGroups = employeeGroups;

            return _employeeGroups;
        }

        public int GetCurrentEmployeeGroupId(List<EmployeeGroup> employeeGroups)
        {
            var currentGroup = this.GetEmployeeGroup(null, GetEmployeeGroups(employeeGroups));
            return currentGroup?.EmployeeGroupId ?? 0;
        }

        public int CurrentEmployeeGroupId
        {
            get
            {
                return CurrentEmployeeGroup?.EmployeeGroupId ?? 0;
            }
        }

        public EmployeeGroup GetCurrentEmployeeGroup(List<EmployeeGroup> employeeGroups)
        {
            return this.GetEmployeeGroup(null, GetEmployeeGroups(employeeGroups));
        }

        public EmployeeGroup CurrentEmployeeGroup
        {
            get
            {
                return this.GetEmployeeGroup(null, GetEmployeeGroups(null));
            }
        }

        public bool? HasCurrentEmployeeGroupAutogenTimeblocks(List<EmployeeGroup> employeeGroups)
        {
            var currentGroup = this.GetEmployeeGroup(null, GetEmployeeGroups(employeeGroups));
            return currentGroup?.AutogenTimeblocks;
        }

        public bool? CurrentEmployeeGroupAutogenTimeblocks
        {
            get
            {
                return this.CurrentEmployeeGroup?.AutogenTimeblocks;
            }
        }

        //ERP
        public int ProjectDefaultTimeCodeId { get; set; }
        public string ProjectDefaultTimeCodeName { get; set; }
        public DateTime OrderDate { get; set; }

        //Reports
        public List<TimeInvoiceTransaction> Transactions { get; set; }

        //Relation names
        public List<string> EmployeeGroupNames { get; set; }
        public string EmployeeGroupNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(EmployeeGroupNames, addWhiteSpace: true);
            }
        }
        public List<string> PayrollGroupNames { get; set; }
        public string PayrollGroupNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(PayrollGroupNames, addWhiteSpace: true);
            }
        }

        public List<string> AnnualLeaveGroupNames { get; set; }
        public string AnnualLeaveGroupNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(AnnualLeaveGroupNames, addWhiteSpace: true);
            }
        }

        public List<string> CategoryNames { get; set; }
        public string CategoryNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(CategoryNames, addWhiteSpace: true);
            }
        }
        public List<string> AccountNames { get; set; }
        public string AccountNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(AccountNames, addWhiteSpace: true);
            }
        }
        public List<string> RoleNames { get; set; }
        public string RoleNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(RoleNames, addWhiteSpace: true);
            }
        }

        //TimeTree
        public int? TimeTreeFinalSalaryStatus { get; set; }
        public Employment TimeTreeEmployment { get; set; }
        public EmployeeGroup TimeTreeEmployeeGroup { get; set; }
        public PayrollGroup TimeTreePayrollGroup { get; set; }
        public List<AttestState> TimeTreeAttestStates { get; set; }
        public DateTime? CalculationStartDate { get; set; }
        public DateTime? CalculationStopDate { get; set; }
        public bool IsStamping { get; set; }
        public bool UseCalculationDates
        {
            get { return (this.CalculationStartDate.HasValue && this.CalculationStopDate.HasValue); }
        }

        public object ParentEmployee { get; set; }

        public DateTime GetTimeTreeStartDate(DateTime startDate)
        {
            return this.UseCalculationDates ? this.CalculationStartDate.Value : startDate;
        }
        public DateTime GetTimeTreeStopDate(DateTime stopDate)
        {
            return this.UseCalculationDates ? this.CalculationStopDate.Value : stopDate;
        }
    }

    public static partial class EntityExtensions
    {
        #region Employee

        public static EmployeeDTO ToDTO(this Employee e, 
            bool includeEmployments = false, 
            bool includeEmployeeGroup = false, 
            bool includePayrollGroup = false, 
            bool includeVacationGroup = false, 
            bool includeEmploymentPriceType = false, 
            bool includeEmploymentAccounting = false,
            bool includeAccountSettingsForSilverlight = false, 
            bool includeFactors = false, 
            bool includeEmployeeTax = false, 
            bool useLastEmployment = false, 
            bool isPayrollCalculation = false, 
            List<EmployeeGroup> employeeGroups = null,
            List<PayrollGroup> payrollGroups = null, 
            List<VacationGroup> vacationGroups = null, 
            List<PayrollPriceType> payrollPriceTypes = null,
            List<EmploymentTypeDTO> employmentTypes = null, 
            List<GenericType> disbursementMethodTerms = null,
            List<GenericType> employeeTaxTypes = null, 
            int? taxYear = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null, 
            DateTime? changesForDate = null)
        {
            if (e == null)
                return null;

            includeEmployments = (includeEmployments || includeEmployeeGroup || includePayrollGroup || includeVacationGroup);

            #region Try load

            if (!e.IsAdded())
            {
                if (includeEmployments && !e.Employment.IsLoaded)
                {
                    e.Employment.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs e.Employment");
                }

                if (includeFactors && !e.EmployeeFactor.IsLoaded)
                {
                    e.EmployeeFactor.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs e.EmployeeFactor");
                }

                if (includeVacationGroup && !e.EmployeeVacationSE.IsLoaded)
                {
                    e.EmployeeVacationSE.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs e.EmployeeVacationSE");
                }
            }

            #endregion

            if (!dateFrom.HasValue && !dateTo.HasValue)
            {
                dateFrom = DateTime.Today;
                dateTo = DateTime.Today;
            }
            else if (!dateFrom.HasValue && dateTo.HasValue)
            {
                dateFrom = DateTime.Today;
                if (dateFrom.Value > dateTo.Value)
                    dateFrom = dateTo;
            }
            else if (dateFrom.HasValue && !dateTo.HasValue)
            {
                dateTo = DateTime.Today;
                if (dateTo.Value < dateFrom.Value)
                    dateTo = dateFrom;
            }

            DateTime? date = e.GetEmploymentDates(dateFrom.Value, dateTo.Value)?.OrderBy(x => x).LastOrDefault();

            EmployeeDTO dto = new EmployeeDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                UserId = e.UserId,
                ContactPersonId = e.ContactPersonId,
                TimeCodeId = e.TimeCodeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                ProjectDefaultTimeCodeId = e.ProjectDefaultTimeCodeId,

                EmployeeNr = e.EmployeeNr,
                EmployeeNrSort = e.EmployeeNrSort,
                Name = e.Name,
                FirstName = e.FirstName,
                LastName = e.LastName,
                SocialSec = e.SocialSec,
                EmploymentDate = e.EmploymentDate,
                EndDate = e.EndDate,
                Vacant = e.Vacant,
                Hidden = e.Hidden,
                UseFlexForce = e.UseFlexForce,
                ExcludeFromPayroll = e.ExcludeFromPayroll,
                CardNumber = e.CardNumber,
                Note = e.ShowNote ? e.Note : string.Empty,
                ShowNote = e.ShowNote,
                DisbursementMethod = (TermGroup_EmployeeDisbursementMethod)e.DisbursementMethod,
                DisbursementAccountNr = e.DisbursementAccountNr,
                DisbursementClearingNr = e.DisbursementClearingNr,
                DisbursementMethodName = disbursementMethodTerms?.FirstOrDefault(i => i.Id == e.DisbursementMethod)?.Name,
                DisbursementCountryCode = e.DisbursementCountryCode,
                DisbursementBIC = e.DisbursementBIC,
                DisbursementIBAN = e.DisbursementIBAN,
                DontValidateDisbursementAccountNr = e.DontValidateDisbursementAccountNr,
                HighRiskProtection = e.HighRiskProtection,
                HighRiskProtectionTo = e.HighRiskProtectionTo,
                MedicalCertificateReminder = e.MedicalCertificateReminder,
                MedicalCertificateDays = e.MedicalCertificateDays,
                Absence105DaysExcluded = e.Absence105DaysExcluded,
                Absence105DaysExcludedDays = e.Absence105DaysExcludedDays,
                ActiveString = e.ActiveString,
                TimeCodeName = e.TimeCode?.Name ?? string.Empty,
                RoleNames = e.RoleNames,
                RoleNamesString = e.RoleNamesString,
                CategoryNames = e.CategoryNames,
                CategoryNamesString = e.CategoryNamesString,
                EmployeeGroupNames = e.EmployeeGroupNames,
                EmployeeGroupNamesString = e.EmployeeGroupNamesString,
                PayrollGroupNames = e.PayrollGroupNames,
                PayrollGroupNamesString = e.PayrollGroupNamesString,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Deleted = e.Deleted,
                DeletedBy = e.DeletedBy,
                State = (SoeEntityState)e.State,
                OriginalEmployeeId = e.EmployeeId,
            };

            if (includeFactors)
                dto.Factors = e.EmployeeFactor?.Where(f => f.State == (int)SoeEntityState.Active)?.ToDTOs()?.ToList() ?? new List<EmployeeFactorDTO>();
            if (includeVacationGroup)
                dto.EmployeeVacationSE = e.EmployeeVacationSE?.FirstOrDefault(v => v.State == (int)SoeEntityState.Active)?.ToDTO();
            if (includeEmployeeTax)
            {
                if (!taxYear.HasValue)
                    taxYear = DateTime.Now.Year;
                dto.EmployeeTaxSE = e.EmployeeTaxSE?.FirstOrDefault(i => i.Year == taxYear && i.State == (int)SoeEntityState.Active)?.ToDTO(employeeTaxTypes);
            }
            if (e.Employment != null)
            {
                if (includeEmployments)
                    dto.Employments = e.Employment.ToDTOs(
                        changesForDate: changesForDate,
                        includeEmployeeGroup: includeEmployeeGroup,
                        includePayrollGroup: includePayrollGroup,
                        includeEmploymentAccounting: includeEmploymentAccounting,
                        includeEmploymentAccountingForSL: includeAccountSettingsForSilverlight,
                        includeEmploymentPriceType: includeEmploymentPriceType,
                        includeEmploymentVacationGroups: includeVacationGroup,
                        includeEmploymentVacationGroupSE: includeVacationGroup,
                        employeeGroups: employeeGroups,
                        payrollGroups: payrollGroups,
                        payrollPriceTypes: payrollPriceTypes,
                        vacationGroups: vacationGroups).ToList();

                dto.FinalSalaryEndDate = e.GetApplyFinalSalaryEndDate();
                if (!dto.FinalSalaryEndDate.HasValue)
                    dto.FinalSalaryEndDateApplied = e.GetActiveEmployments().GetAppliedFinalSalaryEndDate();

                Employment employment = useLastEmployment ? e.GetLastEmployment() : e.GetEmployment(dateFrom.Value, dateTo.Value, forward: false);
                if (employment == null && isPayrollCalculation)
                    employment = e.GetLastEmployment();

                dto.CurrentEmploymentDateFromString = employment?.DateFrom?.ToString("yyyy-MM-dd") ?? string.Empty;
                dto.CurrentEmploymentDateToString = employment?.DateTo?.ToString("yyyy-MM-dd") ?? string.Empty;
                dto.CurrentEmploymentPercent = employment?.GetPercent(date);
                dto.CurrentEmploymentTypeString = employment?.GetEmploymentTypeName(employmentTypes, dateTo) ?? string.Empty;

                if (includeEmployeeGroup)
                {
                    EmployeeGroup employeeGroup = employment?.GetEmployeeGroup(dateFrom.Value, dateTo.Value, employeeGroups, forward: false);
                    dto.CurrentEmployeeGroupId = employeeGroup?.EmployeeGroupId ?? 0;
                    dto.CurrentEmployeeGroupName = employeeGroup?.Name ?? string.Empty;
                    dto.CurrentEmployeeGroupTimeDeviationCauseId = employeeGroup?.TimeDeviationCauseId;
                }

                if (includePayrollGroup)
                {
                    PayrollGroup payrollGroup = employment?.GetPayrollGroup(dateFrom.Value, dateTo.Value, payrollGroups, forward: false);
                    dto.CurrentPayrollGroupId = payrollGroup?.PayrollGroupId ?? 0;
                    dto.CurrentPayrollGroupName = payrollGroup?.Name ?? string.Empty;
                }

                if (includeVacationGroup)
                {
                    VacationGroup vacationGroup = employment?.GetVacationGroup(dateFrom.Value, dateTo.Value, vacationGroups, forward: false);
                    dto.CurrentVacationGroupId = vacationGroup?.VacationGroupId ?? 0;
                    dto.CurrentVacationGroupName = vacationGroup?.Name ?? string.Empty;
                }
            }

            return dto;
        }

        public static List<EmployeeDTO> ToDTOs(this List<Employee> l, bool includeEmployments = false, bool includeEmployeeGroup = false, bool includePayrollGroup = false, bool includeVacationGroup = false, bool includeEmploymentPriceType = false, bool includeAccountingSettings = false, bool includeAccountSettingsForSilverlight = false, bool includeFactors = false, bool includeEmployeeTax = false, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<VacationGroup> vacationGroups = null, List<PayrollPriceType> payrollPriceTypes = null, bool useLastEmployment = false)
        {
            var dtos = new List<EmployeeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeEmployments, includeEmployeeGroup, includePayrollGroup, includeVacationGroup, includeEmploymentPriceType, includeAccountingSettings, includeAccountSettingsForSilverlight, includeFactors, includeEmployeeTax, useLastEmployment: useLastEmployment, employeeGroups: employeeGroups, payrollGroups: payrollGroups, vacationGroups: vacationGroups, payrollPriceTypes: payrollPriceTypes));
                }
            }
            return dtos;
        }

        public static Dictionary<int, string> ToDict(this IEnumerable<Employee> l, bool addEmptyRow, bool concatNumberAndName, bool showInactiveSymbol = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            foreach (Employee e in l)
            {
                if (!dict.ContainsKey(e.EmployeeId))
                    dict.Add(e.EmployeeId, (showInactiveSymbol && e.State == (int)SoeEntityState.Inactive ? "! " : "") + (concatNumberAndName ? String.Format("({0}) {1}", e.EmployeeNr, e.Name) : e.Name));
            }

            return dict;
        }

        public static EmployeeTimeCodeDTO ToEmployeeTimeCodeDTO(this Employee e, DateTime? date, List<EmployeeGroup> employeeGroups = null)
        {
            if (e == null)
                return null;

            var employeeGroup = date.HasValue ? e.GetEmployeeGroup(date, employeeGroups) : null;
            return new EmployeeTimeCodeDTO
            {
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                Name = e.Name,
                DefaultTimeCodeId = e.ProjectDefaultTimeCodeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                EmployeeGroupId = employeeGroup?.EmployeeGroupId ?? 0,
                AutoGenTimeAndBreakForProject = employeeGroup?.AutoGenTimeAndBreakForProject ?? false
            };
        }

        public static List<EmployeeTimeCodeDTO> ToEmployeeTimeCodeDTOs(this List<Employee> l, List<EmployeeGroup> employeeGroups = null)
        {
            var dtos = new List<EmployeeTimeCodeDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToEmployeeTimeCodeDTO(null, employeeGroups));
                }
            }
            return dtos;
        }

        public static EmployeeSmallDTO ToSmallDTO(this Employee e)
        {
            if (e == null)
                return null;

            return new EmployeeSmallDTO()
            {
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                Name = e.Name,
            };
        }

        public static IEnumerable<EmployeeSmallDTO> ToSmallDTOs(this IEnumerable<Employee> l)
        {
            var dtos = new List<EmployeeSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static EmployeeGridDTO ToGridDTO(this Employee e, DateTime date, bool showSocialSec = true, List<VacationGroup> vacationGroups = null, List<EmploymentTypeDTO> employmentTypes = null)
        {
            if (e == null)
                return null;

            Employment employment = e.GetEmployment(date);
            Employment firstEmployment = employment == null ? e.GetFirstEmployment() : null;
            Employment lastEmployment = e.GetLastEmployment();

            DateTime? employmentStart = null, employmentStop = null;
            if (employment != null)
            {
                employmentStart = employment.DateFrom;
                employmentStop = employment.DateTo;
            }
            else if (firstEmployment != null && firstEmployment.DateFrom > date)
            {
                employmentStart = firstEmployment.DateFrom;
                employmentStop = firstEmployment.DateTo;
            }

            return new EmployeeGridDTO()
            {
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                Name = e.Name,
                Age = e.Age,
                Sex = e.ContactPerson != null ? (TermGroup_Sex)e.ContactPerson.Sex : TermGroup_Sex.Unknown,
                Vacant = e.Vacant,
                SocialSec = showSocialSec ? e.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(e.SocialSec),
                WorkTimeWeek = employment?.GetWorkTimeWeek(date) ?? 0,
                Percent = employment?.GetPercent(date) ?? 0,
                EmploymentStart = employmentStart,
                EmploymentStop = employmentStop,
                EmploymentEndDate = lastEmployment?.DateTo,
                EmploymentTypeString = employment?.GetEmploymentTypeName(employmentTypes, date) ?? string.Empty,
                UserBlockedFromDate = e.User?.BlockedFromDate,
                EmployeeGroupNamesString = e.EmployeeGroupNamesString,
                PayrollGroupNamesString = e.PayrollGroupNamesString,
                AnnualLeaveGroupNamesString = e.AnnualLeaveGroupNamesString,
                CurrentVacationGroupName = employment?.GetVacationGroups(date, vacationGroups: vacationGroups)?.Select(i => i.Name).ToCommaSeparated() ?? string.Empty,
                AccountNamesString = e.AccountNamesString,
                CategoryNamesString = e.CategoryNamesString,
                RoleNamesString = e.RoleNamesString,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<EmployeeGridDTO> ToGridDTOs(this IEnumerable<Employee> l, DateTime? date, bool showSocialSec = true, List<VacationGroup> vacationGroups = null, List<EmploymentTypeDTO> employmentTypes = null)
        {
            var dtos = new List<EmployeeGridDTO>();
            if (l != null)
            {
                if (!date.HasValue)
                    date = DateTime.Today;

                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(date.Value, showSocialSec, vacationGroups, employmentTypes));
                }
            }
            return dtos;
        }

        public static List<Employee> Filter(this List<Employee> l, string pattern)
        {
            if (string.IsNullOrEmpty(pattern) || l.IsNullOrEmpty())
                return l;

            List<Employee> matching = new List<Employee>();
            string[] tokens = pattern.Split(',');
            if (tokens.Any())
            {
                foreach (var e in l)
                {
                    foreach (string token in tokens)
                    {
                        string validToken = token.ToLower();

                        bool match = false;
                        if (!String.IsNullOrEmpty(e.EmployeeNr) && e.EmployeeNr.ToLower().Contains(validToken))
                            match = true;
                        else if (!String.IsNullOrEmpty(e.Name) && e.Name.ToLower().Contains(validToken))
                            match = true;

                        if (match)
                            matching.Add(e);
                    }
                }
            }
            return matching;
        }

        public static List<Employee> FilterOnEmployeeNumbers(this List<Employee> l, List<string> employeeNumbers)
        {
            if (l.IsNullOrEmpty())
                return new List<Employee>();
            if (employeeNumbers.IsNullOrEmpty())
                return l;
            return l.Where(e => employeeNumbers.Contains(e.EmployeeNr)).ToList();
        }

        public static List<Employee> WithChanges(this List<Employee> l, DateTime changeDate)
        {
            return l?
                .Where(e =>
                    (e.Created.HasValue && e.Created >= changeDate) 
                    || 
                    (e.Modified.HasValue && e.Modified.Value >= changeDate))
                .ToList() ?? new List<Employee>();
        }

        public static List<Employee> RemoveEmployeesWithoutEmployment(this List<Employee> l, DateTime date)
        {
            List<Employee> valid = new List<Employee>();
            foreach (Employee e in l)
            {
                Employment lastEmployment = e.GetLastEmployment();
                if (lastEmployment != null && (lastEmployment.DateTo ?? DateTime.MaxValue) >= date)
                    valid.Add(e);
            }
            return valid;
        }

        public static Employee GetEmployee(this List<Employee> employees, int employeeId, bool? active = null)
        {
            Employee employee = employees?.FirstOrDefault(i => i.EmployeeId == employeeId && i.State != (int)SoeEntityState.Deleted);
            if (employee == null)
                return null;

            if (active == true && employee.State != (int)SoeEntityState.Active)
                return null;
            else if (active == false && employee.State != (int)SoeEntityState.Inactive)
                return null;

            return employee;
        }

        public static Dictionary<Employee, List<int>> GetCategories(this List<Employee> l, List<CompanyCategoryRecord> categoryRecords, DateTime dateFrom, DateTime dateTo, bool onlyDefaultCategories = false)
        {
            Dictionary<Employee, List<int>> dict = new Dictionary<Employee, List<int>>();

            if (l.IsNullOrEmpty() || categoryRecords.IsNullOrEmpty())
                return dict;

            Dictionary<int, List<CompanyCategoryRecord>> categoryRecordsByEmployee = categoryRecords.GetCategoryRecordsDiscardDates(SoeCategoryRecordEntity.Employee, onlyDefaultCategories).ToDict();

            foreach (var e in l)
            {
                if (!dict.Keys.Any(i => i.EmployeeId == e.EmployeeId))
                    dict.Add(e, categoryRecordsByEmployee
                        .GetCategoryRecords(e.EmployeeId, e.GetTimeTreeStartDate(dateFrom), e.GetTimeTreeStopDate(dateTo))
                        .Select(i => i.CategoryId)
                        .Distinct()
                        .ToList());
            }

            return dict;
        }

        public static List<int> GetIds(this List<Employee> l)
        {
            return l?.Select(e => e.EmployeeId).ToList() ?? new List<int>();
        }

        public static List<int> GetBirthyears(this List<Employee> l)
        {
            if (l.IsNullOrEmpty())
                return new List<int>();

            List<DateTime?> birthDates = l.Select(s => CalendarUtility.GetBirthDateFromSecurityNumber(s.SocialSec)).Where(w => w.HasValue).ToList();
            List<int> birthYears = birthDates.Select(s => s.Value.Year).ToList();
            return birthYears;
        }

        public static List<int> GetBirthyears(this List<EmployeeDTO> l)
        {
            if (l.IsNullOrEmpty())
                return new List<int>();

            List<DateTime?> birthDates = l.Select(s => CalendarUtility.GetBirthDateFromSecurityNumber(s.SocialSec)).Where(w => w.HasValue).ToList();
            List<int> birthYears = birthDates.Select(s => s.Value.Year).Distinct().ToList();
            return birthYears;
        }

        public static string GetEmployeeNr(this Employee e)
        {
            return e?.EmployeeNr ?? string.Empty;
        }

        public static int GetEmploymentDays(this Employee e, DateTime fromDate, DateTime toDate)
        {
            int employmentDays = 0;
            foreach (Employment employment in e.GetActiveEmployments())
            {
                employmentDays += employment.GetEmploymentDays(fromDate, toDate);
            }
            return employmentDays;
        }

        public static bool HasHighRiskProtection(this Employee employee, DateTime date)
        {
            return employee.HighRiskProtection && employee.HighRiskProtectionTo.HasValue && employee.HighRiskProtectionTo.Value >= date;
        }

        public static bool IsHiddenValid(this Employee employee, bool getHidden = false)
        {
            return employee != null && (getHidden || !employee.Hidden);
        }

        public static bool IsMySelf(this Employee e, Employee loggedInUserEmployee)
        {
            return loggedInUserEmployee != null && loggedInUserEmployee.EmployeeId == e?.EmployeeId;
        }

        public static bool? GetAutoGenTimeAndBreakForProject(this Employee employee, DateTime date, List<EmployeeGroup> employeeGroups)
        {
            return employee?.GetEmployeeGroup(date, employeeGroups)?.AutoGenTimeAndBreakForProject;
        }

        public static string GetNrAndDateString(this Employee employee, TimeBlockDate timeBlockDate, int actorCompanyId)
        {
            return $"Employee:{employee?.EmployeeNr},Date:{timeBlockDate?.Date.ToShortDateString()},ActorCompanyId:{actorCompanyId}";
        }

        public static string GetNameOrNumberAndDateString(this Employee employee, TimeBlockDate timeBlockDate, int actorCompanyId)
        {
            return $"Employee:{employee?.NameOrNumber},Date:{timeBlockDate?.Date.ToShortDateString()},ActorCompanyId:{actorCompanyId}";
        }

        #region EmployeeSetting

        public static DayOfWeek GetRestTimeWeekWeekDayStart(this Employee employee, EmployeeGroup employeeGroup, List<EmployeeSetting> settings, DateTime date)
        {
            var setting = settings.GetSetting(employee.EmployeeId, date, TermGroup_EmployeeSettingType.WorkTimeRule, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest_Weekday);
            if (setting != null && setting.IntData.HasValue)
                return (DayOfWeek)setting.IntData.Value;
            else
                return employeeGroup.GetRuleRestTimeWeekStartDayDayOfWeek();
        }

        public static DateTime GetRestTimeWeektStartTime(this Employee employee, EmployeeGroup employeeGroup, List<EmployeeSetting> settings, DateTime date)
        {
            var setting = settings.GetSetting(employee.EmployeeId, date, TermGroup_EmployeeSettingType.WorkTimeRule, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest, TermGroup_EmployeeSettingType.WorkTimeRule_WeeklyRest_TimeOfDay);
            if (setting != null && setting.TimeData.HasValue)
                return setting.TimeData.Value;
            else
                return employeeGroup.GetRuleRestTimeWeekStartTime();
        }

        public static DateTime GetRestDayStartTime(this Employee employee, EmployeeGroup employeeGroup, List<EmployeeSetting> settings, DateTime date)
        {
            var setting = settings.GetSetting(employee.EmployeeId, date, TermGroup_EmployeeSettingType.WorkTimeRule, TermGroup_EmployeeSettingType.WorkTimeRule_DailyRest, TermGroup_EmployeeSettingType.WorkTimeRule_DailyRest_TimeOfDay);
            if (setting != null && setting.TimeData.HasValue)
                return setting.TimeData.Value;
            else
                return employeeGroup.GetRuleRestTimeDayStartTime();
        }

        #endregion

        #region Employment

        #region List

        public static List<Employment> GetActiveEmployments(this Employee e, bool discardTemporaryPrimary = false, bool includeSecondary = false)
        {
            e.TryLoadEmployments();
            return e?.Employment?.GetActiveEmployments(discardTemporaryPrimary, includeSecondary) ?? new List<Employment>();
        }

        public static List<Employment> GetActiveEmploymentsDesc(this Employee e, bool discardTemporaryPrimary = false, bool includeSecondary = false)
        {
            e.TryLoadEmployments();
            return e?.Employment?.GetActiveEmploymentsDesc(discardTemporaryPrimary, includeSecondary) ?? new List<Employment>();
        }

        public static List<Employment> GetEmployments(this Employee e, DateTime dateFrom, DateTime dateTo, bool discardState = false, bool discardTemporaryPrimary = false, bool includeSecondary = false)
        {
            e.TryLoadEmployments();
            return e?.Employment.GetEmployments(dateFrom, dateTo, discardState, discardTemporaryPrimary, includeSecondary) ?? new List<Employment>();
        }

        public static List<Employment> GetEmploymentsDesc(this Employee e, DateTime dateFrom, DateTime dateTo, bool discardState = false, bool discardTemporaryPrimary = false, bool includeSecondary = false)
        {
            e.TryLoadEmployments();
            return e?.Employment.GetEmploymentsDesc(dateFrom, dateTo, discardState, discardTemporaryPrimary, includeSecondary) ?? new List<Employment>();
        }

        public static Dictionary<DateTime, Employment> GetEmploymentsByDate(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            var result = new Dictionary<DateTime, Employment>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                var employment = e.GetEmployment(date);
                if (employment != null)
                {
                    DateTime continueToDate = CalendarUtility.GetEarliestDate(employment.DateTo ?? DateTime.MaxValue, dateTo);

                    for (var d = date; d <= continueToDate; d = d.AddDays(1))
                        result[d] = employment;

                    date = continueToDate.AddDays(1);
                }
                else
                {
                    date = date.AddDays(1);
                }
            }
            return result;
        }

        public static List<(Employment Employment, DateTime DateFrom, DateTime DateTo)> GetEmploymentIntervals(this Employee employee, DateTime dateFrom, DateTime dateTo)
        {
            if (dateFrom > dateTo)
                return new List<(Employment Employment, DateTime DateFrom, DateTime DateTo)>();

            var employmentsByDate = employee.GetEmploymentsByDate(dateFrom, dateTo);

            var employmentDateRanges = employmentsByDate
                .Where(kvp => kvp.Value != null)
                .GroupBy(kvp => kvp.Value)
                .Select(g => (
                    Employment: g.Key,
                    DateFrom: g.Min(x => x.Key),
                    DateTo: g.Max(x => x.Key)
                ))
                .ToList();

            return employmentDateRanges;
        }

        public static List<Employment> GetFinalSalaryEmployments(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            e.TryLoadEmployments();
            return e?.Employment.GetFinalSalaryEmployments(dateFrom, dateTo);
        }

        public static bool HasActiveEmployments(this Employee e)
        {
            e.TryLoadEmployments();
            return e.Employment.HasActiveEmployments();
        }

        public static bool HasFinalSalaryEmployments(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            return !e.GetFinalSalaryEmployments(dateFrom, dateTo).IsNullOrEmpty();
        }

        public static void SetHibernatingPeriods(this List<Employee> l)
        {
            if (l != null && l.Any(e => !e.Employment.IsNullOrEmpty()))
                l.ForEach(e => e.Employment.SetHibernatingPeriods());
        }

        public static Dictionary<EmploymentChangeBatch, List<EmploymentChange>> GetDataChanges(this Employee e, DateTime dateFrom, DateTime dateTo, params TermGroup_EmploymentChangeFieldType[] fieldTypes)
        {
            Dictionary<EmploymentChangeBatch, List<EmploymentChange>> dict = new Dictionary<EmploymentChangeBatch, List<EmploymentChange>>();
            foreach (Employment employment in e.GetEmployments(dateFrom, dateTo).SortByDateAndTemporaryPrimary())
            {
                foreach (EmploymentChangeBatch batch in employment.GetOrderedEmploymentChangeBatches(dateFrom))
                {
                    List<EmploymentChange> changes = batch.GetDataChanges(fieldTypes);
                    if (!changes.IsNullOrEmpty())
                        dict.Add(batch, changes);
                }
            }
            return dict;
        }

        public static void AdjustDatesAfterEmploymentEnd(this Employee e, ref DateTime startDate, ref DateTime stopDate)
        {
            e.GetActiveEmployments().AdjustDatesAfterEmploymentEnd(ref startDate, ref stopDate);
        }

        #endregion

        #region Single

        public static Employment GetEmployment(this Employee e, int employmentId)
        {
            return e.GetActiveEmployments().GetEmployment(employmentId);
        }

        public static Employment GetEmployment(this Employee e, DateTime dateFrom, DateTime dateTo, bool forward = true)
        {
            return e.GetActiveEmployments().GetEmployment(dateFrom, dateTo, forward);
        }

        public static Employment GetEmployment(this Employee e, DateTime? date = null, bool discardTemporaryPrimary = false)
        {
            return e.GetActiveEmployments(discardTemporaryPrimary: discardTemporaryPrimary).GetEmployment(date);
        }

        public static Employment GetFirstEmployment(this Employee e)
        {
            return e.GetActiveEmployments().GetFirstEmployment();
        }

        public static Employment GetLastEmployment(this Employee e, DateTime? limitStartDate = null)
        {
            return e.GetActiveEmployments().GetLastEmployment(limitStartDate);
        }

        public static Employment GetPrevEmployment(this Employee e, DateTime date)
        {
            return e.GetActiveEmployments().GetPrevEmployment(date);
        }

        public static Employment GetNextEmployment(this Employee e, DateTime date)
        {
            return e.GetActiveEmployments().GetNextEmployment(date);
        }

        public static Employment GetNearestEmployment(this Employee e, DateTime date, bool discardTemporaryPrimary = false)
        {
            return e.GetActiveEmployments(discardTemporaryPrimary: discardTemporaryPrimary).GetNearestEmployment(date);
        }

        public static Employment GetNearestEmployment(this Employment e, Employment defaultEmployment, DateTime date)
        {
            return e != null && e.GetDateFromOrMin() > date ? e : defaultEmployment;
        }

        public static int? GetEmploymentId(this Employee e, DateTime date)
        {
            return e?.GetActiveEmployments().GetEmploymentId(date);
        }

        public static int? GetEmploymentId(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            return e?.GetActiveEmployments().GetEmploymentId(dateFrom, dateTo);
        }

        public static bool HasEmployment(this Employee e, DateTime date)
        {
            return e?.GetEmploymentId(date) != null;
        }

        public static bool IsStampingInPeriod(this Employee e, DateTime startDate, DateTime stopDate, List<EmployeeGroup> employeeGroups)
        {
            return e.IsStampingOnDate(startDate, employeeGroups) || e.IsStampingOnDate(stopDate, employeeGroups);
        }

        public static bool IsStampingOnDate(this Employee e, DateTime date, List<EmployeeGroup> employeeGroups)
        {
            return e.GetEmployeeGroup(date, employeeGroups)?.AutogenTimeblocks == false;
        }

        public static DateTime? GetApplyFinalSalaryEndDate(this Employee e)
        {
            return e?.GetActiveEmployments().GetApplyFinalSalaryEndDate();
        }

        public static DateTime? GetAppliedFinalSalaryEndDate(this Employee e)
        {
            return e?.GetActiveEmployments().GetAppliedFinalSalaryEndDate();
        }

        /// <summary>
        /// Return true if the employee has an employment in the period.
        /// Will return true in 2 periods when the deviation period is not the same the salary period
        /// </summary>
        /// <param name="e"></param>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        public static bool IsEmployeedInPeriod(this Employee e, TimePeriod timePeriod)
        {
            if (timePeriod.IsExtraPeriod())
                return false;

            Employment employment = e.GetFirstEmployment();
            return employment != null && (
                (employment.StartsInInterval(timePeriod.StartDate, timePeriod.StopDate)) ||
                (timePeriod.PayrollStartDate.HasValue && timePeriod.PayrollStopDate.HasValue && employment.StartsInInterval(timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value))
                );
        }

        public static bool HasEndDateInPeriodAndFinalsalaryNotChosen(this Employee e, TimePeriod timePeriod)
        {
            if (timePeriod.IsExtraPeriod())
                return false;

            //Add 1 days to the periods last day so that we can check if the employee has an employment tha starts the day after the period ends
            return e.HasEndDateInIntervaldAndFinalsalaryNotChosen(timePeriod.StartDate, timePeriod.StopDate.AddDays(1));                       
        }
        
        public static bool HasEndDateInIntervaldAndFinalsalaryNotChosen(this Employee e, DateTime dateFrom, DateTime dateTo)
        {            
            List<Employment> employments = e.GetEmployments(dateFrom, dateTo);
            Employment lastEmploymentWithEndDate = employments.Where(x => x.DateTo.HasValue).OrderBy(x => x.DateTo.Value).LastOrDefault(i => i.DateTo.Value >= dateFrom && i.DateTo.Value <= dateTo);
            if (lastEmploymentWithEndDate != null)
            {
                Employment nextEmployment = employments.FirstOrDefault(i => i.DateFrom > lastEmploymentWithEndDate.DateTo);
                if (nextEmployment == null && !lastEmploymentWithEndDate.HasApplyFinalSalaryOrManually() && !lastEmploymentWithEndDate.HasAppliedFinalSalary())
                    return true;
            }
            return false;
        }
        
        public static bool HasNewAgreementInPeriod(this Employee e, TimePeriod timePeriod)
        {
            if (timePeriod.IsExtraPeriod() || e.IsEmployeedInPeriod(timePeriod))
                return false;
            // -1 days = to see if this period starts with a new agreement 
            return e.HasNewAgreementInPeriod(timePeriod.StartDate.AddDays(-1), timePeriod.PayrollStopDate.HasValue ? timePeriod.PayrollStopDate.Value : timePeriod.StopDate);                
                                            
        }

        public static bool HasNewAgreementInPeriod(this Employee e, DateTime dateFrom, DateTime dateTo)
        {            
            List<Employment> employments = e.GetEmployments(dateFrom, dateTo);            
            foreach (var employment in employments)
            {
                if (employment.StartsInInterval(dateFrom, dateTo))
                {
                    return true;
                    //Jesper want to remove this check for now. The idea is to check if the employee has a new agreement in the period so that the administrator 
                    //know that the employee has a new agreement and should check that everything is correct on the new employment. ie accounting, payrolltypes and so.
                    //foreach (var innerEmployment in employments)
                    //{
                    //    if (innerEmployment.EmploymentId == employment.EmploymentId)
                    //        continue;

                    //    if(employment.HasDifferentAgreement(innerEmployment, dateFrom, dateTo))
                    //        return true;
                    //}
                }
            }            
            return false;
        }

        public static int GetEmployeeWorkTimeWeekMinutes(this Employee employee, DateTime date)
        {
            return employee?.GetEmployment(date)?.GetWorkTimeWeek(date) ?? 0;
        }

        #endregion

        #region Get EmploymentDates

        public static List<TimeBlockDate> GetTimeBlockDatesInEmployment(this Employee e, List<TimeBlockDate> timeBlockDates)
        {
            return timeBlockDates?.Where(timeBlockDate => e.HasEmployment(timeBlockDate.Date)).ToList() ?? new List<TimeBlockDate>();
        }

        public static List<DateTime> GetEmploymentDates(this Employee e, DateTime dateFrom, DateTime dateTo, List<DateTime> allDates = null)
        {
            return e?.GetActiveEmployments().GetEmploymentDates(dateFrom, dateTo, allDates) ?? new List<DateTime>();
        }

        public static List<DateTime> GetDatesInEmployment(this Employee e, List<DateTime> dates)
        {
            return e != null ? dates.Where(date => e.HasEmployment(date)).ToList() : new List<DateTime>();
        }

        public static DateTime? GetLatestEmploymentDate(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            DateTime currentDate = dateTo;
            while (currentDate >= dateFrom)
            {
                if (e.HasEmployment(currentDate))
                    return currentDate;
                currentDate = currentDate.AddDays(-1);
            }
            return null;
        }

        public static int GetEmploymentDaysLAS(this Employee e, List<EmploymentTypeDTO> employmentTypes)
        {
            if (e == null)
                return 0;

            List<int> employmentTypesNotIncludedInLAS = new List<int>()
            {
                (int)TermGroup_EmploymentType.SE_Trainee
            };

            return e.GetActiveEmployments().Where(i => !employmentTypesNotIncludedInLAS.Contains(i.GetEmploymentType(employmentTypes))).GetEmploymentDaysToDate();
        }

        public static int CalculateWorkTimeWeekAverage(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            return e.CalculateWorkTimeWeekAverage(e.GetEmploymentsByDate(dateFrom, dateTo));
        }

        public static int CalculateFullTimeWorkTimeWeekAverage(this Employee e, EmployeeGroup employeeGroup, Dictionary<DateTime, Employment> employmentsByDate)
        {
            if (e == null)
                return 0;

            int avgBaseWorkTimeWeek = employmentsByDate.CalculateAverage(
                (emp, date) => emp.GetFullTimeWorkTimeWeek(employeeGroup, date)
            );
            return avgBaseWorkTimeWeek;
        }

        public static int CalculateWorkTimeWeekAverage(this Employee e, Dictionary<DateTime, Employment> employmentsByDate)
        {
            int avgWorkTimeWeek = employmentsByDate.CalculateAverage(
                (emp, date) => emp.GetWorkTimeWeek(date)
            );
            return avgWorkTimeWeek;
        }

        public static int CalculateAverage(this Dictionary<DateTime, Employment> itemsByDate, Func<Employment, DateTime, int?> selector)
        {
            var values = itemsByDate
                .Where(kvp => kvp.Value != null)
                .Select(kvp => selector(kvp.Value, kvp.Key))
                .Where(val => val.HasValue)
                .Select(val => val.Value)
                .ToList();

            return values.Any() ? (int)Math.Round(values.Average(), MidpointRounding.AwayFromZero) : 0;
        }

        #endregion

        #region TemporaryPrimary / Hibernating 

        public static List<Employment> GetHibernatingEmployments(this Employee e, Employment employment)
        {
            return employment != null ? e?.GetActiveEmployments(discardTemporaryPrimary: true).GetEmployments(employment.GetDateFromOrMin(), employment.GetDateToOrMax()) : null;
        }

        public static List<DateRangeDTO> GetHibernatingPeriods(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            return e?.GetActiveEmployments().GetHibernatingPeriods(dateFrom, dateTo) ?? new List<DateRangeDTO>();
        }

        #endregion

        #region Load

        public static void TryLoadEmployments(this Employee e)
        {
            try
            {
                if (e != null && !e.IsAdded() && !e.Employment.IsLoaded)
                {
                    e.Employment.Load();
                    if (DateTime.Now.Millisecond == 1) // Leave this logging here for now. Do not remove lazyloading 
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs TryLoadEmployments");
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        public static void LoadEmploymentsAndEmploymentChangeBatch(this Employee e)
        {
            try
            {
                if (e != null && !e.IsAdded())
                {
                    if (!e.Employment.IsLoaded)
                    {
                        e.Employment.Load();
                        if (DateTime.Now.Millisecond == 1) // Leave this logging here for now. Do not remove lazyloading
                            DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs LoadEmploymentsAndEmploymentChangeBatch e.Employment");
                    }

                    foreach (Employment employment in e.Employment)
                    {
                        if (!employment.EmploymentChangeBatch.IsLoaded)
                        {
                            employment.EmploymentChangeBatch.Load();
                            if (DateTime.Now.Millisecond == 1) // Leave this logging here for now. Do not remove lazyloading
                                DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs LoadEmploymentsAndEmploymentChangeBatch employment.EmploymentChangeBatch");
                        }

                        foreach (EmploymentChangeBatch batch in employment.EmploymentChangeBatch)
                        {
                            if (!batch.EmploymentChange.IsLoaded)
                            {
                                batch.EmploymentChange.Load();
                                if (DateTime.Now.Millisecond == 1) // Leave this logging here for now. Do not remove lazyloading
                                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs LoadEmploymentsAndEmploymentChangeBatch batch.EmploymentChange");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        #endregion

        #endregion

        #region EmploymentPriceType

        public static EmploymentPriceType GetPriceType(this Employment e, int payrollPriceTypeId)
        {
            return e?.EmploymentPriceType?.FirstOrDefault(p => p.PayrollPriceTypeId == payrollPriceTypeId && p.State == (int)SoeEntityState.Active);
        }

        public static EmploymentPriceTypePeriod GetPeriod(this EmploymentPriceType e, DateTime date)
        {
            return e?.EmploymentPriceTypePeriod?.Where(p => (!p.FromDate.HasValue || p.FromDate <= date) && p.State == (int)SoeEntityState.Active).OrderByDescending(p => p.FromDate).FirstOrDefault();
        }

        public static EmploymentPriceTypePeriod GetPeriod(this Employment e, DateTime date, int payrollPriceTypeId, List<int?> payrollLevelIds, bool hasNullLevel, out EmploymentPriceTypePeriod actualPayrollPriceType)
        {
            EmploymentPriceType priceType = e.GetPriceType(payrollPriceTypeId);

            actualPayrollPriceType = priceType.GetPeriod(date);
            if (actualPayrollPriceType != null)
            {
                bool hasLevels = !payrollLevelIds.IsNullOrEmpty();
                if (hasLevels && actualPayrollPriceType.PayrollLevelId.HasValue && payrollLevelIds.Contains(actualPayrollPriceType.PayrollLevelId.Value))
                    return actualPayrollPriceType;
                else if ((!hasLevels || hasNullLevel) && !actualPayrollPriceType.PayrollLevelId.HasValue)
                    return actualPayrollPriceType;
            }
            return null;
        }

        #endregion

        #region EmployeeGroup

        public static List<DateTime> GetStampingDates(this Employee e, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups)
        {
            List<DateTime> validEmployeeGroups = new List<DateTime>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                EmployeeGroup employeeGroup = e.GetEmployeeGroup(date, employeeGroups);
                if (employeeGroup != null && !employeeGroup.AutogenTimeblocks)
                    validEmployeeGroups.Add(date);

                date = date.AddDays(1);
            }

            return validEmployeeGroups;
        }

        public static List<EmployeeGroup> GetEmployeeGroups(this Employee e, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups)
        {
            List<EmployeeGroup> validEmployeeGroups = new List<EmployeeGroup>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                EmployeeGroup employeeGroup = e.GetEmployeeGroup(date, employeeGroups);
                if (employeeGroup != null && !validEmployeeGroups.Any(eg => eg.EmployeeGroupId == employeeGroup.EmployeeGroupId))
                    validEmployeeGroups.Add(employeeGroup);

                date = date.AddDays(1);
            }

            return validEmployeeGroups;
        }

        public static Dictionary<DateTime, EmployeeGroup> GetEmployeeGroupsByDate(
            this Employee e,
            DateTime dateFrom,
            DateTime dateTo,
            List<EmployeeGroup> allEmployeeGroups,
            Dictionary<DateTime, Employment> employmentsByDate = null
            )
        {
            var result = new Dictionary<DateTime, EmployeeGroup>();

            if (e == null || dateFrom > dateTo)
                return result;            

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                EmployeeGroup employeeGroup;
                if (!employmentsByDate.IsNullOrEmpty() && employmentsByDate.TryGetValue(date, out Employment employment) && employment != null)
                    employeeGroup = employment.GetEmployeeGroup(date, allEmployeeGroups);
                else
                    employeeGroup = e.GetEmployeeGroup(date, allEmployeeGroups);

                if (employeeGroup != null)
                    result[date] = employeeGroup;

                date = date.AddDays(1);
            }
            return result;
        }

        public static List<(EmployeeGroup EmployeeGroup, DateTime DateFrom, DateTime DateTo)> GetEmployeeGroupIntervals(
            this Employee employee, 
            DateTime dateFrom, 
            DateTime dateTo, 
            List<EmployeeGroup> allEmployeeGroups,
            Dictionary<DateTime, Employment> employmentsByDate = null
            )
        {
            if (dateFrom > dateTo)
                return new List<(EmployeeGroup EmployeeGroup, DateTime DateFrom, DateTime DateTo)>();

            var employeeGroupsByDate = employee.GetEmployeeGroupsByDate(dateFrom, dateTo, allEmployeeGroups, employmentsByDate);

            var employeeGroupDateRanges = employeeGroupsByDate
                .Where(kvp => kvp.Value != null)
                .GroupBy(kvp => kvp.Value)
                .Select(g => (
                    EmployeeGroup: g.Key,
                    DateFrom: g.Min(x => x.Key),
                    DateTo: g.Max(x => x.Key)
                ))
                .ToList();

            return employeeGroupDateRanges;
        }

        public static EmployeeGroup GetEmployeeGroup(this Employee e, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups, bool forward = true)
        {
            EmployeeGroup employeeGroup = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    employeeGroup = e.GetEmployeeGroup(date, employeeGroups);
                    if (employeeGroup != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo.Date;
                while (date >= dateFrom)
                {
                    employeeGroup = e.GetEmployeeGroup(date, employeeGroups);
                    if (employeeGroup != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            return employeeGroup;
        }

        public static EmployeeGroup GetEmployeeGroup(this Employee e, DateTime? date, List<EmployeeGroup> employeeGroups)
        {
            return e?.GetEmployment(date)?.GetEmployeeGroup(date, employeeGroups);
        }

        public static EmployeeGroup GetFirstEmployeeGroup(this Employee e, List<EmployeeGroup> employeeGroups)
        {
            return e?.GetFirstEmployment()?.GetEmployeeGroup(null, employeeGroups);
        }

        public static EmployeeGroup GetLastEmployeeGroup(this Employee e, List<EmployeeGroup> employeeGroups)
        {
            return e?.GetLastEmployment()?.GetEmployeeGroup(null, employeeGroups);
        }

        public static EmployeeGroup GetNextEmployeeGroup(this Employee e, DateTime date, List<EmployeeGroup> employeeGroups)
        {
            return e?.GetNextEmployment(date)?.GetEmployeeGroup(null, employeeGroups);
        }

        public static int GetEmployeeGroupId(this Employee e, DateTime? date = null)
        {
            return e?.GetEmployment(date)?.GetEmployeeGroupId(date) ?? 0;
        }

        public static int GetEmployeeGroupId(this Employee e, DateTime dateFrom, DateTime dateTo)
        {
            int result = 0;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                result = e.GetEmployeeGroupId(date);
                if (result > 0)
                    break;

                date = date.AddDays(1);
            }

            return result;
        }

        #endregion

        #region PayrollGroup

        public static List<PayrollGroup> GetPayrollGroups(this Employee e, DateTime dateFrom, DateTime dateTo, List<PayrollGroup> payrollGroups)
        {
            List<PayrollGroup> filteredPayrollGroups = new List<PayrollGroup>();
            List<Employment> employments = e.GetEmployments(dateFrom, dateTo);
            if (!employments.IsNullOrEmpty())
            {
                foreach (Employment employment in employments)
                {
                    filteredPayrollGroups.AddRange(employment.GetPayrollGroups(dateFrom, dateTo, payrollGroups));
                }
            }
            return filteredPayrollGroups.Where(pg => pg != null).Distinct().ToList();
        }

        public static List<int> GetPayrollGroupIds(this Employee e, DateTime dateFrom, DateTime dateTo, List<PayrollGroup> payrollGroups)
        {
            return e.GetPayrollGroups(dateFrom, dateTo, payrollGroups)?.Select(s => s.PayrollGroupId).Distinct().ToList() ?? new List<int>();
        }

        public static PayrollGroup GetPayrollGroup(this Employee e, DateTime dateFrom, DateTime dateTo, List<PayrollGroup> payrollGroups, bool forward = true)
        {
            PayrollGroup result = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetPayrollGroup(date, payrollGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo.Date;
                while (date >= dateFrom)
                {
                    result = e.GetPayrollGroup(date, payrollGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            return result;
        }

        public static PayrollGroup GetPayrollGroup(this Employee e, DateTime? date, List<PayrollGroup> payrollGroups)
        {
            return e?.GetEmployment(date)?.GetPayrollGroup(date, payrollGroups);
        }

        public static PayrollGroup GetLastPayrollGroup(this Employee e, List<PayrollGroup> payrollGroups = null)
        {
            return e?.GetLastEmployment()?.GetPayrollGroup(null, payrollGroups);
        }

        public static int? GetPayrollGroupId(this Employee e, DateTime dateFrom, DateTime dateTo, bool forward = true)
        {
            int? result = 0;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetPayrollGroupId(date);
                    if (result.HasValue)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo.Date;
                while (date >= dateFrom)
                {
                    result = e.GetPayrollGroupId(date);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            return result;
        }

        public static int? GetPayrollGroupId(this Employee e, DateTime? date = null, bool uselastEmploymentDateAsFallback = false)
        {
            Employment employment = e.GetEmployment(date);
            if (employment == null && uselastEmploymentDateAsFallback)
                employment = e.GetLastEmployment();

            return employment?.GetPayrollGroupId(date);
        }

        public static (int?, DateTime) GetPreviousPayrollGroupId(this Employee e, DateTime dateFrom, DateTime dateTo, int? currentPayrollGroupId)
        {
            int? result = null;
            DateTime date = dateTo.Date;
            while (date >= dateFrom)
            {
                result = e.GetPayrollGroupId(date);
                if (result.HasValue && result != currentPayrollGroupId)
                    break;

                date = date.AddDays(-1);
            }

            return (result, date);

        }

        #endregion

        #region VacationGroup

        public static VacationGroup GetVacationGroup(this Employee e, DateTime dateFrom, DateTime dateTo, List<VacationGroup> vacationGroups = null, bool forward = true)
        {
            VacationGroup result = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetVacationGroup(date, vacationGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo.Date;
                while (date >= dateFrom)
                {
                    result = e.GetVacationGroup(date, vacationGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            return result;
        }
       
        public static VacationGroup GetVacationGroup(this Employee e, DateTime? date = null, List<VacationGroup> vacationGroups = null)
        {
            return e?.GetEmployment(date)?.GetVacationGroup(date, vacationGroups);
        }

        public static EmployeeVacationSE GetEmployeeVacationSE(this Employee employee)
        {
            return employee?.EmployeeVacationSE?.FirstOrDefault(v => v.State == (int)SoeEntityState.Active);
        }

        #endregion

        #endregion

        #region EmployeeUser

        public static EmployeeUserDTO ToDTO(this Employee employee, Company company, User user, ContactPerson contactPerson, DateTime? changesForDate, 
            List<CompanyCategoryRecord> categoryRecords = null, 
            List<EmployeeChild> employeeChilds = null, 
            List<EmployeeChildCareDTO> employeeChildCares = null, 
            List<EmployeeUnionFee> employeeUnionFees = null, 
            List<EmployeeMeeting> employeeMeetings = null, 
            List<TimeScheduleTemplateGroupEmployee> employeeTemplateGroups = null, 
            List<EmployeeTimeWorkAccount> employeeTimeWorkAccounts = null, 
            bool loadEmploymentAccounting = false,
            bool loadEmploymentPriceTypes = false,
            bool loadEmploymentVacationGroups = false,
            bool loadEmploymentVacationGroupSE = false,
            bool loadEmployeeSkills = false
            )
        {
            return GetEmployeeUserDTO(employee, company, user, contactPerson, changesForDate,
                categoryRecords: categoryRecords,
                employeeChilds: employeeChilds,
                employeeChildCares: employeeChildCares,
                employeeUnionFees: employeeUnionFees,
                employeeMeetings: employeeMeetings,
                employeeTemplateGroups: employeeTemplateGroups,
                employeeTimeWorkAccounts: employeeTimeWorkAccounts,
                loadEmploymentAccountingSettings: loadEmploymentAccounting,
                loadEmploymentPriceTypes: loadEmploymentPriceTypes,
                loadEmploymentVacationGroups: loadEmploymentVacationGroups,
                loadEmploymentVacationGroupSE: loadEmploymentVacationGroupSE,
                loadEmployeeSkills: loadEmployeeSkills
                );
        }

        public static EmployeeUserDTO ToDTO(this User user, Employee employee, Company company, ContactPerson contactPerson, DateTime? changesForDate, 
            List<CompanyCategoryRecord> categoryRecords = null, 
            List<EmployeeChild> employeeChilds = null, 
            List<EmployeeChildCareDTO> employeeChildCares = null, 
            List<EmployeeUnionFee> employeeUnionFees = null, 
            List<EmployeeMeeting> employeeMeetings = null, 
            List<TimeScheduleTemplateGroupEmployee> employeeTemplateGroups = null,
            bool loadEmploymentPriceTypes = false,
            bool loadEmploymentAccountingSettings = false,
            bool loadEmploymentVacationGroups = false,
            bool loadEmploymentVacationGroupSE = false,
            bool includeEmployeeSkills = false
            )
        {
            return GetEmployeeUserDTO(employee, company, user, contactPerson, changesForDate,
                categoryRecords: categoryRecords,
                employeeChilds: employeeChilds,
                employeeChildCares:employeeChildCares,
                employeeUnionFees: employeeUnionFees,
                employeeMeetings: employeeMeetings, 
                loadEmploymentAccountingSettings: loadEmploymentAccountingSettings,
                loadEmploymentPriceTypes: loadEmploymentPriceTypes,
                loadEmploymentVacationGroups: loadEmploymentVacationGroups,
                loadEmploymentVacationGroupSE: loadEmploymentVacationGroupSE,
                loadEmployeeSkills: includeEmployeeSkills
                );
        }

        private static EmployeeUserDTO GetEmployeeUserDTO(Employee employee, Company company, User user, ContactPerson contactPerson, DateTime? changesForDate, 
            List<CompanyCategoryRecord> categoryRecords, 
            List<EmployeeChild> employeeChilds, 
            List<EmployeeChildCareDTO> employeeChildCares, 
            List<EmployeeUnionFee> employeeUnionFees, 
            List<EmployeeMeeting> employeeMeetings = null, 
            List<TimeScheduleTemplateGroupEmployee> employeeTemplateGroups = null, 
            List<EmployeeTimeWorkAccount> employeeTimeWorkAccounts = null,
            bool loadEmploymentPriceTypes = false,
            bool loadEmploymentAccountingSettings = false,
            bool loadEmploymentVacationGroups = false,
            bool loadEmploymentVacationGroupSE = false,
            bool loadEmployeeSkills = false
            )
        {
            if (employee == null && user == null)
                return null;

            EmployeeUserDTO dto = new EmployeeUserDTO();

            #region ContactPerson

            if (contactPerson != null)
            {
                dto.ActorContactPersonId = contactPerson.ActorContactPersonId;
                dto.FirstName = contactPerson.FirstName;
                dto.LastName = contactPerson.LastName;
                dto.SocialSec = contactPerson.SocialSec;
                dto.Sex = (TermGroup_Sex)contactPerson.Sex;

                ActorConsent actorConsent = contactPerson.Actor?.ActorConsent?.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.EmployeePortrait);
                if (actorConsent != null)
                {
                    dto.PortraitConsent = actorConsent.HasConsent;
                    dto.PortraitConsentDate = actorConsent.ConsentDate;
                }
            }

            #endregion

            #region User

            if (user != null)
            {
                dto.UserId = user.UserId;
                dto.LicenseId = user.LicenseId;
                dto.DefaultActorCompanyId = user.DefaultActorCompanyId;
                dto.LangId = user.LangId;
                dto.EstatusLoginId = user.EstatusLoginId;
                dto.LoginName = user.LoginName;
                dto.ChangePassword = user.ChangePassword;
                dto.Password = null;
                dto.PasswordHomePage = user.PasswordHomePage;
                dto.Email = user.Email;
                dto.EmailCopy = user.EmailCopy;
                dto.IsMobileUser = user.IsMobileUser;
                dto.BlockedFromDate = user.BlockedFromDate;
                dto.AttestRoleIds = user.AttestRoleUser?.Select(a => a.AttestRoleId).Distinct().ToList();

                if (contactPerson == null)
                {
                    //Set name from User if no ContactPerson exists
                    StringUtility.GetName(user.Name, out string firstName, out string lastName, Constants.APPLICATION_NAMESTANDARD);
                    dto.FirstName = firstName;
                    dto.LastName = lastName;
                }
            }
            else
            {
                dto.LicenseId = company?.LicenseId ?? 0;
            }

            #endregion

            #region Employee

            if (employee != null)
            {
                #region Prereq

                if (!employee.IsAdded())
                {
                    if (!employee.Employment.IsLoaded)
                    {
                        employee.Employment.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs employee.Employment");
                    }
                    if (loadEmployeeSkills && !employee.EmployeeSkill.IsLoaded)
                    {
                        employee.EmployeeSkill.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs employee.employeeskill");
                    }

                    if (loadEmploymentAccountingSettings)
                    {
                        foreach (var employment in employee.Employment)
                        {
                            #region EmploymentAccountStd

                            if (!employment.EmploymentAccountStd.IsLoaded)
                            {
                                employment.EmploymentAccountStd.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs employment.EmploymentAccountStd");
                            }

                            foreach (var employmentAccountStd in employment.EmploymentAccountStd)
                            {
                                #region AccountStd

                                if (!employmentAccountStd.AccountStdReference.IsLoaded)
                                {
                                    employmentAccountStd.AccountStdReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs employmentAccountStd.AccountStdReference");
                                }
                                if (employmentAccountStd.AccountStd != null && !employmentAccountStd.AccountStd.AccountReference.IsLoaded)
                                {
                                    employmentAccountStd.AccountStd.AccountReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs employmentAccountStd.AccountStd.AccountReference");
                                }

                                #endregion

                                #region AccountInternal

                                if (!employmentAccountStd.AccountInternal.IsLoaded)
                                {
                                    employmentAccountStd.AccountInternal.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs employmentAccountStd.AccountInternal");
                                }

                                foreach (AccountInternal accountInternal in employmentAccountStd.AccountInternal)
                                {
                                    if (!accountInternal.AccountReference.IsLoaded)
                                    {
                                        accountInternal.AccountReference.Load();
                                        DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs accountInternal.AccountReference");
                                    }
                                    if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                                    {
                                        accountInternal.Account.AccountDimReference.Load();
                                        DataProjectLogCollector.LogLoadedEntityInExtension("Employee.cs accountInternal.Account.AccountDimReference");
                                    }
                                }

                                #endregion
                            }

                            #endregion
                        }
                    }
                }

                #endregion

                dto.EmployeeId = employee.EmployeeId;
                dto.ActorCompanyId = employee.ActorCompanyId;
                dto.TimeCodeId = employee.TimeCodeId;
                dto.TimeDeviationCauseId = employee.TimeDeviationCauseId;
                dto.CurrentEmployeeGroupId = employee.CurrentEmployeeGroupId;
                dto.EmployeeTemplateId = employee.EmployeeTemplateId;
                dto.EmployeeTemplateName = employee.EmployeeTemplate?.Name;
                dto.EmployeeNr = employee.EmployeeNr;
                dto.Vacant = employee.Vacant;
                dto.EmploymentDate = employee.EmploymentDate;
                dto.EndDate = employee.EndDate;
                dto.UseFlexForce = employee.UseFlexForce;
                dto.CardNumber = employee.CardNumber;
                dto.DisbursementMethod = (TermGroup_EmployeeDisbursementMethod)employee.DisbursementMethod;
                dto.DisbursementAccountNr = employee.DisbursementAccountNr;
                dto.DisbursementClearingNr = employee.DisbursementClearingNr;
                dto.DisbursementCountryCode = employee.DisbursementCountryCode;
                dto.DisbursementBIC = employee.DisbursementBIC;
                dto.DisbursementIBAN = employee.DisbursementIBAN;
                dto.DontValidateDisbursementAccountNr = employee.DontValidateDisbursementAccountNr;
                dto.Note = employee.Note;
                dto.ShowNote = employee.ShowNote;
                dto.HighRiskProtection = employee.HighRiskProtection;
                dto.HighRiskProtectionTo = employee.HighRiskProtectionTo;
                dto.MedicalCertificateReminder = employee.MedicalCertificateReminder;
                dto.MedicalCertificateDays = employee.MedicalCertificateDays;
                dto.Absence105DaysExcluded = employee.Absence105DaysExcluded;
                dto.Absence105DaysExcludedDays = employee.Absence105DaysExcludedDays;
                dto.ExternalCode = employee.ExternalCode;
                dto.WantsExtraShifts = employee.WantsExtraShifts;
                dto.DontNotifyChangeOfDeviations = employee.DontNotifyChangeOfDeviations;
                dto.DontNotifyChangeOfAttestState = employee.DontNotifyChangeOfAttestState;
                dto.PayrollReportsPersonalCategory = employee.PayrollStatisticsPersonalCategory;
                dto.PayrollReportsWorkTimeCategory = employee.PayrollStatisticsWorkTimeCategory;
                dto.PayrollReportsSalaryType = employee.PayrollStatisticsSalaryType;
                dto.PayrollReportsWorkPlaceNumber = employee.PayrollStatisticsWorkPlaceNumber;
                dto.PayrollReportsCFARNumber = employee.PayrollStatisticsCFARNumber;
                dto.WorkPlaceSCB = employee.WorkPlaceSCB;
                dto.PartnerInCloseCompany = employee.PartnerInCloseCompany;
                dto.BenefitAsPension = employee.BenefitAsPension;
                dto.AFACategory = employee.AFACategory;
                dto.AFASpecialAgreement = employee.AFASpecialAgreement;
                dto.AFAWorkplaceNr = employee.AFAWorkplaceNr;
                dto.AFAParttimePensionCode = employee.AFAParttimePensionCode;
                dto.CollectumITPPlan = employee.CollectumITPPlan;
                dto.CollectumAgreedOnProduct = employee.CollectumAgreedOnProduct;
                dto.CollectumCostPlace = employee.CollectumCostPlace;
                dto.CollectumCancellationDate = employee.CollectumCancellationDate;
                dto.CollectumCancellationDateIsLeaveOfAbsence = employee.CollectumCancellationDateIsLeaveOfAbsence;
                dto.KpaRetirementAge = employee.KPARetirementAge;
                dto.KpaBelonging = employee.KPABelonging;
                dto.KpaEndCode = employee.KPAEndCode;
                dto.BygglosenAgreementArea = employee.BygglosenAgreementArea;
                dto.BygglosenAllocationNumber = employee.BygglosenAllocationNumber;
                dto.BygglosenMunicipalCode = employee.BygglosenMunicipalCode;
                dto.BygglosenSalaryFormula = employee.BygglosenSalaryFormula;
                dto.BygglosenProfessionCategory = employee.BygglosenProfessionCategory;
                dto.BygglosenSalaryType = employee.BygglosenSalaryType;
                dto.BygglosenWorkPlaceNumber = employee.BygglosenWorkPlaceNumber;
                dto.BygglosenLendedToOrgNr = employee.BygglosenLendedToOrgNr;
                dto.BygglosenAgreedHourlyPayLevel = employee.BygglosenAgreedHourlyPayLevel.HasValue ? employee.BygglosenAgreedHourlyPayLevel.Value : decimal.Zero;
                dto.KpaAgreementType = employee.KPAAgreementType;
                dto.GtpAgreementNumber = employee.GTPAgreementNumber;
                dto.GtpExcluded = employee.GTPExcluded;
                dto.ExcludeFromPayroll = employee.ExcludeFromPayroll;
                dto.AGIPlaceOfEmploymentIgnore = employee.AGIPlaceOfEmploymentIgnore;
                dto.AGIPlaceOfEmploymentAddress = employee.AGIPlaceOfEmploymentAddress;
                dto.AGIPlaceOfEmploymentCity = employee.AGIPlaceOfEmploymentCity;
                dto.IFAssociationNumber = employee.IFAssociationNumber;
                dto.IFPaymentCode = employee.IFPaymentCode;
                dto.IFWorkPlace = employee.IFWorkPlace;

                dto.Employments = employee.Employment?.ToDTOs(
                    changesForDate: changesForDate,
                    includeEmployeeGroup: true,
                    includePayrollGroup: true,
                    includeEmploymentAccounting: loadEmploymentAccountingSettings,
                    includeEmploymentAccountingForSL: false, //To be removed
                    includeEmploymentPriceType: loadEmploymentPriceTypes,
                    includeEmploymentVacationGroups: loadEmploymentVacationGroups,
                    includeEmploymentVacationGroupSE: loadEmploymentVacationGroupSE
                    ).ToList();

                dto.Accounts = employee.EmployeeAccount?.Where(a => a.State == (int)SoeEntityState.Active && !a.ParentEmployeeAccountId.HasValue).ToDTOs().ToList();
                dto.CategoryRecords = categoryRecords?.ToDTOs(true).ToList();
                dto.CategoryId = categoryRecords?.Select(c => c.CategoryId).FirstOrDefault() ?? 0;
                dto.EmployeeSkills = employee.EmployeeSkill?.ToDTOs(false);
                dto.TemplateGroups = employeeTemplateGroups?.ToDTOs();
                dto.EmployeeChilds = employeeChilds?.Where(c => c.State == (int)SoeEntityState.Active).ToDTOs().ToList();
                dto.ParentalLeaves = employeeChilds?.Where(c => c.State == (int)SoeEntityState.Active).ToDTOs().ToList();
                dto.ChildCares = employeeChildCares;
                dto.EmployeeMeetings = employeeMeetings?.Where(c => c.State == (int)SoeEntityState.Active).ToDTOs(true).ToList();
                dto.UnionFees = employeeUnionFees?.Where(c => c.State == (int)SoeEntityState.Active).ToDTOs().ToList();
                dto.Factors = employee.EmployeeFactor?.Where(f => f.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<EmployeeFactorDTO>();
                dto.EmployeeVacationSE = employee.EmployeeVacationSE != null && employee.EmployeeVacationSE.Any(v => v.State == (int)SoeEntityState.Active) ? employee.EmployeeVacationSE.OrderByDescending(i => i.Created).ThenByDescending(i => i.Modified).First(v => v.State == (int)SoeEntityState.Active).ToDTO() : null;
                dto.EmployeeTimeWorkAccounts = employeeTimeWorkAccounts?.Where(e => e.State == (int)SoeEntityState.Active).ToDTOs().ToList();
                dto.EmployeeSettings = employee.EmployeeSetting.Where(s => s.State == (int)SoeEntityState.Active).ToDTOs();
            }

            #endregion

            #region Common

            //Use Employee's info first, then User
            if (employee != null)
            {
                dto.State = (SoeEntityState)employee.State;
                dto.Created = employee.Created;
                dto.CreatedBy = employee.CreatedBy;
                dto.Modified = employee.Modified;
                dto.ModifiedBy = employee.ModifiedBy;
            }
            else if (user != null)
            {
                dto.State = (SoeEntityState)user.State;
                dto.Created = user.Created;
                dto.CreatedBy = user.CreatedBy;
                dto.Modified = user.Modified;
                dto.ModifiedBy = user.ModifiedBy;
            }

            #endregion

            return dto;
        }

        public static EmployeeUserDTO ToEmployeeUserDTO(this CreateVacantEmployeeDTO vacant, int actorCompanyId)
        {
            if (vacant == null)
                return null;

            EmployeeUserDTO dto = new EmployeeUserDTO()
            {
                ActorCompanyId = actorCompanyId,
                EmployeeNr = vacant.EmployeeNr,
                FirstName = vacant.FirstName,
                LastName = vacant.LastName,
                Employments = new List<EmploymentDTO>(),
                CategoryRecords = !vacant.Categories.IsNullOrEmpty() ? vacant.Categories : null,
                Accounts = !vacant.Accounts.IsNullOrEmpty() ? vacant.Accounts : null,
            };

            dto.Employments.Add(new EmploymentDTO()
            {
                ActorCompanyId = actorCompanyId,
                DateFrom = vacant.EmploymentDateFrom,
                EmployeeGroupId = vacant.EmployeeGroupId,
                WorkTimeWeek = vacant.WorkTimeWeek,
                Percent = vacant.Percent,
            });

            return dto;
        }

        #endregion
    }
}
