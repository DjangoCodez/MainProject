using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class LongtermAbsenceReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly LongtermAbsenceReportDataInput _reportDataInput;
        private readonly LongtermAbsenceReportDataOutput _reportDataOutput;

        private bool loadRatio => _reportDataInput.Columns.Any(a => a.Column == TermGroup_LongtermAbsenceMatrixColumns.Ratio);
        private bool loadExtendedInterval => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_LongtermAbsenceMatrixColumns.StartDate ||
            a.Column == TermGroup_LongtermAbsenceMatrixColumns.StopDate ||
            a.Column == TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysTotal ||
            a.Column == TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysBeforeInterval ||
            a.Column == TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysAfterInterval
        );

        public LongtermAbsenceReportData(ParameterObject parameterObject, LongtermAbsenceReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new LongtermAbsenceReportDataOutput(reportDataInput);
        }

        public LongtermAbsenceReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Prereq

            TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(true);
            if (!TryGetDatesFromSelection(reportResult, out selectionDateFrom, out selectionDateTo))
                return new ActionResult(true);

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out bool electionIncludePreliminary, "includePreliminary");
            TryGetBoolFromSelection(reportResult, out bool selectionShowOnlyTotals, "showOnlyTotals");
            TryGetBoolFromSelection(reportResult, out bool skipTimeScheduleTransactions, "skipTimeScheduleTransactions");
            TryGetTextFromSelection(reportResult, out string numberOfDaysString, "numberOfDays");
            bool filterOnPayrollProducts = TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);

            int.TryParse(numberOfDaysString, out int numberOfDays);

            if (numberOfDays == 0 && TryGetIdFromSelection(reportResult, out int? numberofDaysResult, "numberOfDays") && numberofDaysResult.HasValue)
                numberOfDays = numberofDaysResult.Value;
            if (numberOfDays == 0)
                numberOfDays = 7;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();
            int langId = GetLangId();
            Dictionary<int, string> sysReport = base.GetTermGroupDict(TermGroup.SysReportTemplateType, langId);

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.Input.ActorCompanyId);
                    List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;
                    bool socialSecPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var dims = base.GetAccountDimsFromCache(entitiesReadonly, DataCache.CacheConfig.Company(reportResult.ActorCompanyId));
                    dims.CalculateLevels();

                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);
                    var companyTimePayrollTransactions = TimeTransactionManager.GetTimePayrollTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);
                    var timePayrollTransactions = new Dictionary<int, List<TimePayrollTransactionDTO>>();
                    if (numberOfDays >= CalendarUtility.GetTotalDays(selectionDateFrom, selectionDateTo) + 1)
                        numberOfDays = CalendarUtility.GetTotalDays(selectionDateFrom, selectionDateTo) + 1;
                    Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> companyScheduleTransactionDTOs = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
                    Dictionary<string, List<CompanyCategoryRecord>> categoryRecordsFullKeyDict = new Dictionary<string, List<CompanyCategoryRecord>>();
                    var accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                    List<CategoryAccount> categoryAccounts = new List<CategoryAccount>();
                    if (useAccountHierarchy)
                        employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                    else
                                  categoryAccounts = base.GetCategoryAccountsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));


                    List<TimeAbsenceDetailDTO> timeAbsenceDetails = new List<TimeAbsenceDetailDTO>();

                    if (!useAccountHierarchy)
                    {
                        var categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                        categoryRecordsFullKeyDict = CategoryManager.GetCompanyCategoryRecordsFullKeyDict(categoryRecords, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, base.ActorCompanyId);
                    }

                    if (loadRatio)
                    {
                        timeAbsenceDetails = TimeBlockManager.GetTimeAbsenceDetails(entities, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                        companyScheduleTransactionDTOs = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(selectionDateFrom, selectionDateTo, employees, reportResult.Input.ActorCompanyId, base.RoleId, shiftTypeIds: null, splitOnBreaks: true, removeBreaks: true);
                    }

                    if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                    {
                        var filtered = new Dictionary<int, List<TimePayrollTransactionDTO>>();

                        foreach (var item in companyTimePayrollTransactions)
                        {
                            var values = item.Value.Where(w => w.AccountInternals == null && w.AccountInternals.ValidOnFiltered(validAccountInternals)).ToList();

                            if (values.Any())
                                filtered.Add(item.Key, values);
                        }

                        timePayrollTransactions = filtered;
                    }

                    foreach (var item in companyTimePayrollTransactions)
                    {
                        var filtered = new Dictionary<int, List<TimePayrollTransactionDTO>>();

                        var values = item.Value.Where(w => w.IsAbsence()).ToList();

                        if (values.Any())
                            timePayrollTransactions.Add(item.Key, values);
                    }

                    if (filterOnPayrollProducts)
                    {
                        var filtered = new Dictionary<int, List<TimePayrollTransactionDTO>>();

                        foreach (var item in timePayrollTransactions)
                        {
                            var values = item.Value.Where(w => selectionPayrollProductIds.Contains(w.PayrollProductId)).ToList();

                            if (values.Any())
                                filtered.Add(item.Key, values);
                        }

                        timePayrollTransactions = filtered;
                    }

                    foreach (var item in timePayrollTransactions)
                    {
                        var employee = employees.FirstOrDefault(f => f.EmployeeId == item.Key);
                        List<CompanyCategoryRecord> employeeCategoryRecords = null;

                        string key = useAccountHierarchy ? string.Empty : CompanyCategoryRecord.ConstructKey((int)SoeCategoryRecordEntity.Employee, employee.EmployeeId);
                        if (!string.IsNullOrEmpty(key) && categoryRecordsFullKeyDict.TryGetValue(key, out List<CompanyCategoryRecord> records))
                            employeeCategoryRecords = records.GetCategoryRecords(employee.EmployeeId, discardDateIfEmpty: true);

                        IEnumerable<IGrouping<string, TimePayrollTransactionDTO>> transactionGroupByLevel = item.Value.GroupBy(f => f.SysPayrollTypeLevel1 + "#" + f.SysPayrollTypeLevel2 + "#" + f.SysPayrollTypeLevel3);

                        if (loadRatio)
                        {
                            List<TimeEmployeeScheduleDataSmallDTO> scheduleTransactionDTOs = null;
                            companyScheduleTransactionDTOs.TryGetValue(item.Key, out scheduleTransactionDTOs);

                            foreach (var ab in transactionGroupByLevel)
                                CalculateAndSetAbsenceRatio(scheduleTransactionDTOs, ab.ToList(), timeAbsenceDetails, ab.ToList());
                        }

                        foreach (var group in transactionGroupByLevel)
                        {
                            var tuplesIntervals = GetCoherentAbsenceIntervals(group.ToList(), !loadRatio);

                            var intervalDays = tuplesIntervals.Max(interval => (interval.Item2 - interval.Item1).TotalDays + 1);

                            if (intervalDays > numberOfDays || numberOfDays == 1)
                            {
                                foreach (var interval in tuplesIntervals)
                                {
                                    var days = (interval.Item2 - interval.Item1).TotalDays + 1;
                                    var timePayrollTransaction = interval.Item4.First();
                                    var firstCreatedTimePayollTransaction = interval.Item4.OrderBy(o => o.Created).FirstOrDefault();
                                    var lastModifiedTimePayollTransaction = interval.Item4.OrderByDescending(o => o.Modified).FirstOrDefault();
                                    var startDate = interval.Item1 > selectionDateFrom ? interval.Item1 : selectionDateFrom;
                                    var endDate = interval.Item2 < selectionDateTo ? interval.Item2 : selectionDateTo;
                                    var daysInInterval = (endDate - startDate).TotalDays + 1;
                                    daysInInterval = daysInInterval > 0 ? (int)daysInInterval : 0;

                                    LongtermAbsenceItem longtermAbsenceItem = new LongtermAbsenceItem()
                                    {
                                        EmployeeNr = employee.EmployeeNr,
                                        FirstName = employee.FirstName,
                                        LastName = employee.LastName,
                                        Name = employee.Name,
                                        SocialSec = socialSecPermission ? employee.SocialSec : String.Empty,
                                        PayrollTypeLevel1Name = timePayrollTransaction.SysPayrollTypeLevel1.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                        PayrollTypeLevel2Name = timePayrollTransaction.SysPayrollTypeLevel2.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                        PayrollTypeLevel3Name = timePayrollTransaction.SysPayrollTypeLevel3.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                        PayrollTypeLevel1 = timePayrollTransaction.SysPayrollTypeLevel1.HasValue ? timePayrollTransaction.SysPayrollTypeLevel1.Value : 0,
                                        PayrollTypeLevel2 = timePayrollTransaction.SysPayrollTypeLevel2.HasValue ? timePayrollTransaction.SysPayrollTypeLevel2.Value : 0,
                                        PayrollTypeLevel3 = timePayrollTransaction.SysPayrollTypeLevel3.HasValue ? timePayrollTransaction.SysPayrollTypeLevel3.Value : 0,
                                        NumberOfDaysInInterval = Convert.ToInt32(daysInInterval),
                                        EntireSelectedPeriod = interval.Item1.Date == selectionDateFrom.Date && interval.Item2.Date == selectionDateTo.Date,
                                        StartDateInInterval = interval.Item1,
                                        StopDateInInterval = interval.Item2,
                                        StartDate = interval.Item1,
                                        StopDate = interval.Item2,
                                        NumberOfDaysTotal = Convert.ToInt32(days),
                                        AccountAnalysisFields = timePayrollTransaction.AccountInternals?.AccountAnalysisFields(),
                                        Ratio = interval.Item3,
                                        Created = firstCreatedTimePayollTransaction?.Created ?? CalendarUtility.DATETIME_DEFAULT,
                                        Modified = lastModifiedTimePayollTransaction?.Modified ?? (DateTime?)null
                                    };

                                    var employment = employee.GetEmployment(interval.Item1, interval.Item2);
                                    if (employment != null && useAccountHierarchy && employeeAccounts.Any())
                                    {
                                        longtermAbsenceItem.EmployeeAccountAnalysisFields = employment.AccountAnalysisFields(employeeAccounts.FindAll(r => r.EmployeeId == employee.EmployeeId), employeeCategoryRecords, categoryAccounts, accounts, interval.Item1, interval.Item2);
                                    }

                                    DateTime extendedStartTime = selectionDateFrom;
                                    DateTime extendedStopTime = selectionDateTo;

                                    if (longtermAbsenceItem.StartDateInInterval == selectionDateFrom)
                                        extendedStartTime = selectionDateFrom.Date.AddYears(-3);

                                    if (longtermAbsenceItem.StopDateInInterval == selectionDateTo.Date)
                                        extendedStopTime = selectionDateTo.Date.AddYears(1);

                                    if (loadExtendedInterval && extendedStopTime != selectionDateTo || extendedStartTime != selectionDateFrom)
                                    {
                                        var extendedIntervalTimePayrollTransactionsDict = TimeTransactionManager.GetTimePayrollTransactionDTOForReport(extendedStartTime, extendedStopTime, new List<int>() { employee.EmployeeId }, reportResult.Input.ActorCompanyId);

                                        if (loadRatio)
                                        {
                                            var extendedTimeAbsenceDetails = TimeBlockManager.GetTimeAbsenceDetails(entities, employee.EmployeeId.ObjToList(), extendedStartTime, extendedStopTime);
                                            var removeDatesInSelection = CalendarUtility.GetDatesInInterval(selectionDateFrom, selectionDateTo.Date);
                                            extendedTimeAbsenceDetails = extendedTimeAbsenceDetails.Where(w => !removeDatesInSelection.Contains(w.Date)).ToList();
                                            var scheduleTransactionDTOs = extendedTimeAbsenceDetails.Any(a => a.Ratio == 0) ? TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(extendedStartTime, extendedStopTime, employee.ObjToList(), reportResult.Input.ActorCompanyId, base.RoleId, shiftTypeIds: null, splitOnBreaks: true, removeBreaks: true) : new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();

                                            foreach (var tran in extendedIntervalTimePayrollTransactionsDict.SelectMany(s => s.Value))
                                            {
                                                var absenceDetail = extendedTimeAbsenceDetails.FirstOrDefault(f => f.EmployeeId == tran.EmployeeId && f.Date == tran.Date && (f.TimeDeviationCauseId == tran.TimeDeviationCauseStartId || f.TimeDeviationCauseId == tran.TimeDeviationCauseStopId));
                                                CalculateAndSetAbsenceRatio(scheduleTransactionDTOs.SelectMany(s => s.Value).ToList(), tran.ObjToList(), absenceDetail.ObjToList(), group.ToList());
                                            }
                                        }

                                        if (extendedIntervalTimePayrollTransactionsDict.Any(a => a.Key == employee.EmployeeId))
                                        {
                                            var extendedIntervalTimePayrollTransactions = extendedIntervalTimePayrollTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value?.ToList();
                                            var removeDatesInSelection = CalendarUtility.GetDatesInInterval(selectionDateFrom, selectionDateTo.Date);

                                            if (!extendedIntervalTimePayrollTransactions.IsNullOrEmpty())
                                                extendedIntervalTimePayrollTransactions = extendedIntervalTimePayrollTransactions.Where(w => w.SysPayrollTypeLevel3 == longtermAbsenceItem.PayrollTypeLevel3 && !removeDatesInSelection.Contains(w.Date.Value)).ToList();

                                            if (!extendedIntervalTimePayrollTransactions.IsNullOrEmpty())
                                            {
                                                var tuplesIntervalInExtendedInterval = GetCoherentAbsenceIntervals(extendedIntervalTimePayrollTransactions, !loadRatio);

                                                foreach (var intervalInExtendedInterval in tuplesIntervalInExtendedInterval)
                                                {
                                                    if (intervalInExtendedInterval.Item2 == selectionDateFrom.AddDays(-1) && intervalInExtendedInterval.Item3 == longtermAbsenceItem.Ratio)
                                                    {
                                                        longtermAbsenceItem.NumberOfDaysTotal = (intervalInExtendedInterval.Item2 - intervalInExtendedInterval.Item1).TotalDays + 1 + longtermAbsenceItem.NumberOfDaysInInterval;
                                                        longtermAbsenceItem.NumberOfDaysBeforeInterval = Convert.ToInt32((intervalInExtendedInterval.Item2 - intervalInExtendedInterval.Item1).TotalDays + 1);
                                                        longtermAbsenceItem.StartDate = intervalInExtendedInterval.Item1;
                                                    }

                                                    if (intervalInExtendedInterval.Item1 == selectionDateTo.Date.AddDays(1) && intervalInExtendedInterval.Item3 == longtermAbsenceItem.Ratio)
                                                    {
                                                        longtermAbsenceItem.NumberOfDaysTotal = (intervalInExtendedInterval.Item2 - intervalInExtendedInterval.Item1).TotalDays + 1 + longtermAbsenceItem.NumberOfDaysTotal;
                                                        longtermAbsenceItem.NumberOfDaysAfterInterval = Convert.ToInt32((intervalInExtendedInterval.Item2 - intervalInExtendedInterval.Item1).TotalDays + 1);
                                                        longtermAbsenceItem.StopDate = intervalInExtendedInterval.Item2;
                                                    }
                                                }
                                            }
                                        }

                                        var extendedCreated = extendedIntervalTimePayrollTransactionsDict?[employee.EmployeeId].OrderBy(f => f.Created).FirstOrDefault()?.Created;

                                        if (extendedCreated.HasValue)
                                            longtermAbsenceItem.Created = extendedCreated.Value;

                                        var extendedModified = extendedIntervalTimePayrollTransactionsDict?[employee.EmployeeId].Where(w => w.Modified.HasValue).OrderByDescending(f => f.Modified).FirstOrDefault()?.Modified;

                                        if (extendedModified.HasValue)
                                            longtermAbsenceItem.Modified = extendedModified;
                                    }

                                    _reportDataOutput.LongtermAbsenceItems.Add(longtermAbsenceItem);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(ex);
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        private void CalculateAndSetAbsenceRatio(List<TimeEmployeeScheduleDataSmallDTO> timeEmployeeSchedules, List<TimePayrollTransactionDTO> items, List<TimeAbsenceDetailDTO> timeAbsenceDetails, List<TimePayrollTransactionDTO> allItems)
        {
            foreach (var tran in items)
            {
                var absenceDetail = timeAbsenceDetails.FirstOrDefault(f => f.EmployeeId == tran.EmployeeId && f.Date == tran.Date && (f.TimeDeviationCauseId == tran.TimeDeviationCauseStartId || f.TimeDeviationCauseId == tran.TimeDeviationCauseStopId));

                if (absenceDetail?.Ratio != null)
                {
                    if (absenceDetail.Ratio != 0)
                        tran.AbsenceRatio = absenceDetail.Ratio.Value;
                    else
                        tran.AbsenceRatio = 0;
                }
            }

            foreach (var transOnDate in items.GroupBy(g => g.Date).OrderBy(o => o.Key))
            {
                if (transOnDate.Where(w => w.IsAbsence()).Sum(s => s.Quantity) != 0 && transOnDate.All(a => a.AbsenceRatio == 0))
                {
                    var scheduleOnDate = timeEmployeeSchedules.Where(f => f.Date == transOnDate.Key).ToList();
                    var absenceTransactionsOnDate = transOnDate.Where(w => w.IsAbsence()).ToList();

                    if (!scheduleOnDate.IsNullOrEmpty())
                    {
                        var schedule = scheduleOnDate.Sum(s => Convert.ToInt16(s.Length));

                        if (schedule != 0)
                        {
                            var ratio = absenceTransactionsOnDate.Sum(a => a.Quantity) / (decimal)schedule;
                            absenceTransactionsOnDate.ForEach(f => f.AbsenceRatio = ratio * 100);
                        }
                    }
                }
                else if (transOnDate.All(a => a.AbsenceRatio == 0))
                {
                    var absenceTransactionsWithZeroRatio = transOnDate.Where(w => w.IsAbsence() && w.AbsenceRatio == 0).ToList();

                    foreach (var transaction in absenceTransactionsWithZeroRatio)
                    {
                        var previousDate = allItems.Where(w => w.Date < transaction.Date && w.AbsenceRatio != 0).OrderByDescending(o => o.Date).FirstOrDefault()?.Date;
                        var futureDate = allItems.Where(w => w.Date > transaction.Date && w.AbsenceRatio != 0).OrderBy(o => o.Date).FirstOrDefault()?.Date;

                        if (previousDate != null)
                        {
                            var previousRatio = allItems.First(f => f.Date == previousDate).AbsenceRatio;
                            transaction.AbsenceRatio = previousRatio;
                        }
                        else if (futureDate != null)
                        {
                            var futureRatio = allItems.First(f => f.Date == futureDate).AbsenceRatio;
                            transaction.AbsenceRatio = futureRatio;
                        }
                        else
                        {
                            transaction.AbsenceRatio = 100;
                        }
                    }
                }
            }
        }

        private List<Tuple<DateTime, DateTime, decimal, List<TimePayrollTransactionDTO>>> GetCoherentAbsenceIntervals(List<TimePayrollTransactionDTO> transactions, bool ignoreRatio)
        {
            // Set all transactions to ratio = 0
            if (ignoreRatio)
                transactions.ForEach(f => f.AbsenceRatio = 0);

            // List to store the resulting intervals
            List<Tuple<DateTime, DateTime, decimal, List<TimePayrollTransactionDTO>>> absenceIntervalTuples = new List<Tuple<DateTime, DateTime, decimal, List<TimePayrollTransactionDTO>>>();

            // If there are no transactions, return an empty list
            if (transactions == null || transactions.Count == 0)
            {
                return absenceIntervalTuples;
            }

            // Find the first and last dates in the transactions
            DateTime firstDate = transactions[0].Date.Value;
            DateTime lastDate = transactions[transactions.Count - 1].Date.Value;

            // Initialize the interval variables
            DateTime startDate = firstDate;
            DateTime endDate = startDate;
            decimal startRatio = transactions[0].AbsenceRatio;
            List<TimePayrollTransactionDTO> intervalTransactions = new List<TimePayrollTransactionDTO>();

            // Loop through all dates between the first and last dates
            DateTime currentDate = firstDate;
            while (currentDate <= lastDate)
            {
                // Find any transactions on the current date
                List<TimePayrollTransactionDTO> transactionsOnDate = transactions.Where(t => t.Date == currentDate).ToList();

                // If there are transactions on the current date, add them to the current interval
                if (transactionsOnDate.Count > 0)
                {
                    decimal currentRatio = transactionsOnDate[0].AbsenceRatio;

                    // If the current date is consecutive to the current interval and has the same AbsenceRatio, add the transactions to the current interval
                    if (currentDate == endDate.AddDays(1) && currentRatio == startRatio)
                    {
                        intervalTransactions.AddRange(transactionsOnDate);
                        endDate = currentDate;
                    }
                    // Otherwise, add the current interval to the list and start a new interval with the current transactions
                    else
                    {
                        if (intervalTransactions.Count > 0)
                        {
                            absenceIntervalTuples.Add(new Tuple<DateTime, DateTime, decimal, List<TimePayrollTransactionDTO>>(startDate, endDate, startRatio, intervalTransactions));
                        }
                        startDate = currentDate;
                        endDate = currentDate;
                        startRatio = currentRatio;
                        intervalTransactions = new List<TimePayrollTransactionDTO>(transactionsOnDate);
                    }
                }

                // Move to the next date
                currentDate = currentDate.AddDays(1);
            }

            // If there is a current interval at the end, add it to the list
            if (intervalTransactions.Count > 0)
            {
                absenceIntervalTuples.Add(new Tuple<DateTime, DateTime, decimal, List<TimePayrollTransactionDTO>>(startDate, endDate, startRatio, intervalTransactions));
            }

            return absenceIntervalTuples;
        }
    }

    public class LongtermAbsenceReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_LongtermAbsenceMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public LongtermAbsenceReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_LongtermAbsenceMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_LongtermAbsenceMatrixColumns.Unknown;
        }
    }

    public class LongtermAbsenceReportDataInput
    {
        public CreateReportResult reportResult { get; set; }
        public List<LongtermAbsenceReportDataField> Columns { get; set; }
        public List<int> filterAccountIds { get; set; }

        public LongtermAbsenceReportDataInput(CreateReportResult reportResult, List<LongtermAbsenceReportDataField> columns, List<int> filterAccountIds = null)
        {
            this.reportResult = reportResult;
            this.Columns = columns;
            this.filterAccountIds = filterAccountIds;
        }
    }

    public class LongtermAbsenceReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<LongtermAbsenceItem> LongtermAbsenceItems { get; set; }
        public LongtermAbsenceReportDataInput Input { get; set; }

        public LongtermAbsenceReportDataOutput(LongtermAbsenceReportDataInput input)
        {
            this.LongtermAbsenceItems = new List<LongtermAbsenceItem>();
            this.Input = input;
        }
    }

}
