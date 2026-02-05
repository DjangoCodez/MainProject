using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Employment : IEmployment, ICreatedModified, IState
    {
        public string UniqueId { get; set; }

        #region Employment changes

        #region EmployeeGroup

        public EmployeeGroup GetEmployeeGroup(DateTime? date = null, List<EmployeeGroup> employeeGroups = null, int? fallbackIdIfZero = null)
        {
            int? employeeGroupId = ExtensionCache.Instance.GetEmployeeGroupIdFromExtensionCache(this.EmployeeId, date);
            EmployeeGroup employeeGroup = employeeGroupId.HasValue ? employeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == employeeGroupId.Value) : null;
            if (employeeGroup != null)
                return employeeGroup;

            var dto = this.ApplyEmploymentChanges(date, employeeGroups, TermGroup_EmploymentChangeFieldType.EmployeeGroupId);
            if (dto == null)
                return null;

            if (dto.EmployeeGroupId == 0 && fallbackIdIfZero.HasValidValue())
                dto.EmployeeGroupId = fallbackIdIfZero.Value;

            employeeGroup = employeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == dto.EmployeeGroupId);

            if (employeeGroup == null)
            {
                if (dto.EmployeeGroupId == this.OriginalEmployeeGroupId)
                {
                    this.TryLoadOriginalEmployeeGroup();
                    employeeGroup = this.OriginalEmployeeGroup;
                }
                else
                {
                    var entities = this.GetContext(out bool createdNewContext);
                    if (entities != null)
                    {
                        employeeGroup = entities.EmployeeGroup.FirstOrDefault(i => i.EmployeeGroupId == dto.EmployeeGroupId);
                        if (createdNewContext)
                            entities.Dispose();
                    }
                }
            }

            return employeeGroup;
        }

        public int GetEmployeeGroupId(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date, fieldType: TermGroup_EmploymentChangeFieldType.EmployeeGroupId);
            return dto?.EmployeeGroupId ?? 0;
        }

        public int GetEmployeeGroupId(DateTime dateFrom, DateTime dateTo)
        {
            int result = 0;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                result = GetEmployeeGroupId(date);
                if (result > 0)
                    break;

                date = date.AddDays(1);
            }

            return result;
        }

        #endregion

        #region PayrollGroup

        public PayrollGroup GetPayrollGroup(DateTime? date = null, List<PayrollGroup> payrollGroups = null)
        {
            int? payrollGroupId = ExtensionCache.Instance.GetPayrollGroupIdFromExtensionCache(this.EmployeeId, date);
            PayrollGroup payrollGroup = payrollGroupId.HasValue ? payrollGroups?.FirstOrDefault(i => i.PayrollGroupId == payrollGroupId.Value) : null;
            if (payrollGroup != null)
                return payrollGroup;

            var dto = this.ApplyEmploymentChanges(date, payrollGroups, TermGroup_EmploymentChangeFieldType.PayrollGroupId);
            if (dto?.PayrollGroupId == null)
                return null;

            payrollGroup = payrollGroups?.FirstOrDefault(i => i.PayrollGroupId == dto.PayrollGroupId);
            if (payrollGroup == null)
            {
                if (this.OriginalPayrollGroupId == dto.PayrollGroupId.Value)
                {
                    this.TryLoadOriginalPayrollGroup();
                    payrollGroup = this.OriginalPayrollGroup;
                }
                else
                {
                    var entities = this.GetContext(out bool createdNewContext);
                    if (entities != null)
                    {
                        payrollGroup = entities.PayrollGroup.FirstOrDefault(i => i.PayrollGroupId == dto.PayrollGroupId.Value);
                        if (createdNewContext)
                            entities.Dispose();
                    }
                }
            }

            return payrollGroup;
        }

        public int? GetPayrollGroupId(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date, fieldType: TermGroup_EmploymentChangeFieldType.PayrollGroupId);
            return dto?.PayrollGroupId;
        }

        public int? GetPayrollGroupId(DateTime dateFrom, DateTime dateTo)
        {
            int? result = 0;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                result = GetPayrollGroupId(date);
                if (result.HasValue)
                    break;

                date = date.AddDays(1);
            }

            return result;
        }

        #endregion

        #region AnnualLeaveGroup

        public AnnualLeaveGroup GetAnnualLeaveGroup(DateTime? date = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            int? annualLeaveGroupId = ExtensionCache.Instance.GetAnnualLeaveGroupIdFromExtensionCache(this.EmployeeId, date);
            AnnualLeaveGroup annualLeaveGroup = annualLeaveGroupId.HasValue ? annualLeaveGroups?.FirstOrDefault(i => i.AnnualLeaveGroupId == annualLeaveGroupId.Value) : null;
            if (annualLeaveGroup != null)
                return annualLeaveGroup;

            var dto = this.ApplyEmploymentChanges(date, annualLeaveGroups, TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId);
            if (dto?.AnnualLeaveGroupId == null)
                return null;

            annualLeaveGroup = annualLeaveGroups?.FirstOrDefault(i => i.AnnualLeaveGroupId == dto.AnnualLeaveGroupId);
            if (annualLeaveGroup == null)
            {
                if (this.OriginalAnnualLeaveGroupId == dto.AnnualLeaveGroupId.Value)
                {
                    this.TryLoadOriginalAnnualLeaveGroup();
                    annualLeaveGroup = this.OriginalAnnualLeaveGroup;
                }
                else
                {
                    var entities = this.GetContext(out bool createdNewContext);
                    if (entities != null)
                    {
                        annualLeaveGroup = entities.AnnualLeaveGroup.FirstOrDefault(i => i.AnnualLeaveGroupId == dto.AnnualLeaveGroupId.Value);
                        if (createdNewContext)
                            entities.Dispose();
                    }
                }
            }

            return annualLeaveGroup;
        }

        public int? GetAnnualLeaveGroupId(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date, fieldType: TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId);
            return dto?.AnnualLeaveGroupId;
        }

        public int? GetAnnualLeaveGroupId(DateTime dateFrom, DateTime dateTo)
        {
            int? result = 0;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                result = GetAnnualLeaveGroupId(date);
                if (result.HasValue)
                    break;

                date = date.AddDays(1);
            }

            return result;
        }

        #endregion

        #region EmploymentType

        public EmploymentTypeDTO GetEmploymentTypeDTO(List<EmploymentTypeDTO> employmentTypes, DateTime? date = null)
        {
            if (employmentTypes.IsNullOrEmpty())
                return null;

            var dto = this.ApplyEmploymentChanges(date);
            if (dto == null)
                return null;

            int type = employmentTypes.GetType(dto.EmploymentType);
            return employmentTypes.GetEmploymentType(type);
        }

        public int GetEmploymentType(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.EmploymentType ?? 0;
        }

        public int GetEmploymentType(List<EmploymentTypeDTO> employmentTypes, DateTime? date = null)
        {
            if (employmentTypes.IsNullOrEmpty())
                return 0;

            var dto = this.ApplyEmploymentChanges(date);
            if (dto == null)
                return 0;

            return employmentTypes.GetType(dto.EmploymentType);
        }

        public string GetEmploymentTypeName(List<EmploymentTypeDTO> employmentTypes, DateTime? date = null)
        {
            if (employmentTypes.IsNullOrEmpty())
                return string.Empty;

            var dto = this.ApplyEmploymentChanges(date);
            return dto != null ? employmentTypes.GetName(dto.EmploymentType) : string.Empty;
        }

        #endregion

        #region Name

        public string GetName(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.Name ?? string.Empty;
        }

        #endregion

        #region WorkTimeWeek

        public int GetWorkTimeWeek(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.WorkTimeWeek ?? 0;
        }

        #endregion

        #region FullTimeWorkTimeWeek

        public int? GetFullTimeWorkTimeWeek(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.FullTimeWorkTimeWeek;
        }

        public int GetFullTimeWorkTimeWeek(EmployeeGroup employeeGroup, DateTime? date = null)
        {
            return this.GetFullTimeWorkTimeWeek(date).ToNullable() ?? employeeGroup?.RuleWorkTimeWeek ?? 0;
        }

        #endregion

        #region Percent

        public decimal GetPercent(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            if (employmentPercents.IsNullOrEmpty())
                employmentPercents = new Dictionary<DateTime, decimal>();
            if (employmentPercents.TryGetValue(date.Value, out decimal percentFromDict))
                return percentFromDict;

            var dto = this.ApplyEmploymentChanges(date);
            var result = dto?.Percent ?? 0;

            if (!employmentPercents.ContainsKey(date.Value))
                employmentPercents.Add(date.Value, result);

            return result;
        }

        private Dictionary<DateTime, decimal> employmentPercents;

        #endregion

        #region ExperienceMonths

        public int GetExperienceMonths(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.ExperienceMonths ?? 0;
        }

        public int CalculateExperienceMonths(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return (int)Math.Round(PayrollRulesUtil.CalculateExperienceMonths(this.GetEmploymentDays(fromDate, toDate)), MidpointRounding.AwayFromZero);
        }

        public int GetTotalExperienceMonths(bool useExperienceMonthsOnEmploymentAsStartValue, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (useExperienceMonthsOnEmploymentAsStartValue)
                return this.GetExperienceMonths(toDate) + this.CalculateExperienceMonths(fromDate, toDate);
            else
                return this.GetExperienceMonths(toDate);
        }

        #endregion

        #region ExperienceAgreedOrEstablished

        public bool GetExperienceAgreedOrEstablished(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.ExperienceAgreedOrEstablished ?? false;
        }

        #endregion

        #region SpecialConditions

        public string GetSpecialConditions(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.SpecialConditions ?? string.Empty;
        }

        #endregion

        #region WorkTasks

        public string GetWorkTasks(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.WorkTasks ?? string.Empty;
        }

        #endregion

        #region WorkPlace

        public string GetWorkPlace(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.WorkPlace ?? string.Empty;
        }

        #endregion

        #region BaseWorkTimeWeek

        public int GetBaseWorkTimeWeek(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.BaseWorkTimeWeek ?? 0;
        }

        #endregion

        #region SubstituteFor

        public string GetSubstituteFor(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.SubstituteFor ?? string.Empty;
        }

        #endregion

        #region SubstituteForDueTo

        public string GetSubstituteForDueTo(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.SubstituteForDueTo ?? String.Empty;
        }

        #endregion

        #region ExternalCode

        public string GetExternalCode(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.ExternalCode ?? this.OriginalExternalCode.NullToEmpty();
        }

        #endregion

        #region EndReason

        public int GetEndReason(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.EmploymentEndReason ?? 0;
        }

        public string GetEndReasonName(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.EmploymentEndReasonName ?? string.Empty;
        }
        #endregion

        #region ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment

        public bool? GetExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(DateTime? date = null)
        {
            var dto = this.ApplyEmploymentChanges(date);
            return dto?.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment ?? this.OriginalExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
        }

        #endregion

        private Dictionary<string, List<ApplyEmploymentDay>> employmentDayLookup = new Dictionary<string, List<ApplyEmploymentDay>>();

        private string GetKey(int employeeId, DateTime date)
        {
            return $"{employeeId}_{date:yyyyMMdd}";
        }

        private ApplyEmploymentDay GetEmploymentDay(ApplyEmploymentDayParams parameters)
        {
            if (parameters == null)
                return null;

            string key = GetKey(parameters.EmployeeId, parameters.Date);

            if (employmentDayLookup.TryGetValue(key, out List<ApplyEmploymentDay> days))
            {
                if (days.IsNullOrEmpty() || parameters == null)
                    return null;

                if (parameters.FieldType.HasValue)
                    return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && d.Parameters.FieldType == parameters.FieldType.Value);
                else if (parameters.LookupEmployeeGroupsLoaded && parameters.LookupPayrollGroupsLoaded)
                    return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue && d.Parameters.LookupEmployeeGroupsLoaded == parameters.LookupEmployeeGroupsLoaded && d.Parameters.LookupPayrollGroupsLoaded == parameters.LookupPayrollGroupsLoaded);
                else if (parameters.LookupEmployeeGroupsLoaded)
                    return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue && d.Parameters.LookupEmployeeGroupsLoaded == parameters.LookupEmployeeGroupsLoaded);
                else if (parameters.LookupPayrollGroupsLoaded)
                    return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue && d.Parameters.LookupPayrollGroupsLoaded == parameters.LookupPayrollGroupsLoaded);
                else
                    return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue);
            }
            return null;
        }

        private void AddEmploymentDay(ApplyEmploymentDayParams parameters, EmploymentDTO employment)
        {
            var day = new ApplyEmploymentDay(parameters, employment);

            // Update the lookup dictionary
            string key = GetKey(parameters.EmployeeId, parameters.Date);
            if (!employmentDayLookup.ContainsKey(key))
            {
                employmentDayLookup[key] = new List<ApplyEmploymentDay>();
            }
            employmentDayLookup[key].Add(day);
        }

        public EmploymentDTO ApplyEmploymentChanges<T>(DateTime? date, List<T> lookup, TermGroup_EmploymentChangeFieldType filterType)
        {
            List<EmployeeGroup> employeeGroups = lookup is List<EmployeeGroup> ? lookup as List<EmployeeGroup> : null;
            List<PayrollGroup> payrollGroups = lookup is List<PayrollGroup> ? lookup as List<PayrollGroup> : null;
            List<PayrollPriceType> payrollPriceTypes = lookup is List<PayrollPriceType> ? lookup as List<PayrollPriceType> : null;
            List<AnnualLeaveGroup> annualLeaveGroups = lookup is List<AnnualLeaveGroup> ? lookup as List<AnnualLeaveGroup> : null;
            return ApplyEmploymentChanges(date, employeeGroups, payrollGroups, payrollPriceTypes, filterType, annualLeaveGroups);
        }
        public EmploymentDTO ApplyEmploymentChanges(DateTime? date, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, TermGroup_EmploymentChangeFieldType? fieldType = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            EmploymentDTO employment = null;

            if (date.HasValue)
            {
                date = date.Value.Date;
            }
            else
            {
                //Use today if today is in current Employment, otherwise first date of employment
                if (CalendarUtility.IsDateInRange(DateTime.Today, this.DateFrom, this.DateTo))
                    date = DateTime.Today;
                else
                    date = this.DateFrom ?? this.DateTo;
            }

            if (date.HasValue)
            {
                ApplyEmploymentDayParams parameters = new ApplyEmploymentDayParams(this.EmployeeId, date.Value, fieldType, !employeeGroups.IsNullOrEmpty(), !payrollGroups.IsNullOrEmpty());
                ApplyEmploymentDay applyEmploymentDay = GetEmploymentDay(parameters);

                employment = applyEmploymentDay?.Employment;
                if (employment == null)
                {
                    employment = this.ToDTO(employeeGroups: employeeGroups, payrollGroups: payrollGroups, payrollPriceTypes: payrollPriceTypes);
                    if (employment != null)
                    {
                        bool valid = employment.ApplyEmploymentChanges(date.Value, employeeGroups?.ToDTOs(false).ToList(), payrollGroups?.ToDTOs().ToList(), annualLeaveGroups?.ToDTOs().ToList(), null, null, fieldType);
                        if (valid)
                            AddEmploymentDay(parameters, employment);  // This updates both the list and the dictionary
                        else
                            employment = null;
                    }
                }
            }

            return employment;
        }

        #endregion

        public int Days
        {
            get
            {
                return (int)this.GetDateToOrMax().Subtract(this.GetDateFromOrMin()).TotalDays + 1;
            }
        }
        public int StateId
        {
            get
            {
                return this.State;
            }
        }
        public int FinalSalaryStatusId
        {
            get
            {
                return this.FinalSalaryStatus;
            }
        }
        public List<DateRangeDTO> HibernatingPeriods { get; set; }

        public static Employment Create(Employee employee, EmploymentDTO dto, string createdBy, DateTime? created = null)
        {
            Employment employment = Create(employee, dto.DateFrom, dto.DateTo, createdBy, created);
            if (employment == null)
                return null;

            dto.UniqueId = employment.UniqueId;
            return employment;
        }
        public static Employment Create(Employee employee, DateTime? dateFrom, DateTime? dateTo, string createdBy, DateTime? created = null)
        {
            if (employee == null)
                return null;

            Employment employment = new Employment
            {
                DateFrom = dateFrom ?? CalendarUtility.DATETIME_DEFAULT,
                DateTo = dateTo,
                EmployeeId = employee.EmployeeId,
                ActorCompanyId = employee.ActorCompanyId,
                State = (int)SoeEntityState.Active,
                UniqueId = Guid.NewGuid().ToString(),
            };
            employment.SetCreated(created ?? DateTime.Now, createdBy);
            employee.Employment.Add(employment);
            if (employee.Employment.Count == 1)
            {
                employee.EmploymentDate = employment.DateFrom;
                employee.EndDate = employment.DateTo;
            }
            return employment;
        }

        public EmploymentDateChange Update(EmploymentDTO dto)
        {
            if (dto == null)
                return null;

            EmploymentDateChange dateChangeInterval = GetEmploymentDateChangeInterval(dto);
            this.DateFrom = dto.DateFrom;
            this.DateTo = dto.DateTo;
            this.UniqueId = dto.UniqueId = Guid.NewGuid().ToString();
            return dateChangeInterval;
        }

        public void SetOriginalValues(
            SoeEntityState state,
            EmployeeGroup employeeGroup,
            int? employmentType = null,
            int? baseWorkTimeWeek = null,
            int? workTimeWeek = null,
            decimal? workPercentage = null,
            string name = null,
            int? experienceMonths = null,
            bool? experienceAgreedOrEstablished = null,
            string workTasks = null,
            string workPlace = null,
            string specialConditions = null,
            string substituteFor = null,
            string substituteForDueTo = null,
            int? endReason = null,
            string externalCode = null,
            bool isSecondaryEmployment = false,
            bool isFixedAccounting = false,
            bool updateExperienceMonthsReminder = false,
            int? payrollGroupId = null,
            SoeEmploymentFinalSalaryStatus? finalSalaryStatus = null,
            int? fullTimeWorkTimeWeek = null,
            bool? excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = null,
            int? annualLeaveGroupId = null)
        {
            if (employeeGroup == null)
                return;

            this.State = (int)state;
            this.FinalSalaryStatus = finalSalaryStatus.HasValue ? (int)finalSalaryStatus.Value : 0;
            if (!this.IsSecondaryEmployment && isSecondaryEmployment)
                this.IsSecondaryEmployment = true;
            if (this.IsSecondaryEmployment && this.State == (int)SoeEntityState.Active)
                this.State = (int)SoeEntityState.Hidden;
            this.FixedAccounting = isFixedAccounting;
            this.UpdateExperienceMonthsReminder = updateExperienceMonthsReminder;

            if (this.EmploymentId == 0)
            {
                this.OriginalEmployeeGroupId = employeeGroup.EmployeeGroupId;
                this.OriginalPayrollGroupId = payrollGroupId.ToNullable();
                this.OriginalAnnualLeaveGroupId = annualLeaveGroupId.ToNullable();
                this.OriginalType = employmentType ?? 0;
                this.OriginalBaseWorkTimeWeek = baseWorkTimeWeek ?? 0;
                this.OriginalFullTimeWorkTimeWeek = fullTimeWorkTimeWeek ?? 0;
                this.OriginalWorkTimeWeek = workTimeWeek ?? 0;
                this.OriginalPercent = workPercentage.HasValue ? Decimal.Round(workPercentage.Value, 2) : this.CalculatePercentOriginal(employeeGroup);
                this.OriginalName = null;
                this.OriginalExperienceMonths = experienceMonths ?? 0;
                this.OriginalExperienceAgreedOrEstablished = experienceAgreedOrEstablished.HasValue && experienceAgreedOrEstablished.Value;
                this.OriginalWorkTasks = workTasks;
                this.OriginalWorkPlace = workPlace;
                this.OriginalSpecialConditions = specialConditions;
                this.OriginalSubstituteFor = substituteFor;
                this.OriginalSubstituteForDueTo = substituteForDueTo;
                this.OriginalEndReason = endReason ?? 0;
                this.OriginalExternalCode = externalCode;
                this.OriginalExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = excludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
            }
        }

        public void SetOriginalValuesIfEmpty(
            EmployeeGroup employeeGroup,
            PayrollGroup payrollGroup,
            VacationGroup vacationGroup,
            int? employmentType = null,
            int? baseWorkTimeWeek = null,
            int? workTimeWeek = null,
            decimal? workPercentage = null,
            int? experienceMonths = null,
            bool? experienceAgreedOrEstablished = null,
            string workTasks = null,
            string workPlace = null,
            string specialConditions = null,
            string substituteFor = null,
            string substituteForDueTo = null,
            int? endReason = null,
            string externalCode = null,
            bool forceExperienceMonths = false,
            int? fullTimeWorkTimeWeek = null)
        {
            if (employeeGroup == null)
                return;

            if (!this.OriginalEmployeeGroupReference.IsLoaded)
                this.OriginalEmployeeGroupReference.Load();
            if (this.OriginalEmployeeGroup == null)
                this.OriginalEmployeeGroup = employeeGroup;
            if (!this.OriginalPayrollGroupReference.IsLoaded)
                this.OriginalPayrollGroupReference.Load();
            if (this.OriginalPayrollGroup == null && payrollGroup != null)
                this.OriginalPayrollGroup = payrollGroup;
            if (this.OriginalType == 0 && employmentType > 0)
                this.OriginalType = employmentType.Value;
            if (this.OriginalBaseWorkTimeWeek == 0 && baseWorkTimeWeek > 0)
                this.OriginalBaseWorkTimeWeek = baseWorkTimeWeek.Value;
            if (this.OriginalFullTimeWorkTimeWeek == 0 && fullTimeWorkTimeWeek > 0)
                this.OriginalFullTimeWorkTimeWeek = fullTimeWorkTimeWeek.Value;
            if (this.OriginalWorkTimeWeek == 0 && workTimeWeek > 0)
                this.OriginalWorkTimeWeek = workTimeWeek.Value;
            if (this.OriginalPercent == 0 && workPercentage > 0 && employeeGroup.RuleWorkTimeWeek > 0 && this.OriginalWorkTimeWeek > 0)
                this.OriginalPercent = Decimal.Round(Decimal.Divide(this.OriginalWorkTimeWeek, employeeGroup.RuleWorkTimeWeek) * 100, 2);
            if (this.OriginalExperienceMonths == 0 && (experienceMonths > 0 || forceExperienceMonths))
                this.OriginalExperienceMonths = experienceMonths.Value;
            if (this.OriginalExperienceAgreedOrEstablished && experienceAgreedOrEstablished.HasValue)
                this.OriginalExperienceAgreedOrEstablished = experienceAgreedOrEstablished.Value;
            if (string.IsNullOrEmpty(this.OriginalWorkTasks) && !string.IsNullOrEmpty(workTasks))
                this.OriginalWorkTasks = workTasks;
            if (string.IsNullOrEmpty(this.OriginalWorkPlace) && !string.IsNullOrEmpty(workPlace))
                this.OriginalWorkPlace = workPlace;
            if (string.IsNullOrEmpty(this.OriginalSubstituteFor) && !string.IsNullOrEmpty(substituteFor))
                this.OriginalSubstituteFor = substituteFor;
            if (string.IsNullOrEmpty(this.OriginalSubstituteForDueTo) && !string.IsNullOrEmpty(substituteForDueTo))
                this.OriginalSubstituteForDueTo = substituteForDueTo;
            if (this.OriginalEndReason == 0 && endReason > 0)
                this.OriginalEndReason = endReason.Value;
            if (string.IsNullOrEmpty(this.OriginalExternalCode) && !string.IsNullOrEmpty(externalCode))
                this.OriginalExternalCode = externalCode;
        }

        private EmploymentDateChange GetEmploymentDateChangeInterval(EmploymentDTO dto)
        {
            if (this.State == (int)SoeEntityState.Deleted || dto == null || dto.IsNew())
                return null;

            DateTime? intervalFromDate, intervalToDate;
            SoeEmploymentDateChangeType type = SoeEmploymentDateChangeType.None;

            if (dto.State == SoeEntityState.Deleted)
            {
                if (this.DoDelete(dto.State, out intervalFromDate, out intervalToDate))
                    type = SoeEmploymentDateChangeType.Delete;
            }
            else
            {
                if (this.DoShortenStart(dto.DateFrom, out _, out _))
                    type = SoeEmploymentDateChangeType.ShortenStart;

                if (this.DoShortenStop(dto.DateTo, out intervalFromDate, out intervalToDate))
                    type = SoeEmploymentDateChangeType.ShortenStop;
                else if (this.DoExtendStop(dto.DateTo, out intervalFromDate, out intervalToDate))
                    type = SoeEmploymentDateChangeType.ExtendStop;
            }

            if (type == SoeEmploymentDateChangeType.None)
                return null;
            return new EmploymentDateChange(type, this.GetDateFromOrMin(), this.DateTo, dto.GetDateFromOrMin(), dto.DateTo, intervalFromDate?.Date, intervalToDate?.Date);
        }
        public bool TryShortenStop(DateTime newDateTo)
        {
            if (this.DateFrom > newDateTo)
            {
                this.State = (int)SoeEntityState.Deleted;
                return true;
            }
            else if (!this.DateTo.HasValue || this.DateTo > newDateTo)
            {
                this.DateTo = newDateTo;
                return true;
            }
            return false;
        }
        public bool DoDelete(SoeEntityState newState, out DateTime? from, out DateTime? to)
        {
            from = to = null;
            if (this.StateId != (int)SoeEntityState.Deleted && newState == SoeEntityState.Deleted)
            {
                from = this.DateFrom;
                to = this.DateTo;
                return true;
            }
            return false;
        }
        public bool DoShortenStart(DateTime? newDate, out DateTime? from, out DateTime? to)
        {
            from = to = null;
            if (newDate.HasValue && this.DateFrom.HasValue && CalendarUtility.IsNewDateBeforeOldDate(newDate.Value, this.DateFrom.Value))
            {
                from = newDate.Value;
                to = this.DateFrom.Value.AddDays(-1);
                return true;
            }
            return false;
        }
        public bool DoShortenStop(DateTime? newDate, out DateTime? from, out DateTime? to)
        {
            from = to = null;

            if (newDate.HasValue && CalendarUtility.IsNewDateBeforeOldDate(newDate.Value, this.DateTo))
            {
                from = newDate.Value.AddDays(1);
                to = this.DateTo;
                return true;
            }
            return false;
        }
        public bool DoExtendStop(DateTime? newDate, out DateTime? from, out DateTime? to)
        {
            from = to = null;
            if (this.DateTo.HasValue && CalendarUtility.IsNewDateAfterOldDate(newDate, this.DateTo.Value))
            {
                from = this.DateTo.Value.AddDays(1);
                to = newDate;
                return true;
            }
            return false;
        }

        public bool StartsInInterval(DateTime fromDate, DateTime toDate)
        {
            if (!this.DateFrom.HasValue)
                return false;

            return CalendarUtility.IsDateInRange(this.DateFrom.Value, fromDate, toDate);
        }
        public bool HasDifferentAgreement(Employment e2, DateTime dateFrom, DateTime dateTo)
        {
            if (this.GetEmployeeGroupId(dateFrom, dateTo) != e2.GetEmployeeGroupId(dateFrom, dateTo))
                return true;

            if (this.GetPayrollGroupId(dateFrom, dateTo) != e2.GetPayrollGroupId(dateFrom, dateTo))
                return true;

            if (this.GetVacationGroupId(dateFrom, dateTo) != e2.GetVacationGroupId(dateFrom, dateTo))
                return true;

            return false;
        }

        public static decimal FormatEmploymentPercent(int fulltimeWorkTimeWeekMinutes, int workTimeWeek)
        {
            return fulltimeWorkTimeWeekMinutes == 0 || workTimeWeek == 0 ? 0 : Decimal.Round(Decimal.Multiply(Decimal.Divide(workTimeWeek, fulltimeWorkTimeWeekMinutes), 100), 2);
        }
    }

    public partial class EmploymentChangeBatch : ICreatedNotNull
    {
        public static EmploymentChangeBatch Create(Employee employee, Employment employment, DateTime? fromDate, DateTime? toDate, string comment, string createdBy, DateTime? created)
        {
            if (employee == null || employment == null)
                return null;

            EmploymentChangeBatch batch = new EmploymentChangeBatch
            {
                FromDate = fromDate,
                ToDate = toDate,
                Comment = comment,
                ActorCompanyId = employment.ActorCompanyId,
            };
            batch.SetCreated(created ?? DateTime.Now, createdBy);
            employment.EmploymentChangeBatch.Add(batch);

            if (employment.EmployeeId > 0)
                batch.EmployeeId = employment.EmployeeId;
            else
                batch.Employee = employee;

            if (employment.EmploymentId > 0)
                batch.EmploymentId = employment.EmploymentId;
            else
                batch.Employment = employment;

            return batch;
        }
    }

    public partial class EmploymentChange
    {
        public SoeEntityState State { get; set; }
        public String FieldTypeName { get; set; }

        public static EmploymentChange Create(Employee employee, Employment employment, EmploymentChangeBatch batch, EmploymentChangeDTO dto)
        {
            if (employee == null || employment == null || batch == null)
                return null;

            EmploymentChange change = new EmploymentChange()
            {
                Type = (int)dto.Type,
                FieldType = (int)dto.FieldType,
                FromValue = dto.FromValue,
                ToValue = dto.ToValue,
                FromValueName = dto.FromValueName,
                ToValueName = dto.ToValueName,
            };
            batch.EmploymentChange.Add(change);

            if (employment.EmployeeId > 0)
                change.EmployeeId = employment.EmployeeId;
            else
                change.Employee = employee;

            if (employment.EmploymentId > 0)
                change.EmploymentId = employment.EmploymentId;
            else
                change.Employment = employment;

            return change;
        }
    }

    public partial class EmploymentDateChange
    {
        public SoeEmploymentDateChangeType Type { get; set; }
        public DateTime BeforeDateFrom { get; set; }
        public DateTime? BeforeDateTo { get; set; }
        public DateTime AfterDateFrom { get; set; }
        public DateTime? AfterDateTo { get; set; }
        public DateTime? IntervalDateFrom { get; set; }
        public DateTime? IntervalDateTo { get; set; }

        public EmploymentDateChange(SoeEmploymentDateChangeType type, DateTime beforeDateFrom, DateTime? beforeDateTo, DateTime afterDateFrom, DateTime? afterDateTo, DateTime? intervalDateFrom = null, DateTime? intervalDateTo = null)
        {
            this.Type = type;
            this.BeforeDateFrom = beforeDateFrom;
            this.BeforeDateTo = beforeDateTo;
            this.AfterDateFrom = afterDateFrom;
            this.AfterDateTo = afterDateTo;
            this.IntervalDateFrom = intervalDateFrom;
            this.IntervalDateTo = intervalDateTo;
        }
    }

    public class ApplyEmploymentDayParams
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TermGroup_EmploymentChangeFieldType? FieldType { get; set; }
        public bool LookupEmployeeGroupsLoaded { get; set; }
        public bool LookupPayrollGroupsLoaded { get; set; }

        public ApplyEmploymentDayParams(int employeeId, DateTime date, TermGroup_EmploymentChangeFieldType? fieldType, bool lookupEmployeeGroupsLoaded, bool lookupPayrollGroupsLoaded)
        {
            this.EmployeeId = employeeId;
            this.Date = date;
            this.FieldType = fieldType;
            this.LookupEmployeeGroupsLoaded = lookupEmployeeGroupsLoaded;
            this.LookupPayrollGroupsLoaded = lookupPayrollGroupsLoaded;
        }
    }

    public class ApplyEmploymentDay
    {
        public ApplyEmploymentDayParams Parameters { get; set; }
        public EmploymentDTO Employment { get; set; }

        public ApplyEmploymentDay(ApplyEmploymentDayParams parameters, EmploymentDTO employment)
        {
            this.Parameters = parameters;
            this.Employment = employment;
        }
    }

    public static partial class EntityExtensions
    {
        #region Employment

        public static EmploymentDTO ToDTO(this Employment e,
            DateTime? changesForDate = null,
            bool includeEmployeeGroup = false,
            bool includePayrollGroup = false,
            bool includeEmploymentPriceType = false,
            bool includeEmploymentAccounting = false,
            bool includeEmploymentAccountingForSL = false,
            bool includeEmploymentVacationGroups = false,
            bool includeEmploymentVacationGroupSE = false,
            List<EmployeeGroup> employeeGroups = null,
            List<PayrollGroup> payrollGroups = null,
            List<PayrollPriceType> payrollPriceTypes = null,
            List<VacationGroup> vacationGroups = null)
        {
            if (e == null)
                return null;

            #region Try load

            if (!e.IsAdded())
            {
                //Always load changes
                if (!e.EmploymentChangeBatch.IsLoaded)
                    e.EmploymentChangeBatch.Load();

                foreach (EmploymentChangeBatch batch in e.EmploymentChangeBatch)
                {
                    if (!batch.EmploymentChange.IsLoaded && !batch.IsAdded())
                    {
                        batch.EmploymentChange.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentChange");
                    }
                }

                if (includeEmployeeGroup && !e.OriginalEmployeeGroupReference.IsLoaded)
                {
                    e.OriginalEmployeeGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalEmployeeGroupReference");
                }

                if (includePayrollGroup && !e.OriginalPayrollGroupReference.IsLoaded)
                {
                    e.OriginalPayrollGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalPayrollGroupReference");
                }

                if (includeEmploymentPriceType)
                {
                    if (!e.EmploymentPriceType.IsLoaded)
                    {
                        e.EmploymentPriceType.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentPriceType");
                    }
                    foreach (var priceType in e.EmploymentPriceType)
                    {
                        if (!priceType.EmploymentPriceTypePeriod.IsLoaded)
                        {
                            priceType.EmploymentPriceTypePeriod.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.priceType.EmploymentPriceTypePerio");
                        }
                    }
                }

                if (includeEmploymentAccounting && !e.EmploymentAccountStd.IsLoaded)
                {
                    e.EmploymentAccountStd.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentAccountStd");
                }

                if (includeEmploymentVacationGroups && vacationGroups == null && !e.EmploymentVacationGroup.IsLoaded)
                {
                    e.EmploymentVacationGroup.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentVacationGroup");
                }
            }

            #endregion

            EmploymentDTO dto = new EmploymentDTO()
            {
                #region Static fields (Not present in EmploymentChange)

                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                FixedAccounting = e.FixedAccounting,
                IsSecondaryEmployment = e.IsSecondaryEmployment,
                IsTemporaryPrimary = e.IsTemporaryPrimary,
                UpdateExperienceMonthsReminder = e.UpdateExperienceMonthsReminder,
                State = (SoeEntityState)e.State,

                //Set FK
                EmployeeId = e.EmployeeId,
                EmploymentId = e.EmploymentId,
                ActorCompanyId = e.ActorCompanyId,

                #endregion

                #region Original fields (Present in EmploymentChange)

                EmploymentType = e.OriginalType,
                Name = e.OriginalName,
                Percent = e.OriginalPercent,
                WorkTimeWeek = e.OriginalWorkTimeWeek,
                BaseWorkTimeWeek = e.OriginalBaseWorkTimeWeek,
                FullTimeWorkTimeWeek = e.OriginalFullTimeWorkTimeWeek,
                ExperienceMonths = e.OriginalExperienceMonths,
                ExperienceAgreedOrEstablished = e.OriginalExperienceAgreedOrEstablished,
                WorkTasks = e.OriginalWorkTasks,
                WorkPlace = e.OriginalWorkPlace,
                SpecialConditions = e.OriginalSpecialConditions,
                SubstituteFor = e.OriginalSubstituteFor,
                SubstituteForDueTo = e.OriginalSubstituteForDueTo,
                EmploymentEndReason = e.OriginalEndReason,
                ExternalCode = e.OriginalExternalCode,
                FinalSalaryStatus = (SoeEmploymentFinalSalaryStatus)e.FinalSalaryStatus,
                ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = e.OriginalExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment,

                //Set FK
                EmployeeGroupId = e.OriginalEmployeeGroupId,
                PayrollGroupId = e.OriginalPayrollGroupId,
                AnnualLeaveGroupId = e.OriginalAnnualLeaveGroupId,

                #endregion
            };

            #region Keep original values

            //Relation changes
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmployeeGroupId, e.OriginalEmployeeGroupId.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.PayrollGroupId, e.OriginalPayrollGroupId.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId, e.OriginalAnnualLeaveGroupId.ToString());

            //Field changes
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmploymentType, e.OriginalType.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.Name, e.OriginalName);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.Percent, e.OriginalPercent.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, e.OriginalWorkTimeWeek.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek, e.OriginalBaseWorkTimeWeek.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.FixedAccounting, e.OriginalFullTimeWorkTimeWeek.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExperienceMonths, e.OriginalExperienceMonths.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished, e.OriginalExperienceAgreedOrEstablished.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.WorkTasks, e.OriginalWorkTasks);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.WorkPlace, e.OriginalWorkPlace);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.SpecialConditions, e.OriginalSpecialConditions);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.SubstituteFor, e.OriginalSubstituteFor);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.SubstituteForDueTo, e.OriginalSubstituteForDueTo);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmploymentEndReason, e.OriginalEndReason.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExternalCode, e.OriginalExternalCode);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment, e.OriginalExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment.ToString());

            //Relation name changes
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmployeeGroupName, e.OriginalEmployeeGroup?.Name.ToString() ?? string.Empty);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.PayrollGroupName, e.OriginalPayrollGroup?.Name.ToString() ?? string.Empty);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.PayrollPriceTypeName, string.Empty);

            #endregion

            #region Relations

            dto.Changes = e.EmploymentChangeBatch.ToDTOs(employeeGroups, payrollGroups, payrollPriceTypes).ToList();
            dto.EmployeeGroupName = e.OriginalEmployeeGroup?.Name;
            dto.EmployeeGroupWorkTimeWeek = e.OriginalEmployeeGroup?.RuleWorkTimeWeek ?? 0;
            dto.PayrollGroupName = e.OriginalPayrollGroup?.Name;
            dto.EmploymentVacationGroup = includeEmploymentVacationGroups && e.EmploymentVacationGroup != null ? e.EmploymentVacationGroup.Where(g => g.State == (int)SoeEntityState.Active).ToDTOs(includeEmploymentVacationGroupSE, vacationGroups: vacationGroups).ToList() : null;
            dto.PriceTypes = includeEmploymentPriceType && e.EmploymentPriceType != null ? e.EmploymentPriceType.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(includeEmploymentPriceType, true).ToList() : new List<EmploymentPriceTypeDTO>();
            dto.AnnualLeaveGroupName = e.OriginalAnnualLeaveGroup?.Name;
            dto.CalculatedExperienceMonths = e.CalculateExperienceMonths(changesForDate);
            dto.HibernatingTimeDeviationCauseId = e.IsTemporaryPrimary ? e.GetHibernatingHead()?.TimeDeviationCauseId : null;

            #endregion

            #region Accounts

            if (includeEmploymentAccountingForSL)
            {
                #region CostAccount

                dto.CostAccounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdCost = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Cost);
                    if (employmentAccountStdCost != null)
                    {
                        if (employmentAccountStdCost.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdCost.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdCost.Percent;
                            dto.CostAccounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }

                        if (employmentAccountStdCost.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdCost.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.CostAccounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region IncomeAccount

                dto.IncomeAccounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdIncome = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Income);
                    if (employmentAccountStdIncome != null)
                    {
                        if (employmentAccountStdIncome.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdIncome.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdIncome.Percent;
                            dto.IncomeAccounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }

                        if (employmentAccountStdIncome.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdIncome.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.IncomeAccounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed1Account

                dto.Fixed1Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed1 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed1);
                    if (employmentAccountStdFixed1 != null)
                    {
                        if (employmentAccountStdFixed1.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed1.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed1.Percent;
                            dto.Fixed1Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }

                        if (employmentAccountStdFixed1.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed1.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed1Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed2Account

                dto.Fixed2Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed2 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed2);
                    if (employmentAccountStdFixed2 != null)
                    {
                        if (employmentAccountStdFixed2.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed2.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed2.Percent;
                            dto.Fixed2Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed2.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed2.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed2Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed3Account

                dto.Fixed3Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed3 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed3);
                    if (employmentAccountStdFixed3 != null)
                    {
                        if (employmentAccountStdFixed3.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed3.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed3.Percent;
                            dto.Fixed3Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed3.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed3.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed3Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed4Account

                dto.Fixed4Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed4 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed4);
                    if (employmentAccountStdFixed4 != null)
                    {
                        if (employmentAccountStdFixed4.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed4.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed4.Percent;
                            dto.Fixed4Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed4.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed4.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed4Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed5Account

                dto.Fixed5Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed5 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed5);
                    if (employmentAccountStdFixed5 != null)
                    {
                        if (employmentAccountStdFixed5.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed5.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed5.Percent;
                            dto.Fixed5Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed5.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed5.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed5Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed6Account

                dto.Fixed6Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed6 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed6);
                    if (employmentAccountStdFixed6 != null)
                    {
                        if (employmentAccountStdFixed6.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed6.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed6.Percent;
                            dto.Fixed6Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed6.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed6.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed6Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed7Account

                dto.Fixed7Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed7 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed7);
                    if (employmentAccountStdFixed7 != null)
                    {
                        if (employmentAccountStdFixed7.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed7.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed7.Percent;
                            dto.Fixed7Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed7.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed7.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed7Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion

                #region Fixed8Account

                dto.Fixed8Accounts = new Dictionary<int, AccountSmallDTO>();

                if (e.EmploymentAccountStd != null)
                {
                    var employmentAccountStdFixed8 = e.EmploymentAccountStd.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Fixed8);
                    if (employmentAccountStdFixed8 != null)
                    {
                        if (employmentAccountStdFixed8.AccountStd != null)
                        {
                            AccountSmallDTO accDTO = employmentAccountStdFixed8.AccountStd.Account.ToSmallDTO();
                            accDTO.Percent = employmentAccountStdFixed8.Percent;
                            dto.Fixed8Accounts.Add(Constants.ACCOUNTDIM_STANDARD, accDTO);
                        }
                        if (employmentAccountStdFixed8.AccountInternal != null)
                        {
                            foreach (var accountInternal in employmentAccountStdFixed8.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                            {
                                dto.Fixed8Accounts.Add(accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.ToSmallDTO());
                            }
                        }
                    }
                }

                #endregion
            }
            else if (includeEmploymentAccounting)
            {
                dto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                if (e.EmploymentAccountStd != null && e.EmploymentAccountStd.Count > 0)
                {
                    AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Cost);
                    AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Income);
                    if (e.FixedAccounting)
                    {
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed1);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed2);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed3);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed4);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed5);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed6);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed7);
                        AddAccountingSettingsRowDTO(e, dto, EmploymentAccountType.Fixed8);
                    }
                }
            }

            #endregion

            return dto;
        }

        public static IEnumerable<EmploymentDTO> ToDTOs(this IEnumerable<Employment> l,
            DateTime? changesForDate = null,
            bool includeEmployeeGroup = false,
            bool includePayrollGroup = false,
            bool includeEmploymentAccounting = false,
            bool includeEmploymentAccountingForSL = false,
            bool includeEmploymentPriceType = false,
            bool includeEmploymentVacationGroups = false,
            bool includeEmploymentVacationGroupSE = false,
            List<EmployeeGroup> employeeGroups = null,
            List<PayrollGroup> payrollGroups = null,
            List<PayrollPriceType> payrollPriceTypes = null,
            List<VacationGroup> vacationGroups = null)
        {
            var dtos = new List<EmploymentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(
                        changesForDate,
                        includeEmployeeGroup,
                        includePayrollGroup,
                        includeEmploymentPriceType,
                        includeEmploymentAccounting,
                        includeEmploymentAccountingForSL,
                        includeEmploymentVacationGroups,
                        includeEmploymentVacationGroupSE,
                        employeeGroups,
                        payrollGroups,
                        payrollPriceTypes,
                        vacationGroups
                        ));
                }
                dtos.SetHibernatingPeriods();
            }
            return dtos;
        }

        public static EmploymentDTO ToGridDTO(this Employment e, bool includeEmployeeGroup, bool includePayrollGroup)
        {
            if (e == null)
                return null;

            #region Try load

            if (!e.IsAdded())
            {
                //Always load changes
                if (!e.EmploymentChangeBatch.IsLoaded)
                {
                    e.EmploymentChangeBatch.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentChangeBatch");
                }
                foreach (EmploymentChangeBatch batch in e.EmploymentChangeBatch)
                {
                    if (!batch.EmploymentChange.IsLoaded && !batch.IsAdded())
                    {
                        batch.EmploymentChange.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentChangeBatch");
                    }
                }

                if (includeEmployeeGroup && !e.OriginalEmployeeGroupReference.IsLoaded)
                {
                    e.OriginalEmployeeGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalEmployeeGroupReference");
                }

                if (includePayrollGroup && !e.OriginalPayrollGroupReference.IsLoaded)
                {
                    e.OriginalPayrollGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalPayrollGroupReference");
                }

            }

            #endregion

            EmploymentDTO dto = new EmploymentDTO()
            {
                #region Static fields (Not present in EmploymentChange)

                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                State = (SoeEntityState)e.State,

                //Set FK
                EmployeeId = e.EmployeeId,
                EmploymentId = e.EmploymentId,
                ActorCompanyId = e.ActorCompanyId,

                #endregion

                #region Original fields (Present in EmploymentChange)

                EmploymentType = e.OriginalType,
                Name = e.OriginalName,
                Percent = e.OriginalPercent,
                WorkTimeWeek = e.OriginalWorkTimeWeek,
                BaseWorkTimeWeek = e.OriginalBaseWorkTimeWeek,
                FullTimeWorkTimeWeek = e.OriginalFullTimeWorkTimeWeek,

                //Set FK
                EmployeeGroupId = e.OriginalEmployeeGroupId,
                PayrollGroupId = e.OriginalPayrollGroupId,

                #endregion
            };

            #region Keep original values

            //Relation changes
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmployeeGroupId, e.OriginalEmployeeGroupId.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.PayrollGroupId, e.OriginalPayrollGroupId.ToString());

            //Field changes
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmploymentType, e.OriginalType.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.Name, e.OriginalName);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.Percent, e.OriginalPercent.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, e.OriginalWorkTimeWeek.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek, e.OriginalBaseWorkTimeWeek.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek, e.OriginalFullTimeWorkTimeWeek.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExperienceMonths, e.OriginalExperienceMonths.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished, e.OriginalExperienceAgreedOrEstablished.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.WorkTasks, e.OriginalWorkTasks);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.WorkPlace, e.OriginalWorkPlace);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.SpecialConditions, e.OriginalSpecialConditions);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.SubstituteFor, e.OriginalSubstituteFor);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.SubstituteForDueTo, e.OriginalSubstituteForDueTo);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmploymentEndReason, e.OriginalEndReason.ToString());
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExternalCode, e.OriginalExternalCode);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment, e.OriginalExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment.ToString());

            //Relation name changes
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.EmployeeGroupName, e.OriginalEmployeeGroup?.Name.ToString() ?? string.Empty);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.PayrollGroupName, e.OriginalPayrollGroup?.Name.ToString() ?? string.Empty);
            dto.OriginalValues.Add(TermGroup_EmploymentChangeFieldType.PayrollPriceTypeName, string.Empty);

            #endregion

            #region Relations

            if (e.OriginalEmployeeGroup != null)
                dto.EmployeeGroupName = e.OriginalEmployeeGroup.Name;
            if (e.OriginalPayrollGroup != null)
                dto.PayrollGroupName = e.OriginalPayrollGroup.Name;
            if (e.Employee?.ContactPerson != null)
                dto.EmployeeName = e.Employee.ContactPerson.FirstName + " " + e.Employee.ContactPerson.LastName;

            #endregion

            return dto;
        }

        public static IEnumerable<EmploymentDTO> ToGridDTOs(this IEnumerable<Employment> l, bool includeEmployeeGroup, bool includePayrollGroup)
        {
            var dtos = new List<EmploymentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(includeEmployeeGroup, includePayrollGroup));
                }
            }
            return dtos;
        }

        public static List<EmploymentDTO> ToSplittedDTOs(this IEnumerable<Employment> l, bool includeEmployeeGroup = false, bool includePayrollGroup = false, bool includeEmploymentPriceType = false, bool includeAccountingSettings = false, bool includeAccountSettingsForSilverlight = false, bool includeVacationGroups = false, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<VacationGroup> vacationGroups = null)
        {
            if (l.IsNullOrEmpty())
                return new List<EmploymentDTO>();

            var temporaryPrimaryEmployments = l.OnlyTemporaryPrimary();
            if (!temporaryPrimaryEmployments.Any())
                return l.ToDTOs().ToList();

            if (l.All(e => e.HibernatingPeriods == null))
                l.SetHibernatingPeriods();

            List<EmploymentDTO> dtos = new List<EmploymentDTO>();
            foreach (var e in l.GetActiveEmployments(discardTemporaryPrimary: true))
            {
                dtos.AddRange(e.ToSplittedDTOs(
                    temporaryPrimaryEmployments,
                    includeEmployeeGroup: includeEmployeeGroup,
                    includePayrollGroup: includePayrollGroup,
                    includeEmploymentPriceType: includeEmploymentPriceType,
                    includeAccountingSettings: includeAccountingSettings,
                    includeAccountSettingsForSilverlight: includeAccountSettingsForSilverlight,
                    includeVacationGroups: includeVacationGroups,
                    employeeGroups: employeeGroups,
                    payrollGroups: payrollGroups,
                    payrollPriceTypes: payrollPriceTypes,
                    vacationGroups: vacationGroups));
            }
            return dtos.SortByDate().ToList();
        }

        public static List<EmploymentDTO> ToSplittedDTOs(this Employment e, IEnumerable<Employment> others, bool includeEmployeeGroup = false, bool includePayrollGroup = false, bool includeEmploymentPriceType = false, bool includeAccountingSettings = false, bool includeAccountSettingsForSilverlight = false, bool includeVacationGroups = false, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<VacationGroup> vacationGroups = null)
        {
            if (e == null)
                return new List<EmploymentDTO>();

            List<EmploymentDTO> dtos = new List<EmploymentDTO>();
            foreach (DateRangeDTO noneHibernatingPeriod in e.GetNoneHibernatingPeriods())
            {
                var dto = e.ToDTO(
                    includeEmployeeGroup: includeEmployeeGroup,
                    includePayrollGroup: includePayrollGroup,
                    includeEmploymentPriceType: includeEmploymentPriceType,
                    includeEmploymentAccounting: includeAccountingSettings,
                    includeEmploymentAccountingForSL: includeAccountSettingsForSilverlight,
                    includeEmploymentVacationGroups: includeVacationGroups,
                    employeeGroups: employeeGroups,
                    payrollGroups: payrollGroups,
                    payrollPriceTypes: payrollPriceTypes,
                    vacationGroups: vacationGroups,
                    changesForDate: noneHibernatingPeriod.Start);

                dto.DateFrom = noneHibernatingPeriod.Start;
                dto.DateTo = noneHibernatingPeriod.Stop;
                dto.ApplyEmploymentChanges(dto.GetDateFromOrMin());
                dtos.Add(dto);
            }
            foreach (Employment temporaryPrimaryEmployment in others.OnlyTemporaryPrimary().FilterDates(e.GetDateFromOrMin(), e.GetDateToOrMax()))
            {
                var dto = temporaryPrimaryEmployment.ToDTO(
                    includeEmployeeGroup: includeEmployeeGroup,
                    includePayrollGroup: includePayrollGroup,
                    includeEmploymentPriceType: includeEmploymentPriceType,
                    includeEmploymentAccounting: includeAccountingSettings,
                    includeEmploymentAccountingForSL: includeAccountSettingsForSilverlight,
                    includeEmploymentVacationGroups: includeVacationGroups,
                    employeeGroups: employeeGroups,
                    payrollGroups: payrollGroups,
                    payrollPriceTypes: payrollPriceTypes,
                    vacationGroups: vacationGroups,
                    changesForDate: temporaryPrimaryEmployment.GetDateFromOrMin());
                dto.ApplyEmploymentChanges(dto.GetDateFromOrMin());
                dtos.Add(dto);
            }
            return dtos.SortByDate().ToList();
        }

        public static EmployeeListEmploymentDTO ToListDTO(this Employment e)
        {
            if (e == null)
                return null;

            return new EmployeeListEmploymentDTO()
            {
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
            };
        }

        public static IEnumerable<EmployeeListEmploymentDTO> ToListDTOs(this IEnumerable<Employment> l)
        {
            var dtos = new List<EmployeeListEmploymentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToListDTO());
                }
            }
            return dtos;
        }

        public static List<EmploymentVacationGroup> GetActiveEmploymentVacationGroups(this Employment e)
        {
            e.TryLoadEmploymentVacationGroups();
            return e.EmploymentVacationGroup?.Where(em => em.State == (int)SoeEntityState.Active).ToList() ?? new List<EmploymentVacationGroup>();
        }

        public static List<EmploymentChangeBatch> GetOrderedEmploymentChangeBatches(this Employment e, DateTime? dateFrom = null)
        {
            if (e == null || e.EmploymentChangeBatch.IsNullOrEmpty())
                return new List<EmploymentChangeBatch>();

            var batches = e.EmploymentChangeBatch.ToList();
            if (dateFrom.HasValue)
                batches = batches.Where(b => dateFrom.Value <= (b.FromDate ?? DateTime.MinValue)).ToList();

            return batches
                .OrderBy(b => b.FromDate)
                .ThenByDescending(b => b.ToDate ?? DateTime.MaxValue)
                .ThenByDescending(b => b.Created)
                .ToList();
        }

        public static List<EmploymentChange> GetDataChanges(this Employment e, params TermGroup_EmploymentChangeFieldType[] fieldTypes)
        {
            List<EmploymentChange> changes = new List<EmploymentChange>();
            foreach (EmploymentChangeBatch batch in e.GetOrderedEmploymentChangeBatches())
            {
                changes.AddRange(batch.GetDataChanges(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, TermGroup_EmploymentChangeFieldType.Percent, TermGroup_EmploymentChangeFieldType.EmployeeGroupId));
            }
            return changes;
        }

        public static List<EmploymentChange> GetAllChanges(this Employment e)
        {
            List<EmploymentChange> changes = new List<EmploymentChange>();
            foreach (EmploymentChangeBatch batch in e.GetOrderedEmploymentChangeBatches())
            {
                changes.AddRange(batch.GetChanges());
            }
            return changes;
        }

        public static TimeHibernatingAbsenceHead GetHibernatingHead(this Employment e, bool loadTimeDeviationCause = false)
        {
            TimeHibernatingAbsenceHead hibernateHead = null;
            if (e != null && e.IsTemporaryPrimary)
            {
                try
                {
                    if (!e.TimeHibernatingAbsenceHead.IsLoaded)
                    {
                        e.TimeHibernatingAbsenceHead.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.TimeHibernatingAbsenceHead");
                    }

                    hibernateHead = e.TimeHibernatingAbsenceHead?.FirstOrDefault(h => h.State == (int)SoeEntityState.Active);
                    if (hibernateHead != null && loadTimeDeviationCause && !hibernateHead.TimeDeviationCauseReference.IsLoaded)
                    {
                        hibernateHead.TimeDeviationCauseReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs hibernateHead.TimeDeviationCauseReference");
                    }
                }
                catch
                {
                    //prevent compiler warning
                }
            }
            return hibernateHead;
        }

        public static decimal CalculatePercent(this Employment e, DateTime date)
        {
            if (e == null)
                return 0;

            // Determine if work time should be calculated based on time or percent.
            // Some use time and some use minutes.
            // Check if percent has any decimals, in that case use time instead.
            decimal percent = e.GetPercent(date);
            if (percent % 1 != 0)
            {
                EmployeeGroup employeeGroup = e.GetEmployeeGroup(date);
                if (employeeGroup != null && employeeGroup.RuleWorkTimeWeek > 0)
                    percent = decimal.Multiply(decimal.Divide((decimal)e.GetWorkTimeWeek(date), (decimal)employeeGroup.RuleWorkTimeWeek), 100);
            }

            return percent;
        }

        public static bool HasFixedAccounting(this Employment e)
        {
            return e?.FixedAccounting == true;
        }

        public static decimal CalculatePercentOriginal(this Employment e, EmployeeGroup employeeGroup)
        {
            if (e != null && e.OriginalWorkTimeWeek > 0 && employeeGroup != null && employeeGroup.RuleWorkTimeWeek > 0)
                return Decimal.Round(Decimal.Divide(e.OriginalWorkTimeWeek, employeeGroup.RuleWorkTimeWeek) * 100, 2);
            return 0;
        }

        public static void AdjustDatesAfterEmploymentEnd(this List<Employment> l, ref DateTime startDate, ref DateTime stopDate)
        {
            List<Employment> employments = l?
                .GetEmployments(startDate, stopDate)
                .OrderByDescending(e => e.DateTo ?? DateTime.MaxValue)
                .ToList();

            if (employments.IsNullOrEmpty())
                return;

            Employment employment = 
                employments.GetEmployment(stopDate) ?? 
                employments.GetLastEmployment();

            if (employment == null)
                return;

            if (employment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalary)
            {
                stopDate = l.GetPrevEmployment(employment.GetDateFromOrMin())?.DateTo ?? (employment.DateFrom ?? startDate).AddDays(-1);
            }
            else
            {
                stopDate = CalendarUtility.GetEarliestDate(stopDate, employment.GetDateToOrMax());

                while (employment.DateFrom > startDate)
                {
                    Employment prevEmployment = l.GetPrevEmployment(employment.GetDateFromOrMin());
                    if (prevEmployment != null)
                        employment = prevEmployment;
                    else
                        break;
                }

                startDate = CalendarUtility.GetLatestDate(startDate, employment.GetDateFromOrMin());
            }
        }

        private static void AddAccountingSettingsRowDTO(this Employment e, EmploymentDTO dto, EmploymentAccountType type)
        {
            EmploymentAccountStd employmentAccountStd = e.EmploymentAccountStd.FirstOrDefault(c => c.Type == (int)type);
            if (employmentAccountStd == null)
                return;

            AccountingSettingsRowDTO accountingDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = employmentAccountStd.Percent
            };
            dto.AccountingSettings.Add(accountingDto);

            //AccountStd
            if (employmentAccountStd.AccountStd != null)
            {
                accountingDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                if (employmentAccountStd.AccountStd.Account != null && employmentAccountStd.AccountStd.Account.State == (int)SoeEntityState.Active)
                {
                    accountingDto.Account1Id = employmentAccountStd.AccountStd.AccountId;
                    accountingDto.Account1Nr = employmentAccountStd.AccountStd.Account.AccountNr;
                    accountingDto.Account1Name = employmentAccountStd.AccountStd.Account.Name;
                }
            }

            //AccountInternal
            if (employmentAccountStd.AccountInternal != null)
            {
                int position = 2;
                foreach (var accountInternal in employmentAccountStd.AccountInternal.Where(a => a.Account != null && a.Account.State == (int)SoeEntityState.Active && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    accountingDto.SetAccountValues(position, accountInternal.Account.AccountDim.AccountDimNr, accountInternal.Account.AccountId, accountInternal.Account.AccountNr, accountInternal.Account.Name);
                    position++;
                }
            }
        }

        #region EmployeeGroup

        public static List<EmployeeGroup> GetEmployeeGroups(this Employment e, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups)
        {
            List<EmployeeGroup> result = new List<EmployeeGroup>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                EmployeeGroup employeeGroup = e.GetEmployeeGroup(date, employeeGroups);
                if (employeeGroup != null && !result.Any(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId))
                    result.Add(employeeGroup);

                date = date.AddDays(1);
            }

            return result;
        }

        public static EmployeeGroup GetEmployeeGroup(this Employment e, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups, bool forward = true, bool useLastIfCurrentNotExists = false)
        {
            EmployeeGroup result = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetEmployeeGroup(date, employeeGroups: employeeGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo;
                while (date >= dateFrom)
                {
                    result = e.GetEmployeeGroup(date, employeeGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            if (result == null && useLastIfCurrentNotExists && e.DateTo.HasValue)
                result = e.GetEmployeeGroup(e.DateTo.Value, employeeGroups);

            return result;
        }

        public static void TryLoadOriginalEmployeeGroup(this Employment e)
        {
            try
            {
                if (e != null && !e.IsAdded() && !e.OriginalEmployeeGroupReference.IsLoaded)
                {
                    e.OriginalEmployeeGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalEmployeeGroupReference");
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        #endregion

        #region PayrollGroup

        public static List<PayrollGroup> GetPayrollGroups(this Employment e, DateTime dateFrom, DateTime dateTo, List<PayrollGroup> payrollGroups = null)
        {
            List<PayrollGroup> result = new List<PayrollGroup>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                PayrollGroup payrollGroup = e.GetPayrollGroup(date, payrollGroups);
                if (payrollGroup != null && !result.Any(i => i.PayrollGroupId == payrollGroup.PayrollGroupId))
                    result.Add(payrollGroup);

                date = date.AddDays(1);
            }

            return result;
        }

        public static PayrollGroup GetPayrollGroup(this Employment e, DateTime dateFrom, DateTime dateTo, List<PayrollGroup> payrollGroups, bool forward = true, bool useLastIfCurrentNotExists = false)
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
                DateTime date = dateTo;
                while (date >= dateFrom)
                {
                    result = e.GetPayrollGroup(date, payrollGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            if (result == null && useLastIfCurrentNotExists && e.DateTo.HasValue)
                result = e.GetPayrollGroup(e.DateTo.Value, payrollGroups);

            return result;
        }

        public static void TryLoadOriginalPayrollGroup(this Employment e)
        {
            try
            {
                if (e != null && !e.IsAdded() && !e.OriginalPayrollGroupReference.IsLoaded)
                {
                    e.OriginalPayrollGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalPayrollGroupReference");
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        #endregion

        #region VacationGroup

        public static List<VacationGroupDTO> GetVacationGroups(this Employment e, DateTime? date = null, List<VacationGroup> vacationGroups = null)
        {
            List<VacationGroupDTO> currentVacationGroups = new List<VacationGroupDTO>();

            VacationGroup vacationGroup = e.GetVacationGroup(date, vacationGroups);
            if (vacationGroup != null)
            {
                currentVacationGroups.Add(vacationGroup.ToDTO());

                int counter = 1;
                while (vacationGroup != null && counter <= 10)
                {
                    vacationGroup = e.GetVacationGroup(date, vacationGroups, currentVacationGroups.Select(i => i.VacationGroupId).ToList());
                    if (vacationGroup != null && vacationGroup.FromDate != currentVacationGroups.First().FromDate)
                        vacationGroup = null;
                    if (vacationGroup != null)
                        currentVacationGroups.Add(vacationGroup.ToDTO());

                    counter++;
                }
            }

            return currentVacationGroups;
        }

        public static VacationGroup GetVacationGroup(this Employment e, DateTime dateFrom, DateTime dateTo, List<VacationGroup> vacationGroups = null, bool forward = true)
        {
            VacationGroup result = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetVacationGroup(date, vacationGroups: vacationGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo;
                while (date >= dateFrom)
                {
                    result = e.GetVacationGroup(date, vacationGroups: vacationGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            return result;
        }
        public static int? GetVacationGroupId(this Employment e, DateTime dateFrom, DateTime dateTo, bool forward = true)
        {
            int? result = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetVacationGroupId(date);
                    if (result != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo;
                while (date >= dateFrom)
                {
                    result = e.GetVacationGroupId(date);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            return result;
        }
        public static VacationGroup GetVacationGroup(this Employment e, DateTime? date = null, List<VacationGroup> vacationGroups = null, List<int> skipVacationGroupIds = null)
        {
            if (vacationGroups != null && vacationGroups.Count == 0)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            VacationGroup vacationGroup = null;

            List<EmploymentVacationGroup> employmentVacationGroups = e.GetActiveEmploymentVacationGroups();
            if (!skipVacationGroupIds.IsNullOrEmpty())
                employmentVacationGroups = employmentVacationGroups?.Where(i => !skipVacationGroupIds.Contains(i.VacationGroupId)).ToList() ?? new List<EmploymentVacationGroup>();

            if (!employmentVacationGroups.IsNullOrEmpty())
            {
                EmploymentVacationGroup employmentVacationGroup = employmentVacationGroups.Where(i => i.FromDate.HasValue && i.FromDate.Value <= date).OrderByDescending(i => i.FromDate).FirstOrDefault();
                if (employmentVacationGroup == null)
                    employmentVacationGroup = employmentVacationGroups.FirstOrDefault(i => !i.FromDate.HasValue);
                if (employmentVacationGroup != null)
                {
                    if (vacationGroups != null && vacationGroups.Any(i => i.VacationGroupId == employmentVacationGroup.VacationGroupId))
                        return vacationGroups.FirstOrDefault(i => i.VacationGroupId == employmentVacationGroup.VacationGroupId);

                    if (!e.IsAdded() && !employmentVacationGroup.VacationGroupReference.IsLoaded)
                    {
                        employmentVacationGroup.VacationGroupReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.VacationGroupReference");
                    }
                    vacationGroup = employmentVacationGroup.VacationGroup;
                }
            }

            return vacationGroup;
        }
        public static int? GetVacationGroupId(this Employment e, DateTime? date = null, List<int> skipVacationGroupIds = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            List<EmploymentVacationGroup> employmentVacationGroups = e.GetActiveEmploymentVacationGroups();
            if (!skipVacationGroupIds.IsNullOrEmpty())
                employmentVacationGroups = employmentVacationGroups?.Where(i => !skipVacationGroupIds.Contains(i.VacationGroupId)).ToList() ?? new List<EmploymentVacationGroup>();

            if (!employmentVacationGroups.IsNullOrEmpty())
            {
                EmploymentVacationGroup employmentVacationGroup = employmentVacationGroups.Where(i => i.FromDate.HasValue && i.FromDate.Value <= date).OrderByDescending(i => i.FromDate).FirstOrDefault();
                if (employmentVacationGroup == null)
                    employmentVacationGroup = employmentVacationGroups.FirstOrDefault(i => !i.FromDate.HasValue);
                if (employmentVacationGroup != null)
                    return employmentVacationGroup.VacationGroupId;
            }
            return null;
        }
        public static VacationGroup GetCurrentVacationGroup(this IEnumerable<Employment> l, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return l.GetEmployment(date)?.GetActiveEmploymentVacationGroups().Where(g => !g.FromDate.HasValue || g.FromDate.Value <= date).OrderBy(g => g.FromDate).LastOrDefault()?.VacationGroup;
        }

        public static VacationGroup GetCurrentVacationGroup(this Employment e, DateTime? date = null, List<VacationGroup> vacationGroups = null)
        {
            if (vacationGroups != null)
            {
                var id = e?.GetCurrentEmploymentVacationGroup(date)?.VacationGroupId;

                if (id.HasValue && vacationGroups.Any(a => a.VacationGroupId == id.Value))
                    return vacationGroups.FirstOrDefault(f => f.VacationGroupId == id.Value);
            }
            e.TryLoadEmploymentVacationGroups(true);
            return e?.GetCurrentEmploymentVacationGroup(date)?.VacationGroup;
        }

        public static EmploymentVacationGroup GetCurrentEmploymentVacationGroup(this Employment e, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return e.GetActiveEmploymentVacationGroups().Where(g => !g.FromDate.HasValue || g.FromDate.Value <= date).OrderBy(g => g.FromDate).LastOrDefault();
        }

        public static void TryLoadEmploymentVacationGroups(this Employment e, bool loadVacationGroup = false)
        {
            try
            {
                if (e != null && !e.IsAdded() && !e.EmploymentVacationGroup.IsLoaded)
                {
                    e.EmploymentVacationGroup.Load();

                    if (loadVacationGroup && e.EmploymentVacationGroup.IsLoaded)
                    {
                        foreach (var item in e.EmploymentVacationGroup.Where(w => !w.VacationGroupReference.IsLoaded))
                        {
                            item.VacationGroupReference.Load();
                            if (!item.VacationGroup.VacationGroupSE.IsLoaded)
                                item.VacationGroup.VacationGroupSE.Load();
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

        #region AnnualLeaveGroup

        public static List<AnnualLeaveGroup> GetAnnualLeaveGroups(this Employment e, DateTime dateFrom, DateTime dateTo, List<AnnualLeaveGroup> annualLeaveGroupGroups = null)
        {
            List<AnnualLeaveGroup> result = new List<AnnualLeaveGroup>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                AnnualLeaveGroup annualLeaveGroup = e.GetAnnualLeaveGroup(date, annualLeaveGroupGroups);
                if (annualLeaveGroup != null && !result.Any(i => i.AnnualLeaveGroupId == annualLeaveGroup.AnnualLeaveGroupId))
                    result.Add(annualLeaveGroup);

                date = date.AddDays(1);
            }

            return result;
        }

        public static AnnualLeaveGroup GetAnnualLeaveGroup(this Employment e, DateTime dateFrom, DateTime dateTo, List<AnnualLeaveGroup> annualLeaveGroupGroups, bool forward = true, bool useLastIfCurrentNotExists = false)
        {
            AnnualLeaveGroup result = null;

            if (forward)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    result = e.GetAnnualLeaveGroup(date, annualLeaveGroupGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(1);
                }
            }
            else
            {
                DateTime date = dateTo;
                while (date >= dateFrom)
                {
                    result = e.GetAnnualLeaveGroup(date, annualLeaveGroupGroups);
                    if (result != null)
                        break;

                    date = date.AddDays(-1);
                }
            }

            if (result == null && useLastIfCurrentNotExists && e.DateTo.HasValue)
                result = e.GetAnnualLeaveGroup(e.DateTo.Value, annualLeaveGroupGroups);

            return result;
        }

        public static void TryLoadOriginalAnnualLeaveGroup(this Employment e)
        {
            try
            {
                if (e != null && !e.IsAdded() && !e.OriginalAnnualLeaveGroupReference.IsLoaded)
                {
                    e.OriginalAnnualLeaveGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.OriginalAnnualLeaveGroupReference");
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }

        #endregion

        #endregion

        #region EmploymentChange

        public static EmploymentChangeDTO ToDTO(this EmploymentChange e, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, List<AnnualLeaveGroup> annualLeaveGroups = null)
        {
            if (e == null || e.EmploymentChangeBatch == null)
                return null;

            EmploymentChangeDTO dto = new EmploymentChangeDTO()
            {
                //EmploymentChange
                EmploymentChangeId = e.EmploymentChangeId,
                EmploymentId = e.EmploymentId,
                EmployeeId = e.EmployeeId,
                Type = (TermGroup_EmploymentChangeType)e.Type,
                FieldType = (TermGroup_EmploymentChangeFieldType)e.FieldType,
                FromValue = e.FromValue,
                ToValue = e.ToValue,
                FromValueName = e.FromValueName,
                ToValueName = e.ToValueName,
                State = e.State,

                //EmploymentChangeBatch
                ActorCompanyId = e.EmploymentChangeBatch.ActorCompanyId,
                FromDate = e.EmploymentChangeBatch.FromDate,
                ToDate = e.EmploymentChangeBatch.ToDate,
                Comment = e.EmploymentChangeBatch.Comment,
                Created = e.EmploymentChangeBatch.Created,
                CreatedBy = e.EmploymentChangeBatch.CreatedBy,
            };

            #region Set current names on relations

            try
            {
                switch (dto.FieldType)
                {
                    case TermGroup_EmploymentChangeFieldType.EmployeeGroupId:
                    case TermGroup_EmploymentChangeFieldType.PayrollGroupId:
                    case TermGroup_EmploymentChangeFieldType.PayrollPriceTypeId:
                    case TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId:
                        #region EmployeeGroupId, PayrollGroupId, PayrollPriceTypeId, AnnualLeaveGroupId

                        var cacheItem = ExtensionCache.Instance.GetEmployeePayrollGroupExtensionCache(dto.ActorCompanyId);
                        if (Int32.TryParse(e.ToValue, out int id) && id > 0)
                        {
                            if (dto.FieldType == TermGroup_EmploymentChangeFieldType.EmployeeGroupId)
                            {
                                dto.ToValueName = employeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == id)?.Name ??
                                                  cacheItem?.EmployeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == id)?.Name ??
                                                  string.Empty;
                            }
                            else if (dto.FieldType == TermGroup_EmploymentChangeFieldType.PayrollGroupId)
                            {
                                dto.ToValueName = payrollGroups?.FirstOrDefault(i => i.PayrollGroupId == id)?.Name ??
                                                  cacheItem?.PayrollGroups?.FirstOrDefault(i => i.PayrollGroupId == id)?.Name ??
                                                  string.Empty;
                            }
                            else if (dto.FieldType == TermGroup_EmploymentChangeFieldType.PayrollPriceTypeId)
                            {
                                dto.ToValueName = payrollPriceTypes?.FirstOrDefault(i => i.PayrollPriceTypeId == id)?.Name ??
                                                  cacheItem?.PayrollPriceTypes?.FirstOrDefault(i => i.PayrollPriceTypeId == id)?.Name ??
                                                  string.Empty;
                            }
                            else if (dto.FieldType == TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId)
                            {
                                dto.ToValueName = annualLeaveGroups?.FirstOrDefault(i => i.AnnualLeaveGroupId == id)?.Name ??
                                                  cacheItem?.AnnualLeaveGroups?.FirstOrDefault(i => i.AnnualLeaveGroupId == id)?.Name ??
                                                  string.Empty;
                            }
                        }

                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }

            #endregion

            return dto;
        }

        public static IEnumerable<EmploymentChangeDTO> ToDTOs(this IEnumerable<EmploymentChange> l, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null)
        {
            var dtos = new List<EmploymentChangeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(employeeGroups, payrollGroups, payrollPriceTypes));
                }
            }
            return dtos;
        }

        public static List<EmploymentChange> GetValid(this IEnumerable<EmploymentChange> l)
        {
            List<EmploymentChange> valid = new List<EmploymentChange>();
            foreach (EmploymentChange e in l.OrderBy(i => i.EmploymentChangeBatch.FromDate).ThenByDescending(i => i.EmploymentChangeBatch.Created))
            {
                if (!valid.Any(i => i.EmploymentChangeBatch.FromDate == e.EmploymentChangeBatch.FromDate))
                    valid.Add(e);
            }
            return valid;
        }

        #endregion

        #region EmploymentChangeBatch

        public static IEnumerable<EmploymentChangeDTO> ToDTOs(this IEnumerable<EmploymentChangeBatch> l, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null)
        {
            var dtos = new List<EmploymentChangeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (!e.EmploymentChange.IsLoaded && !e.IsAdded())
                    {
                        e.EmploymentChange.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Employment.cs e.EmploymentChange");
                    }

                    foreach (var change in e.EmploymentChange)
                    {
                        dtos.Add(change.ToDTO(employeeGroups, payrollGroups, payrollPriceTypes));
                    }
                }
            }
            return dtos;
        }

        public static List<EmploymentChangeBatch> ExcludeHibernating(this IEnumerable<EmploymentChangeBatch> l)
        {
            return l?.Where(e => e.EmploymentChange != null && e.EmploymentChange.All(c => c.FieldType == (int)TermGroup_EmploymentChangeFieldType.Hibernating)).ToList() ?? new List<EmploymentChangeBatch>();
        }

        public static List<EmploymentChange> GetInformationChanges(this EmploymentChangeBatch e, params TermGroup_EmploymentChangeFieldType[] fieldTypes)
        {
            if (fieldTypes.IsNullOrEmpty())
                return new List<EmploymentChange>();
            return e?.EmploymentChange.Where(ec => ec.State == (int)SoeEntityState.Active && ec.Type == (int)TermGroup_EmploymentChangeType.Information && fieldTypes.Contains((TermGroup_EmploymentChangeFieldType)ec.FieldType)).ToList() ?? new List<EmploymentChange>();
        }

        public static List<EmploymentChange> GetDataChanges(this EmploymentChangeBatch e, params TermGroup_EmploymentChangeFieldType[] fieldTypes)
        {
            if (fieldTypes.IsNullOrEmpty())
                return new List<EmploymentChange>();
            return e?.EmploymentChange.Where(ec => ec.State == (int)SoeEntityState.Active && ec.Type == (int)TermGroup_EmploymentChangeType.DataChange && fieldTypes.Contains((TermGroup_EmploymentChangeFieldType)ec.FieldType)).ToList() ?? new List<EmploymentChange>();
        }

        public static List<EmploymentChange> GetChanges(this EmploymentChangeBatch e)
        {
            return e?.EmploymentChange.Where(i => i.State == (int)SoeEntityState.Active).ToList() ?? new List<EmploymentChange>();
        }

        public static void UpdateDateFrom(this List<EmploymentChangeBatch> l, DateTime fromDate)
        {
            if (l.IsNullOrEmpty())
                return;
            l.ExcludeHibernating().ForEach(batch => batch.FromDate = fromDate);
        }

        public static void UpdateDateTo(this List<EmploymentChangeBatch> l, DateTime? toDate)
        {
            if (l.IsNullOrEmpty())
                return;
            l.ExcludeHibernating().ForEach(batch => batch.ToDate = toDate);
        }

        #endregion

        #region ApplyEmploymentDay

        public static ApplyEmploymentDay GetEmploymentDay(this List<ApplyEmploymentDay> days, ApplyEmploymentDayParams parameters)
        {
            if (days.IsNullOrEmpty() || parameters == null)
                return null;

            if (parameters.FieldType.HasValue)
                return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && d.Parameters.FieldType == parameters.FieldType.Value);
            else if (parameters.LookupEmployeeGroupsLoaded && parameters.LookupPayrollGroupsLoaded)
                return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue && d.Parameters.LookupEmployeeGroupsLoaded == parameters.LookupEmployeeGroupsLoaded && d.Parameters.LookupPayrollGroupsLoaded == parameters.LookupPayrollGroupsLoaded);
            else if (parameters.LookupEmployeeGroupsLoaded)
                return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue && d.Parameters.LookupEmployeeGroupsLoaded == parameters.LookupEmployeeGroupsLoaded);
            else if (parameters.LookupPayrollGroupsLoaded)
                return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue && d.Parameters.LookupPayrollGroupsLoaded == parameters.LookupPayrollGroupsLoaded);
            else
                return days.FirstOrDefault(d => d.Parameters.EmployeeId == parameters.EmployeeId && d.Parameters.Date == parameters.Date && !d.Parameters.FieldType.HasValue);
        }

        public static void AddEmploymentDay(this List<ApplyEmploymentDay> days, ApplyEmploymentDayParams parameters, EmploymentDTO employment)
        {
            days.Add(new ApplyEmploymentDay(parameters, employment));
        }

        #endregion
    }
}
