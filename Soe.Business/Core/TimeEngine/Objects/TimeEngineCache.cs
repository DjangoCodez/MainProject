using SoftOne.Soe.Business.Core.TimeEngine.Cache;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Global flags

        private bool globalDoNotRecalculateAmounts = false;
        private void SetDoNotCalculateAmounts(bool value)
        {
            this.globalDoNotRecalculateAmounts = value;
        }
        private bool DoNotCalculateAmounts()
        {
            return this.globalDoNotRecalculateAmounts;
        }
        private bool globalDoNotTryRestoreUnhandledShiftsChanges = false;

        #endregion

        #region Current cache

        #region Shift changes

        private List<(int EmployeeId, DateTime Date, bool Extra)> currentHasUnhandledShiftChanges = null;
        private void SetCurrentHasShiftUnhandledChanges(int? employeeId, DateTime? date, bool extra = false)
        {
            if (!employeeId.HasValue || employeeId.Value == 0 || !date.HasValue)
                return;

            if (this.currentHasUnhandledShiftChanges == null)
                this.currentHasUnhandledShiftChanges = new List<(int EmployeeId, DateTime Date, bool Extra)>();
            if (!this.currentHasUnhandledShiftChanges.Any(s => s.EmployeeId == employeeId && s.Date == date.Value.Date))
                this.currentHasUnhandledShiftChanges.Add((employeeId.Value, date.Value.Date, extra));
        }

        #endregion

        #region Days attested

        private List<TimeBlockDate> currentTimeBlockDatesAttested = null;
        private List<TimeBlockDate> GetCurrentAttestedDays(bool excludeFutureDays = true)
        {
            if (this.currentTimeBlockDatesAttested.IsNullOrEmpty())
                return new List<TimeBlockDate>();

            if (excludeFutureDays)
                return this.currentTimeBlockDatesAttested.Where(date => date.Date <= DateTime.Today).ToList();
            else
                return this.currentTimeBlockDatesAttested;

        }
        private void AddCurrentDayAsAttested(AttestTransitionDTO attestTransition, TimeBlockDate timeBlockDate)
        {
            if (this.currentTimeBlockDatesAttested?.Any(i => i.EmployeeId == timeBlockDate.EmployeeId && i.Date == timeBlockDate.Date) ?? false)
                return;
            if (!DoCollectDayAsAttested(timeBlockDate, attestTransition))
                return;

            if (this.currentTimeBlockDatesAttested == null)
                this.currentTimeBlockDatesAttested = new List<TimeBlockDate>();
            this.currentTimeBlockDatesAttested.Add(timeBlockDate);
        }
        private void ClearCurrentAttestedDays()
        {
            this.currentTimeBlockDatesAttested = null;
        }

        #endregion

        #region Days modified - NotifyChangeOfDeviations

        private List<TimeBlockDate> currentNotifyChangeOfDeviations = null;
        public List<TimeBlockDate> GetCurrentNotifyChangeOfDeviationsDays(bool excludeFutureDays = true)
        {
            if (this.currentNotifyChangeOfDeviations.IsNullOrEmpty())
                return new List<TimeBlockDate>();

            if (excludeFutureDays)
                return this.currentNotifyChangeOfDeviations.Where(date => date.Date <= DateTime.Today).ToList();
            else
                return this.currentNotifyChangeOfDeviations;
        }
        public void AddCurrentDayNotifyChangeOfDeviations(TimeBlockDate timeBlockDate, EmployeeGroup employeeGroup)
        {
            if (this.currentNotifyChangeOfDeviations?.Any(i => i.EmployeeId == timeBlockDate.EmployeeId && i.Date == timeBlockDate.Date) ?? false)
                return;
            if (!DoCollectDayAsNotifyChangeOfDeviations(timeBlockDate, employeeGroup))
                return;

            if (this.currentNotifyChangeOfDeviations == null)
                this.currentNotifyChangeOfDeviations = new List<TimeBlockDate>();
            this.currentNotifyChangeOfDeviations.Add(timeBlockDate);
        }
        public void ClearCurrentNotifyChangeOfDeviationsDays()
        {
            this.currentNotifyChangeOfDeviations = null;
        }

        #endregion

        #region Days modified - payroll warning

        private List<TimeBlockDate> currentPayrollWarningDays = null;
        public List<TimeBlockDate> GetCurrentPayrollWarningDays()
        {
            if (this.currentPayrollWarningDays.IsNullOrEmpty())
                return new List<TimeBlockDate>();

            return this.currentPayrollWarningDays;
        }
        public void AddCurrentDaysPayrollWarning(List<TimeBlockDate> timeBlockDates)
        {
            foreach (var timeBlockDate in timeBlockDates)
            {
                AddCurrentDayPayrollWarning(timeBlockDate);
            }
        }
        public void AddCurrentDayPayrollWarning(TimeBlockDate timeBlockDate)
        {
            if (this.currentPayrollWarningDays?.Any(i => i.EmployeeId == timeBlockDate.EmployeeId && i.Date == timeBlockDate.Date) ?? false)
                return;
            if (!DoCollectDaysPayrollWarning(timeBlockDate))
                return;

            if (this.currentPayrollWarningDays == null)
                this.currentPayrollWarningDays = new List<TimeBlockDate>();
            this.currentPayrollWarningDays.Add(timeBlockDate);
        }
        public void ClearCurrentPayrollWarningDays()
        {
            this.currentPayrollWarningDays = null;
        }

        #endregion

        #region Days unlocked

        private Dictionary<int, List<DateTime>> userDaysUnlocked = null;
        private Dictionary<int, List<DateTime>> GetCurrentUnlockedDays()
        {
            return this.userDaysUnlocked ?? new Dictionary<int, List<DateTime>>();
        }
        private void AddCurrentDayAsUnlocked(int userId, DateTime date)
        {
            if (this.userDaysUnlocked?.Any(i => i.Key == userId && i.Value.Contains(date)) ?? false)
                return;

            if (this.userDaysUnlocked == null)
                this.userDaysUnlocked = new Dictionary<int, List<DateTime>>();
            if (!this.userDaysUnlocked.ContainsKey(userId))
                this.userDaysUnlocked.Add(userId, new List<DateTime>());
            if (!this.userDaysUnlocked[userId].Contains(date))
                this.userDaysUnlocked[userId].Add(date);
        }
        private void ClearCurrentUnlockedDays()
        {
            this.userDaysUnlocked = null;
        }

        #endregion

        #region Restore to schedule

        private readonly List<TimeEngineRestoreDay> currentDaysToRestoreToSchedule = new List<TimeEngineRestoreDay>();
        private void SetDayToBeRestoredToSchedule(int? employeeId, DateTime? date, int? templatePeriodId, bool cleanDeviationCauseOnSchedule = false)
        {
            if (!employeeId.HasValue || employeeId.Value == 0 || !date.HasValue)
                return;
            this.currentDaysToRestoreToSchedule.AddDay(employeeId.Value, templatePeriodId, date.Value, cleanDeviationCauseOnSchedule);
        }
        private void SetDayToBeRestoredToSchedule(int employeeId, DateTime date, List<TimeScheduleTemplateBlock> scheduleBlocks, bool cleanDeviationCauseOnSchedule = false)
        {
            foreach (var scheduleBlocksByTemplatePeriod in scheduleBlocks.Where(i => i.Date == date).GroupBy(x => x.TimeScheduleTemplatePeriodId))
            {
                SetDayToBeRestoredToSchedule(employeeId, date, scheduleBlocksByTemplatePeriod.Key, cleanDeviationCauseOnSchedule);
            }
        }

        #endregion

        #region TimeScheduleEmployeePeriod

        private List<TimeScheduleEmployeePeriod> currentEmployeePeriodsCreated = null;
        private List<TimeScheduleEmployeePeriod> CurrentCreatedEmployeePeriods
        {
            get
            {
                return this.currentEmployeePeriodsCreated ?? new List<TimeScheduleEmployeePeriod>();
            }
        }
        private void AddCurrentEmployeePeriodsCreated(TimeScheduleEmployeePeriod employeePeriod)
        {
            if (employeePeriod == null)
                return;

            if (this.currentEmployeePeriodsCreated == null)
                this.currentEmployeePeriodsCreated = new List<TimeScheduleEmployeePeriod>();
            this.currentEmployeePeriodsCreated.Add(employeePeriod);
        }

        #endregion

        #region XEMail

        private List<Tuple<int, DateTime, TermGroup_TimeScheduleTemplateBlockType, bool, bool>> currentSendXEMailEmployeeDates = null;
        private void AddCurrentSendXEMailEmployeeDate(int? employeeId, DateTime? date, TermGroup_TimeScheduleTemplateBlockType type, bool wantedShift = false, bool unWantedShift = false)
        {
            if (!employeeId.HasValue || !date.HasValue)
                return;

            if (this.currentSendXEMailEmployeeDates == null)
                this.currentSendXEMailEmployeeDates = new List<Tuple<int, DateTime, TermGroup_TimeScheduleTemplateBlockType, bool, bool>>();

            if (!this.currentSendXEMailEmployeeDates.Any(x => x.Item1 == employeeId && x.Item2 == date.Value.Date && x.Item3 == type))
            {
                this.currentSendXEMailEmployeeDates.Add(Tuple.Create(employeeId.Value, date.Value.Date, type, wantedShift, unWantedShift));
            }
            else
            {
                var tupleItem = this.currentSendXEMailEmployeeDates.FirstOrDefault(x => x.Item1 == employeeId && x.Item2 == date.Value.Date && x.Item3 == type);
                this.currentSendXEMailEmployeeDates.Remove(tupleItem);
                this.currentSendXEMailEmployeeDates.Add(Tuple.Create(employeeId.Value, date.Value.Date, type, (tupleItem?.Item4 ?? false) || wantedShift, (tupleItem?.Item5 ?? false) || unWantedShift));
            }
        }

        #endregion

        #endregion

        #region Single cache

        private User cachedUser;
        private User GetUserFromCache()
        {
            if (this.cachedUser == null)
            {
                this.cachedUser = entities?.User.FirstOrDefault(u => u.UserId == userId && u.State == (int)SoeEntityState.Active);
                if (this.cachedUser == null)
                {
                    try
                    {
                        this.cachedUser = new User() { Name = "TimeEngineManager" };
                        if (entities != null && cachedUser.IsAdded())
                            base.TryDetachEntity(entities, cachedUser);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString(); //prevent compiler warning
                    }
                }
            }
            return this.cachedUser;
        }

        private int? cachedSysCountryId;
        private int GetSysCountryFromCache()
        {
            if (!this.cachedSysCountryId.HasValue)
                this.cachedSysCountryId = GetSysCountryId();
            return this.cachedSysCountryId.Value;
        }

        #endregion

        #region Company cache

        private List<TimeEngineCompanyCache> companyCacheRepository = null;
        private TimeEngineCompanyCache GetCompanyCache()
        {
            TimeEngineCompanyCache companyCache = companyCacheRepository?.FirstOrDefault(i => i.ActorCompanyId == actorCompanyId);
            if (companyCache == null)
            {
                companyCache = new TimeEngineCompanyCache(actorCompanyId);
                companyCacheRepository ??= new List<TimeEngineCompanyCache>();
                companyCacheRepository.Add(companyCache);
            }
            return companyCache;
        }

        #region Account

        private List<int> GetAccountHierarchySettingAccountFromCache(bool? useAccountHierarchy = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (useAccountHierarchy == false)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            var (accountIds, isSet) = cache.GetAccountHierarchySettingAccount();
            if (!isSet)
            {
                accountIds = GetAccountHierarchySettingAccounts(useAccountHierarchy ?? UseAccountHierarchy(), dateFrom, dateTo);
                cache.SetAccountHierarchySettingAccount(accountIds);
            }
            return accountIds;
        }
        private List<AccountDTO> GetAccountInternalsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<AccountDTO> accountInternals = cache.GetAccountInternals();
            if (accountInternals == null)
            {
                accountInternals = GetAccountInternals().ToDTOs();
                cache.SetAccountInternals(accountInternals);
            }
            return accountInternals;
        }

        #endregion

        #region AccountInternal

        private List<AccountInternal> GetAccountInternalsWithAccountFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<AccountInternal> accountInternals = cache.GetAccountInternalsWithAccount();
            if (accountInternals == null)
            {
                accountInternals = GetAccountInternalsWithAccount();
                cache.SetAccountInternals(accountInternals);
            }
            return accountInternals;
        }
        private List<AccountInternal> GetAccountInternalsWithAccountFromCache(List<AccountInternalDTO> accountInternals)
        {
            return GetAccountInternalsWithAccountFromCache(accountInternals?.Select(i => i.AccountId).ToList());
        }
        private List<AccountInternal> GetAccountInternalsWithAccountFromCache(IEnumerable<int> accountIds)
        {
            List<AccountInternal> accountInternals = new List<AccountInternal>();
            if (!accountIds.IsNullOrEmpty())
            {
                foreach (int accountId in accountIds)
                {
                    AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(accountId);
                    if (accountInternal != null)
                        accountInternals.Add(accountInternal);
                }
            }
            return accountInternals;
        }
        private AccountInternal GetAccountInternalWithAccountFromCache(int? accountId, bool discardState = false)
        {
            if (!accountId.HasValue)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            AccountInternal accountInternal = cache.GetAccountInternalWithAccount(accountId.Value);
            if (accountInternal == null)
            {
                accountInternal = GetAccountInternalWithAccount(accountId.Value, discardState: discardState);
                cache.SetAccountInternal(accountInternal);
            }
            else if (!accountInternal.AccountReference.IsLoaded)
            {
                accountInternal.AccountReference.Load();
            }
            return accountInternal;
        }
        private void AddAccountInternalsToCache(List<AccountInternal> accountInternals)
        {
            if (accountInternals.IsNullOrEmpty())
                return;

            TimeEngineCompanyCache cache = GetCompanyCache();
            accountInternals.ForEach(accountInternal => cache.SetAccountInternal(accountInternal));
        }

        #endregion

        #region AccountStd

        private AccountStd GetAccountStdWithAccountFromCache(int? accountId)
        {
            if (!accountId.HasValue)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            AccountStd accountStd = cache.GetAccountStdWithAccount(accountId.Value);
            if (accountStd == null)
            {
                accountStd = GetAccountStdWithAccount(accountId.Value);
                cache.SetAccountStd(accountStd);
            }
            return accountStd;
        }

        #endregion

        #region AccountDim

        private List<AccountDim> GetAccountDimsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<AccountDim> accountDims = cache.GetAccountDims();
            if (accountDims == null)
            {
                accountDims = GetAccountDims();
                cache.SetAccountDims(accountDims);
            }
            return accountDims;
        }
        private List<AccountDim> GetAccountDimWithAccountsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<AccountDim> accountDims = cache.GetAccountDimsWithAccounts();
            if (accountDims == null)
            {
                accountDims = GetAccountDimsWitAccount();
                cache.SetAccountDimsWithAccounts(accountDims);
            }
            return accountDims;
        }
        private List<AccountDimDTO> GetAccountDimInternalsWithParentFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<AccountDimDTO> accountDims = cache.GetAccountDimInternalsWithParent();
            if (accountDims == null)
            {
                accountDims = GetAccountDimInternalsWithParent();
                cache.SetAccountDimInternalsWithParent(accountDims);
            }
            return accountDims;
        }

        #endregion

        #region AnnualLeaveGroup

        private List<AnnualLeaveGroup> GetAnnualLeaveGroupsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<AnnualLeaveGroup> annualLeaveGroups = cache.GetAnnualLeaveGroups();
            if (annualLeaveGroups == null)
            {
                annualLeaveGroups = GetAnnualLeaveGroups();
                cache.SetAnnualLeaveGroups(annualLeaveGroups);
            }
            return annualLeaveGroups;
        }

        #endregion

        #region AttestState

        private List<AttestStateDTO> GetAttestStatesFromCache(params int[] attestStateIds)
        {
            List<AttestStateDTO> attestStates = new List<AttestStateDTO>();
            foreach (int attestStateId in attestStateIds)
            {
                AttestStateDTO attestState = GetAttestStateFromCache(attestStateId);
                if (attestState != null)
                    attestStates.Add(attestState);
            }
            return attestStates;
        }
        private AttestStateDTO GetAttestStateInitialFromCache(TermGroup_AttestEntity entity)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            var (attestStateInitial, exists) = cache.GetAttestStateInitial(entity);
            if (!exists)
            {
                attestStateInitial = GetAttestStateInitial(entity);
                cache.AddAttestStateInitial(entity, attestStateInitial);
            }
            return attestStateInitial;
        }
        private List<AttestTransitionDTO> GetAttestTransitionsFromCache()
        {
            return base.GetAttestTransitionsFromCache(entities, CacheConfig.Company(actorCompanyId));
        }
        private AttestStateDTO GetAttestStateFromCache(int attestStateId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            AttestStateDTO attestState = cache.GetAttestState(attestStateId);
            if (attestState == null)
            {
                attestState = GetAttestState(attestStateId);
                cache.AddAttestState(attestState);
            }
            return attestState;
        }
        private int[] GetPayrollLockedAttestStateIdsFromCache()
        {
            return new int[]
            {
                GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus),
                GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentLockedAttestStateId),
                GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentApproved1AttestStateId),
                GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentApproved2AttestStateId),
                GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId),
            };
        }
        private ActionResult ValidatePayrollLockedAttestStates(List<TimePayrollTransaction> timePayrollTransactions)
        {
            ActionResult result = new ActionResult(true);
            if (!timePayrollTransactions.IsNullOrEmpty())
            {
                int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();
                if (timePayrollTransactions.Select(x => x.AttestStateId).IsEqualToAny(payrollLockedAttestStateIds))
                    result = new ActionResult((int)ActionResultSave.TimePeriodIsLocked);
            }
            return result;
        }

        #endregion

        #region Company

        private Company GetCompanyFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            Company company = cache.GetCompany();
            if (company == null)
            {
                company = GetCompany();
                cache.SetCompany(company);
            }
            return company;
        }

        #endregion

        #region Company Settings

        private bool UseAccountHierarchy()
        {
            return GetCompanyBoolSettingFromCache(CompanySettingType.UseAccountHierarchy);
        }
        private bool UseStaffing()
        {
            return GetCompanyBoolSettingFromCache(CompanySettingType.TimeUseStaffing);
        }
        private bool UsePayroll()
        {
            return GetCompanyBoolSettingFromCache(CompanySettingType.UsePayroll);
        }
        private int? GetCompanyNullableIntSettingFromCache(CompanySettingType companySettingType)
        {
            return GetCompanyIntSettingFromCache(companySettingType).ToNullable();
        }
        private int GetCompanyIntSettingFromCache(CompanySettingType companySettingType)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            int? setting = cache.GetCompanyIntSetting(companySettingType);
            if (!setting.HasValue)
            {
                setting = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)companySettingType, 0, actorCompanyId, 0);
                cache.SetCompanyIntSetting(companySettingType, setting.Value);
            }
            return setting.Value;
        }
        private string GetCompanyStringSettingFromCache(CompanySettingType companySettingType)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            string setting = cache.GetCompanyStringSetting(companySettingType);
            if (setting == null)
            {
                setting = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)companySettingType, 0, actorCompanyId, 0);
                cache.SetCompanyStringSetting(companySettingType, setting ?? string.Empty);
            }
            return setting;
        }
        private bool GetCompanyBoolSettingFromCache(CompanySettingType companySettingType)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            bool? setting = cache.GetCompanyBoolSetting(companySettingType);
            if (!setting.HasValue)
            {
                setting = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)companySettingType, 0, actorCompanyId, 0);
                cache.SetCompanyBoolSetting(companySettingType, setting.Value);
            }
            return setting.Value;
        }
        private DateTime GetCompanyDateTimeSettingFromCache(CompanySettingType companySettingType)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            DateTime? setting = cache.GetCompanyDateTimeSetting(companySettingType);
            if (!setting.HasValue)
            {
                setting = SettingManager.GetDateSetting(entities, SettingMainType.Company, (int)companySettingType, 0, actorCompanyId, 0);
                cache.SetCompanyDateTimeSetting(companySettingType, setting.Value);
            }
            return setting.Value;
        }

        #endregion

        #region Employee

        private Employee GetHiddenEmployeeFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            Employee hiddenEmployee = cache.GetHiddenEmployee();
            if (hiddenEmployee == null)
            {
                hiddenEmployee = GetHiddenEmployee();
                cache.SetHiddenEmployee(hiddenEmployee);
            }
            return hiddenEmployee;
        }
        private int GetHiddenEmployeeIdFromCache()
        {
            return GetHiddenEmployeeFromCache()?.EmployeeId ?? 0;
        }
        private bool IsHiddenEmployeeFromCache(int employeeId)
        {
            return GetHiddenEmployeeFromCache()?.EmployeeId == employeeId;
        }
        private bool IsVacantEmployeeFromCache(int employeeId)
        {
            return GetEmployeeFromCache(employeeId)?.Vacant ?? false;
        }
        private List<EmployeeAgeDTO> GetEmployeeAgeInfoFromCache(List<Employee> employees)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<EmployeeAgeDTO> employeeAgeInfos = cache.GetEmployeeAgeInfo();
            if (employeeAgeInfos == null)
            {
                employeeAgeInfos = GetEmployeeAgeInfo(employees);
                cache.AddEmployeeAgeInfo(employeeAgeInfos);
            }
            return employeeAgeInfos;
        }

        #endregion

        #region EmployeeGroup

        private List<EmployeeGroup> GetEmployeeGroupsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<EmployeeGroup> employeeGroups = cache.GetEmployeeGroups();
            if (employeeGroups == null)
            {
                employeeGroups = GetEmployeeGroups();
                cache.SetEmployeeGroups(employeeGroups);
            }
            return employeeGroups;
        }
        private List<EmployeeGroup> GetEmployeeGroupsWithWeekendSalaryDayTypesFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<EmployeeGroup> employeeGroups = cache.GetEmployeeGroupsWithWeekendSalaryDayTypes();
            if (employeeGroups == null)
            {
                employeeGroups = GetEmployeeGroupsWithWeekendSalaryDayTypes();
                cache.SetEmployeeGroupsWithWeekendSalaryDayTypes(employeeGroups);
            }
            return employeeGroups;
        }
        private List<EmployeeGroup> GetEmployeeGroupsWithDeviationCausesDayTypesAndTransitionsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<EmployeeGroup> employeeGroups = cache.GetEmployeeGroupsWithDeviationCausesDayTypesAndTransitions();
            if (employeeGroups == null)
            {
                employeeGroups = GetEmployeeGroupsWithDeviationCausesDaytypesAndTransitions();
                cache.SetEmployeeGroupsWithDeviationCausesDayTypesAndTransitions(employeeGroups);
            }
            return employeeGroups;
        }

        #endregion

        #region EvaluatePayrollPriceFormulaInputDTO

        private void InitEvaluatePriceFormulaInputDTO(List<int> employeeIds = null) => GetEvaluatePriceFormulaInputDTOFromCache(employeeIds: employeeIds);

        private EvaluatePayrollPriceFormulaInputDTO GetEvaluatePriceFormulaInputDTOFromCache(bool onlyFromCache = false, List<int> employeeIds = null)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            EvaluatePayrollPriceFormulaInputDTO evaluatePriceFormulaInputDTO = cache.GetEvaluatePriceFormulaInputDTO();
            if (evaluatePriceFormulaInputDTO == null && !onlyFromCache)
            {
                evaluatePriceFormulaInputDTO = CreateEvaluatePriceFormulaInputDTO(employeeIds);
                cache.SetEvaluatePriceFormulaInputDTO(evaluatePriceFormulaInputDTO);
            }
            return evaluatePriceFormulaInputDTO;
        }

        #endregion

        #region DayType

        private List<DayType> GetDayTypesFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<DayType> dayTypes = cache.GetDayTypes();
            if (dayTypes == null)
            {
                dayTypes = GetDayTypes();
                cache.SetDayTypes(dayTypes);
            }
            return dayTypes;
        }

        #endregion

        #region Holiday

        private List<HolidayDTO> GetHolidaysWithDayTypeAndHalfDayFromCache(DateTime? fromDate = null)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<HolidayDTO> allHolidaysWithDayType = cache.GetHolidaysWithDayType();
            if (allHolidaysWithDayType == null)
            {
                allHolidaysWithDayType = GetHolidaysWithDayType(fromDate);
                cache.SetHolidaysWithDayType(allHolidaysWithDayType);
            }
            return allHolidaysWithDayType;
        }
        private List<HolidayDTO> GetHolidaysWithDayTypeAndHalfDaySettingsFromCache(DateTime? fromDate = null)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<HolidayDTO> allHolidaysWithDayType = cache.GetHolidaysWithDayTypeAndHalfDaySettings();
            if (allHolidaysWithDayType == null)
            {
                allHolidaysWithDayType = GetHolidaysWithDayTypeAndHalfDaySettings(fromDate);
                cache.SetHolidaysWithDayTypeAndHalfDaySettings(allHolidaysWithDayType);
            }
            return allHolidaysWithDayType;
        }
        private HolidayDTO GetHolidayWithDayTypeDiscardedStateFromCache(int holidayId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            HolidayDTO holidayWithDayType = cache.GetHolidayWithDayTypeDiscardedState(holidayId);
            if (holidayWithDayType == null)
            {
                holidayWithDayType = GetHolidayWithDayTypeDiscardedState(holidayId).ToDTO(true);
                cache.AddHolidayWithDayTypeDiscardedState(holidayWithDayType);
            }
            return holidayWithDayType;
        }

        #endregion

        #region MassRegistrationTemplates

        private List<MassRegistrationTemplateHeadDTO> GetMassRegistrationTemplatesForPayrollCalculationFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<MassRegistrationTemplateHeadDTO> templates = cache.GetMassRegistrationTemplatesForPayrollCalculation();
            if (templates == null)
            {
                templates = GetMassRegistrationTemplatesForPayrollCalculation();
                cache.AddMassregistrationTemplatesForPayrollCalculation(templates);
            }
            return templates;
        }

        #endregion

        #region PayrollGroup

        private List<PayrollGroup> GetPayrollGroupsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollGroup> payrollGroups = cache.GetPayrollGroups();
            if (payrollGroups == null)
            {
                payrollGroups = GetPayrollGroups();
                cache.SetPayrollGroups(payrollGroups);
            }
            return payrollGroups;
        }
        private List<PayrollGroup> GetPayrollGroupsWithSettingsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollGroup> payrollGroups = cache.GetPayrollGroupsWithSettings();
            if (payrollGroups == null)
            {
                payrollGroups = GetPayrollGroupsWithSettings();
                cache.SetPayrollGroupsWithSettings(payrollGroups);
                cache.SetPayrollGroups(payrollGroups);
            }
            return payrollGroups;
        }
        private PayrollGroup GetPayrollGroupWithSettingsFromCache(int payrollGroupId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollGroup payrollGroup = cache.GetPayrollGroupsWithSettings()?.FirstOrDefault(i => i.PayrollGroupId == payrollGroupId);
            if (payrollGroup == null)
            {
                payrollGroup = GetPayrollGroupWithSettings(payrollGroupId);
                if (payrollGroup != null)
                {
                    cache.AddPayrollGroup(payrollGroup);
                    cache.AddPayrollGroupsWithSettings(payrollGroup);
                }
            }
            return payrollGroup;
        }
        private List<PayrollGroup> GetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProductsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollGroup> payrollGroups = cache.GetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts();
            if (payrollGroups == null)
            {
                payrollGroups = GetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts();
                cache.SetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts(payrollGroups);
            }
            return payrollGroups;
        }

        private List<PayrollGroupAccountStd> GetPayrollGroupAccountsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollGroupAccountStd> payrollGroupAccounts = cache.GetPayrollGroupsAccountStds();
            if (payrollGroupAccounts == null)
            {
                payrollGroupAccounts = GetPayrollGroupAccountStdsForCompany();
                if (payrollGroupAccounts != null)
                {
                    cache.AddPayrollGroupsAccountStds(payrollGroupAccounts);
                }
            }
            return payrollGroupAccounts;
        }

        #endregion

        #region PayrollStartValueHead

        private List<PayrollStartValueHead> GetPayrollStartValueHeadsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollStartValueHead> payrollStartValueHeads = cache.GetPayrollStartValueHeads();
            if (payrollStartValueHeads == null)
            {
                payrollStartValueHeads = GetPayrollStartValueHeads();
                cache.SetPayrollStartValueHeads(payrollStartValueHeads);
            }
            return payrollStartValueHeads;
        }
        private bool HasCompanyPayrollStartValueFromCache(DateTime date)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            bool? hasPayrollStartValues = cache.HasCompanyPayrollStartValues(date);
            if (!hasPayrollStartValues.HasValue)
            {
                hasPayrollStartValues = GetPayrollStartValueHeadsFromCache().Any(p => p.DateFrom <= date && p.DateTo >= date);
                cache.SetCompanyHasPayrollStartValues(date, hasPayrollStartValues.Value);
            }
            return hasPayrollStartValues == true;
        }

        #endregion

        #region PayrollGroupPayrollProduct

        private List<PayrollGroupPayrollProduct> GetPayrollGroupPayrollProductsFromCache(int payrollGroupId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollGroupPayrollProduct> products = cache.GetPayrollGroupPayrollProducts(payrollGroupId);
            if (products == null)
            {
                products = GetPayrollGroupPayrollProducts(payrollGroupId);
                cache.AddPayrollGroupPayrollProducts(payrollGroupId, products);
            }
            return products;
        }

        #endregion

        #region PayrollPriceTypesWithPeriods

        private List<PayrollPriceType> GetPayrollPriceTypesWithPeriodsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollPriceType> payrollPriceTypes = cache.GetPayrollPriceTypesWithPeriods();
            if (payrollPriceTypes == null)
            {
                payrollPriceTypes = GetPayrollPriceTypesWithPeriods();
                cache.SetPayrollPriceTypesWithPeriods(payrollPriceTypes);
            }
            return payrollPriceTypes;
        }

        #endregion

        #region Product

        private Product GetProductFromCache(int productId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            Product product = cache.GetProduct(productId);
            if (product == null)
            {
                product = GetProduct(productId);
                cache.AddProduct(product);
            }
            return product;
        }

        #region InvoiceProduct

        private InvoiceProduct GetInvoiceProductFromCache(int productId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            InvoiceProduct product = cache.GetInvoiceProduct(productId);
            if (product == null)
            {
                product = GetInvoiceProduct(productId);
                cache.AddInvoiceProduct(product);
            }
            return product;
        }

        #endregion

        #region PayrollProduct

        //GrossSalary
        private PayrollProduct GetPayrollProductQualifyingDeduction() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_GrossSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Deduction);
        private List<PayrollProduct> GetPayrollProductMonthlySalaries() => GetPayrollProductsWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_GrossSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_MonthlySalary);
        private PayrollProduct GetPayrollProductVacationCompensationDirectPaid() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_GrossSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation,
            TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_DirectPaid);
        private PayrollProduct GetPayrollProductVariablePrepaymentInvert() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_GrossSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment,
            TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment_Invert);
        private PayrollProduct GetPayrollProductPrepaymentInvert() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_GrossSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment,
            TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment_Invert);
        private PayrollProduct GetPayrollProductWeekendSalary() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_GrossSalary,
            TermGroup_SysPayrollType.SE_GrossSalary_WeekendSalary);
        //Benefit
        private PayrollProduct GetPayrollProductBenefitCompanyCar() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Benefit,
            TermGroup_SysPayrollType.SE_Benefit_CompanyCar);
        private List<PayrollProduct> GetPayrollProductsBenefitInvert() => GetPayrollProductsWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Benefit,
            TermGroup_SysPayrollType.SE_Benefit_Invert);
        //Tax
        private PayrollProduct GetPayrollProductTableTax() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Tax,
            TermGroup_SysPayrollType.SE_Tax_TableTax);
        private PayrollProduct GetPayrollProductSINKTax() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Tax,
            TermGroup_SysPayrollType.SE_Tax_SINK);
        private PayrollProduct GetPayrollProductASINKTax() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Tax,
            TermGroup_SysPayrollType.SE_Tax_ASINK);
        private PayrollProduct GetPayrollProductOneTimeTax() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Tax,
            TermGroup_SysPayrollType.SE_Tax_OneTimeTax);
        //Compensation
        private PayrollProduct GetPayrollProductCompensationVat6Percent() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
           TermGroup_SysPayrollType.SE_Compensation,
           TermGroup_SysPayrollType.SE_Compensation_Vat,
           TermGroup_SysPayrollType.SE_Compensation_Vat_6Percent);
        private PayrollProduct GetPayrollProductCompensationVat12Percent() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
           TermGroup_SysPayrollType.SE_Compensation,
           TermGroup_SysPayrollType.SE_Compensation_Vat,
           TermGroup_SysPayrollType.SE_Compensation_Vat_12Percent);
        private PayrollProduct GetPayrollProductCompensationVat25Percent() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Compensation,
            TermGroup_SysPayrollType.SE_Compensation_Vat,
            TermGroup_SysPayrollType.SE_Compensation_Vat_25Percent);
        private PayrollProduct GetPayrollProductCompensationVat(decimal? vatPercent = null)
        {
            if (vatPercent.HasValue)
            {
                if (vatPercent == 6 && GetPayrollProductCompensationVat6Percent() != null)
                    return GetPayrollProductCompensationVat6Percent();
                if (vatPercent == 12 && GetPayrollProductCompensationVat12Percent() != null)
                    return GetPayrollProductCompensationVat12Percent();
                if (vatPercent == 25 && GetPayrollProductCompensationVat25Percent() != null)
                    return GetPayrollProductCompensationVat25Percent();
            }
            return GetPayrollProductWithSettingsAndAccountInternalsFromCache(
                   TermGroup_SysPayrollType.SE_Compensation,
                   TermGroup_SysPayrollType.SE_Compensation_Vat);
        }
        //Deduction
        private PayrollProduct GetPayrollProductSalaryDistressAmount() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Deduction,
            TermGroup_SysPayrollType.SE_Deduction_SalaryDistress,
            TermGroup_SysPayrollType.SE_Deduction_SalaryDistressAmount);
        private PayrollProduct GetPayrollProductDeductionCarBenefit() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Deduction,
            TermGroup_SysPayrollType.SE_Deduction_CarBenefit);
        private List<PayrollProduct> GetPayrollProductUnionFees() => GetPayrollProductsWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_Deduction,
            TermGroup_SysPayrollType.SE_Deduction_UnionFee);
        //EmploymentTax
        private PayrollProduct GetPayrollProductEmploymentTaxCredit() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_EmploymentTaxCredit);
        private PayrollProduct GetPayrollProductEmploymentTaxDebet() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_EmploymentTaxDebit);
        //SupplementCharge
        private PayrollProduct GetPayrollProductSupplementChargeCredit() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_SupplementChargeCredit);
        private PayrollProduct GetPayrollProductSupplementChargeDebet() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_SupplementChargeDebit);
        //NetSalary
        private PayrollProduct GetPayrollProductNetSalaryPayed() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_NetSalary,
            TermGroup_SysPayrollType.SE_NetSalary_Paid);
        private PayrollProduct GetPayrollProductNetSalaryRound() => GetPayrollProductWithSettingsAndAccountInternalsFromCache(
            TermGroup_SysPayrollType.SE_NetSalary,
            TermGroup_SysPayrollType.SE_NetSalary_Rounded);

        private bool HasPayrollProductFromCache(int? productId)
        {
            return productId.HasValue && GetPayrollProductFromCache(productId.Value) != null;
        }
        private PayrollProduct GetPayrollProductFromCache(int productId, bool includeInactive = false)
        {
            if (productId == 0)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollProduct payrollProduct = cache.GetPayrollProduct(productId, includeInactive);
            if (payrollProduct == null)
            {
                payrollProduct = GetPayrollProduct(productId, includeInactive);
                cache.AddPayrollProduct(payrollProduct);
            }
            return payrollProduct;
        }
        private PayrollProduct GetPayrollProductFromCache(string number, bool includeInactive = false)
        {
            if (string.IsNullOrEmpty(number))
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollProduct payrollProduct = cache.GetPayrollProduct(number, includeInactive);
            if (payrollProduct == null)
            {
                payrollProduct = GetPayrollProduct(number, includeInactive);
                cache.AddPayrollProduct(payrollProduct);
            }
            return payrollProduct;
        }
        private PayrollProduct GetPayrollProductFromCache(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollProduct payrollProduct = cache.GetPayrollProduct(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            if (payrollProduct == null)
            {
                payrollProduct = GetPayrollProduct(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
                cache.AddPayrollProduct(payrollProduct);
            }
            return payrollProduct;
        }
        private List<PayrollProduct> GetPayrollProductWithSettingsFromCache(List<int> productIds)
        {
            if (productIds.IsNullOrEmpty())
                return new List<PayrollProduct>();

            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
            foreach (int productId in productIds.Distinct())
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsFromCache(productId);
                if (payrollProduct != null)
                    payrollProducts.Add(payrollProduct);
            }
            return payrollProducts;
        }
        private PayrollProduct GetPayrollProductWithSettingsFromCache(int productId, bool includeInactive = false)
        {
            if (productId == 0)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollProduct payrollProduct = cache.GetPayrollProductWithSettings(productId, includeInactive);
            if (payrollProduct == null)
            {
                payrollProduct = GetPayrollProductWithSettings(productId, includeInactive);
                cache.AddPayrollProductWithSettings(payrollProduct);
                cache.AddPayrollProduct(payrollProduct);
            }
            return payrollProduct;
        }
        private List<PayrollProduct> GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(List<int> productIds)
        {
            if (productIds.IsNullOrEmpty())
                return new List<PayrollProduct>();

            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
            foreach (int productId in productIds.Distinct())
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(productId);
                if (payrollProduct != null)
                    payrollProducts.Add(payrollProduct);
            }
            return payrollProducts;
        }
        private List<PayrollProduct> GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(List<TimePayrollTransaction> timePayrollTransactions)
        {
            return GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactions.GetProductIds());
        }
        private List<PayrollProduct> GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions)
        {
            return GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollScheduleTransactions.GetProductIds());
        }
        private List<PayrollProduct> GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(List<TimePayrollTransaction> timePayrollTransactions, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions)
        {
            List<int> productIds = timePayrollTransactions.Select(x => x.ProductId).ToList();
            productIds.AddRange(timePayrollScheduleTransactions.Select(x => x.ProductId).ToList());
            productIds = productIds.Distinct().ToList();

            return GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(productIds);
        }

        private PayrollProduct GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(int productId, bool includeInactive = false)
        {
            if (productId == 0)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollProduct payrollProduct = cache.GetPayrollProductWithSettingsAndAccountInternalsAndStds(productId, includeInactive);
            if (payrollProduct == null)
            {
                payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStds(productId, includeInactive);
                cache.AddPayrollProductWithSettingsAndAccountInternalsAndStds(payrollProduct);
                cache.AddPayrollProductWithSettingsAndAccountInternals(payrollProduct);
                cache.AddPayrollProductWithSettings(payrollProduct);
                cache.AddPayrollProduct(payrollProduct);
            }
            return payrollProduct;
        }
        private PayrollProduct GetPayrollProductWithSettingsAndAccountInternalsFromCache(TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            PayrollProduct payrollProduct = cache.GetPayrollProductWithSettingsAndAccountInternals(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            if (payrollProduct == null)
            {
                payrollProduct = GetPayrollProductWithSettingsAndAccountInternals(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
                cache.AddPayrollProductWithSettingsAndAccountInternals(payrollProduct);
                cache.AddPayrollProductWithSettings(payrollProduct);
                cache.AddPayrollProduct(payrollProduct);
            }
            return payrollProduct;
        }
        private List<PayrollProduct> GetPayrollProductsWithSettingsAndAccountInternalsFromCache(TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType sysPayrollTypeLevel2)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<PayrollProduct> payrollProducts = cache.GetPayrollProductsWithSettingsAndAccountInternalsByLevel2((int)sysPayrollTypeLevel2);
            if (payrollProducts == null)
            {
                payrollProducts = GetPayrollProductsWithSettingsAndAccountInternals(sysPayrollTypeLevel1, sysPayrollTypeLevel2);
                cache.SetPayrollProductsWithSettingsAndAccountInternalsByLevel2((int)sysPayrollTypeLevel2, payrollProducts);
                cache.AddPayrollProductsWithSettingsAndAccountInternals(payrollProducts);
                cache.AddPayrollProducts(payrollProducts);
            }
            return payrollProducts;
        }
        private List<PayrollProduct> GetPayrollProductsWithSettingsAndAccountInternalsAndStdsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            return cache.GetPayrollProductsWithSettingsAndAccountInternalsAndStds();
        }

        #endregion

        #endregion

        #region ShiftType

        private ShiftType GetShiftTypeFromCache(int shiftTypeId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            ShiftType shiftType = cache.GetShiftType(shiftTypeId);
            if (shiftType == null)
            {
                shiftType = GetShiftType(shiftTypeId);
                cache.AddShiftType(shiftType);
            }
            return shiftType;
        }
        private ShiftType GetShiftTypeWithAccountsFromCache(int shiftTypeId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            ShiftType shiftType = cache.GetShiftType(shiftTypeId);
            if (shiftType == null)
            {
                shiftType = GetShiftTypeWithAccounts(shiftTypeId);
                cache.AddShiftType(shiftType);
            }
            else if (!shiftType.AccountInternal.IsLoaded)
                shiftType.AccountInternal.Load();
            return shiftType;
        }
        private ShiftType GetShiftTypeByAccountsWithAccountsFromCache(int accountId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            ShiftType shiftType = cache.GetShiftTypeByAccount(accountId);
            if (shiftType == null)
            {
                shiftType = GetShiftTypeByAccountWithAccounts(accountId);
                cache.AddShiftType(shiftType);
            }
            else if (!shiftType.AccountInternal.IsLoaded)
                shiftType.AccountInternal.Load();
            return shiftType;
        }
        private List<int> GetShiftTypeIdsHandlingMoneyFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<int> shiftTypeIdsHandlingMoney = cache.GetShiftTypeIdsHandlingMoney();
            if (shiftTypeIdsHandlingMoney == null)
            {
                shiftTypeIdsHandlingMoney = GetShiftTypeIdsHandlingMoney();
                cache.SetShiftTypeIdsHandlingMoney(shiftTypeIdsHandlingMoney);
            }
            return shiftTypeIdsHandlingMoney;
        }

        #endregion

        #region TimeAbsenceRuleHeads

        private List<TimeAbsenceRuleHead> GetTimeAbsenceRuleHeadsWithRowsFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<TimeAbsenceRuleHead> timeAbsenceRuleHeads = cache.GetTimeAbsenceRuleHeadsWithRows();
            if (timeAbsenceRuleHeads == null)
            {
                timeAbsenceRuleHeads = GetTimeAbsenceRuleHeadsWithRows();
                cache.AddTimeAbsenceRuleHeads(timeAbsenceRuleHeads);
            }
            return timeAbsenceRuleHeads;
        }
        private List<TimeAbsenceRuleHead> GetTimeAbsenceRuleHeadsWithRowsFromCache(int employeeGroupId)
        {
            return GetTimeAbsenceRuleHeadsWithRowsFromCache()
                .FilterByEmployeeeGroup(employeeGroupId);
        }
        private TimeAbsenceRuleHead GetTimeAbsenceRuleHeadWithRowsFromCache(TermGroup_TimeAbsenceRuleType type, int employeeId, DateTime date)
        {
            int? employeeGroupId = GetEmployeeGroupIdFromCache(employeeId, date);
            return employeeGroupId.HasValue ? GetTimeAbsenceRuleHeadWithRowsFromCache(type, employeeGroupId.Value) : null;
        }
        private TimeAbsenceRuleHead GetTimeAbsenceRuleHeadWithRowsFromCache(TermGroup_TimeAbsenceRuleType type, int employeeGroupId)
        {
            return GetTimeAbsenceRuleHeadsWithRowsFromCache()
                .FilterByEmployeeeGroup(employeeGroupId)
                .FirstOrDefault(e => e.Type == (int)type);
        }

        #endregion

        #region TimeAccumulators

        public List<TimeAccumulator> GetTimeAccumulatorsForTimeWorkAccountFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<TimeAccumulator> timeAccumulators = cache.GetTimeAccumulatorsForTimeWorkAccount();
            if (timeAccumulators == null)
            {
                timeAccumulators = GetTimeAccumulatorsForTimeWorkAccount();
                cache.SetTimeAccumulatorsForTimeWorkAccount(timeAccumulators);
            }
            return timeAccumulators;
        }

        #endregion

        #region TimeBlock

        private TimeBlock GetTimeBlockWithAccountsDiscardedStateFromCache(int timeBlockId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeBlock timeBlock = cache.GetTimeBlockWithAccounts(timeBlockId);
            if (timeBlock == null)
            {
                timeBlock = GetTimeBlockWithAccountsDiscardedState(timeBlockId);
                cache.AddTimeBlockWithAccounts(timeBlock);
            }
            return timeBlock;
        }

        #endregion

        #region TimeCode

        private List<TimeCode> GetTimeCodesFromCache(IEnumerable<int> timeCodeIds, bool loadRules = false)
        {
            List<TimeCode> timeCodes = new List<TimeCode>();
            foreach (int timeCodeId in timeCodeIds.Distinct())
            {
                TimeCode timeCode = GetTimeCodeFromCache(timeCodeId, loadRules);
                if (timeCode != null)
                    timeCodes.Add(timeCode);
            }
            return timeCodes;
        }
        private TimeCode GetTimeCodeFromCache(int timeCodeId, bool loadRules = false)
        {
            if (timeCodeId == 0)
                return null;

            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeCode timeCode = cache.GetTimeCode(timeCodeId) ?? cache.GetTimeCodeWithProducts(timeCodeId);
            if (timeCode == null)
            {
                timeCode = loadRules ? GetTimeCodeWithRules(timeCodeId) : GetTimeCode(timeCodeId);
                cache.AddTimeCode(timeCode);
            }
            else if (loadRules && !timeCode.TimeCodeRule.IsLoaded)
                timeCode.TimeCodeRule.Load();
            return timeCode;
        }
        private TimeCode GetTimeCodeWithProductsFromCache(int timeCodeId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeCode timeCode = cache.GetTimeCodeWithProducts(timeCodeId);
            if (timeCode == null)
            {
                timeCode = GetTimeCodeWithProducts(timeCodeId);
                cache.AddTimeCodeWithProducts(timeCode);
            }
            return timeCode;
        }
        private (TimeCode, PayrollProduct) GetTimeCodeAndPayrollProductFromVacationGroupFromCache(VacationGroupDTO vacationGroup)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            if (!cache.TryGetVacationGroupReplacementTimeCodeAndProduct(vacationGroup.VacationGroupId, out TimeCode timeCode, out PayrollProduct payrollProduct))
            {
                (timeCode, payrollProduct) = GetVacationGroupReplacementTimeCodeAndProduct(vacationGroup);
                cache.AddVacationGroupReplacementTimeCodeAndProduct(vacationGroup.VacationGroupId, timeCode, payrollProduct);
            }
            return (timeCode, payrollProduct);
        }
        private void AddTimeCodeToCache(TimeCode timeCode)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            cache.AddTimeCode(timeCode);
        }
        private void SynchTimeScheduleTemplateBlockToCache(TimeScheduleTemplateBlock templateBlock)
        {
            if (templateBlock == null)
                return;

            if (templateBlock.TimeCode != null)
                AddTimeCodeToCache(templateBlock.TimeCode);
            if (templateBlock.TimeScheduleTemplateBlockTask != null)
                AddTimeScheduleTemplateBlockTasksToCache(templateBlock.TimeScheduleTemplateBlockTask.ToList());
            if (templateBlock.AccountInternal != null)
                AddAccountInternalsToCache(templateBlock.AccountInternal.ToList());
        }

        #endregion

        #region TimeCodeRanking

        private bool UseTimeCodeRankingFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            bool? useTimeCodeRanking = cache.UseTimeCodeRanking();
            if (!useTimeCodeRanking.HasValue)
            {
                useTimeCodeRanking = UseTimeCodeRanking();
                cache.SetUseTimeCodeRanking(useTimeCodeRanking.Value);
            }
            return useTimeCodeRanking.Value;
        }
        private TimeCodeRankingGroup GetTimeCodeRankingGroupWithRankingsFromCache(DateTime date)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeCodeRankingGroup timeCodeRankingGroup = cache.GetTimeCodeRankingGroupWithRankings(date);
            if (timeCodeRankingGroup == null)
            {
                timeCodeRankingGroup = GetTimeCodeRankingGroupWithRankings(date);
                cache.AddTimeCodeRankingGroup(date, timeCodeRankingGroup);
            }
            return timeCodeRankingGroup;
        }

        #endregion

        #region TimeCodeBreak

        private TimeCodeBreak GetTimeCodeBreakFromCache(int timeCodeId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeCodeBreak timeCodeBreak = cache.GetTimeCodeBreak(timeCodeId);
            if (timeCodeBreak == null)
            {
                timeCodeBreak = GetTimeCodeBreak(timeCodeId);
                cache.AddTimeCodeBreak(timeCodeBreak);
            }
            return timeCodeBreak;
        }
        private TimeCodeBreak GetTimeCodeBreakForEmployeeGroupFromCache(int timeCodeBreakGroupId, int employeeGroupId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeCodeBreak timeCodeBreak = cache.GetTimeCodeBreak(timeCodeBreakGroupId, employeeGroupId);
            if (timeCodeBreak == null)
            {
                timeCodeBreak = GetTimeCodeBreakForEmployeeGroup(timeCodeBreakGroupId, employeeGroupId);
                cache.AddTimeCodeBreak(timeCodeBreak);
            }
            return timeCodeBreak;
        }
        private (DateTime, DateTime) GetTimeCodeBreakWindowFromCache(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            if (!cache.TryGetTimeCodeBreakWindow(timeCodeBreak, scheduleIn, scheduleOut, out DateTime breakWindowStart, out DateTime breakWindowStop))
            {
                (breakWindowStart, breakWindowStop) = GetTimeCodeBreakWindow(timeCodeBreak, scheduleIn, scheduleOut);
                cache.AddTimeCodeBreakWindow(timeCodeBreak, scheduleIn, scheduleOut, breakWindowStart, breakWindowStop);
            }
            return (breakWindowStart, breakWindowStop);
        }

        #endregion

        #region TimeDeviationCause

        private List<TimeDeviationCause> GetTimeDeviationCausesFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<TimeDeviationCause> timeDeviationCauses = cache.GetTimeDeviationCauses();
            if (timeDeviationCauses == null)
            {
                timeDeviationCauses = GetTimeDeviationCauses();
                cache.SetTimeDeviationCauses(timeDeviationCauses);
            }
            return timeDeviationCauses;
        }
        private TimeDeviationCause GetTimeDeviationCauseFromCache(int timeDeviationCauseId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeDeviationCause timeDeviationCause = cache.GetTimeDeviationCause(timeDeviationCauseId);
            if (timeDeviationCause == null)
                timeDeviationCause = GetTimeDeviationCause(timeDeviationCauseId);
            return timeDeviationCause;
        }
        private TimeDeviationCause GetTimeDeviationCauseWithTimeCodeFromCache(int timeDeviationCauseId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeDeviationCause timeDeviationCause = cache.GetTimeDeviationCause(timeDeviationCauseId);
            if (timeDeviationCause == null)
                timeDeviationCause = GetTimeDeviationCauseWithTimeCode(timeDeviationCauseId);
            else if (!timeDeviationCause.TimeCodeReference.IsLoaded)
                timeDeviationCause.TimeCodeReference.Load();
            return timeDeviationCause;
        }

        #endregion

        #region TimeLeisureCode

        private List<TimeLeisureCode> GetTimeLeisureCodesFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<TimeLeisureCode> timeLeisureCodes = cache.GetTimeLeisureCodes();
            if (timeLeisureCodes == null)
            {
                timeLeisureCodes = GetTimeLeisureCodes();
                cache.SetTimeLeisureCodes(timeLeisureCodes);
            }
            return timeLeisureCodes;
        }

        private List<EmployeeGroupTimeLeisureCodeSetting> GetEmployeeGroupTimeLeisureCodeSettingsFromCache(int timeLeisureCodeId, int employeeGroupId, TermGroup_TimeLeisureCodeSettingType type)
        {
            if (!base.HasTimeLeisureCodesFromCache(entities, ActorCompanyId))
                return new List<EmployeeGroupTimeLeisureCodeSetting>();

            if (timeLeisureCodeId == 0 || employeeGroupId == 0)
                return new List<EmployeeGroupTimeLeisureCodeSetting>();

            GetTimeLeisureCodesFromCache(); // Ensure that timeLeisureCodes are loaded

            TimeEngineCompanyCache cache = GetCompanyCache();
            return cache.GetEmployeeGroupTimeLeisureCodeSettings(timeLeisureCodeId, employeeGroupId, type);
        }

        private List<int> GetTimeLeisureCodesTimeScheduleTypeIdsFromCache(List<int> timeLeisureCodeIds, int employeeGroupId)
        {
            return timeLeisureCodeIds.SelectMany(s => GetTimeLeisureCodesTimeScheduleTypeIdsFromCache(s, employeeGroupId)).Distinct().ToList();
        }

        private List<int> GetTimeLeisureCodesTimeScheduleTypeIdsFromCache(int timeLeisureCodeId, int employeeGroupId)
        {
            if (timeLeisureCodeId == 0 || employeeGroupId == 0)
                return new List<int>();

            var settings = GetEmployeeGroupTimeLeisureCodeSettingsFromCache(timeLeisureCodeId, employeeGroupId, TermGroup_TimeLeisureCodeSettingType.ScheduleType);
            var timeScheduleTypeIds = settings?.Where(w => w.IntData.HasValue).Select(s => s.IntData.Value).ToList() ?? new List<int>();
            TimeEngineCompanyCache cache = GetCompanyCache();
            var timeScheduleTypes = cache.GetTimeScheduleTypesWithFactor();
            if (timeScheduleTypes == null)
                return new List<int>();
            return timeScheduleTypes.Where(w => timeScheduleTypeIds.Contains(w.TimeScheduleTypeId)).Select(s => s.TimeScheduleTypeId).ToList();
        }

        #endregion

        #region TimePeriod

        private List<TimePeriod> GetTimePeriodsFromCache(int timePeriodHeadId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<TimePeriod> timePeriods = cache.GetTimePeriods(timePeriodHeadId);
            if (timePeriods == null)
            {
                timePeriods = GetTimePeriods(timePeriodHeadId);
                cache.SetTimePeriods(timePeriods, timePeriodHeadId);
            }
            return timePeriods;
        }
        private TimePeriod GetTimePeriodFromCache(int timePeriodId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimePeriod timePeriod = cache.GetTimePeriod(timePeriodId);
            if (timePeriod == null)
            {
                timePeriod = GetTimePeriod(timePeriodId);
                cache.AddTimePeriod(timePeriod);
            }
            return timePeriod;
        }

        #endregion

        #region TimeScheduleEmployeePeriod

        private TimeScheduleEmployeePeriod GetTimeScheduleEmployeePeriodFromCache(int timeScheduleEmployeePeriodId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleEmployeePeriod timeScheduleEmployeePeriod = cache.GetTimeScheduleEmployeePeriod(timeScheduleEmployeePeriodId);
            if (timeScheduleEmployeePeriod == null)
            {
                timeScheduleEmployeePeriod = GetTimeScheduleEmployeePeriod(timeScheduleEmployeePeriodId);
                cache.AddTimeScheduleEmployeePeriod(timeScheduleEmployeePeriod);
            }
            return timeScheduleEmployeePeriod;
        }
        private TimeScheduleEmployeePeriod GetTimeScheduleEmployeePeriodFromCache(int employeeId, DateTime date)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleEmployeePeriod timeScheduleEmployeePeriod = cache.GetTimeScheduleEmployeePeriod(employeeId, date);
            if (timeScheduleEmployeePeriod == null)
            {
                timeScheduleEmployeePeriod = GetTimeScheduleEmployeePeriod(employeeId, date);
                cache.AddTimeScheduleEmployeePeriod(timeScheduleEmployeePeriod);
            }
            return timeScheduleEmployeePeriod;
        }
        private void AddTimeScheduleEmployeePeriodsToCache(int employeeId, DateTime startDate, DateTime stopDate)
        {
            List<TimeScheduleEmployeePeriod> employeePeriods = GetTimeScheduleEmployeePeriods(employeeId, startDate, stopDate);
            if (employeePeriods == null)
                return;

            TimeEngineCompanyCache cache = GetCompanyCache();
            cache.AddTimeScheduleEmployeePeriods(employeePeriods);
        }

        #endregion

        #region TimeScheduleTemplateBlock

        public List<TimeScheduleTemplateBlock> GetTimeScheduleTemplateBlocksForCompanyFromCache(DateTime dateFrom, DateTime dateTo, List<int> accountIds, int? timeScheduleScenarioHeadId = null)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            if (!cache.TryGetScheduleBlocksForCompanyDiscardScenario(dateFrom, dateTo, out List<TimeScheduleTemplateBlock> templateBlocks, out _))
            {
                templateBlocks = GetScheduleBlocksForCompanyDiscardScenario(dateFrom, dateTo, accountIds);
                cache.AddScheduleTemplateBlocks(templateBlocks);
            }
            return templateBlocks.Where(i => i.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId).ToList();
        }
        private TimeScheduleTemplateBlock GetTimeScheduleTemplateBlockWithAccountsFromCache(int timeScheduleTemplateBlockId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleTemplateBlock timeScheduleTemplateBlock = cache.GetScheduleTemplateBlocksBlocksWithAccounts(timeScheduleTemplateBlockId);
            if (timeScheduleTemplateBlock == null)
            {
                timeScheduleTemplateBlock = GetScheduleBlockWithAccounting(timeScheduleTemplateBlockId);
                cache.AddScheduleTemplateBlocksBlocksWithAccounts(timeScheduleTemplateBlock);
            }
            return timeScheduleTemplateBlock;
        }

        #endregion

        #region TimeScheduleTemplateBlockTask

        private TimeScheduleTemplateBlockTask GetTimeScheduleTemplateBlockTaskFromCache(int timeScheduleTemplateBlockTaskId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleTemplateBlockTask timeScheduleTemplateBlockTask = cache.GetTimeScheduleTemplateBlockTask(timeScheduleTemplateBlockTaskId);
            if (timeScheduleTemplateBlockTask == null)
            {
                timeScheduleTemplateBlockTask = GetTimeScheduleTemplateBlockTask(timeScheduleTemplateBlockTaskId);
                cache.AddTimeScheduleTemplateBlockTask(timeScheduleTemplateBlockTask);
            }
            return timeScheduleTemplateBlockTask;
        }
        private void AddTimeScheduleTemplateBlockTasksToCache(List<TimeScheduleTemplateBlockTask> timeScheduleTemplateBlockTasks)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            cache.AddTimeScheduleTemplateBlockTasks(timeScheduleTemplateBlockTasks);
        }

        #endregion

        #region TimeScheduleTemplateHead

        private TimeScheduleTemplateHead GetTimeScheduleTemplateHeadWithPeriodsFromCache(int timeScheduleTemplateHeadId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleTemplateHead timeScheduleTemplateHead = cache.GetTimeScheduleTemplateHeadWithPeriods(timeScheduleTemplateHeadId);
            if (timeScheduleTemplateHead == null)
            {
                timeScheduleTemplateHead = GetTimeScheduleTemplateHeadWithPeriods(timeScheduleTemplateHeadId);
                cache.AddTimeScheduleTemplateHeadWithPeriods(timeScheduleTemplateHead);
            }
            return timeScheduleTemplateHead;
        }

        private TimeScheduleTemplateHead GetTimeScheduleTemplateHeadFromCache(int timeScheduleTemplateHeadId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleTemplateHead timeScheduleTemplateHead = cache.GetTimeScheduleTemplateHead(timeScheduleTemplateHeadId);
            if (timeScheduleTemplateHead == null)
            {
                timeScheduleTemplateHead = GetTimeScheduleTemplateHead(timeScheduleTemplateHeadId);
                cache.AddTimeScheduleTemplateHead(timeScheduleTemplateHead);
            }
            return timeScheduleTemplateHead;
        }

        #endregion

        #region TimeScheduleTemplatePeriod

        private TimeScheduleTemplatePeriod GetTimeScheduleTemplatePeriodFromCache(int timeScheduleTemplatePeriodId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = cache.GetTimeScheduleTemplatePeriod(timeScheduleTemplatePeriodId);
            if (timeScheduleTemplatePeriod == null)
            {
                timeScheduleTemplatePeriod = GetTimeScheduleTemplatePeriod(timeScheduleTemplatePeriodId);
                cache.AddTimeScheduleTemplatePeriod(timeScheduleTemplatePeriod);
            }
            return timeScheduleTemplatePeriod;
        }

        #endregion

        #region TimeScheduleType

        private List<TimeScheduleType> GetTimeScheduleTypesWithFactorFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<TimeScheduleType> timeScheduleTypes = cache.GetTimeScheduleTypesWithFactor();
            if (timeScheduleTypes == null)
            {
                timeScheduleTypes = GetTimeScheduleTypesWithFactor();
                cache.AddTimeScheduleTypesWithFactor(timeScheduleTypes);
            }
            return timeScheduleTypes;
        }
        private List<TimeScheduleType> GetTimeScheduleTypesWithIgnoreIfExtraShiftFromCache()
        {
            return GetTimeScheduleTypesFromCache(entities, CacheConfig.Company(base.ActorCompanyId))
                .Where(st => st.IgnoreIfExtraShift)
                .ToList();
        }
        private List<int> GetTimeScheduleTypeIdsIsNotScheduleTimeFromCache()
        {
            return GetTimeScheduleTypesWithFactorFromCache().GetTimeScheduleTypeIdsIsNotScheduleTime();
        }
        private TimeScheduleType GetTimeScheduleTypeFromCache(int timeScheduleTypeId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeScheduleType timeScheduleType = cache.GetTimeScheduleType(timeScheduleTypeId) ?? cache.GetTimeScheduleTypeWithFactors(timeScheduleTypeId);
            if (timeScheduleType == null)
            {
                timeScheduleType = GetTimeScheduleType(timeScheduleTypeId);
                cache.AddTimeScheduleType(timeScheduleType);
            }
            return timeScheduleType;
        }

        #endregion

        #region TimeTerminal

        private TimeTerminal GetTimeTerminalFromCache(int timeTerminalId, bool discardState = false)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            TimeTerminal timeTerminal = cache.GetTimeTerminal(timeTerminalId);
            if (timeTerminal == null)
            {
                timeTerminal = GetTimeTerminal(timeTerminalId, discardState: discardState);
                cache.AddTimeTerminal(timeTerminal);
            }
            return timeTerminal;
        }

        #endregion

        #region TimeWorkReduction

        private bool UseTimeWorkReductionFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            bool? useWorkTimeReduction = cache.UseTimeWorkReduction();
            if (!useWorkTimeReduction.HasValue)
            {
                useWorkTimeReduction = UseTimeWorkReduction();
                cache.SetUseTimeWorkReduction(useWorkTimeReduction.Value);
            }
            return useWorkTimeReduction.Value;
        }

        #endregion

        #region VacationGroup

        private List<VacationGroup> GetVacationGroupsWithSEFromCache()
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            List<VacationGroup> vacationGroups = cache.GetVacationGroupsWithSE();
            if (vacationGroups == null)
            {
                vacationGroups = GetVacationGroupsWithSE(base.ActorCompanyId);
                cache.SetVacationGroups(vacationGroups);
            }
            return vacationGroups;
        }
        private VacationGroupSE GetVacationGroupSEFromCache(int vacationGroupId)
        {
            TimeEngineCompanyCache cache = GetCompanyCache();
            VacationGroupSE vacationGroup = cache.GetVacationGroupSE(vacationGroupId);
            if (vacationGroup == null)
            {
                vacationGroup = GetVacationGroupSE(vacationGroupId);
                cache.AddVacationGroup(vacationGroup);
            }
            return vacationGroup;
        }

        #endregion

        #endregion

        #region Employee cache

        private bool isEmployeeCacheForAllEmployees = false;
        private List<TimeEngineEmployeeCache> employeeCacheRepository = null;
        private TimeEngineEmployeeCache GetEmployeeCache(int employeeId)
        {
            TimeEngineEmployeeCache employeeCache = employeeCacheRepository?.FirstOrDefault(i => i.EmployeeId == employeeId);
            if (employeeCache == null)
            {
                employeeCache = new TimeEngineEmployeeCache(employeeId);
                if (employeeCacheRepository == null)
                    employeeCacheRepository = new List<TimeEngineEmployeeCache>();
                employeeCacheRepository.Add(employeeCache);
            }
            return employeeCache;
        }

        #region AccountingPrio

        private AccountingPrioDTO GetAccountingPrioByEmployeeFromCache(DateTime? date, Employee employee, int projectId = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            AccountingPrioDTO accountingPrio = cache.GetAccountingPrio(date.Value, null);
            if (accountingPrio == null)
            {
                accountingPrio = GetPayrollProductAccount(ProductAccountType.Purchase, employee, 0, projectId, date.Value);
                if (accountingPrio != null && projectId == 0)
                    cache.AddAccountingPrio(accountingPrio, date.Value, null);
            }
            return accountingPrio;
        }
        private AccountingPrioDTO GetAccountingPrioByPayrollProductFromCache(PayrollProduct payrollProduct, Employee employee, DateTime? date = null, int projectId = 0)
        {
            if (payrollProduct == null || employee == null)
                return null;
            if (!date.HasValue)
                date = DateTime.Today;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            AccountingPrioDTO accountingPrio = cache.GetAccountingPrio(date.Value, payrollProduct.ProductId);
            if (accountingPrio == null)
            {
                if (payrollProduct.IsEmploymentTax()) //ProductAccountType.Purchase = EmploymentTax
                    accountingPrio = GetPayrollProductAccount(ProductAccountType.Purchase, employee, payrollProduct.ProductId, 0, date.Value);
                else if (payrollProduct.IsSupplementCharge()) //ProductAccountType.VAT = SupplementCharge
                    accountingPrio = GetPayrollProductAccount(ProductAccountType.VAT, employee, payrollProduct.ProductId, 0, date.Value);
                else
                    accountingPrio = GetPayrollProductAccount(ProductAccountType.Purchase, employee, payrollProduct.ProductId, projectId, date.Value);

                if (accountingPrio != null && projectId == 0)
                    cache.AddAccountingPrio(accountingPrio, date.Value, payrollProduct.ProductId);
            }
            return accountingPrio;
        }
        private void SetAccountingPrioByEmployeeFromCache(DateTime fromDate, DateTime toDate, Employee employee)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);

            DateTime currentDate = fromDate.Date;
            while (currentDate <= toDate)
            {
                AccountingPrioDTO accountingPrio = cache.GetAccountingPrio(currentDate, null);
                if (accountingPrio == null)
                {
                    accountingPrio = GetPayrollProductAccount(ProductAccountType.Purchase, employee, 0, 0, currentDate);
                    if (accountingPrio != null)
                        cache.AddAccountingPrio(accountingPrio, currentDate, null);
                }
                currentDate = currentDate.AddDays(1);
            }
        }

        #endregion

        #region Absence - five vacation days per week

        private bool IsVacationFiveDaysPerWeekFromCache(VacationGroupDTO vacationGroup, int employeeId, DateTime date)
        {
            if (!UseVacationFiveDaysPerWeek(vacationGroup))
                return false;

            int weekNr = CalendarUtility.GetWeekNr(date);
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            bool? isVacationWholeWeek = cache.IsVacationFiveDaysPerWeekWholeWeek(weekNr);
            if (!isVacationWholeWeek.HasValue)
            {
                isVacationWholeWeek = HasVacationWholeWeek(employeeId, date);
                cache.AddVacationFiveDaysPerWeekWholeWeek(weekNr, isVacationWholeWeek.Value);
            }

            return isVacationWholeWeek.Value || HasCoherentVacation5Days(employeeId, date);
        }

        private void SetVacationFiveDays(VacationGroupDTO vacationGroup, ApplyAbsenceDay absenceDay, int employeeId, DateTime date, int scheduleMinutes)
        {
            absenceDay.SetVacationFiveDays(date, scheduleMinutes, IsVacationGroupHoliday(vacationGroup, employeeId, date));
        }

        private bool IsVacationGroupHoliday(VacationGroupDTO vacationGroup, int employeeId, DateTime date)
        {
            if (vacationGroup?.VacationGroupSE == null)
                return false;

            List<VacationGroupSEDayType> vacationGroupSEDayTypes = GetVacationGroupSEDayTypesFromCache(employeeId, vacationGroup.VacationGroupSE.VacationGroupSEId);
            if (vacationGroupSEDayTypes.IsNullOrEmpty())
                return false;

            DayType daytype = GetDayTypeForEmployeeFromCache(employeeId, date);
            if (daytype == null)
                return false;

            return vacationGroupSEDayTypes.Any(d => d.DayTypeId == daytype.DayTypeId);
        }

        #endregion

        #region DayType

        private List<DayType> GetDayTypesWithStandardWeekDayFromCache(Employee employee)
        {
            if (employee == null)
                return new List<DayType>();

            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            List<DayType> dayTypes = cache.GetDayTypesWithStandardWeekDay();
            if (dayTypes == null)
            {
                dayTypes = GetDayTypesWithStandardWeekDay();
                cache.SetDayTypesWithStandardWeekDay(dayTypes);
            }
            return dayTypes;
        }
        private DayType GetDayTypeForEmployeeFromCache(int employeeId, DateTime date, bool doNotCheckHoliday = false)
        {
            return GetDayTypeForEmployeeFromCache(GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId), date, doNotCheckHoliday);
        }
        private DayType GetDayTypeForEmployeeFromCache(Employee employee, DateTime date, bool doNotCheckHoliday = false)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            DayType dayType = cache.GetDayType(date, doNotCheckHoliday);
            if (dayType == null)
            {
                dayType = GetDayType(employee, date, doNotCheckHoliday);
                cache.AddDayType(date, dayType, doNotCheckHoliday);
            }
            return dayType;
        }
        private void CalculateDayTypeForEmployee(List<GrossNetCostDTO> dtos, bool doNotCheckHoliday, List<HolidayDTO> holidays, List<DayType> dayTypes, Employee employee)
        {
            if (employee == null || IsHiddenEmployeeFromCache(employee.EmployeeId))
                return;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            cache.SetEmployee(employee);
            cache.SetDayTypesWithStandardWeekDay(dayTypes);
            if (!doNotCheckHoliday)
                GetCompanyCache().SetHolidaysWithDayType(holidays);

            DateTime? prevDate = null;
            int? prevDayTypeId = null;
            foreach (GrossNetCostDTO dto in dtos.Where(i => i.EmployeeId == employee.EmployeeId).OrderBy(g => g.StartTime.Date))
            {
                if (dto.StartTime.Date != prevDate)
                    prevDayTypeId = GetDayTypeForEmployeeFromCache(employee.EmployeeId, dto.StartTime.Date, doNotCheckHoliday)?.DayTypeId;
                dto.CalculatedDayTypeId = prevDayTypeId;
                prevDate = dto.StartTime.Date;
            }
        }
        private void CalculateDayTypeForEmployees(List<GrossNetCostDTO> dtos, bool doNotCheckHoliday, List<HolidayDTO> holidays, List<DayType> dayTypes)
        {
            List<int> employeeIds = dtos.Select(d => d.EmployeeId).Distinct().ToList();
            if (employeeIds.IsNullOrEmpty())
                return;

            List<Employee> employees = (from e in entities.Employee
                                         .Include("Employment.OriginalEmployeeGroup.DayType")
                                         .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                                        where employeeIds.Contains(e.EmployeeId)
                                        select e).ToList();

            foreach (Employee employee in employees)
            {
                TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
                cache.SetEmployee(employee);
                cache.SetDayTypesWithStandardWeekDay(dayTypes);
                if (!doNotCheckHoliday)
                    GetCompanyCache().SetHolidaysWithDayType(holidays);
            }

            foreach (var group in dtos.GroupBy(i => i.EmployeeId))
            {
                if (IsHiddenEmployeeFromCache(group.Key))
                    continue;

                DateTime? prevDate = null;
                int? prevDayTypeId = null;
                foreach (var dto in group.OrderBy(g => g.StartTime.Date))
                {
                    if (dto.StartTime.Date != prevDate)
                        prevDayTypeId = GetDayTypeForEmployeeFromCache(group.Key, dto.StartTime.Date, doNotCheckHoliday)?.DayTypeId;
                    dto.CalculatedDayTypeId = prevDayTypeId;
                    prevDate = dto.StartTime.Date;
                }
            }
        }

        #endregion

        #region Employee

        private void AddEmployeesWithEmploymentToCache(List<int> employeeIds, bool onlyActive = true)
        {
            GetEmployeesWithEmployment(employeeIds, onlyActive);
        }
        private bool TryGetAllEmployeesFromCache(bool onlyActive, out List<Employee> cachedEmployees)
        {
            cachedEmployees = employeeCacheRepository?.Where(cache => cache.GetEmployee() != null).Select(cache => cache.GetEmployee()).ToList() ?? new List<Employee>();
            if (onlyActive)
                cachedEmployees = cachedEmployees.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            return this.isEmployeeCacheForAllEmployees;
        }
        private void AddEmployeesToCache(List<Employee> employees, bool isAllEmployees = false)
        {
            foreach (Employee employee in employees.Where(x => x.EmployeeId > 0).ToList())
            {
                TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
                if (cache.GetEmployee() == null)
                    cache.SetEmployee(employee);
            }

            if (isAllEmployees)
                this.isEmployeeCacheForAllEmployees = true;
        }
        private Employee GetEmployeeFromCache(int employeeId, bool getHidden = true)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            Employee employee = cache.GetEmployee();
            if (employee == null || !employee.IsHiddenValid(getHidden))
            {
                employee = GetEmployee(employeeId, getHidden);
                cache.SetEmployee(employee);
            }
            return employee;
        }
        private Employee GetEmployeeWithContactPersonAndEmploymentFromCache(int employeeId, bool getHidden = true, bool onlyActive = true)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            Employee employee = cache.GetEmployee();
            if (employee == null || !employee.IsHiddenValid(getHidden))
            {
                employee = GetEmployeeWithContactPersonAndEmployment(employeeId, getHidden, onlyActive);
                cache.SetEmployee(employee);
            }
            else
                employee.LoadEmploymentsAndEmploymentChangeBatch();
            return employee;
        }
        private Employee GetEmployeeWithContactPersonFromCache(int employeeId, bool getHidden = true)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            Employee employee = cache.GetEmployee();
            if (employee == null || !employee.IsHiddenValid(getHidden))
            {
                employee = GetEmployeeWithContactPerson(employeeId, getHidden);
                cache.SetEmployee(employee);
            }
            else if (employee.ContactPerson == null && !employee.ContactPersonReference.IsLoaded)
                employee.ContactPersonReference.Load();
            return employee;
        }
        private int? GetEmployeeGroupIdFromCache(int employeeId, DateTime date)
        {
            return GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId)?.GetEmployeeGroupId(date);
        }
        private bool HasEmployeeHighRiskProtectionFromCache(Employee employee, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            bool? hasHighRiskProtection = cache.HasEmployeeHighRiskProtection(date);
            if (!hasHighRiskProtection.HasValue)
            {
                hasHighRiskProtection = HasEmployeeHighRiskProtection(employee, date);
                cache.SetEmployeeHighRiskProtection(date, hasHighRiskProtection.Value);
            }
            return hasHighRiskProtection.Value;
        }
        private DateTime? GetEmployeeBirthDateFromCache(Employee employee)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            if (!cache.TryGetEmployeeBirthDate(out DateTime? birthDate))
            {
                birthDate = GetEmployeeBirthDate(employee);
                cache.SetBirthDate(birthDate);
            }
            return birthDate;
        }

        #endregion

        #region EmployeeChild

        private EmployeeChild GetEmployeeChildFromCache(int employeeId, int employeeChildId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            EmployeeChild employeeChild = cache.GetEmployeeChild(employeeChildId);
            if (employeeChild == null)
            {
                employeeChild = GetEmployeeChild(employeeChildId);
                if (employeeChild != null)
                    cache.AddEmployeeChild(employeeChild);
            }
            return employeeChild;
        }

        #endregion

        #region EmployeeFactor

        private List<EmployeeFactor> GetEmployeeFactorsFromCache(int employeeId, TermGroup_EmployeeFactorType type)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<EmployeeFactor> employeeFactors = cache.GetEmployeeFactors();
            if (employeeFactors == null)
            {
                employeeFactors = GetEmployeeFactors(employeeId);
                if (employeeFactors != null)
                    cache.SetEmployeeFactors(employeeFactors);
            }
            return employeeFactors?.Where(f => f.Type == (int)type).ToList() ?? new List<EmployeeFactor>();
        }
        private EmployeeFactor GetEmployeeFactorFromCache(int employeeId, TermGroup_EmployeeFactorType type, DateTime date)
        {
            return GetEmployeeFactorsFromCache(employeeId, type).GetEmployeeFactor(date);
        }

        #endregion

        #region EmployeeGroup

        private EmployeeGroup GetEmployeeGroupFromCache(int employeeId, DateTime date, bool getHidden = true)
        {
            Employee employee = GetEmployeeFromCache(employeeId, getHidden);
            if (employee == null || !employee.IsHiddenValid(getHidden))
                return null;
            return employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache());
        }
        private int GetBreakDayMinutesAfterMidnightFromCache(int employeeId, DateTime date)
        {
            return GetEmployeeGroupFromCache(employeeId, date)?.BreakDayMinutesAfterMidnight ?? 0;
        }
        private List<AttestTransitionDTO> GetAttestTransitionsForEmployeeGroupFromCache(int employeeGroupId, TermGroup_AttestEntity entity)
        {
            CacheConfig config = CacheConfig.Company(ActorCompanyId, employeeGroupId, (int)entity);
            return base.GetAttestTransitionsForEmployeeGroupFromCache(entities, employeeGroupId, entity, config);
        }

        #endregion

        #region EmployeeSchedule

        private EmployeeSchedule GetEmployeeScheduleFromCache(int employeeId, int employeeScheduleId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            EmployeeSchedule employeeSchedule = cache.GetEmployeeSchedule(employeeScheduleId);
            if (employeeSchedule == null)
            {
                employeeSchedule = GetEmployeeSchedule(employeeScheduleId, employeeId);
                if (employeeSchedule != null)
                    cache.AddEmployeeSchedule(employeeSchedule);
            }
            return employeeSchedule;
        }
        private EmployeeSchedule GetEmployeeScheduleFromCache(int employeeId, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            EmployeeSchedule employeeSchedule = cache.GetEmployeeSchedule(date);
            if (employeeSchedule == null)
            {
                employeeSchedule = GetEmployeeSchedule(employeeId, date);
                if (employeeSchedule != null)
                    cache.AddEmployeeSchedule(employeeSchedule);
            }
            return employeeSchedule;
        }
        private void AddEmployeeSchedulesToCache(List<EmployeeSchedule> employeeSchedules)
        {
            foreach (EmployeeSchedule employeeSchedule in employeeSchedules)
            {
                TimeEngineEmployeeCache cache = GetEmployeeCache(employeeSchedule.EmployeeId);
                if (cache.GetEmployeeSchedule(employeeSchedule.StartDate) == null)
                    cache.AddEmployeeSchedule(employeeSchedule);
            }
        }

        #endregion

        #region EmployeeSetting

        private void InitEmployeeSettingsCache(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<EmployeeSetting> employeeSettings = EmployeeManager.GetEmployeeSettings(this.entities, base.ActorCompanyId, employeeId, dateFrom, dateTo);

            DateTime date = dateFrom.Date;
            while(date <= dateTo)
            {
                cache.SetEmployeeSettings(date, employeeSettings.FilterByDate(date));
                date = date.AddDays(1);
            }
        }

        private List<EmployeeSetting> GetEmployeeSettingsFromCache(int employeeId, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<EmployeeSetting> employeeSettings = cache.GetEmployeeSettings(date);
            if (employeeSettings == null)
            {
                employeeSettings = EmployeeManager.GetEmployeeSettings(this.entities, base.ActorCompanyId, employeeId, date, date);
                if (employeeSettings != null)
                    cache.SetEmployeeSettings(date, employeeSettings);
            }
            return employeeSettings;
        }

        #endregion

        #region EmployeeTimePeriod

        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsFromCache(int employeeId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<EmployeeTimePeriod> employeeTimePeriods = cache.GetEmployeeTimePeriods();
            if (employeeTimePeriods == null)
            {
                employeeTimePeriods = GetEmployeeTimePeriodsWithTimePeriod(employeeId);
                cache.SetEmployeeTimePeriods(employeeTimePeriods);
            }
            return employeeTimePeriods;
        }

        #endregion

        #region EmployeeVacationSE

        private EmployeeVacationSE GetEmployeeVacationSEFromCache(int employeeId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            EmployeeVacationSE employeeVacationSE = cache.GetEmployeeVacationSE();
            if (employeeVacationSE == null)
            {
                employeeVacationSE = GetEmployeeVacationSEFromEmployee(employeeId);
                cache.SetEmployeeVacationSE(employeeVacationSE);
            }
            return employeeVacationSE;
        }

        #endregion

        #region EmploymentAccountStd

        private List<EmploymentAccountStd> GetEmploymentAccountingFromCache(Employment employment)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employment.EmployeeId);
            List<EmploymentAccountStd> employmentAccountStds = cache.GetEmploymentAccountStds(employment.EmploymentId);
            if (employmentAccountStds == null)
            {
                employmentAccountStds = GetEmploymentAccountStds(employment.EmploymentId);
                cache.AddEmploymentAccountStds(employment.EmploymentId, employmentAccountStds);
            }
            return employmentAccountStds;
        }

        #endregion

        #region EmploymentVacationGroup

        private List<EmploymentVacationGroup> GetEmploymentVacationGroupWithVacationGroupFromCache(int employeeId, int employmentId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<EmploymentVacationGroup> employmentVacationGroups = cache.GetEmploymentVacationGroupWithVacationGroup(employmentId);
            if (employmentVacationGroups == null)
            {
                employmentVacationGroups = GetEmploymentVacationGroupsWithVacationGroup(employmentId);
                cache.AddEmploymentVacationGroupWithVacationGroup(employmentId, employmentVacationGroups);
            }
            return employmentVacationGroups;
        }

        private VacationGroupDTO GetVacationGroupFromCache(int employeeId, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            var (employmentVacationGroup, exists) = cache.GetEmploymentVacationGroupWithVacationGroup(date);
            if (!exists)
            {
                employmentVacationGroup = GetEmploymentVacationGroupWithVacationGroup(date, employeeId);
                cache.AddEmploymentVacationGroupWithVacationGroup(date, employmentVacationGroup);
            }

            VacationGroupDTO dto = employmentVacationGroup?.VacationGroup?.ToDTO();
            if (dto != null)
                dto.RealDateFrom = dto.CalculateFromDate(date);

            return dto;
        }

        private List<VacationGroupSEDayType> GetVacationGroupSEDayTypesFromCache(int employeeId, int vacationGroupSEId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            var (vacationGroupSEDayTypes, exists) = cache.GetVacationGroupSEDayType(vacationGroupSEId);
            if (!exists)
            {
                vacationGroupSEDayTypes = GetVacationGroupSEDayTypes(vacationGroupSEId);
                cache.AddVacationGroupSEDayType(vacationGroupSEId, vacationGroupSEDayTypes);
            }
            return vacationGroupSEDayTypes;
        }

        #endregion

        #region ExtraShiftCalculation

        private ExtraShiftCalculationPeriod GetExtraShiftCalculationFromCache(Employee employee, DateTime periodDateFrom, DateTime periodDateTo, bool useIgnoreIfExtraShifts)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            ExtraShiftCalculationPeriod extraShiftCalculation = cache.GetExtraShiftCalculationPeriod(periodDateFrom, periodDateTo, useIgnoreIfExtraShifts);
            if (extraShiftCalculation == null)
            {
                extraShiftCalculation = GetExtraShiftCalculation(employee, periodDateFrom, periodDateTo, useIgnoreIfExtraShifts);
                AddExtraShiftCalculationToCache(employee.EmployeeId, extraShiftCalculation);
            }
            return extraShiftCalculation;
        }
        private void AddExtraShiftCalculationToCache(int employeeId, ExtraShiftCalculationPeriod extraShiftCalculation)
        {
            if (extraShiftCalculation == null)
                return;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            cache.AddExtraShiftCalculation(extraShiftCalculation);
        }

        #endregion

        #region FixedPayrollRow

        private List<FixedPayrollRowDTO> GetFixedPayrollRowsFromCache(int employeeId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<FixedPayrollRowDTO> fixedPayrollRows = cache.GetFixedPayrollRowDTOs();
            if (fixedPayrollRows == null)
            {
                fixedPayrollRows = GetFixedPayrollRows(employeeId).ToDTOs(false).ToList();
                cache.AddFixedPayrollRowDTOs(fixedPayrollRows);
            }
            return fixedPayrollRows;
        }
        private void AddFixedPayrollRowsToCache(List<int> employeeIds, List<FixedPayrollRowDTO> fixedPayrollRows)
        {
            if (fixedPayrollRows == null)
                return;

            Dictionary<int, List<FixedPayrollRowDTO>> dict = fixedPayrollRows.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, l => l.ToList());
            foreach (int employeeId in employeeIds)
            {
                TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
                //Save an empty list if no payrollrows are found so we dont do a db fetch again
                cache.AddFixedPayrollRowDTOs(dict.ContainsKey(employeeId) ? dict[employeeId] : new List<FixedPayrollRowDTO>());
            }
        }

        #endregion

        #region PayrollStartValueRow

        private List<PayrollStartValueRow> GetPayrollStartValueRowsForEmployeeFromCache(int employeeId, DateTime date, int? sysPayrollTypeLevel3 = null)
        {
            if (!HasCompanyPayrollStartValueFromCache(date))
                return null;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<PayrollStartValueRow> payrollStartValueRows = cache.GetPayrollStartValueRows(date);
            if (payrollStartValueRows == null)
            {
                payrollStartValueRows = GetPayrollStartValueRowsForEmployee(employeeId, date);
                cache.SetPayrollStartValueRows(date, payrollStartValueRows);
            }
            if (sysPayrollTypeLevel3.HasValue)
                payrollStartValueRows = payrollStartValueRows.Filter(sysPayrollTypeLevel3.Value);

            return payrollStartValueRows;
        }
        private int GetPayrollStartValueRowScheduleMinutesFromCache(int employeeId, DateTime date, int? sysPayrollTypeLevel3 = null)
        {
            return GetPayrollStartValueRowsForEmployeeFromCache(employeeId, date, sysPayrollTypeLevel3)?.Where(p => p.ScheduleTimeMinutes.HasValue).Sum(p => p.ScheduleTimeMinutes.Value) ?? 0;
        }
        private bool HasPayrollStartValueRowScheduleMinutesFromCache(int employeeId, DateTime date, int? sysPayrollTypeLevel3 = null)
        {
            return GetPayrollStartValueRowsForEmployeeFromCache(employeeId, date, sysPayrollTypeLevel3)?.Any(p => p.ScheduleTimeMinutes.HasValue && p.ScheduleTimeMinutes.Value > 0) ?? false;
        }

        #endregion

        #region SicknessPeriod

        private SicknessPeriod GetEmployeeSicknessPeriodFromCache(int employeeId, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            return cache.GetSicknessPeriod(date);
        }
        private void AddEmployeeSicknessPeriodToCache(int employeeId, SicknessPeriod sicknessPeriod)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            cache.AddSicknessPeriod(sicknessPeriod);
        }
        private void ClearEmployeeSicknessPeriodFromCache(int employeeId, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            cache.RemoveSicknessPeriod(date);
        }

        #endregion

        #region SicknessSalary

        private bool HasEmployeeRightToSicknessSalaryFromCache(Employee employee, DateTime date)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employee.EmployeeId);
            bool? hasEmployeeRightToSicknessSalary = cache.HasEmployeeRightToSicknessSalary(date);
            if (!hasEmployeeRightToSicknessSalary.HasValue)
            {
                hasEmployeeRightToSicknessSalary = HasEmployeeRightToSicknessSalary(employee, date);
                cache.AddHasEmployeeRightToSicknessSalary(date, hasEmployeeRightToSicknessSalary.Value);
            }
            return hasEmployeeRightToSicknessSalary.Value;
        }

        #endregion

        #region TimeAccumulatorBalance

        public List<TimeAccumulatorBalance> GetTimeAccumulatorBalancesFromCache(int employeeId)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeAccumulatorBalance> timeAccumulatorBalances = cache.GetTimeAccumulatorBalances();
            if (timeAccumulatorBalances == null)
            {
                timeAccumulatorBalances = GetTimeAccumulatorBalances(employeeId);
                cache.SetTimeAccumulatorBalances(timeAccumulatorBalances);
            }
            return timeAccumulatorBalances;
        }

        #endregion

        #region TimeBlockDate

        private List<TimeBlockDate> GetTimeBlockDatesFromCache(int employeeId, IEnumerable<int> timeBlockDateIds)
        {
            if (timeBlockDateIds.IsNullOrEmpty())
                return new List<TimeBlockDate>();

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeBlockDate> timeBlockDates = cache.GetTimeBlockDates(timeBlockDateIds, out List<int> timeBlockDateIdsNotCached);
            if (timeBlockDateIdsNotCached.Any())
            {
                List<TimeBlockDate> timeBlockDatesNotCached = GetTimeBlockDates(employeeId, timeBlockDateIdsNotCached);
                timeBlockDatesNotCached.ForEach(timeBlockDate => cache.AddTimeBlockDate(timeBlockDate));
                timeBlockDates.AddRange(timeBlockDatesNotCached);
            }
            return timeBlockDates;
        }
        private List<TimeBlockDate> GetTimeBlockDatesFromCache(int employeeId, IEnumerable<DateTime> dates)
        {
            if (dates.IsNullOrEmpty())
                return new List<TimeBlockDate>();

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeBlockDate> timeBlockDates = cache.GetTimeBlockDates(dates, out List<DateTime> datesNotCached);
            if (datesNotCached.Any())
            {
                List<TimeBlockDate> timeBlockDatesNotCached = GetTimeBlockDates(employeeId, datesNotCached);
                timeBlockDatesNotCached.ForEach(timeBlockDate => cache.AddTimeBlockDate(timeBlockDate));
                timeBlockDates.AddRange(timeBlockDatesNotCached);
            }
            return timeBlockDates;
        }
        private List<TimeBlockDate> GetTimeBlockDatesFromCache(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            if (dateFrom > dateTo)
                return new List<TimeBlockDate>();

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);

            List<TimeBlockDate> timeBlockDates = new List<TimeBlockDate>();
            bool fetchedFromCache = true;

            DateTime currentDate = dateFrom;
            while (currentDate <= dateTo)
            {
                var (timeBlockDate, exists) = cache.GetTimeBlockDate(currentDate);
                if (!exists)
                {
                    fetchedFromCache = false; //Only get from cache if entire interval exists
                    break;
                }

                if (timeBlockDate != null)
                    timeBlockDates.Add(timeBlockDate);
                currentDate = currentDate.AddDays(1);
            }

            if (!fetchedFromCache)
            {
                if (timeBlockDates.Any())
                    timeBlockDates.Clear();
                timeBlockDates.AddRange(GetTimeBlockDates(employeeId, dateFrom, dateTo));
                cache.AddTimeBlockDates(timeBlockDates, dateFrom, dateTo);
            }

            return timeBlockDates;
        }
        private TimeBlockDate GetTimeBlockDateFromCache(int employeeId, int timeBlockDateId)
        {
            if (employeeId == 0 || timeBlockDateId == 0)
                return null;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            TimeBlockDate timeBlockDate = cache.GetTimeBlockDate(timeBlockDateId);
            if (timeBlockDate == null)
            {
                timeBlockDate = GetTimeBlockDate(timeBlockDateId, employeeId);
                if (timeBlockDate != null)
                    cache.AddTimeBlockDate(timeBlockDate);
            }
            return timeBlockDate;
        }
        private TimeBlockDate GetTimeBlockDateFromCache(int employeeId, DateTime date, bool createIfNotExists = false)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            var (timeBlockDate, exists) = cache.GetTimeBlockDate(date);
            if (!exists)
            {
                timeBlockDate = GetTimeBlockDate(employeeId, date, createIfNotExists);
                cache.AddTimeBlockDate(timeBlockDate);
            }
            else if (timeBlockDate == null && createIfNotExists)
            {
                timeBlockDate = CreateTimeBlockDate(employeeId, date);
                cache.AddTimeBlockDate(timeBlockDate);
            }
            return timeBlockDate;
        }
        private void AddTimeBlockDatesToCache(int employeeId, List<DateTime> dates)
        {
            AddTimeBlockDatesToCache(employeeId, GetTimeBlockDates(employeeId, dates));
        }
        private void AddTimeBlockDatesToCache(int employeeId, List<TimeBlockDate> timeBlockDates)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            foreach (var timeBlockDatesByDate in timeBlockDates.Where(tbd => tbd.EmployeeId == employeeId).GroupBy(tbd => tbd.Date))
            {
                cache.AddTimeBlockDate(timeBlockDatesByDate.First());
            }
        }
        private void CreateTimeBlockDateIfNotExists(int employeeId, DateTime date)
        {
            GetTimeBlockDateFromCache(employeeId, date, createIfNotExists: true);
        }

        #endregion

        #region TimeScheduleTemplateBlock

        private DateTime? GetScheduleInFromCache(int employeeId, DateTime date, bool nextDay = false)
        {
            DateTime? scheduleIn = GetScheduleBlocksFromCache(employeeId, date)?.GetScheduleIn();
            if (scheduleIn.HasValue && nextDay)
                scheduleIn = scheduleIn.Value.AddDays(1);
            return scheduleIn;
        }
        private DateTime? GetScheduleOutFromCache(int employeeId, DateTime date)
        {
            return GetScheduleBlocksFromCache(employeeId, date)?.GetScheduleOut();
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksFromCache(int employeeId, DateTime dateFrom, DateTime dateTo, int? timeScheduleScenarioHeadId = null)
        {
            var result = new List<TimeScheduleTemplateBlock>();
            var currentDate = dateFrom.Date;
            while (currentDate <= dateTo.Date)
            {
                var scheduleBlocks = GetScheduleBlocksFromCache(employeeId, currentDate, timeScheduleScenarioHeadId);
                if (!scheduleBlocks.IsNullOrEmpty())
                    result.AddRange(scheduleBlocks);
                currentDate = currentDate.AddDays(1);
            }
            return result;
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksFromCache(int employeeId, DateTime date, int? timeScheduleScenarioHeadId = null)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeScheduleTemplateBlock> scheduleBlocks = cache.GetScheduleBlocks(date);
            if (scheduleBlocks == null)
            {
                scheduleBlocks = GetScheduleBlocksForEmployeeDiscardScenario(employeeId, date);
                cache.AddScheduleBlocks(scheduleBlocks);
            }
            scheduleBlocks = scheduleBlocks.FilterScenario(timeScheduleScenarioHeadId);
            return scheduleBlocks;
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksInOvertimePeriod(int employeeId, DateTime currentDate)
        {
            var (periodStart, periodStop) = GetDatesForOvertimePeriod(currentDate, true);
            return GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employeeId, periodStart, periodStop);
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksWithTimeCodeAndStaffingFromCache(int employeeId, DateTime date, int? timeScheduleScenarioHeadId = null, bool includeStandBy = false, bool includeOnDuty = false)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeScheduleTemplateBlock> scheduleBlocks = cache.GetScheduleBlocksWithTimeCodeAndStaffing(date);
            if (scheduleBlocks == null)
            {
                if (UseStaffing())
                    scheduleBlocks = GetScheduleBlocksForEmployeeWithTimeCodeAndStaffingDiscardScenario(employeeId, date);
                else
                    scheduleBlocks = GetScheduleBlocksForEmployeeWithTimeCodeDiscardScenario(employeeId, date);
                cache.AddScheduleBlocksWithTimeCodeAndStaffing(scheduleBlocks);
                cache.AddScheduleBlocks(scheduleBlocks);
            }
            scheduleBlocks = scheduleBlocks.FilterScenario(timeScheduleScenarioHeadId);
            scheduleBlocks = scheduleBlocks.FilterScheduleType(includeStandBy, includeOnDuty);
            return scheduleBlocks;
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksWithTimeCodeAndStaffingFromCache(int employeeId, DateTime dateFrom, DateTime dateTo, int? timeScheduleScenarioHeadId = null, bool includeStandBy = false, bool includeOnDuty = false)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeScheduleTemplateBlock> scheduleBlocks = cache.GetScheduleBlocks(CalendarUtility.GetDates(dateFrom, dateTo));
            if (scheduleBlocks == null)
            {
                if (UseStaffing())
                    scheduleBlocks = GetScheduleBlocksForEmployeeWithTimeCodeAndStaffingDiscardScenario(employeeId, dateFrom, dateTo);
                else
                    scheduleBlocks = GetScheduleBlocksForEmployeeWithTimeCodeDiscardScenario(employeeId, dateFrom, dateTo);
            }
            cache.AddScheduleBlocksWithTimeCodeAndStaffing(scheduleBlocks);
            cache.AddScheduleBlocks(scheduleBlocks);
            scheduleBlocks = scheduleBlocks.FilterScenario(timeScheduleScenarioHeadId);
            scheduleBlocks = scheduleBlocks.FilterScheduleType(includeStandBy, includeOnDuty);
            return scheduleBlocks;
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksWithTimeCodeAndStaffingFromCache(int employeeId, List<DateTime> dates, int? timeScheduleScenarioHeadId = null, bool includeStandBy = false, bool includeOnDuty = false)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeScheduleTemplateBlock> scheduleBlocks = cache.GetScheduleBlocks(dates);
            if (scheduleBlocks == null)
            {
                if (UseStaffing())
                    scheduleBlocks = GetScheduleBlocksForEmployeeWithTimeCodeAndStaffingDiscardScenario(employeeId, dates);
                else
                    scheduleBlocks = GetScheduleBlocksForEmployeeWithTimeCodeDiscardScenario(employeeId, dates);
            }
            cache.AddScheduleBlocksWithTimeCodeAndStaffing(scheduleBlocks);
            cache.AddScheduleBlocks(scheduleBlocks);
            scheduleBlocks = scheduleBlocks.FilterScenario(timeScheduleScenarioHeadId);
            scheduleBlocks = scheduleBlocks.FilterScheduleType(includeStandBy, includeOnDuty);
            return scheduleBlocks;
        }
        private List<TimeScheduleTemplateBlock> GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(int? timeScheduleScenarioHeadId, int employeeId, DateTime date, bool includeStandBy = false, bool includeOnDuty = false)
        {
            return GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employeeId, date, timeScheduleScenarioHeadId, includeStandBy: includeStandBy, includeOnDuty: includeOnDuty).DiscardZero();
        }
        private void DetachSchedule(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (scheduleBlocks.IsNullOrEmpty())
                return;
            ClearScheduleFromCache(scheduleBlocks);
            base.TryDetachEntitys(entities, scheduleBlocks); // Detach schedule scheduleBlocks to prevent them from being saved
        }
        private void ClearScheduleFromCache(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (scheduleBlocks.IsNullOrEmpty())
                return;
            foreach (var scheduleBlocksByEmployee in scheduleBlocks.Where(b => b.EmployeeId.HasValue && b.Date.HasValue).GroupBy(b => b.EmployeeId.Value))
            {
                ClearScheduleFromCache(scheduleBlocksByEmployee.Key, scheduleBlocksByEmployee.Select(b => b.Date.Value).ToList());
            }
        }
        private void ClearScheduleFromCache(int employeeId, List<DateTime> dates)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            cache.ClearScheduleFromCache(dates);
        }

        #endregion

        #region TimeScheduleTemplateHead

        private void AddTimeScheduleTemplateHeadsToCache(List<TimeScheduleTemplateHead> timeScheduleTemplateHeads)
        {
            foreach (TimeScheduleTemplateHead timeScheduleTemplateHead in timeScheduleTemplateHeads.Where(w => w.EmployeeId.HasValue))
            {
                TimeEngineEmployeeCache cache = GetEmployeeCache(timeScheduleTemplateHead.EmployeeId.Value);
                if (cache.GetTimeScheduleTemplateHead(timeScheduleTemplateHead.StartDate.Value) == null)
                    cache.AddTimeScheduleTemplateHead(timeScheduleTemplateHead);
            }
        }

        #endregion

        #region TimeScheduleTemplateGroup

        private void AddTimeScheduleTemplateGroupsToCache(List<TimeScheduleTemplateGroupDTO> timeScheduleTemplateGroups)
        {
            foreach (TimeScheduleTemplateGroupDTO timeScheduleTemplateGroup in timeScheduleTemplateGroups)
            {
                foreach (TimeScheduleTemplateGroupEmployeeDTO timeScheduleTemplateGroupEmployee in timeScheduleTemplateGroup.Employees.Where(w => w.State == (int)SoeEntityState.Active))
                {
                    TimeEngineEmployeeCache cache = GetEmployeeCache(timeScheduleTemplateGroupEmployee.EmployeeId);
                    cache.AddTimeScheduleTemplateGroup(timeScheduleTemplateGroup);
                }
            }
        }

        #endregion

        #region TimeScheduleTemplatePeriod

        private bool AddTimeScheduleTemplatePeriodsToCache(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            var dict = GetTimeScheduleTemplatePeriodsGroupedByDate(employeeId, dateFrom, dateTo);
            AddTimeScheduleTemplatePeriodsToCache(employeeId, dict);
            return dict?.Keys != null && dict.Keys.Any();
        }
        private void AddTimeScheduleTemplatePeriodsToCache(int employeeId, Dictionary<DateTime, TimeScheduleTemplatePeriod> dict)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            foreach (var pair in dict)
            {
                cache.AddTimeScheduleTemplatePeriod(pair.Key, pair.Value);
            }
        }
        private int? GetTimeScheduleTemplatePeriodIdFromCache(int employeeId, DateTime date)
        {
            return GetTimeScheduleTemplatePeriodFromCache(employeeId, date)?.TimeScheduleTemplatePeriodId;
        }
        private TimeScheduleTemplatePeriod GetTimeScheduleTemplatePeriodFromCache(int employeeId, DateTime date, bool checkEmployeeSchedule = true)
        {
            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = cache.GetTimeScheduleTemplatePeriod(date);
            if (timeScheduleTemplatePeriod == null)
            {
                timeScheduleTemplatePeriod = GetTimeScheduleTemplatePeriod(employeeId, date, checkEmployeeSchedule);
                cache.AddTimeScheduleTemplatePeriod(date, timeScheduleTemplatePeriod);
            }
            return timeScheduleTemplatePeriod;
        }

        #endregion

        #region TimeCodeTransaction

        private List<TimeCodeTransaction> GetTimeCodeTransactionsForDayDiscardStateFromCache(int employeeId, int timeBlockDateId)
        {
            if (timeBlockDateId < 0)
                return null;

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimeCodeTransaction> timeCodeTransactions = cache.GetTimeCodeTransactions(timeBlockDateId);
            if (timeCodeTransactions == null)
            {
                timeCodeTransactions = GetTimeCodeTransactionsDiscardState(timeBlockDateId);
                cache.AddTimeCodeTransactions(timeBlockDateId, timeCodeTransactions);
            }
            return timeCodeTransactions;
        }

        #endregion

        #region TimePayrollTransaction

        private List<TimePayrollTransaction> GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(int employeeId, DateTime? dateFrom, DateTime dateTo, int sysPayrollTypeLevel3, int? sysPayrollTypeLevel4 = null, EmployeeChild employeeChild = null)
        {
            List<TimeBlockDate> timeBlockDates = null;
            if (dateFrom.HasValue)
                timeBlockDates = GetTimeBlockDatesFromCache(employeeId, CalendarUtility.GetBeginningOfDay(dateFrom), CalendarUtility.GetEndOfDay(dateTo));

            List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, sysPayrollTypeLevel3, sysPayrollTypeLevel4, dateTo, employeeChild);
            timePayrollTransactions = timePayrollTransactions.Where(t => t.TimeBlockDate.Date <= dateTo).ToList();

            return timePayrollTransactions;
        }
        private List<TimePayrollTransaction> GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(int employeeId, List<TimeBlockDate> timeBlockDates, int sysPayrollTypeLevel3, int? sysPayrollTypeLevel4 = null, DateTime? dateTo = null, EmployeeChild employeeChild = null)
        {
            if (timeBlockDates.IsNullOrEmpty() && !dateTo.HasValue)
                return new List<TimePayrollTransaction>();
            if (timeBlockDates.IsNullOrEmpty() && (employeeChild == null && sysPayrollTypeLevel3 != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService_Total))
                return new List<TimePayrollTransaction>();

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);

            List<TimePayrollTransaction> timePayrollTransactions = cache.GetTimePayrollTransactionsWithTimeBlockDate(timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached, sysPayrollTypeLevel3, employeeChild);
            if (timePayrollTransactions != null)
            {
                //Handle days not present in cache
                if (!timeBlockDatesNotCached.IsNullOrEmpty())
                {
                    List<TimePayrollTransaction> timePayrollTransactionsNotCached = GetTimePayrollTransactionsForLevel3WithTimeBlockDate(employeeId, timeBlockDatesNotCached, sysPayrollTypeLevel3, employeeChild, isReloadForTimeBlockDatesNotCached: true);
                    timePayrollTransactions.AddRange(timePayrollTransactionsNotCached);
                    cache.AddTimePayrollTransactionsWithTimeBlockDate(timeBlockDatesNotCached, timePayrollTransactionsNotCached, sysPayrollTypeLevel3, employeeChild);
                }
            }
            else
            {
                //For EmployeeChild, count all days, both back and future
                if (!timeBlockDates.IsNullOrEmpty() || employeeChild != null)
                    timePayrollTransactions = GetTimePayrollTransactionsForLevel3WithTimeBlockDate(employeeId, timeBlockDates, sysPayrollTypeLevel3, employeeChild);
                else if (dateTo.HasValue)
                    timePayrollTransactions = GetTimePayrollTransactionsForLevel3WithTimeBlockDate(employeeId, dateTo.Value, sysPayrollTypeLevel3);

                if (timeBlockDates.IsNullOrEmpty())
                    timeBlockDates = timePayrollTransactions.Select(i => i.TimeBlockDate).Distinct().ToList();
                cache.AddTimePayrollTransactionsWithTimeBlockDate(timeBlockDates, timePayrollTransactions, sysPayrollTypeLevel3, employeeChild);
            }

            //Filter on level4 if passed. But is still cached on level3
            if (sysPayrollTypeLevel4.HasValue)
                timePayrollTransactions = timePayrollTransactions.Where(i => i.SysPayrollTypeLevel4 == sysPayrollTypeLevel4.Value).ToList();

            return timePayrollTransactions;
        }
        private List<TimePayrollTransaction> GetAbsenceVacationReplacementTimePayrollTransactionWithTimeBlockDateFromCache(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);
            List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employeeId, dateFrom, dateTo);
            return GetAbsenceVacationReplacementTimePayrollTransactionWithTimeBlockDateFromCache(employeeId, timeBlockDates);
        }
        private List<TimePayrollTransaction> GetAbsenceVacationReplacementTimePayrollTransactionWithTimeBlockDateFromCache(int employeeId, List<TimeBlockDate> timeBlockDates)
        {
            if (timeBlockDates.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            TimeEngineEmployeeCache cache = GetEmployeeCache(employeeId);
            List<TimePayrollTransaction> timePayrollTransactions = cache.GetVacationReplacementTimePayrollTransactionsWithTimeBlockDate(timeBlockDates, out List<TimeBlockDate> timeBlockDatesNotCached);
            if (timePayrollTransactions != null)
            {
                if (!timeBlockDatesNotCached.IsNullOrEmpty())
                {
                    List<TimePayrollTransaction> timePayrollTransactionsNotCached = GetTimePayrollTransactionsForVacationReplacement(employeeId, timeBlockDatesNotCached.Select(i => i.TimeBlockDateId).Distinct().ToList());
                    timePayrollTransactions.AddRange(timePayrollTransactionsNotCached);
                    cache.AddVacationReplacementTimePayrollTransactionsWithTimeBlockDate(timeBlockDatesNotCached, timePayrollTransactionsNotCached);
                }
            }
            else
            {
                timeBlockDates = timeBlockDates.OrderBy(i => i.Date).ToList();
                timePayrollTransactions = GetTimePayrollTransactionsForVacationReplacement(employeeId, timeBlockDates.First().Date, timeBlockDates.Last().Date);
                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                {
                    timePayrollTransaction.TimeBlockDate = timeBlockDates.FirstOrDefault(i => i.TimeBlockDateId == timePayrollTransaction.TimeBlockDateId);
                }

                //Insert to cache
                cache.AddVacationReplacementTimePayrollTransactionsWithTimeBlockDate(timeBlockDates, timePayrollTransactions);
            }

            return timePayrollTransactions;
        }
        private List<TimeBlockDate> GetDaysWithAbsenceFromCache(int employeeId, List<TimeBlockDate> timeBlockDates, int sysPayrollTypeLevel3, int? sysPayrollTypeLevel4 = null)
        {
            List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employeeId, timeBlockDates, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            return timeBlockDates.Where(tbd => timePayrollTransactions.Any(tpt => tpt.TimeBlockDateId == tbd.TimeBlockDateId)).OrderBy(i => i.Date).ToList();
        }
        private void ClearTimePayrollTransactionsWithTimeBlockDateFromCache(TimeBlockDate timeBlockDate)
        {
            if (timeBlockDate == null)
                return;

            TimeEngineEmployeeCache cache = GetEmployeeCache(timeBlockDate.EmployeeId);
            cache.ClearTimePayrollTransactionsWithTimeBlockDate(timeBlockDate);
        }

        #endregion

        #endregion

        #region Clear cache

        public void ClearCachedContent()
        {
            if (forcedExternalEntities)
                return;

            entities = null;
            this.templateRepository = new TimeEngineTemplateRepository();

            ClearCache();
        }
        private void ClearCache()
        {
            ClearSingleCache();
            ClearComplexCache();
        }
        private void ClearSingleCache()
        {
            cachedUser = null;
        }
        private void ClearComplexCache()
        {
            companyCacheRepository = null;
            employeeCacheRepository = null;
            isEmployeeCacheForAllEmployees = false;
        }

        #endregion
    }
}
