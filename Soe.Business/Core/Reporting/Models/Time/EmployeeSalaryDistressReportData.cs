
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class EmployeeSalaryDistressReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeSalaryDistressReportDataInput _reportDataInput;
        private readonly EmployeeSalaryDistressReportDataOutput _reportDataOutput;

        private bool LoadAbscence
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("absence"));
            }
        }

        public EmployeeSalaryDistressReportData(ParameterObject parameterObject, EmployeeSalaryDistressReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new EmployeeSalaryDistressReportDataOutput(reportDataInput);
            _reportDataInput = reportDataInput;
        }

        public static List<EmployeeSalaryDistressReportDataField> GetPossibleDataFields()
        {
            List<EmployeeSalaryDistressReportDataField> possibleFields = new List<EmployeeSalaryDistressReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeSalaryDistressMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeSalaryDistressReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeSalaryDistressReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

           bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);

            if (selectionEmployeeIds.Count == 0)
                employees = EmployeeManager.GetAllEmployees(reportResult.Input.ActorCompanyId, active: selectionActiveEmployees);
            else
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds);

            #endregion

            #region Terms and dictionaries

            int langId = GetLangId();
            Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);
            Dictionary<int, string> amountType = base.GetTermGroupDict(TermGroup.EmployeeTaxSalaryDistressAmountType, langId);
            Dictionary<int, List<EmployeeTaxSE>> taxDict = new Dictionary<int, List<EmployeeTaxSE>>();
            List<int> validEmployess = new List<int>();
            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (employees != null)
                {
                    #region Permissions

                    bool employmentPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollSalaryPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

                    #endregion

                    #region Content
                    foreach (Employee employee in employees) {
                        int year = selectionDateFrom.Year;
                        bool valid = false;
                        while (year <= selectionDateTo.AddDays(1).Year)
                        {
                            EmployeeTaxSE empTax = EmployeeManager.GetEmployeeTaxSE(entities, employee.EmployeeId, year);
                            if (empTax != null && empTax.SalaryDistressAmountType != (int)TermGroup_EmployeeTaxSalaryDistressAmountType.NotSelected)
                            {
                                if (!taxDict.ContainsKey(employee.EmployeeId))
                                    taxDict[employee.EmployeeId] = new List<EmployeeTaxSE>();

                                taxDict[employee.EmployeeId].Add(empTax);
                                valid = true;
                            }
                            year++;
                        }
                        if(valid)
                            validEmployess.Add(employee.EmployeeId);
                    }
                    List<TimePayrollStatisticsSmallDTO> abcenses = new List<TimePayrollStatisticsSmallDTO>();
                    List <TimePayrollTransaction> transactions = TimeTransactionManager.GetTimePayrollTransactionsForEmployees(entities, validEmployess, selectionDateFrom, selectionDateTo, sysPayrollTypeLevel2: TermGroup_SysPayrollType.SE_Deduction_SalaryDistress, includeTimeBlockDate: true);
                    List<TimePeriod> timePeriods = TimePeriodManager.GetDefaultTimePeriods(TermGroup_TimePeriodType.Payroll, false, null, null, reportResult.Input.ActorCompanyId).Where(w => w.StopDate >= selectionDateFrom && w.StartDate <= selectionDateTo).ToList();
                    if(LoadAbscence)
                        abcenses = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, Company.ActorCompanyId, employees.Where(w=> validEmployess.Contains(w.EmployeeId)).ToList(), timePeriods.Select(s=> s.TimePeriodId).ToList(), setPensionCompany: true, ignoreAccounting: true);

                   
                    foreach (int employeeId in validEmployess)
                    {
                        #region Prereq
                        Employee employee = employees.FirstOrDefault(w => w.EmployeeId == employeeId);
 
                        if (employee == null ||  !employmentPermission || !payrollPermission || !payrollSalaryPermission)
                            continue;

                        #endregion
                        foreach (var transactionsByProduct in transactions.GroupBy(w => w.ProductId))
                        {
                            PayrollProductDTO product = base.GetPayrollProductFromCache(entities, CacheConfig.Company(reportResult.Input.ActorCompanyId), transactionsByProduct.Key);

                            foreach (TimePayrollTransaction transaction in transactionsByProduct.Where(w => w.EmployeeId == employeeId))
                            {
                                StringBuilder abcense = new StringBuilder();
                                var timePeriod = timePeriods.FirstOrDefault(w => w.TimePeriodId == transaction.TimePeriodId);
                                if (timePeriod?.PaymentDate == null)
                                    continue;

                                var absenceTransactionsOnEmployee = abcenses.Where(w => w.EmployeeId == employee.EmployeeId && w.Date >= timePeriod.StartDate && w.Date <= timePeriod.StopDate).ToList();

                                if (absenceTransactionsOnEmployee.Any())
                                {
                                    if (absenceTransactionsOnEmployee.Any(a => PayrollRulesUtil.IsAbsenceSick(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2, a.SysPayrollTypeLevel3, a.SysPayrollTypeLevel4)))
                                    {
                                        abcense.Append("Sjuk");
                                    }

                                    if (absenceTransactionsOnEmployee.Any(a => PayrollRulesUtil.IsLeaveOfAbsence(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2, a.SysPayrollTypeLevel3, a.SysPayrollTypeLevel4)))
                                    {
                                        abcense.Append(abcense.Length > 0 ? ", " : "");
                                        abcense.Append("Tjänsteledig");
                                    }

                                    if (absenceTransactionsOnEmployee.Any(a => PayrollRulesUtil.IsParentalLeave(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2, a.SysPayrollTypeLevel3, a.SysPayrollTypeLevel4)))
                                    {
                                        abcense.Append(abcense.Length > 0 ? ", " : "");
                                        abcense.Append("Föräldraledig");
                                    }

                                    if (abcense.Length == 0)
                                    {
                                        abcense.Append("Annan ledighet");
                                    }
                                }

                                EmployeeTaxSE employeeTax = taxDict[employeeId].FirstOrDefault(w => w.Year == timePeriod.PaymentDate.Value.Year);
                                EmployeeSalaryDistressItem employeeItem = new EmployeeSalaryDistressItem
                                {
                                    EmployeeNr = employee.EmployeeNr,
                                    EmployeeName = employee.Name,
                                    FirstName = employee.FirstName,
                                    LastName = employee.LastName,
                                    Gender = GetValueFromDict((int)employee.Sex, sexDict),
                                    SSN = showSocialSec ? employee.SocialSec : string.Empty,
                                    Date = transaction.Date,
                                    PaymentDate = timePeriods.FirstOrDefault(w => w.TimePeriodId == transaction.TimePeriodId)?.PaymentDate ?? null,
                                    PayrollProductNumber = product.Number,
                                    PayrollProductName = product.Name,
                                    UnitPrice = transaction.UnitPrice.Value,
                                    Quantity = transaction.Quantity,
                                    Amount = transaction.Amount.Value,
                                    ManualAdded = transaction.IsAdded,
                                    ReservedAmounts = employeeTax?.SalaryDistressReservedAmount ?? decimal.Zero,
                                    CaseNumber = employeeTax?.SalaryDistressCase ?? string.Empty,
                                    SeizureAmountType = employeeTax != null ? GetValueFromDict(employeeTax.SalaryDistressAmountType, amountType) : string.Empty,
                                    SalaryDistressAmount = employeeTax?.SalaryDistressAmount ?? decimal.Zero,
                                    Absence = abcense.ToString(),
                                };

                                _reportDataOutput.Employees.Add(employeeItem);
                            }
                        }

                    }

                    #endregion
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
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

    public class EmployeeSalaryDistressReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeSalaryDistressMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeSalaryDistressReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeSalaryDistressMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeSalaryDistressMatrixColumns.Unknown;
        }
    }

    public class EmployeeSalaryDistressReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeSalaryDistressReportDataField> Columns { get; set; }

        public EmployeeSalaryDistressReportDataInput(CreateReportResult reportResult, List<EmployeeSalaryDistressReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;

        }
    }

    public class EmployeeSalaryDistressReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeSalaryDistressItem> Employees { get; set; }
        public EmployeeSalaryDistressReportDataInput Input { get; set; }

        public EmployeeSalaryDistressReportDataOutput(EmployeeSalaryDistressReportDataInput input)
        {
            this.Employees = new List<EmployeeSalaryDistressItem>();
            this.Input = input;
        }
    }
}