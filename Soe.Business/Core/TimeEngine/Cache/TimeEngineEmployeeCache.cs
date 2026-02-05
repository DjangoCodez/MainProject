using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine.Cache
{
    #region Enums

    internal enum EmployeeAbsenceCacheType
    {
        AbsenceByLevel3 = 1,
        AbsenceByVacationReplacement = 2,
    }

    #endregion

    #region Classes

    internal class TimeEngineEmployeeCache
    {
        #region Variables

        public int EmployeeId;

        private EmployeeAbsenceCache employeeAbsenceCache;
        private void InitEmployeeAbsenceCache()
        {
            if (this.employeeAbsenceCache == null)
                this.employeeAbsenceCache = new EmployeeAbsenceCache();
        }
        private bool IsInitialized()
        {
            return this.employeeAbsenceCache != null;
        }

        #endregion

        #region Ctor

        public TimeEngineEmployeeCache(int employeeId)
        {
            this.EmployeeId = employeeId;
        }

        #endregion

        #region Accounting priority

        private List<AccountingPrioCacheItem> accountingPrioCacheItems;
        public AccountingPrioDTO GetAccountingPrio(DateTime date, int? productId)
        {
            return GetAccountingPrioCacheItem(date, productId)?.AccountingPrioDTO;
        }
        public AccountingPrioCacheItem GetAccountingPrioCacheItem(DateTime date, int? productId)
        {
            AccountingPrioCacheItem item = null;
            if (productId.HasValue)
                item = this.accountingPrioCacheItems?.FirstOrDefault(i => i.Date == date && i.ProductId.HasValue && i.ProductId.Value == productId.Value);
            else
                item = this.accountingPrioCacheItems?.FirstOrDefault(i => i.Date == date);
            return item;
        }
        public void AddAccountingPrio(AccountingPrioDTO accountingPrio, DateTime date, int? productId)
        {
            if (this.accountingPrioCacheItems == null)
                this.accountingPrioCacheItems = new List<AccountingPrioCacheItem>();

            var item = GetAccountingPrioCacheItem(date, productId);
            if (item == null)
            {
                item = new AccountingPrioCacheItem(accountingPrio, date, productId);
                this.accountingPrioCacheItems.Add(item);
            }
            else
            {
                item.AccountingPrioDTO = accountingPrio;
            }
        }

        #endregion

        #region Absence - five vacation days per week

        private Dictionary<int, bool> vacationFiveDaysPerWeekWholeWeekDict;
        public bool? IsVacationFiveDaysPerWeekWholeWeek(int weekNr)
        {
            if (this.vacationFiveDaysPerWeekWholeWeekDict != null && this.vacationFiveDaysPerWeekWholeWeekDict.ContainsKey(weekNr))
                return this.vacationFiveDaysPerWeekWholeWeekDict[weekNr];
            return null;
        }
        public void AddVacationFiveDaysPerWeekWholeWeek(int weekNr, bool useVacationFiveDaysPerWeek)
        {
            if (this.vacationFiveDaysPerWeekWholeWeekDict == null)
                this.vacationFiveDaysPerWeekWholeWeekDict = new Dictionary<int, bool>();
            if (!this.vacationFiveDaysPerWeekWholeWeekDict.ContainsKey(weekNr))
                this.vacationFiveDaysPerWeekWholeWeekDict.Add(weekNr, useVacationFiveDaysPerWeek);
        }

        #endregion

        #region DayType

        private List<Tuple<DateTime, bool, DayType>> dayTypes;
        public DayType GetDayType(DateTime date, bool doNotCheckHoliday = false)
        {
            if (dayTypes == null)
                return null;

            return dayTypes.FirstOrDefault(i => i.Item1 == date && i.Item2 == doNotCheckHoliday)?.Item3;
        }
        public bool ContainsDayType(DateTime date, bool doNotCheckHoliday = false)
        {
            return GetDayType(date, doNotCheckHoliday) != null;
        }
        public void AddDayType(DateTime date, DayType dayType, bool doNotCheckHoliday = false)
        {
            if (dayTypes == null)
                dayTypes = new List<Tuple<DateTime, bool, DayType>>();

            if (!ContainsDayType(date, doNotCheckHoliday))
                dayTypes.Add(Tuple.Create(date, doNotCheckHoliday, dayType));
        }

        private List<DayType> dayTypesWithStandardWeekDay = null;
        public List<DayType> GetDayTypesWithStandardWeekDay()
        {
            return this.dayTypesWithStandardWeekDay;
        }
        public void SetDayTypesWithStandardWeekDay(List<DayType> dayTypes)
        {
            if (dayTypes == null)
                return;

            this.dayTypesWithStandardWeekDay = dayTypes;
        }

        #endregion

        #region Employee

        private Employee employee;
        public Employee GetEmployee()
        {
            return this.employee;
        }
        public void SetEmployee(Employee employee)
        {
            if (employee != null)
                this.employee = employee;
        }

        private Dictionary<DateTime, bool> employeeHighRiskProtectionDict;
        public bool? HasEmployeeHighRiskProtection(DateTime date)
        {
            return this.employeeHighRiskProtectionDict != null && this.employeeHighRiskProtectionDict.ContainsKey(date) ? this.employeeHighRiskProtectionDict[date] : (bool?)null;
        }
        public void SetEmployeeHighRiskProtection(DateTime date, bool hasHighRiskProtection)
        {
            if (this.employeeHighRiskProtectionDict == null)
                this.employeeHighRiskProtectionDict = new Dictionary<DateTime, bool>();

            if (this.employeeHighRiskProtectionDict.ContainsKey(date))
                this.employeeHighRiskProtectionDict[date] = hasHighRiskProtection;
            else
                this.employeeHighRiskProtectionDict.Add(date, hasHighRiskProtection);
        }

        private bool hasSetBirthDate = false;
        private DateTime? birthDate = null;
        public bool TryGetEmployeeBirthDate(out DateTime? birthDate)
        {
            birthDate = this.hasSetBirthDate ? this.birthDate : (DateTime?)null;
            return hasSetBirthDate;
        }
        public void SetBirthDate(DateTime? birthDate)
        {
            this.birthDate = birthDate;
            this.hasSetBirthDate = true;
        }

        #endregion

        #region EmployeeChild

        private Dictionary<int, EmployeeChild> employeeChildsDict = null;
        public EmployeeChild GetEmployeeChild(int employeeChildId)
        {
            if (this.employeeChildsDict != null && this.employeeChildsDict.ContainsKey(employeeChildId))
                return this.employeeChildsDict[employeeChildId];
            return null;
        }
        public void AddEmployeeChild(EmployeeChild employeeChild)
        {
            if (employeeChild == null)
                return;

            if (this.employeeChildsDict == null)
                this.employeeChildsDict = new Dictionary<int, EmployeeChild>();

            if (!this.employeeChildsDict.ContainsKey(employeeChild.EmployeeChildId))
                this.employeeChildsDict.Add(employeeChild.EmployeeChildId, employeeChild);
        }

        #endregion

        #region EmployeeFactors

        private List<EmployeeFactor> employeeFactors;
        public List<EmployeeFactor> GetEmployeeFactors()
        {
            return this.employeeFactors;
        }
        public void SetEmployeeFactors(List<EmployeeFactor> employeeFactors)
        {
            if (employeeFactors == null)
                return;
            this.employeeFactors = employeeFactors;
        }

        #endregion

        #region EmployeeSchedule

        private List<EmployeeSchedule> employeeSchedules;
        public EmployeeSchedule GetEmployeeSchedule(int employeeScheduleId)
        {
            return this.employeeSchedules?.FirstOrDefault(i => i.EmployeeScheduleId == employeeScheduleId);
        }
        public EmployeeSchedule GetEmployeeSchedule(DateTime date)
        {
            return this.employeeSchedules?.FirstOrDefault(i => i.StartDate <= date && i.StopDate >= date);
        }
        public void AddEmployeeSchedule(EmployeeSchedule employeeSchedule)
        {
            if (employeeSchedule == null)
                return;

            if (this.employeeSchedules == null)
                this.employeeSchedules = new List<EmployeeSchedule>();

            EmployeeSchedule existingEmployeeSchedule = GetEmployeeSchedule(employeeSchedule.EmployeeScheduleId);
            if (existingEmployeeSchedule == null)
                this.employeeSchedules.Add(employeeSchedule);
        }

        #endregion

        #region EmployeeSettings

        private Dictionary<DateTime, List<EmployeeSetting>> employeeSettings;
        public List<EmployeeSetting> GetEmployeeSettings(DateTime date)
        {
            if (this.employeeSettings != null && this.employeeSettings.ContainsKey(date))
                return this.employeeSettings[date];
            return null;
        }
        public void SetEmployeeSettings(DateTime date, List<EmployeeSetting> settings)
        {
            if (this.employeeSettings == null)
                this.employeeSettings = new Dictionary<DateTime, List<EmployeeSetting>>();

            if (!this.employeeSettings.ContainsKey(date))
                this.employeeSettings.Add(date, settings);
            else
                this.employeeSettings[date] = settings;           
        }

        #endregion

        #region EmployeeTimePeriod

        private List<EmployeeTimePeriod> employeeTimePeriods;
        public List<EmployeeTimePeriod> GetEmployeeTimePeriods()
        {
            return employeeTimePeriods;
        }
        public void SetEmployeeTimePeriods(List<EmployeeTimePeriod> employeeTimePeriods)
        {
            if (employeeTimePeriods == null)
                return;

            this.employeeTimePeriods = employeeTimePeriods;
        }

        #endregion

        #region EmploymentAccountStd

        private Dictionary<int, List<EmploymentAccountStd>> employmentAccountStdsDict;
        public List<EmploymentAccountStd> GetEmploymentAccountStds(int employmentId)
        {
            return this.employmentAccountStdsDict != null && this.employmentAccountStdsDict.ContainsKey(employmentId) ? this.employmentAccountStdsDict[employmentId] : null;
        }
        public void AddEmploymentAccountStds(int employmentId, List<EmploymentAccountStd> employmentAccountStds)
        {
            if (employmentAccountStds == null)
                return;

            if (this.employmentAccountStdsDict == null)
                this.employmentAccountStdsDict = new Dictionary<int, List<EmploymentAccountStd>>();
            if (this.employmentAccountStdsDict.ContainsKey(employmentId))
                this.employmentAccountStdsDict[employmentId] = employmentAccountStds;
            else
                this.employmentAccountStdsDict.Add(employmentId, employmentAccountStds);
        }

        #endregion

        #region EmploymentVacationGroup

        private Dictionary<DateTime, EmploymentVacationGroup> employmentVacationGroupsDict;
        public (EmploymentVacationGroup employmentVacationGroup, bool exists) GetEmploymentVacationGroupWithVacationGroup(DateTime date)
        {
            date = date.Date;
            return ContainsEmploymentVacationGroup(date) ? (this.employmentVacationGroupsDict.GetValue(date), true) : (null, false);
        }
        public void AddEmploymentVacationGroupWithVacationGroup(DateTime date, EmploymentVacationGroup employmentVacationGroup)
        {
            if (this.employmentVacationGroupsDict == null)
                this.employmentVacationGroupsDict = new Dictionary<DateTime, EmploymentVacationGroup>();

            date = date.Date;

            if (!ContainsEmploymentVacationGroup(date))
                this.employmentVacationGroupsDict.Add(date, employmentVacationGroup);
            else if (employmentVacationGroupsDict[date] == null)
                this.employmentVacationGroupsDict[date] = employmentVacationGroup;
        }
        private bool ContainsEmploymentVacationGroup(DateTime date)
        {
            return this.employmentVacationGroupsDict?.ContainsKey(date) ?? false;
        }

        private Dictionary<int, List<EmploymentVacationGroup>> employmentIdVacationGroupsDict;
        public List<EmploymentVacationGroup> GetEmploymentVacationGroupWithVacationGroup(int employmentId)
        {
            return this.employmentIdVacationGroupsDict.GetList(employmentId, nullIfNotFound: true);
        }
        public void AddEmploymentVacationGroupWithVacationGroup(int employmentId, List<EmploymentVacationGroup> employmentVacationGroups)
        {
            if (this.employmentIdVacationGroupsDict == null)
                this.employmentIdVacationGroupsDict = new Dictionary<int, List<EmploymentVacationGroup>>();

            if (!this.employmentIdVacationGroupsDict.ContainsKey(employmentId))
                this.employmentIdVacationGroupsDict.Add(employmentId, employmentVacationGroups);
            else if (employmentIdVacationGroupsDict[employmentId] == null)
                this.employmentIdVacationGroupsDict[employmentId] = employmentVacationGroups;
        }

        private Dictionary<int, List<VacationGroupSEDayType>> vacationGroupSEDayTypesDict;
        public (List<VacationGroupSEDayType> vacationGroupSEDayType, bool exists) GetVacationGroupSEDayType(int vacationGroupSEId)
        {
            return ContainsVacationGroupSEDayType(vacationGroupSEId) ? (this.vacationGroupSEDayTypesDict.GetList(vacationGroupSEId), true) : (null, false);
        }
        public void AddVacationGroupSEDayType(int vacationGroupSEId, List<VacationGroupSEDayType> vacationGroupSEDayTypes)
        {
            if (this.vacationGroupSEDayTypesDict == null)
                this.vacationGroupSEDayTypesDict = new Dictionary<int, List<VacationGroupSEDayType>>();

            if (!this.vacationGroupSEDayTypesDict.ContainsKey(vacationGroupSEId))
                this.vacationGroupSEDayTypesDict.Add(vacationGroupSEId, vacationGroupSEDayTypes);
            else if (vacationGroupSEDayTypesDict[vacationGroupSEId] == null)
                this.vacationGroupSEDayTypesDict[vacationGroupSEId] = vacationGroupSEDayTypes;
        }
        private bool ContainsVacationGroupSEDayType(int vacationGroupSEId)
        {
            return this.vacationGroupSEDayTypesDict?.ContainsKey(vacationGroupSEId) ?? false;
        }

        #endregion

        #region EmployeeVacationSE

        private EmployeeVacationSE employeeVacationSE;
        public EmployeeVacationSE GetEmployeeVacationSE()
        {
            return this.employeeVacationSE;
        }
        public void SetEmployeeVacationSE(EmployeeVacationSE employeeVacationSE)
        {
            if (employeeVacationSE != null)
                this.employeeVacationSE = employeeVacationSE;
        }

        #endregion

        #region Extra Shift

        private List<ExtraShiftCalculationPeriod> extraShiftCalculations;
        public ExtraShiftCalculationPeriod GetExtraShiftCalculationPeriod(DateTime dateFrom, DateTime dateTo, bool useIgnoreIfExtraShifts)
        {
            if (this.extraShiftCalculations == null)
                return null;

            foreach (ExtraShiftCalculationPeriod extraShiftCalculation in this.extraShiftCalculations)
            {
                if (extraShiftCalculation.IsSame(dateFrom, dateTo, useIgnoreIfExtraShifts))
                    return extraShiftCalculation;
            }

            return null;
        }
        public void AddExtraShiftCalculation(ExtraShiftCalculationPeriod extraShiftCalculation)
        {
            if (extraShiftCalculation == null || (this.extraShiftCalculations != null && this.extraShiftCalculations.Any(i => i.IsSame(extraShiftCalculation.DateFrom, extraShiftCalculation.DateTo, extraShiftCalculation.UseIgnoreIfExtraShifts))))
                return;

            if (this.extraShiftCalculations == null)
                this.extraShiftCalculations = new List<ExtraShiftCalculationPeriod>();
            this.extraShiftCalculations.Add(extraShiftCalculation);
        }

        #endregion

        #region FixedPayrollRow

        private List<FixedPayrollRowDTO> fixedPayrollRows = null;
        public List<FixedPayrollRowDTO> GetFixedPayrollRowDTOs()
        {
            return this.fixedPayrollRows;
        }

        public void AddFixedPayrollRowDTOs(List<FixedPayrollRowDTO> rows)
        {
            if (rows == null)
                return;

            if (this.fixedPayrollRows == null)
                this.fixedPayrollRows = new List<FixedPayrollRowDTO>();
            
            foreach (var row in rows)
            {
                if (!this.fixedPayrollRows.Any(x => x.FixedPayrollRowId == row.FixedPayrollRowId))
                    this.fixedPayrollRows.Add(row);
            }           
        }

        #endregion

        #region PayrollStartValueRow

        private Dictionary<DateTime, List<PayrollStartValueRow>> payrollStartValueRows = null;
        public List<PayrollStartValueRow> GetPayrollStartValueRows(DateTime date)
        {
            if (this.payrollStartValueRows == null || !this.payrollStartValueRows.ContainsKey(date))
                return null;
            return this.payrollStartValueRows[date];
        }
        public void SetPayrollStartValueRows(DateTime date, List<PayrollStartValueRow> payrollStartValueRows)
        {
            if (this.payrollStartValueRows == null)
                this.payrollStartValueRows = new Dictionary<DateTime, List<PayrollStartValueRow>>();
            if (this.payrollStartValueRows.ContainsKey(date))
                this.payrollStartValueRows[date] = payrollStartValueRows;
            else
                this.payrollStartValueRows.Add(date, payrollStartValueRows);
        }

        #endregion

        #region SicknessPeriod

        private List<SicknessPeriod> sicknessPeriods;
        public SicknessPeriod GetSicknessPeriod(DateTime date)
        {
            if (this.sicknessPeriods == null)
                return null;

            foreach (SicknessPeriod sicknessPeriod in this.sicknessPeriods)
            {
                if (sicknessPeriod.IsSamePeriod(date))
                    return sicknessPeriod;
            }

            return null;
        }
        public void AddSicknessPeriod(SicknessPeriod sicknessPeriod)
        {
            if (sicknessPeriod == null || (this.sicknessPeriods != null && this.sicknessPeriods.Any(i => i.Guid == sicknessPeriod.Guid)))
                return;

            if (this.sicknessPeriods == null)
                this.sicknessPeriods = new List<SicknessPeriod>();
            this.sicknessPeriods.Add(sicknessPeriod);
        }
        public void RemoveSicknessPeriod(DateTime date)
        {
            this.sicknessPeriods = this.sicknessPeriods?.Where(i => !i.Dates.Contains(date)).ToList();
        }

        #endregion

        #region SicknessSalary

        Dictionary<DateTime, bool> hasEmployeeRightToSicknessSalaryByDate;
        public bool? HasEmployeeRightToSicknessSalary(DateTime date)
        {
            if (this.hasEmployeeRightToSicknessSalaryByDate == null || !this.hasEmployeeRightToSicknessSalaryByDate.ContainsKey(date))
                return null;
            return this.hasEmployeeRightToSicknessSalaryByDate[date];
        }
        public void AddHasEmployeeRightToSicknessSalary(DateTime date, bool value)
        {
            if (this.hasEmployeeRightToSicknessSalaryByDate == null)
                hasEmployeeRightToSicknessSalaryByDate = new Dictionary<DateTime, bool>();
            if (!this.hasEmployeeRightToSicknessSalaryByDate.ContainsKey(date))
                hasEmployeeRightToSicknessSalaryByDate.Add(date, value);
        }

        #endregion

        #region TimeAccumulatorBalances

        private List<TimeAccumulatorBalance> timeAccumulatorBalances;
        public List<TimeAccumulatorBalance> GetTimeAccumulatorBalances()
        {
            return timeAccumulatorBalances;
        }
        public void SetTimeAccumulatorBalances(List<TimeAccumulatorBalance> timeAccumulatorBalances)
        {
            if (timeAccumulatorBalances == null)
                return;

            this.timeAccumulatorBalances = timeAccumulatorBalances;
        }

        #endregion

        #region TimeCodeTransaction

        Dictionary<int, List<TimeCodeTransaction>> timeCodeTransactionsByTimeBlockDate = null;
        public List<TimeCodeTransaction> GetTimeCodeTransactions(int timeBlockDateId)
        {
            return this.timeCodeTransactionsByTimeBlockDate?.GetValue(timeBlockDateId);
        }
        public void AddTimeCodeTransactions(int timeBlockDateId, List<TimeCodeTransaction> timeCodeTransactions)
        {
            if (this.timeCodeTransactionsByTimeBlockDate == null)
                this.timeCodeTransactionsByTimeBlockDate = new Dictionary<int, List<TimeCodeTransaction>>();
            if (!this.timeCodeTransactionsByTimeBlockDate.ContainsKey(timeBlockDateId))
                this.timeCodeTransactionsByTimeBlockDate.Add(timeBlockDateId, timeCodeTransactions);
        }

        #endregion

        #region TimePayrollTransactions

        public List<TimePayrollTransaction> GetTimePayrollTransactionsWithTimeBlockDate(List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached, int sysPayrollTypeLevel3, EmployeeChild employeeChild = null)
        {
            InitEmployeeAbsenceCache();
            return this.employeeAbsenceCache.GetLevel3TimePayrollTransactionsWithTimeBlockDate(timeBlockDates, out timeBlockDatesNotCached, sysPayrollTypeLevel3, employeeChild);
        }
        public List<TimePayrollTransaction> GetVacationReplacementTimePayrollTransactionsWithTimeBlockDate(List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached)
        {
            InitEmployeeAbsenceCache();
            return this.employeeAbsenceCache.GetVacationReplacementTimePayrollTransactionsWithTimeBlockDate(timeBlockDates, out timeBlockDatesNotCached);
        }
        public void AddTimePayrollTransactionsWithTimeBlockDate(List<TimeBlockDate> timeBlockDates, List<TimePayrollTransaction> timePayrollTransactions, int sysPayrollTypeLevel3, EmployeeChild employeeChild = null)
        {
            InitEmployeeAbsenceCache();
            this.employeeAbsenceCache.AddLevel3TimePayrollTransactionsWithTimeBlockDate(timePayrollTransactions, timeBlockDates, sysPayrollTypeLevel3, employeeChild);
        }
        public void AddVacationReplacementTimePayrollTransactionsWithTimeBlockDate(List<TimeBlockDate> timeBlockDates, List<TimePayrollTransaction> timePayrollTransactions)
        {
            InitEmployeeAbsenceCache();
            this.employeeAbsenceCache.AddVacationReplacementTimePayrollTransactionsWithTimeBlockDate(timePayrollTransactions, timeBlockDates);
        }
        public void ClearTimePayrollTransactionsWithTimeBlockDate(TimeBlockDate timeBlockDate)
        {
            if (!IsInitialized())
                return;

            this.employeeAbsenceCache.ClearTimePayrollTransactionsWithTimeBlockDate(timeBlockDate);
        }

        #endregion

        #region TimeBlockDate

        private Dictionary<DateTime, TimeBlockDate> timeBlockDatesDict;
        public TimeBlockDate GetTimeBlockDate(int timeBlockDateId)
        {
            if (timeBlockDateId == 0)
                return null;

            return timeBlockDatesDict?.Values.FirstOrDefault(timeBlockDate => timeBlockDate != null && timeBlockDate.TimeBlockDateId == timeBlockDateId);
        }
        public (TimeBlockDate timeBlockDate, bool exists) GetTimeBlockDate(DateTime date)
        {
            date = date.Date;
            return ContainsTimeBlockDate(date) ? (timeBlockDatesDict[date], true) : (null, false);
        }
        public List<TimeBlockDate> GetTimeBlockDates(IEnumerable<int> timeBlockDateIds, out List<int> timeBlockDateIdsNotCached)
        {
            if (timeBlockDateIds.IsNullOrEmpty())
            {
                timeBlockDateIdsNotCached = new List<int>();
                return new List<TimeBlockDate>();
            }
            else
            {
                List<TimeBlockDate> timeBlockDatesCached = timeBlockDatesDict?.Values.Where(timeBlockDate => timeBlockDate != null && timeBlockDateIds.Contains(timeBlockDate.TimeBlockDateId)).ToList() ?? new List<TimeBlockDate>();
                List<int> timeBlockDatesIdsCached = timeBlockDatesCached.Select(i => i.TimeBlockDateId).ToList();
                timeBlockDateIdsNotCached = timeBlockDateIds.Except(timeBlockDatesIdsCached).ToList();
                return timeBlockDatesCached;
            }
        }
        public List<TimeBlockDate> GetTimeBlockDates(IEnumerable<DateTime> dates, out List<DateTime> datesNotCached)
        {
            if (dates.IsNullOrEmpty())
            {
                datesNotCached = new List<DateTime>();
                return new List<TimeBlockDate>();
            }
            else
            {
                List<TimeBlockDate> timeBlockDatesCached = timeBlockDatesDict?.Values.Where(timeBlockDate => timeBlockDate != null && dates.Contains(timeBlockDate.Date)).ToList() ?? new List<TimeBlockDate>();
                List<DateTime> datesCached = timeBlockDatesCached.Select(i => i.Date).ToList();
                datesNotCached = dates.Except(datesCached).ToList();
                return timeBlockDatesCached;
            }
        }
        private bool ContainsTimeBlockDate(DateTime date)
        {
            return this.timeBlockDatesDict?.ContainsKey(date) ?? false;
        }
        public void AddTimeBlockDate(TimeBlockDate timeBlockDate, DateTime? date = null)
        {
            if (this.timeBlockDatesDict == null)
                this.timeBlockDatesDict = new Dictionary<DateTime, TimeBlockDate>();

            date = date?.Date ?? timeBlockDate?.Date.Date;
            if (!date.HasValue)
                return;

            if (!ContainsTimeBlockDate(date.Value))
                this.timeBlockDatesDict.Add(date.Value, timeBlockDate);
            else if (this.timeBlockDatesDict[date.Value] == null)
                this.timeBlockDatesDict[date.Value] = timeBlockDate;
        }
        public void AddTimeBlockDates(List<TimeBlockDate> timeBlockDates, DateTime dateFrom, DateTime dateTo)
        {
            if (timeBlockDates == null)
                return;

            int totalDays = (int)dateTo.Date.Subtract(dateFrom.Date).TotalDays + 1;
            if (totalDays != timeBlockDates.Count)
            {
                DateTime date = dateFrom;
                while (date <= dateTo)
                {
                    AddTimeBlockDate(timeBlockDates.FirstOrDefault(i => i.Date == date.Date), date);
                    date = date.AddDays(1);
                }
            }
            else
            {
                foreach (TimeBlockDate timeBlockDate in timeBlockDates)
                {
                    AddTimeBlockDate(timeBlockDate);
                }
            }
        }

        #endregion

        #region TimeScheduleTemplateBlock

        private Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksDict;
        public List<TimeScheduleTemplateBlock> GetScheduleBlocks(List<DateTime> dates)
        {
            List<TimeScheduleTemplateBlock> scheduleBlocks = new List<TimeScheduleTemplateBlock>();
            foreach (DateTime date in dates)
            {
                List<TimeScheduleTemplateBlock> scheduleBlocksForDate = this.scheduleBlocksDict.GetList(date, nullIfNotFound: true);
                if (scheduleBlocksForDate == null)
                    return null; // all or nothing
                scheduleBlocks.AddRange(scheduleBlocksForDate);
            }
            return scheduleBlocks;            
        }
        public List<TimeScheduleTemplateBlock> GetScheduleBlocks(DateTime date)
        {
            return this.scheduleBlocksDict.GetList(date, nullIfNotFound: true);
        }
        public void AddScheduleBlocks(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (scheduleBlocks.IsNullOrEmpty())
                return;

            if (this.scheduleBlocksDict == null)
                this.scheduleBlocksDict = new Dictionary<DateTime, List<TimeScheduleTemplateBlock>>();
            foreach (var scheduleBlocksByDate in scheduleBlocks.Where(b => b.Date.HasValue).GroupBy(b => b.Date.Value))
            {
                if (!this.scheduleBlocksDict.ContainsKey(scheduleBlocksByDate.Key))
                    this.scheduleBlocksDict.Add(scheduleBlocksByDate.Key, scheduleBlocksByDate.ToList());
            }
        }

        private Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksWithTimeCodeAndStaffingDict;
        public List<TimeScheduleTemplateBlock> GetScheduleBlocksWithTimeCodeAndStaffing(DateTime date)
        {
            return this.scheduleBlocksWithTimeCodeAndStaffingDict.GetList(date, nullIfNotFound: true);
        }
        public void AddScheduleBlocksWithTimeCodeAndStaffing(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (scheduleBlocks.IsNullOrEmpty())
                return;

            if (this.scheduleBlocksWithTimeCodeAndStaffingDict == null)
                this.scheduleBlocksWithTimeCodeAndStaffingDict = new Dictionary<DateTime, List<TimeScheduleTemplateBlock>>();
            foreach (var scheduleBlocksByDate in scheduleBlocks.Where(b => b.Date.HasValue).GroupBy(b => b.Date.Value))
            {
                if (!this.scheduleBlocksWithTimeCodeAndStaffingDict.ContainsKey(scheduleBlocksByDate.Key))
                    this.scheduleBlocksWithTimeCodeAndStaffingDict.Add(scheduleBlocksByDate.Key, scheduleBlocksByDate.ToList());
            }
        }

        public void ClearScheduleFromCache(List<DateTime> dates)
        {
            dates?.ForEach(date => ClearScheduleFromCache(date));
        }
        public void ClearScheduleFromCache(DateTime date)
        {
            if (this.scheduleBlocksDict != null && this.scheduleBlocksDict.ContainsKey(date))
                this.scheduleBlocksDict.Remove(date);
            if (this.scheduleBlocksWithTimeCodeAndStaffingDict != null && this.scheduleBlocksWithTimeCodeAndStaffingDict.ContainsKey(date))
                this.scheduleBlocksWithTimeCodeAndStaffingDict.Remove(date);
        }

        #endregion

        #region TimeScheduleTemplateHead

        private List<TimeScheduleTemplateHead> timeScheduleTemplateHeads;
        public List<TimeScheduleTemplateHead> GetTimeScheduleTemplateHeads(int employeeId)
        {
            return this.timeScheduleTemplateHeads?.Where(i => i.EmployeeId == employeeId).ToList();
        }

        public TimeScheduleTemplateHead GetTimeScheduleTemplateHead(int timeScheduleTemplateHeadId)
        {
            return this.timeScheduleTemplateHeads?.FirstOrDefault(i => i.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId);
        }
        public TimeScheduleTemplateHead GetTimeScheduleTemplateHead(DateTime date)
        {
            return this.timeScheduleTemplateHeads?.FirstOrDefault(i => i.StartDate <= date && i.StopDate >= date);
        }
        public void AddTimeScheduleTemplateHead(TimeScheduleTemplateHead timeScheduleTemplateHead)
        {
            if (timeScheduleTemplateHead == null)
                return;

            if (this.timeScheduleTemplateHeads == null)
                this.timeScheduleTemplateHeads = new List<TimeScheduleTemplateHead>();

            TimeScheduleTemplateHead existingTimeScheduleTemplateHead = GetTimeScheduleTemplateHead(timeScheduleTemplateHead.TimeScheduleTemplateHeadId);
            if (existingTimeScheduleTemplateHead == null)
                this.timeScheduleTemplateHeads.Add(timeScheduleTemplateHead);
        }

        #endregion

        #region TimeScheduleTemplateGroup

        private List<TimeScheduleTemplateGroupDTO> timeScheduleTemplateGroups;
        public List<TimeScheduleTemplateGroupDTO> GetTimeScheduleTemplateGroups(int employeeId)
        {
            return this.timeScheduleTemplateGroups?.Where(i => i.State == SoeEntityState.Active && i.Employees.Any(a => a.State == SoeEntityState.Active && a.EmployeeId == employeeId)).ToList();
        }
        public TimeScheduleTemplateGroupDTO GetTimeScheduleTemplateGroup(int timeScheduleTemplateGroupId)
        {
            return this.timeScheduleTemplateGroups?.FirstOrDefault(i => i.TimeScheduleTemplateGroupId == timeScheduleTemplateGroupId);
        }
        public void AddTimeScheduleTemplateGroup(TimeScheduleTemplateGroupDTO timeScheduleTemplateGroup)
        {
            if (timeScheduleTemplateGroup == null)
                return;

            if (GetTimeScheduleTemplateGroup(timeScheduleTemplateGroup.TimeScheduleTemplateGroupId) == null)
            {
                if (this.timeScheduleTemplateGroups == null)
                    this.timeScheduleTemplateGroups = new List<TimeScheduleTemplateGroupDTO>();
                this.timeScheduleTemplateGroups.Add(timeScheduleTemplateGroup);
            }                
        }

        #endregion

        #region TimeScheduleTemplatePeriod
                
        private Dictionary<DateTime, TimeScheduleTemplatePeriod> timeScheduleTemplatePeriodsDict;        
        public TimeScheduleTemplatePeriod GetTimeScheduleTemplatePeriod(DateTime date)
        {            
            return timeScheduleTemplatePeriodsDict != null && timeScheduleTemplatePeriodsDict.ContainsKey(date) ? timeScheduleTemplatePeriodsDict[date] : null;
        }
        public void AddTimeScheduleTemplatePeriod(DateTime date, TimeScheduleTemplatePeriod timeScheduleTemplatePeriod)
        {
            if (timeScheduleTemplatePeriod == null)
                return;

            if (this.timeScheduleTemplatePeriodsDict == null)
                this.timeScheduleTemplatePeriodsDict = new Dictionary<DateTime, TimeScheduleTemplatePeriod>();

            if (!this.timeScheduleTemplatePeriodsDict.ContainsKey(date))
                this.timeScheduleTemplatePeriodsDict.Add(date, timeScheduleTemplatePeriod);   
        }

        #endregion

    }

    internal class EmployeeAbsenceCache
    {
        #region Variables

        private readonly List<EmployeeAbsenceContainerCache> containerCache;

        #endregion

        #region Ctor

        public EmployeeAbsenceCache()
        {
            this.containerCache = new List<EmployeeAbsenceContainerCache>();
        }

        #endregion

        #region Public methods

        public List<TimePayrollTransaction> GetLevel3TimePayrollTransactionsWithTimeBlockDate(List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached, int sysPayrollTypeLevel3, EmployeeChild employeeChild = null)
        {
            return GetTimePayrollTransactions(EmployeeAbsenceCacheType.AbsenceByLevel3, timeBlockDates, out timeBlockDatesNotCached, sysPayrollTypeLevel3, employeeChild: employeeChild);
        }

        public List<TimePayrollTransaction> GetVacationReplacementTimePayrollTransactionsWithTimeBlockDate(List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached)
        {
            return GetTimePayrollTransactions(EmployeeAbsenceCacheType.AbsenceByVacationReplacement, timeBlockDates, out timeBlockDatesNotCached);
        }

        public void AddLevel3TimePayrollTransactionsWithTimeBlockDate(List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates, int sysPayrollTypeLevel3, EmployeeChild employeeChild = null)
        {
            if (timePayrollTransactions == null || timeBlockDates.IsNullOrEmpty())
                return;
            CreateContainer(EmployeeAbsenceCacheType.AbsenceByLevel3, timePayrollTransactions, timeBlockDates, sysPayrollTypeLevel3, employeeChild: employeeChild);
        }

        public void AddVacationReplacementTimePayrollTransactionsWithTimeBlockDate(List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates, EmployeeChild employeeChild = null)
        {
            if (timePayrollTransactions == null)
                return;
            CreateContainer(EmployeeAbsenceCacheType.AbsenceByVacationReplacement, timePayrollTransactions, timeBlockDates, employeeChild: employeeChild);
        }

        public void ClearTimePayrollTransactionsWithTimeBlockDate(TimeBlockDate timeBlockDate)
        {
            //Clear day for all levels
            foreach (var container in this.containerCache)
            {
                container.ClearTimePayrollTransactions(timeBlockDate);
            }
        }

        #region Help-methods

        private List<TimePayrollTransaction> GetTimePayrollTransactions(EmployeeAbsenceCacheType type, List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached, int? sysPayrollTypeLevel3 = null, EmployeeChild employeeChild = null)
        {
            List<TimePayrollTransaction> timePayrollTransactions = null;
            timeBlockDatesNotCached = new List<TimeBlockDate>();

            var container = GetContainer(type, sysPayrollTypeLevel3);
            if (container == null)
            {
                if (!timeBlockDates.IsNullOrEmpty())
                    timeBlockDatesNotCached.AddRange(timeBlockDates);
            }
            else
            {
                timePayrollTransactions = container.GetTimePayrollTransactions(timeBlockDates, out timeBlockDatesNotCached, employeeChild: employeeChild);
            }

            return timePayrollTransactions;
        }

        private EmployeeAbsenceContainerCache GetContainer(EmployeeAbsenceCacheType type, int? sysPayrollTypeLevel3 = null)
        {
            return this.containerCache.FirstOrDefault(i => i.Type == type && (!sysPayrollTypeLevel3.HasValue || (i.SysPayrollTypeLevel3.HasValue && i.SysPayrollTypeLevel3.Value == sysPayrollTypeLevel3.Value)));
        }

        private void CreateContainer(EmployeeAbsenceCacheType type, List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates, int? sysPayrollTypeLevel3 = null, EmployeeChild employeeChild = null)
        {
            var container = GetContainer(type, sysPayrollTypeLevel3);
            if (container == null)
            {
                container = new EmployeeAbsenceContainerCache(type, timePayrollTransactions, timeBlockDates, sysPayrollTypeLevel3, employeeChild: employeeChild);
                this.containerCache.Add(container);
            }
            else
            {
                //Update container with transactions for new dates
                container.AddTimePayrollTransactions(timePayrollTransactions, timeBlockDates, employeeChild: employeeChild);
            }
        }

        #endregion

        #endregion
    }

    internal class EmployeeAbsenceContainerCache
    {
        #region Variables

        //Init params
        public EmployeeAbsenceCacheType Type { get; set; }
        public int? SysPayrollTypeLevel3 { get; }

        //Private params
        private readonly EmployeeAbsenceTransactionsCache transactionsCache;

        #endregion

        #region Ctor

        public EmployeeAbsenceContainerCache(EmployeeAbsenceCacheType type, List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates, int? sysPayrollTypeLevel3 = null, EmployeeChild employeeChild = null)
        {
            this.Type = type;
            this.SysPayrollTypeLevel3 = sysPayrollTypeLevel3;
            this.transactionsCache = new EmployeeAbsenceTransactionsCache(timePayrollTransactions, timeBlockDates, employeeChild: employeeChild);
        }

        #endregion

        #region Public methods

        public List<TimePayrollTransaction> GetTimePayrollTransactions(List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached, EmployeeChild employeeChild = null)
        {
            return this.transactionsCache.GetTimePayrollTransactions(timeBlockDates, out timeBlockDatesNotCached, employeeChild: employeeChild);
        }

        public void AddTimePayrollTransactions(List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates, EmployeeChild employeeChild = null)
        {
            this.transactionsCache.AddTimePayrollTransactions(timePayrollTransactions, timeBlockDates, employeeChild: employeeChild);
        }

        public void ClearTimePayrollTransactions(TimeBlockDate timeBlockDate)
        {
            this.transactionsCache.ClearTimePayrollTransactions(timeBlockDate);
        }

        #endregion
    }

    internal class EmployeeAbsenceTransactionsCache
    {
        #region Variables

        //Private params
        private readonly Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsDict;
        private readonly Dictionary<int, List<TimePayrollTransaction>> employeeChildTimePayrollTransactionsDict;
        private readonly Dictionary<int, List<TimeBlockDate>> employeeChildTimeBlockDatesToReload;

        #endregion

        #region Ctor

        public EmployeeAbsenceTransactionsCache(List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates = null, EmployeeChild employeeChild = null)
        {
            this.timePayrollTransactionsDict = new Dictionary<int, List<TimePayrollTransaction>>();
            this.employeeChildTimePayrollTransactionsDict = new Dictionary<int, List<TimePayrollTransaction>>();
            this.employeeChildTimeBlockDatesToReload = new Dictionary<int, List<TimeBlockDate>>();

            AddTimePayrollTransactions(timePayrollTransactions, timeBlockDates, employeeChild: employeeChild);
        }

        #endregion

        #region Public methods

        public List<TimePayrollTransaction> GetTimePayrollTransactions(List<TimeBlockDate> timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached, EmployeeChild employeeChild = null)
        {
            timeBlockDatesNotCached = new List<TimeBlockDate>();

            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();

            if (employeeChild != null)
            {
                if (this.employeeChildTimePayrollTransactionsDict.IsNullOrEmpty() || !employeeChildTimePayrollTransactionsDict.ContainsKey(employeeChild.EmployeeChildId))
                    return null;

                timePayrollTransactions.AddRange(employeeChildTimePayrollTransactionsDict[employeeChild.EmployeeChildId]);
                if (this.employeeChildTimeBlockDatesToReload.Any())
                {
                    timeBlockDatesNotCached.AddRange(employeeChildTimeBlockDatesToReload.SelectMany(i => i.Value));
                    employeeChildTimeBlockDatesToReload.Clear();
                }

                if (timeBlockDates != null)
                {
                    if (timeBlockDates.Any())
                    {
                        List<int> timeBlockDatesIds = timeBlockDates.Select(i => i.TimeBlockDateId).ToList();
                        timePayrollTransactions = timePayrollTransactions.Where(i => timeBlockDatesIds.Contains(i.TimeBlockDateId)).ToList();
                    }
                    else
                    {
                        return new List<TimePayrollTransaction>();
                    }
                }
            }
            else
            {
                if (this.timePayrollTransactionsDict.IsNullOrEmpty() || timeBlockDates.IsNullOrEmpty())
                    return null;

                foreach (TimeBlockDate timeBlockDate in timeBlockDates.OrderBy(i => i.Date))
                {
                    if (this.timePayrollTransactionsDict.ContainsKey(timeBlockDate.TimeBlockDateId))
                        timePayrollTransactions.AddRange(this.timePayrollTransactionsDict[timeBlockDate.TimeBlockDateId]);
                    else
                        timeBlockDatesNotCached.Add(timeBlockDate);
                }
            }

            return timePayrollTransactions;
        }

        public void AddTimePayrollTransactions(List<TimePayrollTransaction> timePayrollTransactions, List<TimeBlockDate> timeBlockDates = null, EmployeeChild employeeChild = null)
        {
            if (timePayrollTransactions == null)
                return;

            if (employeeChild != null)
            {
                if (this.employeeChildTimePayrollTransactionsDict.ContainsKey(employeeChild.EmployeeChildId))
                    this.employeeChildTimePayrollTransactionsDict[employeeChild.EmployeeChildId] = this.employeeChildTimePayrollTransactionsDict[employeeChild.EmployeeChildId].Concat(timePayrollTransactions).OrderBy(i => i.TimeBlockDate.Date).ToList();
                else
                    this.employeeChildTimePayrollTransactionsDict.Add(employeeChild.EmployeeChildId, timePayrollTransactions.OrderBy(i => i.TimeBlockDate.Date).ToList());
            }
            else
            {
                if (timeBlockDates != null)
                {
                    foreach (TimeBlockDate timeBlockDate in timeBlockDates.OrderBy(i => i.Date))
                    {
                        if (!this.timePayrollTransactionsDict.ContainsKey(timeBlockDate.TimeBlockDateId))
                            this.timePayrollTransactionsDict.Add(timeBlockDate.TimeBlockDateId, timePayrollTransactions.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList());
                    }
                }
                else
                {
                    foreach (var grouping in timePayrollTransactions.GroupBy(i => i.TimeBlockDateId))
                    {
                        if (!this.timePayrollTransactionsDict.ContainsKey(grouping.Key))
                            this.timePayrollTransactionsDict.Add(grouping.Key, grouping.ToList());
                    }
                }
            }
        }

        public void ClearTimePayrollTransactions(TimeBlockDate timeBlockDate)
        {
            if (timeBlockDate == null || timeBlockDate.TimeBlockDateId <= 0)
                return;

            //TimePayrollTransactions
            if (this.timePayrollTransactionsDict.ContainsKey(timeBlockDate.TimeBlockDateId))
                this.timePayrollTransactionsDict.Remove(timeBlockDate.TimeBlockDateId);

            //TimePayrollTransactions with EmployeeChild
            List<int> employeeChildIds = employeeChildTimePayrollTransactionsDict.Keys.ToList();
            foreach (int employeeChildId in employeeChildIds)
            {
                List<TimeBlockDate> timeBlockDatesToReload;
                if (employeeChildTimeBlockDatesToReload.ContainsKey(employeeChildId))
                {
                    timeBlockDatesToReload = employeeChildTimeBlockDatesToReload[employeeChildId];
                    if (timeBlockDatesToReload.Any(tbd => tbd.TimeBlockDateId == timeBlockDate.TimeBlockDateId))
                        continue;
                }
                else
                    timeBlockDatesToReload = new List<TimeBlockDate>();

                //Add to reload
                timeBlockDatesToReload.Add(timeBlockDate);
                employeeChildTimeBlockDatesToReload[employeeChildId] = timeBlockDatesToReload;

                //Remove from cached
                employeeChildTimePayrollTransactionsDict[employeeChildId] = employeeChildTimePayrollTransactionsDict[employeeChildId].Where(i => i.TimeBlockDateId != timeBlockDate.TimeBlockDateId).ToList();
            }
        }

        #endregion
    }

    #endregion
}
