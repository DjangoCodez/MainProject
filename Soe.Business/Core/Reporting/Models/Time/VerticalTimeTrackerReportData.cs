using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class VerticalTimeTrackerReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly VerticalTimeTrackerReportDataInput _reportDataInput;
        private readonly VerticalTimeTrackerReportDataOutput _reportDataOutput;

        private bool? hasEmployeeSelection { get; set; }
        private bool hasEmployee
        {
            get
            {
                if (hasEmployeeSelection.HasValue)
                    return hasEmployeeSelection.Value;

                hasEmployeeSelection = _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_VerticalTimeTrackerMatrixColumns>(a.Column).Contains("Employee"));

                return hasEmployeeSelection.Value;
            }
        }

        bool? loadAccountInternal { get; set; }
        bool LoadAccountInternal
        {
            get
            {
                if (loadAccountInternal.HasValue)
                    return loadAccountInternal.Value;

                loadAccountInternal = _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
                return loadAccountInternal.Value;
            }
        }

        bool loadSchedule => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_VerticalTimeTrackerMatrixColumns>(a.Column).Contains("Schedule"));
        bool loadScheduleCost => _reportDataInput.Columns.Any(a => a.Column == TermGroup_VerticalTimeTrackerMatrixColumns.ScheduleCost);
        bool loadTimeCost => _reportDataInput.Columns.Any(a => a.Column == TermGroup_VerticalTimeTrackerMatrixColumns.TimeCost);

        public VerticalTimeTrackerReportData(ParameterObject parameterObject, VerticalTimeTrackerReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new VerticalTimeTrackerReportDataOutput(reportDataInput);
        }

        public static List<VerticalTimeTrackerReportDataField> GetPossibleDataFields()
        {
            List<VerticalTimeTrackerReportDataField> possibleFields = new List<VerticalTimeTrackerReportDataField>();
            EnumUtility.GetValues<TermGroup_VerticalTimeTrackerMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new VerticalTimeTrackerReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public VerticalTimeTrackerReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private bool hasScheduleColumns => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_VerticalTimeTrackerMatrixColumns>(a.Column).Contains("Schedule"));
        private bool hasTimeColumns => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_VerticalTimeTrackerMatrixColumns>(a.Column).Contains("Time"));

        private ActionResult LoadData()
        {
            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);

            TryGetBoolFromSelection(reportResult, out bool selectionIsCalendarDay, "selectionIsCalendarDay");
            TryGetBoolFromSelection(reportResult, out bool fillWholeDay, "selectionIsFillDay");

            employees = employees ?? EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
                    var orginalSelectionAccountIds = selectionAccountIds;
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var dims = base.GetAccountDimsFromCache(entitiesReadonly, DataCache.CacheConfig.Company(reportResult.ActorCompanyId));
                    dims.CalculateLevels();
                    var defaultAccountDim = AccountManager.GetDefaultEmployeeAccountDim(reportResult.ActorCompanyId);
                    var accounts = AccountManager.GetAccountsByCompany(reportResult.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs(true, true);

                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);
                    var selectedDims = new List<AccountDimDTO>();
                    List<int> selectionDimIds = new List<int>();
                    foreach (var col in matrixColumnsSelection.Columns)
                    {
                        var idValue = GetIdValueFromColumn(col);
                        if (idValue != 0 && col.Options?.Key != null && int.TryParse(col.Options.Key, out int value) && !selectionDimIds.Contains(value))
                            selectionDimIds.Add(value);
                    }
                    if (selectionDimIds.Any())
                        selectedDims = dims.Where(w => selectionDimIds.Contains(w.AccountDimId)).ToList();

                    if (selectedDims.Count > 3 && base.IsMartinServera())
                    {
                        selectedDims = selectedDims.OrderByDescending(o => o.Level).Take(2).ToList();
                        selectionDimIds = selectedDims.Select(s => s.AccountDimId).ToList();
                    }

                    var fetchFromDate = selectionDateFrom;
                    var fetchToDate = selectionDateTo;

                    if (selectionIsCalendarDay)
                    {
                        fetchFromDate = selectionDateFrom.AddDays(-1);
                        fetchToDate = selectionDateTo.AddDays(1);
                    }

                    LogCollector.LogInfo($"{Environment.MachineName} VerticalTimeTracker FetchFromDate: {fetchFromDate}, FetchToDate: {fetchToDate}");

                    if (base.IsMartinServera())
                    {
                        var matchingAccounts = selectionDimIds.Any() ? accounts.Where(f => selectionDimIds.Contains(f.AccountDimId) && !f.HierarchyOnly).ToList() : accounts.Where(w => !w.HierarchyOnly).ToList();
                        var employeeIds = employees.Select(s => s.EmployeeId).ToList();

                        var accountIdsOntransaction = entities.TimePayrollTransaction.Include("AccountInternal").Where(w => employeeIds.Contains(w.EmployeeId) && w.TimeBlockDate.Date >= fetchFromDate && w.TimeBlockDate.Date <= fetchToDate).SelectMany(s => s.AccountInternal.Select(ss => ss.AccountId)).Distinct().ToList();

                        var accountIdsOnSchedule = entities.TimeScheduleTemplateBlock.Where(w => w.EmployeeId.HasValue && employeeIds.Contains(w.EmployeeId.Value) && w.Date.HasValue && w.Date >= fetchFromDate && w.Date <= fetchToDate && w.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None).Select(ss => ss.AccountId).Distinct().Where(w => w.HasValue).Select(s => s.Value).ToList();

                        var accountInternalIdsOnScheduleBlock = entities.TimeScheduleTemplateBlock.Include("AccountInternal").Where(w => w.EmployeeId.HasValue && employeeIds.Contains(w.EmployeeId.Value) && w.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && w.Date.HasValue && w.Date >= fetchFromDate && w.Date <= fetchToDate).SelectMany(s => s.AccountInternal).Where(a => a != null).Select(s => s.AccountId).Distinct().ToList();

                        var endOfLastDate = CalendarUtility.GetEndOfDay(fetchToDate.Date);
                        var accountIdsOnTimeStamp = entities.TimeStampEntry.Where(w => employeeIds.Contains(w.EmployeeId) && w.Time >= fetchFromDate && w.Time <= endOfLastDate && w.AccountId.HasValue).Select(s => s.AccountId).Distinct().Select(s => s.Value).ToList();
                        var accountIdsOnTimeStampExtended = entities.TimeStampEntryExtended.Where(w => employeeIds.Contains(w.TimeStampEntry.EmployeeId) && w.TimeStampEntry.Time >= fetchFromDate && w.TimeStampEntry.Time <= endOfLastDate && w.AccountId.HasValue).Select(s => s.AccountId).Distinct().Select(s => s.Value).ToList();

                        matchingAccounts = matchingAccounts.Where(w => accountIdsOntransaction.Contains(w.AccountId) ||
                                                                       accountIdsOnSchedule.Contains(w.AccountId) ||
                                                                       accountInternalIdsOnScheduleBlock.Contains(w.AccountId) ||
                                                                       accountIdsOnTimeStamp.Contains(w.AccountId) ||
                                                                       accountIdsOnTimeStampExtended.Contains(w.AccountId)).ToList();

                        if (matchingAccounts != null)
                        {
                            selectionAccountIds = matchingAccounts.Select(s => s.AccountId).ToList();
                            selectionAccountIds = selectionAccountIds.Distinct().ToList();
                        }
                    }

                    List<StaffingStatisticsReportDataField> staffingStatisticsReportDataReportDataFields = new List<StaffingStatisticsReportDataField>();

                    foreach (var col in matrixColumnsSelection.Columns)
                    {
                        var idValue = GetIdValueFromColumn(col);

                        if (idValue != 0 && col.Options?.Key != null && int.TryParse(col.Options.Key, out int value))
                        {
                            EnumUtility.GetValue(col.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_StaffingStatisticsMatrixColumns), col.Options?.Key == null ? "" : col.Options.Key);

                            if (id != 0)
                            {
                                var staffingStatisticsMatrixColumns = (TermGroup_StaffingStatisticsMatrixColumns)id;
                                var clone = col.CloneDTO();
                                if (staffingStatisticsMatrixColumns == TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNames || staffingStatisticsMatrixColumns == TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNrs)
                                {
                                    staffingStatisticsReportDataReportDataFields.Add(new StaffingStatisticsReportDataField(clone));
                                }
                            }
                        }
                    }

                    var selectedAccountHierarchyFields = new List<StaffingStatisticsReportDataField>();
                    foreach (var accountHierarchyField in staffingStatisticsReportDataReportDataFields)
                    {
                        foreach (var dim in selectedDims)
                        {
                            if (accountHierarchyField.Selection.Field.Contains(dim.AccountDimId.ToString()))
                                selectedAccountHierarchyFields.Add(accountHierarchyField);
                        }
                    }

                    var selectionClone = matrixColumnsSelection.CloneDTO();
                    List<MatrixColumnSelectionDTO> matrixColumnSelections = new List<MatrixColumnSelectionDTO>();
                    foreach (var column in selectionClone.Columns)
                    {
                        foreach (var dim in selectedDims)
                        {
                            if (column.Field.Contains(dim.AccountDimId.ToString()))
                                matrixColumnSelections.Add(column);
                        }
                    }
                    selectionClone.Columns = matrixColumnSelections;
                    StaffingStatisticsReportData accountHierarchyReportData = new StaffingStatisticsReportData(parameterObject, reportDataInput: new StaffingStatisticsReportDataInput(reportResult, selectedAccountHierarchyFields));
                    StaffingStatisticsReportDataOutput output = accountHierarchyReportData.CreateOutput(reportResult);
                    var groupOnDim = accounts.GroupBy(g => g.AccountDimId);
                    List<AccountDTO> emptyAccounts = new List<AccountDTO>();

                    foreach (var dim in groupOnDim)
                    {
                        if (selectionDimIds.Contains(dim.Key))
                        {
                            emptyAccounts.Add(new AccountDTO() { AccountDimId = dim.Key, Name = "", AccountNr = "0", AccountId = -1 * dim.Key });
                        }
                    }

                    accounts.AddRange(emptyAccounts);
                    selectionAccountIds.AddRange(emptyAccounts.Select(s => s.AccountId).ToList());
                    var groupList = accountHierarchyReportData.GetStaffingStatisticsGroups(entities, matrixColumnsSelection: selectionClone, selectionAccountIds, selectedDims, dims, accounts, useAccountHierarchy, false);

                    var verticalTimeTrackerItems = new List<VerticalTimeTrackerItem>();
                    var loadTimeStamps = CalendarUtility.GetDates(fetchFromDate, fetchToDate).Any(a => a.Date == DateTime.Today);
                    var staffingStatisticsGroupItem = TimeScheduleManager.GetStaffingStatisticsGroupItem(groupList, selectionEmployeeIds, fetchFromDate, fetchToDate, loadSchedule: true, false, false, true, accounts, null, null, employees, Guid.NewGuid().ToString(), true, loadTimeStampEntries: loadTimeStamps, addEmptyAccounts: true, allAccountsMustbeOnEntity: selectedDims.Any());

                    Dictionary<int, Dictionary<DateTime, decimal>> employeeHourlyPaysDict = new Dictionary<int, Dictionary<DateTime, decimal>>();
                    if (loadScheduleCost || loadTimeCost)
                    {
                        foreach (var employee in employees)
                        {
                            var valuesDict = PayrollManager.GetEmployeeHourlyPays(reportResult.ActorCompanyId, employee, fetchFromDate, fetchToDate);
                            if (valuesDict != null && !employeeHourlyPaysDict.ContainsKey(employee.EmployeeId))
                                employeeHourlyPaysDict.Add(employee.EmployeeId, valuesDict);
                        }
                    }

                    #endregion

                    #region Content

                    var items = new List<VerticalTimeTrackerItem>();
                    var dates = CalendarUtility.GetDates(selectionDateFrom, selectionDateTo).OrderBy(o => o).ToList();

                    if (selectionAccountIds.Count == 0)
                    {
                        var allAccountIdsOnScheduleAndTransactions = staffingStatisticsGroupItem.StaffingStatisticsRows.SelectMany(s => s.
                        TimeScheduleBlocksDict.SelectMany(ss => ss.Value.Where(w => w?.AccountInternal != null).Select(sss => sss.AccountInternal))).SelectMany(s => s.Select(ss => ss.AccountId)).Distinct().ToList();
                        allAccountIdsOnScheduleAndTransactions.AddRange(staffingStatisticsGroupItem.StaffingStatisticsRows.SelectMany(s => s.TimeScheduleBlocksDict.SelectMany(ss => ss.Value.Where(w => w?.AccountId != null)).Select(s1 => s1.AccountId.Value)));
                        allAccountIdsOnScheduleAndTransactions.AddRange(staffingStatisticsGroupItem.StaffingStatisticsRows.SelectMany(s => s.TransactionsDict.SelectMany(ss => ss.Value.Where(w => w?.AccountInternals != null).Select(sss => sss.AccountInternals))).SelectMany(s => s.Select(ss => ss.AccountId)).Distinct().ToList());
                        allAccountIdsOnScheduleAndTransactions.AddRange(employees.SelectMany(s => s.EmployeeAccount).Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value).Distinct().ToList());

                        var timeStampDTOs = staffingStatisticsGroupItem.StaffingStatisticsRows.SelectMany(s => s.TimeStampEntries).ToList();
                        if (timeStampDTOs.Any())
                        {
                            allAccountIdsOnScheduleAndTransactions.AddRange(timeStampDTOs.Where(w => w.AccountId.HasValue && w.AccountId.Value != 0).Select(s => s.AccountId.Value));
                            allAccountIdsOnScheduleAndTransactions.AddRange(timeStampDTOs.Where(w => w.Extended != null).SelectMany(s => s.Extended).Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value));
                        }
                        selectionAccountIds = allAccountIdsOnScheduleAndTransactions.Distinct().ToList();
                    }

                    Dictionary<int, DateTime?> lastRealTransactionOnTodayDict = new Dictionary<int, DateTime?>();

                    if (staffingStatisticsGroupItem.EmployeeTimePayrollTransactionDict != null && CalendarUtility.GetDates(selectionDateFrom.Date, selectionDateTo.Date).Contains(DateTime.Today))
                    {
                        foreach (var employee in employees)
                        {
                            if (staffingStatisticsGroupItem.EmployeeTimePayrollTransactionDict.TryGetValue(employee.EmployeeId, out List<TimePayrollTransactionDTO> timePayrollTransactions))
                            {
                                var lastRealTransaction = timePayrollTransactions.Where(w => w.Date.HasValue && w.StopTime.HasValue && CalendarUtility.MergeDateAndDefaultTime(w.Date.Value, w.StopTime.Value, true).Date == DateTime.Today && !w.IsAbsence()).OrderByDescending(o => CalendarUtility.MergeDateAndDefaultTime(o.Date.Value, o.StopTime.Value, true)).FirstOrDefault();
                                if (lastRealTransaction != null)
                                    lastRealTransactionOnTodayDict.Add(employee.EmployeeId, CalendarUtility.MergeDateAndDefaultTime(lastRealTransaction.Date.Value, lastRealTransaction.StopTime));
                            }
                        }
                    }

                    var accountsOnDimDict = accounts.GroupBy(g => g.AccountDimId).ToDictionary(k => k.Key, v => v.ToList());

                    staffingStatisticsGroupItem.StaffingStatisticsRows = staffingStatisticsGroupItem.StaffingStatisticsRows.Where(w => w.HasData).ToList();

                    foreach (var item in staffingStatisticsGroupItem.StaffingStatisticsRows)
                    {
                        var companyScheduleTransactionDTOs = item.TimeScheduleBlocksDict.SelectMany(s => s.Value.Where(w => !w.TimeDeviationCauseId.HasValue)).GroupBy(g => g.Date).ToDictionary(k => k.Key, v => v.ToList());
                        var companyTimePayrollTransactionDTOs = item.TransactionsDict.SelectMany(s => s.Value.Where(w => w.IsAddedOrOverTime() || w.IsWorkTime())).GroupBy(g => g.Date).ToDictionary(k => k.Key, v => v.ToList());
                        var fakedCompanyTimePayrollTransactionsDTOs = item.FakedAllTransactionsDict.SelectMany(s => s.Value).GroupBy(g => g.Date).ToDictionary(k => k.Key, v => v.ToList());

                        var accountInternalsInGroup = companyScheduleTransactionDTOs.SelectMany(s => s.Value).Where(w => !w.IsBreak && w != null && w.Date.HasValue && w.Date.Value >= selectionDateFrom && w.Date.Value <= selectionDateTo).SelectMany(s => s.AccountIdsIncludingAccountIdOnBlock).Distinct().ToList();
                        accountInternalsInGroup.AddRange(companyTimePayrollTransactionDTOs.SelectMany(s => s.Value).Select(s => s.AccountInternals).Distinct().SelectMany(s => s.Select(ss => ss.AccountId)).Distinct().ToList());
                        accountInternalsInGroup.AddRange(fakedCompanyTimePayrollTransactionsDTOs.SelectMany(s => s.Value).Select(s => s.AccountInternals).Distinct().SelectMany(s => s.Select(ss => ss.AccountId)).Distinct().ToList());
                        accountInternalsInGroup = accountInternalsInGroup.Distinct().Where(w => w != 0).ToList();
                        List<int> validAccountIds = new List<int>();

                        foreach (var dim in selectedDims)
                        {
                            if (accountsOnDimDict.TryGetValue(dim.AccountDimId, out List<AccountDTO> accountsOnDim))
                            {
                                var selectedOnDim = accountsOnDim.Where(w => selectionAccountIds.Contains(w.AccountId) && !w.HierarchyOnly).ToList();
                                if (selectedOnDim.Any())
                                {
                                    validAccountIds.AddRange(selectedOnDim.Select(s => s.AccountId).Where(w => selectionAccountIds.Contains(w)));
                                }
                                else
                                    validAccountIds.AddRange(accountsOnDim.Select(s => s.AccountId));
                            };
                        }

                        accountInternalsInGroup = accountInternalsInGroup.Where(w => validAccountIds.Contains(w)).ToList();
                        var accountsInRow = accounts.Where(w => accountInternalsInGroup.Contains(w.AccountId)).ToList();
                        List<AccountAnalysisField> accountAnalysisFields = new List<AccountAnalysisField>();
                        foreach (var account in accountsInRow)
                            accountAnalysisFields.Add(new AccountAnalysisField(account));

                        if (LoadAccountInternal && !accountsInRow.Any() && selectionAccountIds.Any())
                            continue;

                        //if (oneWithNoAccountingCreated && LoadAccountInternal) && !accountsInRow.Select(s => s.AccountId).Any(a => selectionAccountIds.Contains(a)))
                        //    continue;

                        var accountIdsOnRow = accountsInRow.Select(s => s.AccountId).ToList();
                        foreach (var date in dates)
                        {
                            bool isLastDateInDates = dates.OrderByDescending(d => d.Date).First() == date;

                            var timeStampsOnDate = DateTime.Today == date ? (item.TimeStampEntries?.Where(w => w.Time.Date == date || w.Date == date).ToList() ?? null) : null;
                            companyScheduleTransactionDTOs.TryGetValue(date, out List<TimeScheduleTemplateBlock> schedules);
                            companyTimePayrollTransactionDTOs.TryGetValue(date, out List<TimePayrollTransactionDTO> timePayrollTransactions);
                            fakedCompanyTimePayrollTransactionsDTOs.TryGetValue(date, out List<TimePayrollTransactionDTO> fakedTimePayrollTransactions);

                            if (selectionIsCalendarDay)
                            {
                                companyScheduleTransactionDTOs.TryGetValue(date.AddDays(-1), out List<TimeScheduleTemplateBlock> dayBeforeSchedules);
                                companyTimePayrollTransactionDTOs.TryGetValue(date.AddDays(-1), out List<TimePayrollTransactionDTO> dayBeforeTimePayrollTransactions);
                                fakedCompanyTimePayrollTransactionsDTOs.TryGetValue(date.AddDays(-1), out List<TimePayrollTransactionDTO> dayBeforeFakedTimePayrollTransactions);
                                companyScheduleTransactionDTOs.TryGetValue(date.AddDays(1), out List<TimeScheduleTemplateBlock> dayAfterSchedules);
                                companyTimePayrollTransactionDTOs.TryGetValue(date.AddDays(1), out List<TimePayrollTransactionDTO> dayAfterTimePayrollTransactions);
                                fakedCompanyTimePayrollTransactionsDTOs.TryGetValue(date.AddDays(1), out List<TimePayrollTransactionDTO> dayAfterFakedTimePayrollTransactions);

                                if (timePayrollTransactions == null)
                                    timePayrollTransactions = new List<TimePayrollTransactionDTO>();

                                if (fakedTimePayrollTransactions == null)
                                    fakedTimePayrollTransactions = new List<TimePayrollTransactionDTO>();

                                if (companyScheduleTransactionDTOs == null)
                                    schedules = new List<TimeScheduleTemplateBlock>();

                                if (schedules == null)
                                    schedules = new List<TimeScheduleTemplateBlock>();

                                if (!dayBeforeSchedules.IsNullOrEmpty())
                                    schedules = schedules.Concat(dayBeforeSchedules).ToList();
                                if (!dayBeforeTimePayrollTransactions.IsNullOrEmpty())
                                    timePayrollTransactions = timePayrollTransactions.Concat(dayBeforeTimePayrollTransactions).ToList();
                                if (!dayBeforeFakedTimePayrollTransactions.IsNullOrEmpty())
                                    fakedTimePayrollTransactions = fakedTimePayrollTransactions.Concat(dayBeforeFakedTimePayrollTransactions).ToList();
                                if (!dayAfterSchedules.IsNullOrEmpty())
                                    schedules = schedules.Concat(dayAfterSchedules).ToList();
                                if (!dayAfterTimePayrollTransactions.IsNullOrEmpty())
                                    timePayrollTransactions = timePayrollTransactions.Concat(dayAfterTimePayrollTransactions).ToList();
                                if (!dayAfterFakedTimePayrollTransactions.IsNullOrEmpty())
                                    fakedTimePayrollTransactions = fakedTimePayrollTransactions.Concat(dayAfterFakedTimePayrollTransactions).ToList();
                            }

                            schedules = schedules.Filter(fetchFromDate, fetchToDate);

                            if (companyScheduleTransactionDTOs == null && companyTimePayrollTransactionDTOs == null)
                            {
                                if (DateTime.Today.Date == date || timeStampsOnDate == null)
                                    continue;

                                var accountidsOnTimeStamps = timeStampsOnDate.SelectMany(s => s.GetAccountIds());
                                if (!accountIdsOnRow.ContainsAny(accountidsOnTimeStamps))
                                    continue;
                            }


                            var firstTime = date;
                            DateTime? foundFirstTime = null;

                            if (!selectionIsCalendarDay)
                                schedules = schedules.Where(w => w.Date == date).ToList();

                            var times = new List<DateTime?>();
                            times.Add(timeStampsOnDate?.OrderBy(o => o.Time).FirstOrDefault()?.Time);
                            times.Add(schedules?.OrderBy(o => CalendarUtility.MergeDateAndDefaultTime(o.Date.Value, o.StartTime, true)).FirstOrDefault()?.ActualStartTime);
                            times.Add(timePayrollTransactions?.OrderBy(o => CalendarUtility.MergeDateAndDefaultTime(o.Date.Value, o.StartTime, true)).Select(s => CalendarUtility.MergeDateAndDefaultTime(s.Date.Value, s.StartTime, true)).FirstOrDefault());
                            times = times.Where(w => w.HasValue && w != DateTime.MinValue && w != DateTime.MaxValue).Select(s => (s.Value.Year == 1900 || s.Value.Year == 1899) ? CalendarUtility.MergeDateAndDefaultTime(date, s, true) : s).OrderBy(o => o).ToList();
                            foundFirstTime = times.FirstOrDefault();

                            if (!fillWholeDay && foundFirstTime.HasValue)
                            {
                                if (selectionIsCalendarDay && foundFirstTime.Value < date)
                                    firstTime = date;
                                else
                                    firstTime = foundFirstTime.Value;
                            }
                            else if (selectionIsCalendarDay && foundFirstTime.HasValue && foundFirstTime < date)
                                firstTime = date;

                            var lastTime = date.AddDays(1).AddMinutes(-15);
                            DateTime? foundlastTime = null;

                            times = new List<DateTime?>();
                            var lastTimeStamp = timeStampsOnDate?.OrderByDescending(o => o.Time).FirstOrDefault()?.Time;

                            if (lastTimeStamp.HasValue && lastTimeStamp.Value.Date == DateTime.Today && lastTimeStamp.Value < DateTime.Now)
                                lastTimeStamp = DateTime.Now;

                            times.Add(lastTimeStamp);

                            var tomorrowIsInSelection = dates.Any(a => a.Date == date.AddDays(1));
                            if (tomorrowIsInSelection)
                            {
                                schedules = schedules.Filter(fetchFromDate, date.Date.AddDays(1));
                            }

                            times.Add(schedules?.OrderByDescending(o => CalendarUtility.MergeDateAndDefaultTime(o.Date.Value, o.StopTime, true)).FirstOrDefault()?.ActualStopTime);
                            times.Add(timePayrollTransactions?.OrderByDescending(o => CalendarUtility.MergeDateAndDefaultTime(o.Date.Value, o.StopTime, true)).Select(s => CalendarUtility.MergeDateAndDefaultTime(s.Date.Value, s.StopTime, true)).FirstOrDefault());
                            times = times.Where(w => w.HasValue && w != DateTime.MinValue && w != DateTime.MaxValue).Select(s => (s.Value.Year == 1900 || s.Value.Year == 1899) ? CalendarUtility.MergeDateAndDefaultTime(date, s, true) : s).OrderByDescending(o => o).ToList();

                            if (tomorrowIsInSelection && times.Any(a => a.Value > date.Date.AddDays(1)))
                            {
                                times = times.Where(w => w.Value <= date.Date.AddDays(1)).ToList();

                                if (!times.Any())
                                    times.Add(date.Date.AddDays(1));
                            }

                            foundlastTime = times.FirstOrDefault();

                            if (!foundlastTime.HasValue)
                                foundlastTime = date.Date.AddDays(1);

                            if (isLastDateInDates)
                            {
                                if (fillWholeDay && tomorrowIsInSelection && foundlastTime.HasValue && foundlastTime.Value > date.Date.AddDays(1))
                                    lastTime = date.Date.AddDays(1);
                                else if (fillWholeDay && !selectionIsCalendarDay && foundlastTime.HasValue && foundlastTime.Value > date.Date.AddDays(1))
                                    lastTime = foundlastTime.Value; // if over midnight fill to last time
                                else if (selectionIsCalendarDay && foundlastTime.HasValue && foundlastTime.Value > date.Date.AddDays(1))
                                    lastTime = date.Date.AddDays(1); // if after midnight, set to midnight 
                                else if (fillWholeDay && foundlastTime.HasValue && foundlastTime.Value < date.Date.AddDays(1))
                                    lastTime = date.Date.AddDays(1); // if before midnight but its fille whole day, fill to midnight
                                else
                                    lastTime = foundlastTime.Value;
                            }
                            else
                            {
                                lastTime = foundlastTime ?? foundFirstTime ?? date.Date;

                                if (fillWholeDay && tomorrowIsInSelection)
                                    lastTime = date.Date.AddDays(1);
                            }

                            if (!foundlastTime.HasValue || foundlastTime.Value < lastTime)
                                foundlastTime = lastTime;

                            var timeIntervals = GetDateInterval(15, CalendarUtility.AdjustAccordingToInterval(firstTime, 0, 15, true), CalendarUtility.AdjustToEndOfInterval(lastTime, 15).AddMinutes(15));

                            if (timeIntervals.Any() && timeIntervals.Last().TimeFrom == lastTime)
                                timeIntervals.RemoveAt(timeIntervals.Count - 1);

                            bool hasTimeTransactionsOnDate = !timePayrollTransactions.IsNullOrEmpty();

                            //TODO. Group on selected accountDims
                            //TODO. Then group on the intervals
                            //TODO. If the date is today, time should be added to now, and the last interval should be now

                            foreach (var interval in timeIntervals)
                            {
                                VerticalTimeTrackerItem verticalItem = new VerticalTimeTrackerItem();
                                verticalItem.AccountAnalysisFields = accountAnalysisFields;
                                var timeStampsOnInterval = timeStampsOnDate ?? new List<TimeStampEntryDTO>();
                                var schedulesOnInterval = schedules?.Where(w => CalendarUtility.GetOverlappingMinutes(interval.TimeFrom, interval.TimeTo, w.ActualStartTime.Value, w.ActualStopTime.Value) > 0).ToList();
                                var paidAttendanceTimePayrollTransactionsOnInterval = timePayrollTransactions?.Where(w =>
                                CalendarUtility.GetOverlappingMinutes(CalendarUtility.MergeDateAndDefaultTime(w.Date.Value, w.StartTime), CalendarUtility.MergeDateAndDefaultTime(w.Date.Value, w.StopTime), interval.TimeFrom, interval.TimeTo) > 0).ToList();
                                var fakedPaidAttendanceTimePayrollTransactionsOnInterval = fakedTimePayrollTransactions?.Where(w => CalendarUtility.GetOverlappingMinutes(interval.TimeFrom, interval.TimeTo, w.StartTime.Value, w.StopTime.Value) > 0).ToList();

                                verticalItem.Date = selectionIsCalendarDay ? interval.TimeFrom.Date : date;
                                verticalItem.StartTime = interval.TimeFrom;
                                verticalItem.StopTime = interval.TimeTo;


                                int scheduleInMInutes = 0;
                                int scheduleHeads = 0;
                                decimal scheduleCost = 0;
                                int timeInMinutes = 0;
                                int timeHeads = 0;
                                decimal timeCost = 0;

                                List<int> scheduleBlockIds = new List<int>();
                                if (!hasEmployee)
                                {
                                    // Add empty data before the first found interval with data
                                    if (foundFirstTime.HasValue && foundFirstTime.Value > interval.TimeTo)
                                    {
                                        verticalItem.StartDate = interval.TimeFrom;
                                        verticalItem.EndDate = interval.TimeTo;
                                        verticalItem.Time = timeInMinutes;
                                        verticalItem.Schedule = scheduleInMInutes;
                                        verticalItem.TimeCost = timeCost;
                                        verticalItem.ScheduleCost = scheduleCost;
                                        verticalTimeTrackerItems.Add(verticalItem);
                                        continue;
                                    }
                                    // add empty data after the last found interval with data
                                    else if (foundlastTime.HasValue && foundlastTime.Value < interval.TimeFrom)
                                    {
                                        verticalItem.StartDate = interval.TimeFrom;
                                        verticalItem.EndDate = interval.TimeTo;
                                        verticalItem.Time = timeInMinutes;
                                        verticalItem.Schedule = scheduleInMInutes;
                                        verticalItem.TimeCost = timeCost;
                                        verticalItem.ScheduleCost = scheduleCost;
                                        verticalTimeTrackerItems.Add(verticalItem);
                                        continue;
                                    }
                                }

                                foreach (var employee in employees)
                                {
                                    if (hasEmployee)
                                    {
                                        // Add empty data before the first found interval with data
                                        if (foundFirstTime.HasValue && foundFirstTime.Value > interval.TimeTo)
                                        {
                                            verticalItem.StartDate = interval.TimeFrom;
                                            verticalItem.EndDate = interval.TimeTo;
                                            verticalItem.Time = timeInMinutes;
                                            verticalItem.Schedule = scheduleInMInutes;
                                            verticalItem.TimeCost = timeCost;
                                            verticalItem.ScheduleCost = scheduleCost;
                                            verticalItem.EmployeeId = employee.EmployeeId;
                                            verticalItem.EmployeeNr = employee.EmployeeNr;
                                            verticalItem.EmployeeName = employee.Name;
                                            verticalTimeTrackerItems.Add(verticalItem.Clone());
                                            continue;
                                        }
                                        // add empty data after the last found interval with data
                                        else if (foundlastTime.HasValue && foundlastTime.Value < interval.TimeFrom)
                                        {
                                            verticalItem.StartDate = interval.TimeFrom;
                                            verticalItem.EndDate = interval.TimeTo;
                                            verticalItem.Time = timeInMinutes;
                                            verticalItem.Schedule = scheduleInMInutes;
                                            verticalItem.TimeCost = timeCost;
                                            verticalItem.ScheduleCost = scheduleCost;
                                            verticalItem.ScheduleBlockIds = string.Empty;
                                            verticalItem.EmployeeId = employee.EmployeeId;
                                            verticalItem.EmployeeNr = employee.EmployeeNr;
                                            verticalItem.EmployeeName = employee.Name;
                                            verticalTimeTrackerItems.Add(verticalItem.Clone());
                                            continue;
                                        }
                                    }
                                    var paidAttendenceTimeEmployeeTransactions = paidAttendanceTimePayrollTransactionsOnInterval?.Where(w => w.EmployeeId == employee.EmployeeId).ToList();
                                    var fakedPaidAttendenceTimeEmployeeTransactions = fakedPaidAttendanceTimePayrollTransactionsOnInterval?.Where(w => w.EmployeeId == employee.EmployeeId).ToList();
                                    var employeeSchedule = schedulesOnInterval?.Where(f => f.EmployeeId == employee.EmployeeId).ToList();
                                    var timestampsOnEmployee = timeStampsOnInterval?.Where(w => w.EmployeeId == employee.EmployeeId).ToList() ?? new List<TimeStampEntryDTO>();

                                    if (hasEmployee && fillWholeDay && paidAttendenceTimeEmployeeTransactions.IsNullOrEmpty() && employeeSchedule.IsNullOrEmpty() && timestampsOnEmployee.IsNullOrEmpty())
                                    {
                                        verticalItem.StartDate = interval.TimeFrom;
                                        verticalItem.EndDate = interval.TimeTo;
                                        verticalItem.Time = timeInMinutes;
                                        verticalItem.Schedule = scheduleInMInutes;
                                        verticalItem.TimeCost = timeCost;
                                        verticalItem.ScheduleCost = scheduleCost;
                                        verticalItem.EmployeeId = employee.EmployeeId;
                                        verticalItem.EmployeeNr = employee.EmployeeNr;
                                        verticalItem.EmployeeName = employee.Name;
                                        verticalItem.ScheduleBlockIds = string.Empty;
                                        verticalTimeTrackerItems.Add(verticalItem.Clone());
                                        continue;
                                    }
                                    else
                                    {
                                        var hadSchedule = false;
                                        var hadTime = false;
                                        var lastTimeStampsOnDate = date == DateTime.Today ? timestampsOnEmployee.Where(w => w.Time <= interval.TimeTo).OrderByDescending(o => o.Time).ToList() : null;
                                        DateTime? lastInTimeStamp = null;
                                        if (!lastTimeStampsOnDate.IsNullOrEmpty() && lastTimeStampsOnDate.Count == 1)
                                            lastInTimeStamp = lastTimeStampsOnDate.FirstOrDefault().Time;
                                        else if (!lastTimeStampsOnDate.IsNullOrEmpty() && lastTimeStampsOnDate.Count > 1)
                                            lastInTimeStamp = lastTimeStampsOnDate.Where(w => w.Type == TimeStampEntryType.In).FirstOrDefault()?.Time;

                                        var lastRealTransactionOnDate = lastRealTransactionOnTodayDict.ContainsKey(employee.EmployeeId) ? lastRealTransactionOnTodayDict.GetValue(employee.EmployeeId) : null;

                                        if (DateTime.Today == date)
                                        {
                                            var lastTransaction = paidAttendenceTimeEmployeeTransactions?.Where(w => w.EmployeeId == employee.EmployeeId).OrderByDescending(o => CalendarUtility.MergeDateAndDefaultTime(date, o.StopTime)).FirstOrDefault();

                                            if (lastTransaction?.StopTime != null)
                                            {
                                                if (lastInTimeStamp.HasValue &&
                                                    lastInTimeStamp.Value >= CalendarUtility.MergeDateAndDefaultTime(date, lastTransaction.StopTime.Value) &&
                                                    CalendarUtility.MergeDateAndDefaultTime(date, lastTransaction.StopTime.Value) < DateTime.Now)
                                                {
                                                    lastInTimeStamp = CalendarUtility.MergeDateAndDefaultTime(date, lastTransaction.StopTime);
                                                }
                                                else if (!lastInTimeStamp.HasValue && CalendarUtility.MergeDateAndDefaultTime(date, lastTransaction.StopTime.Value) < DateTime.Now)
                                                {
                                                    lastInTimeStamp = CalendarUtility.MergeDateAndDefaultTime(date, lastTransaction.StopTime);
                                                }
                                            }

                                            if (lastInTimeStamp.HasValue && lastInTimeStamp.Value >= interval.TimeFrom && lastInTimeStamp <= interval.TimeTo && timestampsOnEmployee.Any(a => a.Time < interval.TimeFrom))
                                            {
                                                lastInTimeStamp = interval.TimeFrom;
                                            }
                                        }

                                        // loop through all minutes in interval
                                        for (DateTime time = interval.TimeFrom; time < interval.TimeTo; time = time.AddMinutes(1))
                                        {
                                            // check if there is a schedule at this time
                                            var schedule = employeeSchedule?.FirstOrDefault(f => time >= f.ActualStartTime && time < f.ActualStopTime && !f.IsBreak);
                                            var breakSchedule = employeeSchedule?.FirstOrDefault(f => time >= f.ActualStartTime && time < f.ActualStopTime && f.IsBreak);
                                            if (schedule != null && breakSchedule == null)
                                            {
                                                scheduleBlockIds.Add(schedule.TimeScheduleTemplateBlockId);
                                                scheduleInMInutes++;
                                                hadSchedule = true;
                                            }

                                            // check if there is a time payroll transaction at this time
                                            var timePayrollTransaction = paidAttendenceTimeEmployeeTransactions?.FirstOrDefault(f => CalendarUtility.GetOverlappingMinutes(time, time.AddMinutes(1), CalendarUtility.MergeDateAndDefaultTime(f.Date.Value, f.StartTime), CalendarUtility.MergeDateAndDefaultTime(f.Date.Value, f.StopTime)) != 0);

                                            if (timePayrollTransaction != null)
                                            {
                                                timeInMinutes++;
                                                hadTime = true;
                                            }
                                            else if (!fakedPaidAttendanceTimePayrollTransactionsOnInterval.IsNullOrEmpty() && (!lastRealTransactionOnDate.HasValue || lastRealTransactionOnDate.Value < time))
                                            {
                                                var fakedTimePayrollTransaction = fakedPaidAttendenceTimeEmployeeTransactions.FirstOrDefault(f => CalendarUtility.GetOverlappingMinutes(time, time.AddMinutes(1), f.StartTime.Value, f.StopTime.Value) != 0);

                                                if (fakedTimePayrollTransaction != null)
                                                {
                                                    timeInMinutes++;
                                                    hadTime = true;
                                                }
                                            }
                                        }

                                        if (hadTime)
                                            timeHeads++;

                                        if (hadSchedule)
                                            scheduleHeads++;

                                        if (loadScheduleCost)
                                            scheduleCost += GetCost(employee.EmployeeId, date, scheduleInMInutes, employeeHourlyPaysDict);

                                        if (loadTimeCost)
                                            timeCost += GetCost(employee.EmployeeId, date, timeInMinutes, employeeHourlyPaysDict);

                                        if (hasEmployee && (hadTime || hadSchedule || fillWholeDay))
                                        {
                                            verticalItem.EmployeeId = employee.EmployeeId;
                                            verticalItem.EmployeeNr = employee.EmployeeNr;
                                            verticalItem.EmployeeName = employee.Name;
                                            verticalItem.StartDate = interval.TimeFrom;
                                            verticalItem.EndDate = interval.TimeTo;
                                            verticalItem.Time = timeInMinutes;
                                            verticalItem.Schedule = scheduleInMInutes;
                                            verticalItem.TimeCost = timeCost;
                                            verticalItem.ScheduleCost = scheduleCost;
                                            verticalItem.ScheduleBlockIds = string.Join("_", scheduleBlockIds.OrderBy(o => o));
                                            scheduleInMInutes = 0;
                                            timeInMinutes = 0;
                                            scheduleCost = 0;
                                            timeCost = 0;
                                            verticalTimeTrackerItems.Add(verticalItem.Clone());
                                        }
                                    }
                                }

                                if (!hasEmployee && (scheduleHeads > 0 || timeHeads > 0 || fillWholeDay))
                                {
                                    verticalItem.StartDate = interval.TimeFrom;
                                    verticalItem.EndDate = interval.TimeTo;
                                    verticalItem.Time = timeInMinutes;
                                    verticalItem.Schedule = scheduleInMInutes;
                                    verticalItem.TimeCost = timeCost;
                                    verticalItem.ScheduleCost = scheduleCost;
                                    verticalItem.ScheduleBlockIds = string.Join("_", scheduleBlockIds.OrderBy(o => o));
                                    verticalItem.TimeHeads = timeHeads;
                                    verticalItem.ScheduleHeads = scheduleHeads;
                                    verticalTimeTrackerItems.Add(verticalItem.Clone());
                                }
                            }

                            var removedItems = new List<VerticalTimeTrackerItem>();

                            foreach (var grouped in verticalTimeTrackerItems.GroupBy(g => g.EmployeeId))
                            {
                                if (!fillWholeDay && grouped.Sum(s =>
                                  (hasScheduleColumns ? s.Schedule + s.ScheduleCost : 0) +
                                  (hasTimeColumns ? s.Time + s.TimeCost : 0)) == 0)
                                    removedItems.AddRange(grouped);
                            }

                            if (removedItems.Any())
                                verticalTimeTrackerItems = verticalTimeTrackerItems.Except(removedItems).ToList();
                        }
                    }

                    verticalTimeTrackerItems = verticalTimeTrackerItems.GroupBy(g => g.GroupByProps()).Select(s => s.First()).ToList();
                    _reportDataOutput.VerticalTimeTrackerItems = verticalTimeTrackerItems.OrderBy(o => o.EmployeeNr).ThenBy(t => t.StartTime).ToList();

                    #endregion
                }

                #region Close repository

                base.personalDataRepository.GenerateLogs();

                #endregion

                return new ActionResult();
            }
        }

        private decimal GetCost(int employeeId, DateTime date, int minutes, Dictionary<int, Dictionary<DateTime, decimal>> employeeHourlyPaysDict)
        {
            if (employeeHourlyPaysDict != null && employeeHourlyPaysDict.TryGetValue(employeeId, out Dictionary<DateTime, decimal> values) && values.TryGetValue(date, out decimal value))
            {
                var valuePerMinute = decimal.Divide(value, 60);
                return decimal.Round(valuePerMinute * (decimal)minutes, 2);
            }
            return 0;
        }

        private class DateInterval
        {
            internal DateTime TimeFrom { get; set; }
            internal DateTime TimeTo { get; set; }
        }

        private List<DateInterval> GetDateInterval(int intervalInMinutes, DateTime timeFrom, DateTime timeTo)
        {
            List<DateInterval> dateIntervals = new List<DateInterval>();

            if (timeFrom > timeTo)
                timeTo = timeFrom;

            if (timeFrom == timeTo)
                timeTo = timeFrom.Date.AddDays(1);

            DateTime time = timeFrom;
            while (time < timeTo)
            {
                DateInterval dateInterval = new DateInterval();
                dateInterval.TimeFrom = time;
                dateInterval.TimeTo = time.AddMinutes(intervalInMinutes);
                dateIntervals.Add(dateInterval);
                time = dateInterval.TimeTo;
            }

            return dateIntervals;
        }
    }

    public class VerticalTimeTrackerReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_VerticalTimeTrackerMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public VerticalTimeTrackerReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_VerticalTimeTrackerMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_VerticalTimeTrackerMatrixColumns.EmployeeNr;
        }
    }

    public class VerticalTimeTrackerReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<VerticalTimeTrackerReportDataField> Columns { get; set; }

        public VerticalTimeTrackerReportDataInput(CreateReportResult reportResult, List<VerticalTimeTrackerReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class VerticalTimeTrackerReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public VerticalTimeTrackerReportDataInput Input { get; set; }
        public List<VerticalTimeTrackerItem> VerticalTimeTrackerItems { get; set; }

        public VerticalTimeTrackerReportDataOutput(VerticalTimeTrackerReportDataInput input)
        {
            this.Input = input;
            this.VerticalTimeTrackerItems = new List<VerticalTimeTrackerItem>();
        }
    }
}

