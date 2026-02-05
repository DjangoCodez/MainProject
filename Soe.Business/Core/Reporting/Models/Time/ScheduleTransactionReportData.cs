using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class ScheduleTransactionReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly ScheduleTransactionReportDataInput _reportDataInput;
        private readonly ScheduleTransactionReportDataOutput _reportDataOutput;

        private bool LoadSubstituteShifts
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                                      a.Column == TermGroup_ScheduleTransactionMatrixColumns.SubstituteShift ||
                                      a.Column == TermGroup_ScheduleTransactionMatrixColumns.SubstituteShiftCalculated);
            }
        }
        private bool LoadAccountInternal
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
            }
        }
        private bool LoadShiftTypes
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeName ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeScheduleTypeName);
            }
        }
        private bool loadShiftTypesScheduleType
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeScheduleTypeName);
            }
        }
        private bool LoadScheduleTypes
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.ScheduleTypeName);
            }
        }
        private bool MergeOnAccount
        {
            get
            {
                return !_reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.EmployeeName ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.EmployeeNr ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.SocialSec ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.Date ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.ScheduleTypeName ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.StartTime ||
                       a.Column == TermGroup_ScheduleTransactionMatrixColumns.StopTime);
            }
        }
        private bool LoadExtraFields
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.ColumnKey.ToLower().Contains("extrafield"));
            }
        }
        private bool LoadTimeCodes
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_ScheduleTransactionMatrixColumns.TimeCodeCode ||
                      a.Column == TermGroup_ScheduleTransactionMatrixColumns.TimeCodeName
                      );
            }
        }
        private bool LoadTimeRules
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                      a.Column == TermGroup_ScheduleTransactionMatrixColumns.TimeRuleName);
            }
        }
        private bool LoadEmploymentCalender
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                                     a.Column == TermGroup_ScheduleTransactionMatrixColumns.EmploymentPercent ||
                                     a.Column == TermGroup_ScheduleTransactionMatrixColumns.EmployeeGroup);
            }
        }
        private bool LoadEmployeeGroups
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                                                    a.Column == TermGroup_ScheduleTransactionMatrixColumns.EmployeeGroup);
            }
        }
        private bool LoadCost
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                                     a.Column == TermGroup_ScheduleTransactionMatrixColumns.GrossCost ||
                                     a.Column == TermGroup_ScheduleTransactionMatrixColumns.NetCost);
            }
        }

        public ScheduleTransactionReportData(ParameterObject parameterObject, ScheduleTransactionReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new ScheduleTransactionReportDataOutput(reportDataInput, parameterObject);
        }

        public static List<ScheduleTransactionReportDataReportDataField> GetPossibleDataFields()
        {
            List<ScheduleTransactionReportDataReportDataField> possibleFields = new List<ScheduleTransactionReportDataReportDataField>();
            EnumUtility.GetValues<TermGroup_ScheduleTransactionMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new ScheduleTransactionReportDataReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        #region Loaded Data

        private Dictionary<int, List<TimeScheduleTransactionItem>> companyScheduleTransactionDTOs;

        #endregion

        public ScheduleTransactionReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            ActionResult result = new ActionResult();

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out bool selectionExcludeFreeDays, "excludeFreeDays");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeBreaks, "includeBreaks");
            TryGetBoolFromSelection(reportResult, out bool selectionExcludeAbsence, "excludeAbsence");
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetIdsFromSelection(reportResult, out List<int> shiftTypeIds, "shiftTypes", true);

            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;
            List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();

            List<EmployeeGroup> employeeGroups = null;
            List<PayrollGroup> payrollGroups = null;
            List<PayrollPriceType> payrollPriceTypes = null;
            List<AnnualLeaveGroup> annualLeaveGroups = null;

            #endregion

            try
            {
                #region Data

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true);

                _reportDataOutput.Employees = employees.ToDTOs().ToList();

                Parallel.Invoke(() =>
                {
                    companyScheduleTransactionDTOs = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(selectionDateFrom, selectionDateTo, employees, reportResult.Input.ActorCompanyId, base.RoleId, shiftTypeIds: shiftTypeIds, splitOnBreaks: true, removeBreaks: !selectionIncludeBreaks, addAmounts: LoadCost).ToTimeScheduleTransactionItemList();

                    if (selectionExcludeFreeDays)
                    {
                        Dictionary<int, List<TimeScheduleTransactionItem>> filteredScheduleTransactionDTOs = new Dictionary<int, List<TimeScheduleTransactionItem>>();

                        foreach (var companyScheduleTransactionDTO in companyScheduleTransactionDTOs)
                            filteredScheduleTransactionDTOs.Add(companyScheduleTransactionDTO.Key, companyScheduleTransactionDTO.Value.Where(t => t.Length > 0).ToList());

                        companyScheduleTransactionDTOs = filteredScheduleTransactionDTOs;
                    }
                    if (!selectionIncludeBreaks)
                    {
                        Dictionary<int, List<TimeScheduleTransactionItem>> filteredScheduleTransactionDTOs = new Dictionary<int, List<TimeScheduleTransactionItem>>();

                        foreach (var companyScheduleTransactionDTO in companyScheduleTransactionDTOs)
                            filteredScheduleTransactionDTOs.Add(companyScheduleTransactionDTO.Key, companyScheduleTransactionDTO.Value.Where(t => !t.IsBreak).ToList());

                        companyScheduleTransactionDTOs = filteredScheduleTransactionDTOs;
                    }
                    if (selectionExcludeAbsence && _reportDataOutput.TimeCodes != null)
                    {
                        List<int> absenceTimeCodeIds = _reportDataOutput.TimeCodes.Where(t => t.Type == (int)SoeTimeCodeType.Absense).Select(t => t.TimeCodeId).ToList();

                        Dictionary<int, List<TimeScheduleTransactionItem>> filteredScheduleTransactionDTOs = new Dictionary<int, List<TimeScheduleTransactionItem>>();

                        foreach (var companyScheduleTransactionDTO in companyScheduleTransactionDTOs)
                            filteredScheduleTransactionDTOs.Add(companyScheduleTransactionDTO.Key, companyScheduleTransactionDTO.Value.Where(t => !absenceTimeCodeIds.Contains(t.TimeCodeId)).ToList());

                        companyScheduleTransactionDTOs = filteredScheduleTransactionDTOs;
                    }
                },
                () =>
                {
                    if (LoadShiftTypes)
                        _reportDataOutput.ShiftTypes = base.GetShiftTypesFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId), loadShiftTypesScheduleType).ToDTOs().ToList();

                    if (LoadScheduleTypes)
                        _reportDataOutput.TimeScheduleTypes = base.GetTimeScheduleTypesFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId), false).ToDTOs(false).ToList();

                    if (LoadExtraFields)
                        extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();

                    if (LoadTimeCodes || selectionExcludeAbsence)
                        _reportDataOutput.TimeCodes = TimeCodeManager.GetTimeCodes(reportResult.Input.ActorCompanyId);

                    if (LoadTimeRules)
                        _reportDataOutput.TimeRules = TimeRuleManager.GetTimeRulesFromCache(entitiesReadOnly, reportResult.Input.ActorCompanyId);

                    if (LoadSubstituteShifts)
                    {
                        //Set commandtimeout to 1 seconds per person and month if selectionemployeeids is more than 50. dates is selelectiondatefrom to selectiondateto
                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        entitiesReadOnly.CommandTimeout = Convert.ToInt32(selectionEmployeeIds.Count > 50 ? selectionEmployeeIds.Count * (decimal.Divide(CalendarUtility.GetDates(selectionDateFrom, selectionDateTo).Count, 30)) : 100);
                        if (entitiesReadOnly.CommandTimeout > 1200)
                            entitiesReadOnly.CommandTimeout = 1200;

                        _reportDataOutput.SubstituteShifts = TimeScheduleManager.GetSubstituteShifts(entitiesReadOnly, reportResult.ActorCompanyId, selectionEmployeeIds, CalendarUtility.GetDates(selectionDateFrom, selectionDateTo));
                    }

                    if (LoadEmploymentCalender)
                    {
                        employeeGroups = base.GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId));
                        payrollGroups = GetPayrollGroupsFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId));
                        payrollPriceTypes = GetPayrollPriceTypesFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId));
                        annualLeaveGroups = base.GetAnnualLeaveGroupsFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId));
                        _reportDataOutput.EmploymentCalenderDTOs = EmployeeManager.GetEmploymentCalenderDTOs(employees, selectionDateFrom, selectionDateTo, employeeGroups, payrollGroups, payrollPriceTypes, annualLeaveGroups: annualLeaveGroups);
                    }

                });

                _reportDataOutput.ScheduleTransactions = new Dictionary<int, List<TimeScheduleTransactionItem>>();

                if (filterOnAccounting && selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount && !selectionAccountIds.IsNullOrEmpty())
                {
                    foreach (var employeeGroup in companyScheduleTransactionDTOs.GroupBy(g => g.Key))
                    {
                        List<TimeScheduleTransactionItem> filtered = new List<TimeScheduleTransactionItem>();

                        foreach (var group in employeeGroup.SelectMany(s => s.Value).GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                        {
                            foreach (var item in group.ToList())
                            {
                                if (item.AccountInternals != null && item.AccountInternals.ValidOnFiltered(validAccountInternals))
                                {
                                    filtered.Add(item);
                                    filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                                }
                            }
                        }
                        if (filtered.Any())
                            _reportDataOutput.ScheduleTransactions.Add(employeeGroup.Key, filtered.Distinct().ToList());
                    }

                }
                else if (filterOnAccounting && selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock && !selectionAccountIds.IsNullOrEmpty())
                {
                    foreach (var employeeGroup in companyScheduleTransactionDTOs.GroupBy(g => g.Key))
                    {
                        List<TimeScheduleTransactionItem> filtered = new List<TimeScheduleTransactionItem>();

                        foreach (var group in employeeGroup.SelectMany(s => s.Value).GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                        {
                            foreach (var item in group.Where(w => w.AccountId.HasValue && selectionAccountIds.Contains(w.AccountId.Value)).ToList())
                            {
                                filtered.Add(item);
                                filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                            }
                        }
                        if (filtered.Any())
                            _reportDataOutput.ScheduleTransactions.Add(employeeGroup.Key, filtered.Distinct().ToList());
                    }
                }
                else if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                {
                    bool validForAccountOnBlockMatch = validAccountInternals.GroupBy(g => g.AccountDimId).Count() == 1;
                    foreach (var employeeGroup in companyScheduleTransactionDTOs.GroupBy(g => g.Key))
                    {
                        List<TimeScheduleTransactionItem> filtered = new List<TimeScheduleTransactionItem>();

                        foreach (var group in employeeGroup.SelectMany(s => s.Value).GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                        {
                            foreach (var item in group.ToList())
                            {
                                if (item.AccountInternals != null && item.AccountInternals.ValidOnFiltered(validAccountInternals))
                                {
                                    filtered.Add(item);
                                    filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                                }
                                else if (validForAccountOnBlockMatch && item.AccountId.HasValue && selectionAccountIds.Contains(item.AccountId.Value))
                                {
                                    filtered.Add(item);
                                    filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                                }
                            }
                        }
                        if (filtered.Any())
                            _reportDataOutput.ScheduleTransactions.Add(employeeGroup.Key, filtered.Distinct().ToList());
                    }
                }
                else
                    _reportDataOutput.ScheduleTransactions = companyScheduleTransactionDTOs;

                if (LoadAccountInternal)
                {
                    var tempDict = _reportDataOutput.ScheduleTransactions;
                    _reportDataOutput.ScheduleTransactions = new Dictionary<int, List<TimeScheduleTransactionItem>>();
                    foreach (KeyValuePair<int, List<TimeScheduleTransactionItem>> keyValuePair in tempDict)
                    {
                        _reportDataOutput.ScheduleTransactions.Add(keyValuePair.Key, keyValuePair.Value.ArrangeAccountHierarchy());
                    }
                }

                #region Load ExtraFields

                if (LoadExtraFields && extraFieldRecords.Any())
                {
                    Dictionary<int, string> yesNoDictionary = base.GetTermGroupDict(TermGroup.YesNo, base.GetLangId());
                    var tempDictionary = _reportDataOutput.ScheduleTransactions;
                    _reportDataOutput.ScheduleTransactions = new Dictionary<int, List<TimeScheduleTransactionItem>>();

                    foreach (KeyValuePair<int, List<TimeScheduleTransactionItem>> keyValuePair in tempDictionary)
                    {
                        _reportDataOutput.ScheduleTransactions.Add(keyValuePair.Key, keyValuePair.Value.AddExtraFields(extraFieldRecords.Where(x => x.RecordId == keyValuePair.Key).ToList(), _reportDataInput.Columns.Where(w => w.ColumnKey.ToLower().Contains("extraFieldEmployee".ToLower())).ToList(), yesNoDictionary));
                    }
                }

                List<TimeScheduleTransactionItem> merged = new List<TimeScheduleTransactionItem>();

                if (!MergeOnAccount)
                {
                    foreach (var transes in _reportDataOutput.ScheduleTransactions.Select(s => s.Value))
                    {
                        if (transes.Count > 0)
                        {
                            foreach (TimeScheduleTransactionItem transactionPart in transes)
                            {
                                if (transactionPart.NetLength == 0 && transactionPart.Length > 0)
                                    transactionPart.NetLength = transactionPart.Length;
                                if (transactionPart.NetQuantity == 0 && transactionPart.Quantity > 0)
                                    transactionPart.NetQuantity = transactionPart.Quantity;
                            }
                        }

                        foreach (var trans in transes.GroupBy(g => g.GroupOn(_reportDataInput.Columns.Select(s => s.Column).ToList(), MergeOnAccount)))
                        {
                            merged.Add(MergeScheduleTransactionRecords(trans));
                        }
                    }
                    merged = merged.OrderBy(o => o.Date).ThenBy(t => t.StartTime).ToList().OrderBy(o => o.EmployeeId).ToList();
                }
                else
                {
                    foreach (var transes in _reportDataOutput.ScheduleTransactions.SelectMany(s => s.Value).GroupBy(g => g.GetAccountingIdString()))
                    {
                        foreach (var trans in transes.GroupBy(g => g.GroupOn(_reportDataInput.Columns.Select(s => s.Column).ToList(), MergeOnAccount)))
                        {
                            merged.Add(MergeScheduleTransactionRecords(trans));
                        }
                    }
                }

                List<TimeScheduleTransactionItem> finalMerged = new List<TimeScheduleTransactionItem>();

                foreach (var trans in merged.GroupBy(g => g.GroupOn(_reportDataInput.Columns.Select(s => s.Column).ToList(), MergeOnAccount)))
                {
                    finalMerged.Add(MergeScheduleTransactionRecords(trans));
                }

                if (finalMerged.Any() && LoadSubstituteShifts && _reportDataOutput.SubstituteShifts.Any())
                {
                    foreach (var groupOnEmployee in finalMerged.GroupBy(g => g.EmployeeId))
                    {
                        var substitueShiftsDueToAbsence = _reportDataOutput.GetSubstituteShiftDTOs(groupOnEmployee.Key).Where(w => w.IsAssignedDueToAbsence).ToList();

                        if (substitueShiftsDueToAbsence.Any())
                        {
                            foreach (var shift in groupOnEmployee)
                            {
                                shift.SubstituteShiftCalculated = substitueShiftsDueToAbsence.Any(f => f.Link == shift.Link && f.Date == shift.Date && f.EmployeeId == shift.EmployeeId);
                            }
                        }
                    }
                }

                if (finalMerged.Any() && LoadEmploymentCalender && _reportDataOutput.EmploymentCalenderDTOs.Any())
                {
                    foreach (var groupOnEmployee in finalMerged.GroupBy(g => g.EmployeeId))
                    {
                        var employmentCalenderDTOs = _reportDataOutput.GetEmploymentCalenderDTOs(groupOnEmployee.Key);

                        if (employmentCalenderDTOs.Any())
                        {
                            foreach (var groupByDate in groupOnEmployee.GroupBy(g => g.Date))
                            {
                                var employmentCalenderDTO = employmentCalenderDTOs.FirstOrDefault(f => f.Date == groupByDate.Key);

                                if (employmentCalenderDTO != null)
                                {
                                    groupByDate.ToList().ForEach(f => f.EmploymentPercent = employmentCalenderDTO.Percent);

                                    if (LoadEmployeeGroups && !employeeGroups.IsNullOrEmpty())
                                    {
                                        var employeeGroup = employeeGroups.FirstOrDefault(f => f.EmployeeGroupId == employmentCalenderDTO.EmployeeGroupId);
                                        if (employeeGroup != null)
                                        {
                                            groupByDate.ToList().ForEach(f => f.EmployeeGroup = employeeGroup.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _reportDataOutput.ScheduleTransactions = finalMerged.Distinct().GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(ex);
            }

            return result;
        }

        public TimeScheduleTransactionItem MergeScheduleTransactionRecords(IGrouping<string, TimeScheduleTransactionItem> transaction)
        {
            var sorted = transaction.OrderBy(o => o.StartTime).ToList();
            var first = sorted.First();
            var last = sorted.Last();

            TimeScheduleTransactionItem mergedTrans = first;
            mergedTrans.Amount = transaction.Where(s => !s.IsBreak).Sum(s => s.Amount);
            mergedTrans.GrossAmount = transaction.Where(s => !s.IsBreak).Sum(s => s.GrossAmount);
            mergedTrans.NetLength = transaction.Where(s => !s.IsBreak).Sum(s => s.NetLength);
            mergedTrans.NetQuantity = transaction.Where(s => !s.IsBreak).Sum(s => s.NetQuantity);
            mergedTrans.StopTime = last.StopTime;
            mergedTrans.Link = last.Link;
            mergedTrans.SubstituteShift = last.StartTime != last.StopTime && last.SubstituteShift;
            mergedTrans.ExtraShift = last.StartTime != last.StopTime && last.ExtraShift;
            mergedTrans.SubstituteShiftCalculated = last.SubstituteShiftCalculated;

            return mergedTrans;
        }
    }

    public class ScheduleTransactionReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ScheduleTransactionMatrixColumns Column { get; set; }
        public string ColumnKey { get; private set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ScheduleTransactionReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ScheduleTransactionMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ScheduleTransactionMatrixColumns.Unknown;
        }
    }

    public class ScheduleTransactionReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<ScheduleTransactionReportDataReportDataField> Columns { get; set; }
        public int ActorCompanyId { get; set; }

        public ScheduleTransactionReportDataInput(CreateReportResult reportResult, List<ScheduleTransactionReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.ActorCompanyId = reportResult.ActorCompanyId;
        }
    }

    public class ScheduleTransactionReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public Dictionary<int, List<TimeScheduleTransactionItem>> ScheduleTransactions { get; set; }
        public List<EmployeeDTO> Employees { get; set; }
        public ScheduleTransactionReportDataInput Input { get; set; }
        public List<TimeScheduleTypeDTO> TimeScheduleTypes { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        public List<TimeCode> TimeCodes { get; set; }
        public List<TimeRule> TimeRules { get; set; }
        public List<SubstituteShiftDTO> SubstituteShifts { get; set; }

        public ScheduleTransactionReportDataOutput(ScheduleTransactionReportDataInput input, ParameterObject parameterObject)
        {
            this.ScheduleTransactions = new Dictionary<int, List<TimeScheduleTransactionItem>>();
            this.SubstituteShifts = new List<SubstituteShiftDTO>();
            this.EmploymentCalenderDTOs = new List<EmploymentCalenderDTO>();
            this.Employees = new List<EmployeeDTO>();
            this.Input = input;
        }

        private Dictionary<int, List<SubstituteShiftDTO>> substituteShiftsDict { get; set; }
        public Dictionary<int, List<SubstituteShiftDTO>> SubstituteShiftsDict()
        {
            if (substituteShiftsDict == null)
            {
                substituteShiftsDict = new Dictionary<int, List<SubstituteShiftDTO>>();
                foreach (var item in SubstituteShifts.GroupBy(g => g.EmployeeId))
                {
                    if (!substituteShiftsDict.ContainsKey(item.Key))
                        substituteShiftsDict.Add(item.Key, item.ToList());
                }
            }
            return substituteShiftsDict;
        }

        public List<SubstituteShiftDTO> GetSubstituteShiftDTOs(int employeeId)
        {
            if (SubstituteShiftsDict().ContainsKey(employeeId))
                return SubstituteShiftsDict()[employeeId];
            else
                return new List<SubstituteShiftDTO>();
        }

        public List<EmploymentCalenderDTO> EmploymentCalenderDTOs { get; set; }

        private Dictionary<int, List<EmploymentCalenderDTO>> employmentCalenderDict { get; set; }
        public Dictionary<int, List<EmploymentCalenderDTO>> EmploymentCalenderDict
        {
            get
            {
                if (this.employmentCalenderDict == null)
                {
                    this.employmentCalenderDict = new Dictionary<int, List<EmploymentCalenderDTO>>();
                    foreach (var item in EmploymentCalenderDTOs.GroupBy(g => g.EmployeeId))
                    {
                        if (!this.employmentCalenderDict.ContainsKey(item.Key))
                            this.employmentCalenderDict.Add(item.Key, item.ToList());
                    }
                }
                return this.employmentCalenderDict;
            }
        }

        public List<EmploymentCalenderDTO> GetEmploymentCalenderDTOs(int employeeId)
        {
            if (EmploymentCalenderDict.ContainsKey(employeeId))
                return EmploymentCalenderDict[employeeId];
            else
                return new List<EmploymentCalenderDTO>();
        }
    }

    public static class ScheduleTransactionReportDataExtentions
    {
        public static List<TimeScheduleTransactionItem> GetTimeScheduleTransactionItemObjectList(this List<TimeEmployeeScheduleDataSmallDTO> timeEmployeeScheduleDataSmallDTOList)
        {
            List<TimeScheduleTransactionItem> tranactionList = new List<TimeScheduleTransactionItem>();

            foreach (TimeEmployeeScheduleDataSmallDTO tranactionSamllDTO in timeEmployeeScheduleDataSmallDTOList)
            {
                tranactionList.Add(tranactionSamllDTO.GetTimeScheduleTransactionItemObject());
            }

            return tranactionList;
        }

        public static TimeScheduleTransactionItem GetTimeScheduleTransactionItemObject(this TimeEmployeeScheduleDataSmallDTO tranactionSamllDTO)
        {
            PropertyInfo[] dTOpropertyInfos = typeof(TimeEmployeeScheduleDataSmallDTO).GetProperties();
            PropertyInfo[] tranactionItempropertyInfos = typeof(TimeScheduleTransactionItem).GetProperties();

            string[] excludePr0pertyList = { "Quantity", "Length", "IsZeroSchedule", "QuantityHours" };

            TimeScheduleTransactionItem timeScheduleTransactionItem = new TimeScheduleTransactionItem();
            foreach (string dTOPropertyName in dTOpropertyInfos.Select(p => p.Name))
            {
                bool? canWtiteToDestination = tranactionItempropertyInfos?.FirstOrDefault(x => x.Name == dTOPropertyName)?.CanWrite;
                if (!excludePr0pertyList.Contains(dTOPropertyName) && (canWtiteToDestination.HasValue && canWtiteToDestination.Value))
                {
                    timeScheduleTransactionItem.TrySetEntityProperty(dTOPropertyName, tranactionSamllDTO.GetPropertyValue(dTOPropertyName));
                }
            }

            return timeScheduleTransactionItem;
        }

        public static Dictionary<int, List<TimeScheduleTransactionItem>> ToTimeScheduleTransactionItemList(this Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> input)
        {
            Dictionary<int, List<TimeScheduleTransactionItem>> result = new Dictionary<int, List<TimeScheduleTransactionItem>>();

            foreach (KeyValuePair<int, List<TimeEmployeeScheduleDataSmallDTO>> item in input)
            {
                result.Add(item.Key, item.Value.GetTimeScheduleTransactionItemObjectList());
            }

            return result;
        }

        public static List<TimeScheduleTransactionItem> ArrangeAccountHierarchy(this List<TimeScheduleTransactionItem> inputList)
        {
            inputList.ForEach(x => x.AccountInternals.ForEach(dim => x.AccountAnalysisFields.Add(new AccountAnalysisField(dim))));

            return inputList;
        }

        public static List<TimeScheduleTransactionItem> AddExtraFields(this List<TimeScheduleTransactionItem> inputList, List<ExtraFieldRecordDTO> extraFieldRecordDTOs, List<ScheduleTransactionReportDataReportDataField> extraFieldColumnList, Dictionary<int, string> yesNoDictionary)
        {
            foreach (var column in extraFieldColumnList)
            {
                if (column.Selection?.Options?.Key != null && int.TryParse(column.Selection.Options.Key, out int recordId))
                {
                    var matchOnEmployee = extraFieldRecordDTOs.FirstOrDefault(f => f.ExtraFieldId == recordId);
                    if (matchOnEmployee != null)
                    {
                        foreach (var transactionItem in inputList)
                        {
                            transactionItem.ExtraFieldAnalysisFields.Add(new ExtraFieldAnalysisField(matchOnEmployee, yesNoDictionary));
                        }
                        continue;
                    }
                }

                foreach (var transactionItem in inputList)
                {
                    transactionItem.ExtraFieldAnalysisFields.Add(new ExtraFieldAnalysisField(null));
                }
            }
            return inputList;
        }

        public static List<TimeScheduleTransactionItem> GetOverlappedBreaks(this List<TimeScheduleTransactionItem> l, List<TimeScheduleTransactionItem> breakitems, bool includeBreaksThatOnlyStartInScheduleBlock = false)
        {
            List<TimeScheduleTransactionItem> breaks = new List<TimeScheduleTransactionItem>();
            foreach (var shift in l)
            {
                breaks.AddRange(shift.GetOverlappedBreaks(breakitems, includeBreaksThatOnlyStartInScheduleBlock));
            }

            return breaks;
        }

        public static List<TimeScheduleTransactionItem> GetOverlappedBreaks(this TimeScheduleTransactionItem e, List<TimeScheduleTransactionItem> breaks, bool includeBreaksThatOnlyStartInScheduleBlock = false)
        {
            List<TimeScheduleTransactionItem> overlappedBreaks = new List<TimeScheduleTransactionItem>();

            foreach (var brk in breaks.Where(x => x.IsBreak && x.EmployeeId == e.EmployeeId && x.Date == e.Date).ToList())
            {
                if (brk.StartTime >= e.StartTime && brk.StopTime <= e.StopTime)
                    overlappedBreaks.Add(brk);
                else if (includeBreaksThatOnlyStartInScheduleBlock && brk.StartTime >= e.StartTime && brk.StartTime < e.StopTime)
                    overlappedBreaks.Add(brk);
            }

            return overlappedBreaks;
        }
    }
}
