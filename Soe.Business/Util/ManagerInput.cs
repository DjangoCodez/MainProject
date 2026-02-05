using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    #region ManagerInput

    public class ManagerInput
    {
        public int ActorCompanyId { get; }
        public int UserId { get; }
        public int RoleId { get; }
        public int EmployeeId { get; }

        private ManagerInputLoadRepository loadRepository;

        public ManagerInput(int actorCompanyId, int userId, int roleId, params InputLoadType[] loadTypes)
        {
            this.ActorCompanyId = actorCompanyId;
            this.UserId = userId;
            this.RoleId = roleId;

            this.Init(loadTypes);
        }

        public ManagerInput(int actorCompanyId, int userId, int roleId, int employeeId, params InputLoadType[] loadTypes)
        {
            this.ActorCompanyId = actorCompanyId;
            this.UserId = userId;
            this.RoleId = roleId;
            this.EmployeeId = employeeId;

            this.Init(loadTypes);
        }

        private void Init(params InputLoadType[] loadTypes)
        {
            this.loadRepository = new ManagerInputLoadRepository();
            foreach (InputLoadType loadType in loadTypes)
            {
                this.loadRepository.SetLoading(loadType, true);
            }
        }

        public bool DoLoad(InputLoadType type, bool loadAll = false)
        {
            if (loadAll)
                return true;
            return this.loadRepository.DoLoad(type);
        }

        public void SetLoading(InputLoadType key, bool value)
        {
            this.loadRepository.SetLoading(key, value);
        }
    }

    #endregion

    #region ManagerInputLoadRepository

    public class ManagerInputLoadRepository
    {
        public Dictionary<InputLoadType, bool> Loadings { get; }

        public ManagerInputLoadRepository()
        {
            this.Loadings = new Dictionary<InputLoadType, bool>();
        }

        public void AddLoadings(Dictionary<InputLoadType, bool> loadings)
        {
            foreach (var loading in loadings)
            {
                SetLoading(loading.Key, loading.Value);
            }
        }

        public void SetLoading(InputLoadType key, bool value)
        {
            if (key == InputLoadType.None)
                return;

            if (this.Loadings.ContainsKey(key))
                this.Loadings[key] = value;
            else
                this.Loadings.Add(key, value);
        }

        public bool DoLoad(InputLoadType type)
        {
            return this.Loadings.ContainsKey(type) && this.Loadings[type];
        }
    }

    #endregion

    #region GetAttestEmployeeInput

    public class GetAttestEmployeeInput : ManagerInput
    {
        //Mandatory
        public SoeAttestTreeMode Mode { get; }
        public bool IsModePayrollCalculation
        {
            get
            {
                return this.Mode == SoeAttestTreeMode.PayrollCalculation;
            }
        }
        public bool IsModeTimeAttest
        {
            get
            {
                return this.Mode == SoeAttestTreeMode.TimeAttest;
            }
        }
        public SoeAttestDevice Device { get; }
        public bool IsWeb
        {
            get
            {
                return this.Device == SoeAttestDevice.Web;
            }
        }
        public bool IsMobile
        {
            get
            {
                return this.Device == SoeAttestDevice.Mobile;
            }
        }
        public DateTime StartDate { get; private set; }
        public DateTime StopDate { get; private set; }
        public int? TimePeriodId { get; }

        //Optional
        public string CacheKeyToUse { get; private set; }
        public List<int> FilterAccountIds { get; private set; }
        public bool HasFilterAccountIds
        {
            get
            {
                return !this.FilterAccountIds.IsNullOrEmpty();
            }
        }
        public List<int> FilterShiftTypeIds { get; private set; }
        public bool HasFilterShiftTypeIds
        {
            get
            {
                return !this.FilterShiftTypeIds.IsNullOrEmpty();
            }
        }
        public bool IsScenario
        {
            get
            {
                return this.TimeScheduleScenarioHeadId.HasValue;
            }
        }
        public bool DoMergeTransactions { get; private set; }
        public bool DoGetHidden { get; private set; }
        public bool DoGetVacant { get; private set; }
        public bool DoGetOnlyActive { get; private set; }
        public bool DoGetSecondaryAccounts { get; private set; }
        public bool DoGetOnDuty { get; private set; }
        public bool DoCalculateEmploymentTaxAndSupplementChargeCost { get; private set; }
        public bool DoNotShowDaysOutsideEmployeeAccount { get; private set; }
        public int? TimeScheduleScenarioHeadId { get; private set; }
        public bool ValidateEmployee { get; private set; }
        public Employee EmployeeForUser { get; private set; }
        public Employee Employee { get; private set; }
        public List<TimeBlockDate> TimeBlockDatesForEmployee { get; private set; }
        public List<CompanyCategoryRecord> CategoryRecordsForEmployee { get; private set; }
        public List<HolidayDTO> Holidays { get; private set; }
        public List<AccountDimDTO> AccountDims { get; private set; }
        public List<EmployeeGroup> EmployeeGroups { get; private set; }
        public List<PayrollGroup> PayrollGroups { get; private set; }
        public List<PayrollPriceType> PayrollPriceTypes { get; private set; }
        public TermGroup_EmployeeSelectionAccountingType EmployeeSelectionAccountingType { get; private set; }
        public List<AccountInternalDTO> SelectionValidAccountInternals { get; private set; }
        public bool IncludeDeviationOnZeroDayFromScenario { get; private set; }
        public bool MobileMyTime { get; private set; }
        public bool HasDayFilter { get; private set; }

        private GetAttestEmployeeInput(SoeAttestDevice device, SoeAttestTreeMode mode, int actorCompanyId, int userId, int roleId, int employeeId, DateTime startDate, DateTime stopDate, int? timePeriodId, params InputLoadType[] loadTypes) : base(actorCompanyId, userId, roleId, employeeId, loadTypes)
        {
            this.Device = device;
            this.Mode = mode;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.TimePeriodId = timePeriodId;

            this.Init();
        }

        private static GetAttestEmployeeInput CreateInputForWeb(SoeAttestTreeMode mode, int actorCompanyId, int userId, int roleId, int employeeId, DateTime startDate, DateTime stopDate, int? timePeriodId = null, string cacheKeyToUse = null, params InputLoadType[] loadTypes)
        {
            GetAttestEmployeeInput input = new GetAttestEmployeeInput(SoeAttestDevice.Web, mode, actorCompanyId, userId, roleId, employeeId, startDate, stopDate, timePeriodId, loadTypes)
            {
                CacheKeyToUse = cacheKeyToUse
            };
            return input;
        }
        public static GetAttestEmployeeInput CreateAttestInputForWeb(int actorCompanyId, int userId, int roleId, int employeeId, DateTime startDate, DateTime stopDate, int? timePeriodId = null, string cacheKeyToUse = null, params InputLoadType[] loadTypes)
        {
            return CreateInputForWeb(SoeAttestTreeMode.TimeAttest, actorCompanyId, userId, roleId, employeeId, startDate, stopDate, timePeriodId, cacheKeyToUse, loadTypes);
        }

        public static GetAttestEmployeeInput CreatePayrollInputForWeb(int actorCompanyId, int userId, int roleId, int employeeId, DateTime startDate, DateTime stopDate, int? timePeriodId = null, string cacheKeyToUse = null, params InputLoadType[] loadTypes)
        {
            return CreateInputForWeb(SoeAttestTreeMode.PayrollCalculation, actorCompanyId, userId, roleId, employeeId, startDate, stopDate, timePeriodId, cacheKeyToUse, loadTypes);
        }

        public static GetAttestEmployeeInput CreateInputForMobile(int actorCompanyId, int userId, int roleId, int employeeId, DateTime startDate, DateTime stopDate, int? timePeriodId = null, bool mobileMyTime = false)
        {
            GetAttestEmployeeInput input = new GetAttestEmployeeInput(SoeAttestDevice.Mobile, SoeAttestTreeMode.TimeAttest, actorCompanyId, userId, roleId, employeeId, startDate, stopDate, timePeriodId);
            input.SetLoading(InputLoadType.TimeStamps, true);
            input.SetOptionalParameters(mobileMyTime: mobileMyTime);
            return input;
        }

        public void SetOptionalParameters(
            List<HolidayDTO> holidays = null,
            List<AccountDimDTO> accountDims = null,
            List<EmployeeGroup> employeeGroups = null,
            List<PayrollGroup> payrollGroups = null,
            List<PayrollPriceType> payrollPriceTypes = null,
            Employee employee = null,
            Employee employeeForUser = null,
            List<TimeBlockDate> timeBlockDatesForEmployee = null,
            List<CompanyCategoryRecord> categoryRecordsForEmployee = null,
            bool doMergeTransactions = false,
            bool doGetOnlyActive = false,
            bool doGetHidden = true,
            bool doCalculateEmploymentTaxAndSupplementChargeCost = false,
            bool doNotShowDaysOutsideEmployeeAccount = false,
            bool validateEmployee = false,
            List<int> filterAccountIds = null,
            List<int> filterShiftTypeIds = null,
            int? timeScheduleScenarioHeadId = null,
            List<AccountInternalDTO> validAccountInternals = null,
            TermGroup_EmployeeSelectionAccountingType employeeSelectionAccountingType = TermGroup_EmployeeSelectionAccountingType.EmployeeCategory,
            bool doGetOnDuty = false,
            bool includeDeviationOnZeroDayFromScenario = false,
            bool doGetSecondaryAccounts = false,
            bool mobileMyTime = false,
            bool hasDayFilter = false
            )
        {
            if (holidays != null)
                this.Holidays = holidays;
            if (accountDims != null)
                this.AccountDims = accountDims;
            if (employeeGroups != null)
                this.EmployeeGroups = employeeGroups;
            if (payrollGroups != null)
                this.PayrollGroups = payrollGroups;
            if (payrollPriceTypes != null)
                this.PayrollPriceTypes = payrollPriceTypes;
            if (employee != null)
                this.Employee = employee;
            if (employeeForUser != null)
                this.EmployeeForUser = employeeForUser;
            if (timeBlockDatesForEmployee != null)
                this.TimeBlockDatesForEmployee = timeBlockDatesForEmployee;
            if (categoryRecordsForEmployee != null)
                this.CategoryRecordsForEmployee = categoryRecordsForEmployee;
            if (doMergeTransactions)
                this.DoMergeTransactions = doMergeTransactions;
            if (doGetOnlyActive)
                this.DoGetOnlyActive = doGetOnlyActive;
            if (doGetHidden)
                this.DoGetHidden = doGetHidden;
            if (doCalculateEmploymentTaxAndSupplementChargeCost)
                this.DoCalculateEmploymentTaxAndSupplementChargeCost = doCalculateEmploymentTaxAndSupplementChargeCost;
            if (doNotShowDaysOutsideEmployeeAccount)
                this.DoNotShowDaysOutsideEmployeeAccount = doNotShowDaysOutsideEmployeeAccount;
            if (validateEmployee)
                this.ValidateEmployee = validateEmployee;
            if (filterAccountIds != null)
                this.FilterAccountIds = filterAccountIds;
            if (filterShiftTypeIds != null)
                this.FilterShiftTypeIds = filterShiftTypeIds;
            if (timeScheduleScenarioHeadId.HasValue)
                this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.Value;
            if (validAccountInternals != null)
                this.SelectionValidAccountInternals = validAccountInternals;
            if (doGetOnDuty)
                this.DoGetOnDuty = doGetOnDuty;
            if (includeDeviationOnZeroDayFromScenario)
                this.IncludeDeviationOnZeroDayFromScenario = includeDeviationOnZeroDayFromScenario;
            if (doGetSecondaryAccounts)
                this.DoGetSecondaryAccounts = doGetSecondaryAccounts;
            if (mobileMyTime)
                this.MobileMyTime = mobileMyTime;
            if (hasDayFilter)
                this.HasDayFilter = hasDayFilter;

            this.EmployeeSelectionAccountingType = employeeSelectionAccountingType;
        }

        private void Init()
        {
            //Default values
            this.FilterAccountIds = null;
            this.FilterShiftTypeIds = null;
            this.Holidays = null;
            this.AccountDims = null;
            this.DoMergeTransactions = false;
            this.DoGetHidden = true;
            this.DoGetVacant = false;
            this.DoGetOnlyActive = true;
            this.DoGetOnDuty = false;
            this.DoCalculateEmploymentTaxAndSupplementChargeCost = false;
            this.DoNotShowDaysOutsideEmployeeAccount = false;
            this.ValidateEmployee = false;
            this.IncludeDeviationOnZeroDayFromScenario = false;
        }

        public void UpdateDates(DateTime startDate, DateTime stopDate)
        {
            if (startDate <= stopDate)
            {
                this.StartDate = startDate;
                this.StopDate = stopDate;
            }
        }

        public void SetHolidays(List<HolidayDTO> holidays)
        {
            this.Holidays = holidays;
        }

        public void SetAccountDims(List<AccountDimDTO> accountDims)
        {
            this.AccountDims = accountDims;
        }

        public void ResetDoNotShowDaysOutsideEmployeeAccount()
        {
            this.DoNotShowDaysOutsideEmployeeAccount = false;
        }

        public void CalculateLoadingsForGrid(List<AgGridColumnSettingDTO> columnSettings, bool includeDaytype, bool includeTemplateSchedule, bool includeSchedule, bool includeShifts, bool includeTimeStamps, bool includeTimeBlocks, bool includeProjectTimeBlocks, bool includeTimeCodeTransactions, bool includeTimeInvoiceTransactions, bool includeTimePayrollTransactions, bool includeSums, bool includeAttestStates)
        {
            List<InputLoadType> loadings = new List<InputLoadType>();

            bool hasSettings = !columnSettings.IsNullOrEmpty();
            bool loadSums = !hasSettings;
            bool loadTemplateSchedule = !hasSettings;
            bool loadSchedule = !hasSettings;
            bool loadAttestState = !hasSettings;

            if (hasSettings)
            {
                var columns = Enum.GetValues(typeof(AgGridTimeAttestEmployee)).Cast<AgGridTimeAttestEmployee>().ToDictionary(k => (int)k, v => v.ToString());

                foreach (AgGridColumnSettingDTO columnSetting in columnSettings.Where(c => !c.IsSystemColumn()))
                {
                    AgGridTimeAttestEmployee column = (AgGridTimeAttestEmployee)columns.FirstOrDefault(c => c.Value == columnSetting.colId).Key;
                    switch (column)
                    {
                        case AgGridTimeAttestEmployee.day:
                        case AgGridTimeAttestEmployee.date:
                        case AgGridTimeAttestEmployee.dayName:
                        case AgGridTimeAttestEmployee.weekNr:
                            break;
                        case AgGridTimeAttestEmployee.attestStateColor:
                        case AgGridTimeAttestEmployee.attestStateName:
                            if (!columnSetting.hide)
                                loadAttestState = true;
                            break;
                        case AgGridTimeAttestEmployee.workedInsideScheduleColor:
                        case AgGridTimeAttestEmployee.workedOutsideScheduleColor:
                        case AgGridTimeAttestEmployee.absenceTimeColor:
                        case AgGridTimeAttestEmployee.standbyTimeColor:
                            //Cannot hide
                            break;
                        case AgGridTimeAttestEmployee.templateScheduleStartTime:
                        case AgGridTimeAttestEmployee.templateScheduleStopTime:
                        case AgGridTimeAttestEmployee.templateScheduleTime:
                        case AgGridTimeAttestEmployee.templateScheduleBreakTime:
                            if (!columnSetting.hide)
                                loadTemplateSchedule = true;
                            break;
                        case AgGridTimeAttestEmployee.scheduleStartTime:
                        case AgGridTimeAttestEmployee.scheduleStopTime:
                        case AgGridTimeAttestEmployee.scheduleTime:
                        case AgGridTimeAttestEmployee.scheduleBreakTime:
                        case AgGridTimeAttestEmployee.isPreliminary:
                            if (!columnSetting.hide)
                                loadSchedule = true;
                            break;
                        case AgGridTimeAttestEmployee.sumExpenseRows:
                        case AgGridTimeAttestEmployee.sumExpenseAmount:
                        case AgGridTimeAttestEmployee.sumTimeWorkedScheduledTime:
                        case AgGridTimeAttestEmployee.sumTimeAccumulator:
                        case AgGridTimeAttestEmployee.sumTimeAccumulatorOverTime:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsence:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsenceText:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsenceVacation:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsenceSick:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsenceLeaveOfAbsence:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsenceParentalLeave:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAbsenceTemporaryParentalLeave:
                        case AgGridTimeAttestEmployee.sumGrossSalaryWeekendSalary:
                        case AgGridTimeAttestEmployee.sumGrossSalaryDuty:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAdditionalTime:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAdditionalTime35:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAdditionalTime70:
                        case AgGridTimeAttestEmployee.sumGrossSalaryAdditionalTime100:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition40:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition50:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition57:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition70:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition79:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition100:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOBAddition113:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOvertime:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOvertime35:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOvertime50:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOvertime70:
                        case AgGridTimeAttestEmployee.sumGrossSalaryOvertime100:
                            if (!columnSetting.hide)
                                loadSums = true;
                            break;
                        case AgGridTimeAttestEmployee.informations:
                        case AgGridTimeAttestEmployee.warnings:
                        case AgGridTimeAttestEmployee.comment:
                            //Cannot hide
                            break;
                    }
                }
            }

            loadings.Add(InputLoadType.UnhandledShiftChanges);
            if (includeDaytype)
                loadings.Add(InputLoadType.DayType);
            if (includeTemplateSchedule && loadTemplateSchedule)
                loadings.Add(InputLoadType.TemplateSchedule);
            if (includeSums && loadSums)
                loadings.Add(InputLoadType.SumsAll);
            if (includeSchedule && loadSchedule)
                loadings.Add(InputLoadType.Schedule);
            if (includeShifts)
                loadings.Add(InputLoadType.Shifts);
            if (includeTimeStamps)
                loadings.Add(InputLoadType.TimeStamps);
            if (includeTimeBlocks) //Cannot exlude TimeBlocks even if user disabled those columns. Because expand day wont work then.
                loadings.Add(InputLoadType.TimeBlocks);
            if (includeProjectTimeBlocks)
                loadings.Add(InputLoadType.ProjectTimeBlocks);
            if (includeTimeCodeTransactions)
                loadings.Add(InputLoadType.TimeCodeTransactions);
            if (includeTimeInvoiceTransactions)
                loadings.Add(InputLoadType.TimeInvoiceTransactions);
            if (includeTimePayrollTransactions)
                loadings.Add(InputLoadType.TimePayrollTransactions);
            if (includeAttestStates && loadAttestState)
                loadings.Add(InputLoadType.AttestState);

            foreach (InputLoadType loading in loadings)
            {
                this.SetLoading(loading, true);
            }
        }
    }

    #endregion

    #region GetAttestEmployeePeriodsInput

    public class GetAttestEmployeePeriodsInput : ManagerInput
    {
        //Mandatory
        public SoeAttestDevice Device { get; }
        public bool IsWeb
        {
            get
            {
                return this.Device == SoeAttestDevice.Web;
            }
        }
        public bool IsMobile
        {
            get
            {
                return this.Device == SoeAttestDevice.Mobile;
            }
        }
        public DateTime StartDate { get; }
        public DateTime StopDate { get; }
        public List<int> FilterEmployeeIds { get; }
        public bool IsAdditional { get; set; }
        public bool IncludeAdditionalEmployees { get; set; }
        public bool DoNotShowDaysOutsideEmployeeAccount { get; set; }

        //Optional
        public string CacheKeyToUse { get; private set; }
        public int? TimePeriodId { get; private set; }
        public TermGroup_AttestTreeGrouping Grouping { get; private set; }
        public int GroupId { get; private set; }

        private GetAttestEmployeePeriodsInput(SoeAttestDevice device, int actorCompanyId, int userId, DateTime startDate, DateTime stopDate, List<int> filterEmployeeIds, params InputLoadType[] loadTypes) : base(actorCompanyId, userId, 0, 0, loadTypes)
        {
            this.Device = device;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.FilterEmployeeIds = filterEmployeeIds;
        }

        public static GetAttestEmployeePeriodsInput CreateInputForWeb(int actorCompanyId, int userId, DateTime startDate, DateTime stopDate, int? timePeriodId, TermGroup_AttestTreeGrouping grouping, int groupId, List<int> filterEmployeeIds = null, bool isAdditional = false, bool includeAdditionalEmployees = false, bool doNotShowDaysOutsideEmployeeAccount = false, string cachekeyToUse = null, params InputLoadType[] loadTypes)
        {
            GetAttestEmployeePeriodsInput input = new GetAttestEmployeePeriodsInput(SoeAttestDevice.Web, actorCompanyId, userId, startDate, stopDate, filterEmployeeIds, loadTypes);
            input.TimePeriodId = timePeriodId;
            input.IsAdditional = isAdditional;
            input.IncludeAdditionalEmployees = includeAdditionalEmployees;
            input.DoNotShowDaysOutsideEmployeeAccount = doNotShowDaysOutsideEmployeeAccount;
            input.Grouping = grouping;
            input.GroupId = groupId;
            input.CacheKeyToUse = cachekeyToUse;
            return input;
        }

        public static GetAttestEmployeePeriodsInput CreateInputForMobile(int actorCompanyId, int userId, DateTime startDate, DateTime stopDate, List<int> filterEmployeeIds = null, bool includeAdditionalEmployees = false)
        {
            GetAttestEmployeePeriodsInput input = new GetAttestEmployeePeriodsInput(SoeAttestDevice.Mobile, actorCompanyId, userId, startDate, stopDate, filterEmployeeIds);
            input.IncludeAdditionalEmployees = includeAdditionalEmployees;
            input.Grouping = TermGroup_AttestTreeGrouping.All;
            input.GroupId = 0;
            return input;
        }

        public void CalculateLoadingsForGrid(List<AgGridColumnSettingDTO> columnSettings, bool includeSchedule, bool includeTimeBlocks, bool includeSums, bool includeAttestStates)
        {
            List<InputLoadType> loadings = new List<InputLoadType>();

            bool hasSettings = !columnSettings.IsNullOrEmpty();
            bool loadSums = !hasSettings;
            bool loadSchedule = !hasSettings;
            bool loadTimeBlocks = !hasSettings;
            bool loadAttestState = !hasSettings;

            if (hasSettings)
            {
                var columns = Enum.GetValues(typeof(AgGridTimeAttestGroup)).Cast<AgGridTimeAttestGroup>().ToDictionary(k => (int)k, v => v.ToString());

                foreach (AgGridColumnSettingDTO columnSetting in columnSettings.Where(c => !c.IsSystemColumn()))
                {
                    AgGridTimeAttestGroup column = (AgGridTimeAttestGroup)columns.FirstOrDefault(c => c.Value == columnSetting.colId).Key;
                    switch (column)
                    {
                        case AgGridTimeAttestGroup.employeeNrAndName:
                        case AgGridTimeAttestGroup.view:
                        case AgGridTimeAttestGroup.info:
                            //Cannot hide
                            break;
                        case AgGridTimeAttestGroup.attestStateColor:
                        case AgGridTimeAttestGroup.attestStateName:
                            if (!columnSetting.hide)
                                loadAttestState = true;
                            break;
                        case AgGridTimeAttestGroup.scheduleDays:
                        case AgGridTimeAttestGroup.scheduleTimeInfo:
                        case AgGridTimeAttestGroup.scheduleBreakTimeInfo:
                            if (!columnSetting.hide)
                                loadSchedule = true;
                            break;
                        case AgGridTimeAttestGroup.presenceDays:
                        case AgGridTimeAttestGroup.presenceTimeInfo:
                        case AgGridTimeAttestGroup.presenceBreakTimeInfo:
                            if (!columnSetting.hide)
                                loadTimeBlocks = true;
                            break;
                        case AgGridTimeAttestGroup.sumExpenseRows:
                        case AgGridTimeAttestGroup.sumExpenseAmount:
                        case AgGridTimeAttestGroup.sumTimeWorkedScheduledTimeText:
                        case AgGridTimeAttestGroup.sumTimeAccumulatorText:
                        case AgGridTimeAttestGroup.sumTimeAccumulatorOverTimeText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAbsenceText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAbsenceVacationText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAbsenceSickText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAbsenceLeaveOfAbsenceText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAbsenceParentalLeaveText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAbsenceTemporaryParentalLeaveText:
                        case AgGridTimeAttestGroup.sumGrossSalaryWeekendSalaryText:
                        case AgGridTimeAttestGroup.sumGrossSalaryDutyText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAdditionalTimeText:
                        case AgGridTimeAttestGroup.sumGrossSalaryAdditionalTime35Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryAdditionalTime70Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryAdditionalTime100Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOBAdditionText:
                        case AgGridTimeAttestGroup.sumGrossSalaryOBAddition50Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOBAddition70Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOBAddition100Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOvertimeText:
                        case AgGridTimeAttestGroup.sumGrossSalaryOvertime35Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOvertime50Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOvertime70Text:
                        case AgGridTimeAttestGroup.sumGrossSalaryOvertime100Text:
                        case AgGridTimeAttestGroup.sumInvoicedTimeText:
                            if (!columnSetting.hide)
                                loadSums = true;
                            break;
                    }
                }
            }
            if (includeSums && loadSums)
                loadings.Add(InputLoadType.SumsAll);
            if (includeSchedule && loadSchedule)
                loadings.Add(InputLoadType.Shifts);
            if (includeTimeBlocks && loadTimeBlocks)
                loadings.Add(InputLoadType.TimeBlocks);
            if (includeAttestStates && loadAttestState)
                loadings.Add(InputLoadType.AttestState);
            loadings.Add(InputLoadType.UnhandledShiftChanges);

            foreach (InputLoadType loading in loadings)
            {
                this.SetLoading(loading, true);
            }
        }
    }

    #endregion

    #region GetTimeAccumulatorItemsInput

    public class GetTimeAccumulatorItemsInput : ManagerInput
    {
        public DateTime StartDate { get; private set; }
        public DateTime StopDate { get; private set; }
        public bool AddSourceIds { get; private set; }
        public bool CalculateDay { get; private set; }
        public bool CalculatePeriod { get; private set; }
        public bool CalculatePlanningPeriod { get; private set; }
        public bool CalculateYear { get; private set; }
        public bool OverrideDateOnYear { get; private set; }
        public bool CalculateAccToday { get; private set; }
        public bool CalculateAccumulatedValue { get; private set; }
        public bool CalculateRange { get; set; }
        public bool CalculateRangeValue { get; set; }        
        public bool IncludeBalanceYear { get; private set; }
        public List<Employee> Employees { get; private set; }
        public List<int> TimeAccumulatorIds { get; private set; }

        public bool CalculateOnlyPlanningPeriod
        {
            get
            {
                return this.CalculatePlanningPeriod && !this.CalculateDay && !this.CalculatePeriod && !this.CalculateYear && !this.CalculateAccToday;
            }
        }


        private GetTimeAccumulatorItemsInput(GetTimeAccumulatorItemsInput input, int employeeId) : base(input.ActorCompanyId, input.UserId, 0, employeeId)
        {
            this.StartDate = input.StartDate;
            this.StopDate = input.StopDate;
            this.AddSourceIds = input.AddSourceIds;
            this.CalculateDay = input.CalculateDay;
            this.CalculatePeriod = input.CalculatePeriod;
            this.CalculatePlanningPeriod = input.CalculatePlanningPeriod;
            this.CalculateYear = input.CalculateYear;
            this.CalculateAccToday = input.CalculateAccToday;
            this.CalculateAccumulatedValue = input.CalculateAccumulatedValue;
            this.CalculateRange = input.CalculateRange;
            this.CalculateRangeValue = input.CalculateRangeValue;
            this.IncludeBalanceYear = input.IncludeBalanceYear;
            this.Employees = input.Employees;
            this.TimeAccumulatorIds = input.TimeAccumulatorIds;
            this.OverrideDateOnYear = input.OverrideDateOnYear;
        }

        private GetTimeAccumulatorItemsInput(int actorCompanyId, int userId, int employeeId, DateTime startDate, DateTime stopDate, bool addSourceIds, bool calculateDay, bool calculatePeriod, bool calculatePlanningPeriod, bool calculateYear, bool calculateAccToday, bool calculateAccTodayValue, bool calculateRange, bool calculateRangeValue, bool includeBalanceYear, List<Employee> employees = null, List<int> timeAccumulatorIds = null, bool overrideDateOnYear = false) : base(actorCompanyId, userId, 0, employeeId)
        {
            this.StartDate = CalendarUtility.GetBeginningOfDay(startDate);
            this.StopDate = CalendarUtility.GetEndOfDay(stopDate);
            this.AddSourceIds = addSourceIds;
            this.CalculateDay = calculateDay;
            this.CalculatePeriod = calculatePeriod;
            this.CalculatePlanningPeriod = calculatePlanningPeriod;
            this.CalculateYear = calculateYear;
            this.CalculateAccToday = calculateAccToday;
            this.CalculateAccumulatedValue = calculateAccToday && calculateAccTodayValue;
            this.CalculateRange = calculateRange;
            this.CalculateRangeValue = calculateRangeValue;
            this.IncludeBalanceYear = includeBalanceYear;
            this.Employees = employees;
            this.TimeAccumulatorIds = timeAccumulatorIds;
            this.OverrideDateOnYear = overrideDateOnYear;
        }

        public static GetTimeAccumulatorItemsInput CreateInput(int actorCompanyId, int userId, int employeeId, DateTime startDate, DateTime stopDate, bool addSourceIds = false, bool calculateDay = false, bool calculatePeriod = false, bool calculatePlanningPeriod = false, bool calculateYear = false, bool calculateAccToday = false, bool calculateAccTodayValue = false, bool calculateRange = false, bool calculateRangeValue = false, bool includeBalanceYear = false, List<Employee> employees = null, List<int> timeAccumulatorIds = null, bool overrideDateOnYear = false)
        {
            return new GetTimeAccumulatorItemsInput(actorCompanyId, userId, employeeId, startDate, stopDate, addSourceIds, calculateDay, calculatePeriod, calculatePlanningPeriod, calculateYear, calculateAccToday, calculateAccTodayValue, calculateRange, calculateRangeValue, includeBalanceYear, employees, timeAccumulatorIds, overrideDateOnYear);
        }

        public static GetTimeAccumulatorItemsInput CreateInput(GetTimeAccumulatorItemsInput input, int employeeId)
        {
            return new GetTimeAccumulatorItemsInput(input, employeeId);
        }

        public bool CalculateAny()
        {
            return
                this.CalculateDay ||
                this.CalculatePeriod ||
                this.CalculatePlanningPeriod ||
                this.CalculateYear ||
                this.CalculateAccToday;
        }
    }

    #endregion

    #region GetTimeAbsenceRulesInput

    public class GetTimeAbsenceRulesInput
    {
        public int ActorCompanyId { get; set; }
        public int? TimeAbsenceRuleHeadId { get; set; }
        public bool LoadCompany { get; set; }
        public bool LoadTimeCode { get; set; }
        public bool LoadEmployeeGroups { get; set; }
        public bool LoadRows { get; set; }
        public bool LoadRowProducts { get; set; }

        public GetTimeAbsenceRulesInput(int actorCompanyId, int? timeAbsenceRuleHeadId = null)
        {
            this.ActorCompanyId = actorCompanyId;
            this.TimeAbsenceRuleHeadId = timeAbsenceRuleHeadId;
        }

        public bool LoadAnyRelation()
        {
            return
                this.LoadCompany ||
                this.LoadTimeCode ||
                this.LoadEmployeeGroups ||
                this.LoadRows ||
                this.LoadRowProducts;
        }
    }

    #endregion
}