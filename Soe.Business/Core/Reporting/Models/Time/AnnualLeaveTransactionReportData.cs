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
    public class AnnualLeaveTransactionReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly AnnualLeaveTransactionReportDataInput _reportDataInput;
        private readonly AnnualLeaveTransactionReportDataOutput _reportDataOutput;
        private Dictionary<int, string> _transactionTypeDictionary;

        public AnnualLeaveTransactionReportData(ParameterObject parameterObject, AnnualLeaveTransactionReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new AnnualLeaveTransactionReportDataOutput(reportDataInput);
        }

        public static List<AnnualLeaveTransactionReportDataField> GetPossibleDataFields()
        {
            List<AnnualLeaveTransactionReportDataField> possibleFields = new List<AnnualLeaveTransactionReportDataField>();
            EnumUtility.GetValues<TermGroup_AnnualLeaveTransactionMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new AnnualLeaveTransactionReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public AnnualLeaveTransactionReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq

            // Annual Leave wasn't used until 2017/01/01, therefore always set from-date to that
            DateTime selectionDateFrom = new DateTime(2017, 01, 01);
            if (!TryGetDateFromSelection(reportResult, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out bool selectionOnlyInactive, out bool? selectionActiveEmployees);

            _transactionTypeDictionary = base.GetTermGroupDict(TermGroup.AnnualLeaveTransactionType, base.GetLangId());

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion
            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    if (selectionEmployeeIds.Any())
                    {
                        #region loadData
                        if (employees == null)
                            employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees);

                        List<AnnualLeaveTransaction> annualLeaveTransactions = AnnualLeaveManager.GetAnnualLeaveTransactions(entities, reportResult.ActorCompanyId, excludeYearly: true, excludeManually: true);

                        List<AnnualLeaveBalance> annualLeaveBalances = AnnualLeaveManager.GetAnnualLeaveBalance(selectionDateTo, selectionEmployeeIds, reportResult.ActorCompanyId);
                        #endregion

                        foreach (AnnualLeaveTransaction annualLeaveTransaction in annualLeaveTransactions)
                        {
                            // Earlier date from spent and earned should decide length of day and also if transaction should be shown
                            DateTime? earliestDate = null;
                            if (annualLeaveTransaction.DateEarned.HasValue)
                            {
                                earliestDate = annualLeaveTransaction.DateEarned;
                            }
                            if (annualLeaveTransaction.DateSpent.HasValue)
                            {
                                if (earliestDate == null || annualLeaveTransaction.DateSpent < earliestDate) earliestDate = annualLeaveTransaction.DateSpent;
                            }

                            // Don't show trans if earliest date is after dateTo selection
                            if (earliestDate > selectionDateTo)
                                continue;

                            Employee employee = employees.FirstOrDefault(e => e.EmployeeId == annualLeaveTransaction.EmployeeId);
                            if (employee == null)
                                continue;

                            AnnualLeaveBalance annualLeaveBalance = annualLeaveBalances.FirstOrDefault(b => b.EmployeeId == employee.EmployeeId);

                            int minutesPerDay = AnnualLeaveManager.GetAnnualLeaveShiftLengthForEmployee(earliestDate.Value, employee.EmployeeId, reportResult.ActorCompanyId);


                            var annualLeaveItem = CreateItem(employee, annualLeaveTransaction, annualLeaveBalance, minutesPerDay);

                            _reportDataOutput.AnnualLeaveTransactionItems.Add(annualLeaveItem);

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

        private AnnualLeaveTransactionItem CreateItem(Employee employee, AnnualLeaveTransaction annualLeaveTransaction, AnnualLeaveBalance annualLeaveBalance, int minutesPerDay)
        {
            return new AnnualLeaveTransactionItem
            {
                EmployeeNr = employee.EmployeeNr,
                EmployeeName = employee.Name,
                DateEarned = annualLeaveTransaction.DateEarned,
                YearEarned = annualLeaveTransaction.DateEarned,
                DateSpent = annualLeaveTransaction.DateSpent,
                Hours = annualLeaveTransaction.AccumulatedMinutes,
                EarnedHours = annualLeaveTransaction.MinutesEarned,
                EarnedDays = minutesPerDay > 0 ? (decimal)(annualLeaveTransaction.MinutesEarned / (minutesPerDay)) : 0,
                SpentHours = annualLeaveTransaction.MinutesSpent,
                SpentDays = minutesPerDay > 0 ? (decimal)(annualLeaveTransaction.MinutesSpent / (minutesPerDay)) : 0,
                BalanceHours = (decimal)annualLeaveBalance?.AnnualLeaveBalanceMinutes,
                BalanceDays = annualLeaveBalance?.AnnualLeaveBalanceDays ?? 0,
                TypeName = GetValueFromDict(annualLeaveTransaction.Type, _transactionTypeDictionary), 
            };

        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class AnnualLeaveTransactionReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_AnnualLeaveTransactionMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public AnnualLeaveTransactionReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_AnnualLeaveTransactionMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_AnnualLeaveTransactionMatrixColumns.Unknown;
        }
    }

    public class AnnualLeaveTransactionReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AnnualLeaveTransactionReportDataField> Columns { get; set; }

        public AnnualLeaveTransactionReportDataInput(CreateReportResult reportResult, List<AnnualLeaveTransactionReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class AnnualLeaveTransactionReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public AnnualLeaveTransactionReportDataInput Input { get; set; }
        public List<AnnualLeaveTransactionItem> AnnualLeaveTransactionItems { get; set; }

        public AnnualLeaveTransactionReportDataOutput(AnnualLeaveTransactionReportDataInput input)
        {
            this.Input = input;
            this.AnnualLeaveTransactionItems = new List<AnnualLeaveTransactionItem>();
        }
    }

}

