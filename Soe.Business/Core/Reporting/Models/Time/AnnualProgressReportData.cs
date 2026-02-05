using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class AnnualProgressReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly AnnualProgressReportDataInput _reportDataInput;
        private readonly AnnualProgressReportDataOutput _reportDataOutput;

        public AnnualProgressReportData(ParameterObject parameterObject, AnnualProgressReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new AnnualProgressReportDataOutput(reportDataInput);
        }

        public AnnualProgressReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(true);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(true);

            bool selectionHasSpecifiedEmployeeIds = SelectionHasSpecifiedEmployeeIds(reportResult);
            
            if (!selectionHasSpecifiedEmployeeIds)
            {
                employees = EmployeeManager.GetAllEmployees(reportResult.ActorCompanyId, loadEmployment: true);
                selectionEmployeeIds = employees.Select(s => s.EmployeeId).ToList();
            }

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
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var dims = base.GetAccountDimsFromCache(entities, DataCache.CacheConfig.Company(reportResult.ActorCompanyId));
                    dims.CalculateLevels();

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
                    StaffingStatisticsReportData accountHierarchyReportData = new StaffingStatisticsReportData(parameterObject, reportDataInput: new StaffingStatisticsReportDataInput(reportResult, staffingStatisticsReportDataReportDataFields));
                    StaffingStatisticsReportDataOutput output = accountHierarchyReportData.CreateOutput(reportResult);
                    var groupList = accountHierarchyReportData.GetStaffingStatisticsGroups(entities, matrixColumnsSelection: matrixColumnsSelection, selectionAccountIds, selectedDims, dims, accounts, useAccountHierarchy, false);
                    var staffingStatisticsGroupItem = TimeScheduleManager.GetStaffingStatisticsGroupItem(groupList, selectionEmployeeIds, CalendarUtility.GetBeginningOfYear(selectionDateFrom).AddYears(-1), CalendarUtility.GetEndOfYear(selectionDateTo), false, false, true, true, accounts, null, null, employees, Guid.NewGuid().ToString(), true, allAccountsMustbeOnEntity:true);

                    List<AnnualProgressItem> items = new List<AnnualProgressItem>();

                    foreach (var group in staffingStatisticsGroupItem.StaffingStatisticsRows)
                    {
                        var frequencies = group.StaffingNeedsFrequencies.Where(w => w.TimeFrom.Year == selectionDateFrom.Year && w.FrequencyType == FrequencyType.Actual).ToList();
                        var budgetFrequencies = group.StaffingNeedsFrequencies.Where(w => w.TimeFrom.Year == selectionDateFrom.Year && w.FrequencyType == FrequencyType.Budget).ToList();
                        var lastYearFrequencies = group.LastYearStaffingNeedsFrequencies.Where(w => w.FrequencyType == FrequencyType.Actual).ToList();
                        var transactions = group.TransactionsDict.Values.SelectMany(s => s).Where(w => (w.IsWorkTime() || w.IsAddedOrOverTime()));
                        var transactionsWithoutInternalAccounts = transactions.Where(w => w.AccountInternals == null || w.AccountInternals.Count == 0).ToList();
                        var employeeIdIdsOnTransactionsWithoutInternalAccounts = transactionsWithoutInternalAccounts.Select(s => s.EmployeeId).Distinct().ToList();
                        var transactionsLackingCorrectAccount = transactions.Where(w => !w.AccountInternals.Any(a => a.AccountId == group.StaffingStatisticsGroup.AccountId)).ToList();
                        var employeeIdIdsOnTransactionsLackingCorrectAccount = transactionsLackingCorrectAccount.Select(s => s.EmployeeId).Distinct().ToList();
                        var thisYearWorkTimeTransactions = transactions.Where(w => (w.IsWorkTime() || w.IsAddedOrOverTime()) && w.Date >= CalendarUtility.GetBeginningOfYear(selectionDateFrom)).ToList();
                        var lastYearWorkTimeTransactions = transactions.Where(w => (w.IsWorkTime() || w.IsAddedOrOverTime()) && w.Date >= CalendarUtility.GetBeginningOfYear(selectionDateFrom.AddYears(-1)) && w.Date <= CalendarUtility.GetEndOfYear(selectionDateFrom.AddYears(-1))).ToList();
                        var previousSunday = selectionDateTo.DayOfWeek == DayOfWeek.Sunday ? CalendarUtility.GetEndOfDay(selectionDateTo) : CalendarUtility.GetEndOfDay(CalendarUtility.GetBeginningOfWeek(selectionDateTo).AddDays(-1));
                        var previousEndOfMonth = CalendarUtility.GetEndOfDay(CalendarUtility.GetBeginningOfMonth(selectionDateTo).AddDays(-1));
                        decimal ratioOfYear = decimal.Divide(CalendarUtility.GetNumberOfWeeksToDate(previousSunday), 25);
                        var workTimehoursBudgetToPreviousSunday = decimal.Divide(budgetFrequencies.Where(w => CalendarUtility.GetEndOfDay(w.TimeFrom) <= previousSunday).Sum(s => s.Minutes), 60);
                        var workTimehoursBudgetToPreviousEndOfMonth = decimal.Divide(budgetFrequencies.Where(w => CalendarUtility.GetEndOfDay(w.TimeFrom) <= previousEndOfMonth).Sum(s => s.Minutes), 60);
                        var budgetWorkTimeHoursInYear = decimal.Divide(budgetFrequencies.Sum(s => s.NbrOfMinutes), 60);
                        AnnualProgressItem annualProgressItem = new AnnualProgressItem();
                        annualProgressItem.Date = previousSunday.Date;

                        //Målsättning per vecka - Budgeterat antal arbetstimmar från frekvenser delat i antal veckor
                        annualProgressItem.GoalPerWeek = decimal.Divide(budgetWorkTimeHoursInYear, 52);

                        //Målsättning per månad - Budgeterat antal arbetstimmar från frekvenser delat i antal månader
                        annualProgressItem.GoalPerMonth = decimal.Divide(budgetWorkTimeHoursInYear, 12);

                        //Timmar föregående år medel per vecka - Faktiskt antal timmar från transaktioner delat i antal veckor
                        annualProgressItem.LastYearAveragePerWeek = decimal.Divide(decimal.Divide(lastYearWorkTimeTransactions.Sum(s => s.Quantity), 60), 52);

                        //Timmar föregående år medel per månad - Faktiskt antal timmar från transaktioner delat i antal veckor
                        annualProgressItem.LastYearAveragePerMonth = decimal.Divide(lastYearWorkTimeTransactions.Sum(s => s.Quantity), 60) / 12;

                        //Timmar föregående år medel per månad - Faktiskt antal timmar från transaktioner delat i antal månader
                        annualProgressItem.DifferenceAverageFromLastYearPerMonth = decimal.Divide(decimal.Divide(lastYearWorkTimeTransactions.Sum(s => s.Quantity), 60), 12);

                        //Timmer per vecka tills föregående söndag
                        annualProgressItem.AveragePerWeekToDate = decimal.Divide(decimal.Divide(thisYearWorkTimeTransactions.Where(w => w.Date <= previousSunday).Sum(s => s.Quantity), 60), CalendarUtility.GetNumberOfWeeksToDate(previousSunday));

                        // Timmar per månad tills föregående månad
                        annualProgressItem.AveragePerMonthToDate = decimal.Divide(decimal.Divide(thisYearWorkTimeTransactions.Where(w => w.Date <= previousEndOfMonth).Sum(s => s.Quantity), 60), previousEndOfMonth.Month);

                        //Avvikelse mot föregående år AveragePerWeekToDate - LastYearAveragePerWeek
                        annualProgressItem.DifferenceAverageFromLastYearPerWeek = annualProgressItem.AveragePerWeekToDate - annualProgressItem.LastYearAveragePerWeek;

                        //Avvikelse mot föregående år AveragePerMonthToDate - LastYearAveragePerMonth
                        annualProgressItem.DifferenceAverageFromLastYearPerMonth = annualProgressItem.AveragePerMonthToDate - annualProgressItem.LastYearAveragePerMonth;

                        //Avvikelse mot mål innevarande år AveragePerWeekToDate - GoalPerWeek
                        annualProgressItem.DifferenceAverageFromGoalPerWeek = annualProgressItem.AveragePerWeekToDate - annualProgressItem.GoalPerWeek;

                        //Avvikelse mot mål innevarande år AveragePerMonthToDate - GoalPerMonth
                        annualProgressItem.DifferenceAverageFromGoalPerMonth = annualProgressItem.AveragePerMonthToDate - annualProgressItem.GoalPerMonth;

                        //Arbetstimmar tills föregående söndag
                        annualProgressItem.WorkingHoursToDate = decimal.Divide(thisYearWorkTimeTransactions.Where(w => w.Date <= previousSunday).Sum(s => s.Quantity), 60);

                        //Försäljning till dagens datum
                        if (_reportDataInput.Columns.Any(a => a.Column == TermGroup_AnnualProgressMatrixColumns.SalesToDate))
                            annualProgressItem.SalesToDate = frequencies.Where(w => w.TimeFrom.Date >= CalendarUtility.GetBeginningOfYear(selectionDateFrom) && w.TimeFrom.Date <= previousSunday).Sum(s => s.Amount);

                        //Antal timmar som finns kvar per vecka (YTG)
                        annualProgressItem.RemainingYearAveragePerWeek = decimal.Divide((budgetWorkTimeHoursInYear - annualProgressItem.WorkingHoursToDate), CalendarUtility.GetNumberOfWeeksRemainingInYear(previousSunday));

                        //Antal timmar som finns kvar per månad (YTG)
                        var demoninator = 12 - previousEndOfMonth.Month;
                        if (demoninator == 0)
                            demoninator = 1;
                        annualProgressItem.RemainingYearAveragePerMonth = (budgetWorkTimeHoursInYear - thisYearWorkTimeTransactions.Sum(s => (decimal.Divide(s.Quantity, 60)))) / demoninator;

                        //Försäljning per arbetad timme till dagens datum                        
                        annualProgressItem.FPATToDate = annualProgressItem.WorkingHoursToDate != 0 ? decimal.Divide(annualProgressItem.SalesToDate, annualProgressItem.WorkingHoursToDate) : 0;

                        //Målsättning försäljning per arbetad timme
                        annualProgressItem.FPATGoal = budgetWorkTimeHoursInYear != 0 ? decimal.Divide(budgetFrequencies.Sum(s => s.Amount), budgetWorkTimeHoursInYear) : 0;

                        annualProgressItem.AccountAnalysisFields = new List<AccountAnalysisField>();
                        if (!NumberUtility.CheckIfAllIntAndDecimalPropertiesAreZeroOrNull(annualProgressItem))
                        {
                            foreach (var accountId in group.StaffingStatisticsGroup.ConnectedAccountIds)
                            {
                                var acc = accounts.FirstOrDefault(a => a.AccountId == accountId);
                                if (acc != null)
                                    annualProgressItem.AccountAnalysisFields.Add(new AccountAnalysisField(acc));
                            }
                            var account = accounts.FirstOrDefault(a => a.AccountId == group.StaffingStatisticsGroup.AccountId);
                            if (account != null)
                                annualProgressItem.AccountAnalysisFields.Add(new AccountAnalysisField(account));

                            _reportDataOutput.AnnualProgressItems.Add(annualProgressItem);
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
    }

    public class AnnualProgressReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_AnnualProgressMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public AnnualProgressReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_AnnualProgressMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_AnnualProgressMatrixColumns.Unknown;
        }
    }

    public class AnnualProgressReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AnnualProgressReportDataField> Columns { get; set; }
        public List<AccountDTO> AccountInternals { get; set; }
        public List<int> FilterAccountIds { get; set; }
        public AnnualProgressReportDataInput(CreateReportResult reportResult, List<AnnualProgressReportDataField> columns, List<AccountDimDTO> accountDims, List<AccountDTO> accountInternals, List<int> filterAccountIds = null)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.AccountInternals = accountInternals;
            this.FilterAccountIds = filterAccountIds;
        }
    }

    public class AnnualProgressReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<AnnualProgressItem> AnnualProgressItems { get; set; }
        public AnnualProgressReportDataInput Input { get; set; }

        public AnnualProgressReportDataOutput(AnnualProgressReportDataInput input)
        {
            this.AnnualProgressItems = new List<AnnualProgressItem>();
            this.Input = input;
        }
    }
}
