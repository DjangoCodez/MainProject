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
    public class EmployeeSalaryUnionFeesReportData : TimeReportDataManager, IReportDataModel
    {
        readonly EmployeeSalaryUnionFeesReportDataOutput _reportDataOutput;

        public EmployeeSalaryUnionFeesReportData(ParameterObject parameterObject, EmployeeSalaryUnionFeesReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new EmployeeSalaryUnionFeesReportDataOutput(reportDataInput);
        }

        public static List<EmployeeSalaryUnionFeesReportDataField> GetPossibleDataFields()
        {
            List<EmployeeSalaryUnionFeesReportDataField> possibleFields = new List<EmployeeSalaryUnionFeesReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeSalaryUnionFeesMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeSalaryUnionFeesReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeSalaryUnionFeesReportDataOutput CreateOutput(CreateReportResult reportResult)
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

                    List<TimePayrollTransaction> transactions = TimeTransactionManager.GetTimePayrollTransactionsForEmployees(entities, employees.Select(s=> s.EmployeeId).ToList(), selectionDateFrom, selectionDateTo, sysPayrollTypeLevel2: TermGroup_SysPayrollType.SE_Deduction_UnionFee, includeTimeBlockDate: true);
                    List<TimePeriod> timePeriods = TimePeriodManager.GetDefaultTimePeriods(TermGroup_TimePeriodType.Payroll, false, null, null, reportResult.Input.ActorCompanyId).Where(w => w.StopDate >= selectionDateFrom && w.StartDate <= selectionDateTo).ToList();
                    List<UnionFeeDTO> unions = (List<UnionFeeDTO>)PayrollManager.GetUnionFees(reportResult.Input.ActorCompanyId, false, true).ToDTOs();
                    List<PayrollPriceType> priceTypes = base.GetPayrollPriceTypesFromCache(entities, CacheConfig.Company(reportResult.Input.ActorCompanyId));
                    List<EmployeeSalaryUnionFeesItem> transactionList = new List<EmployeeSalaryUnionFeesItem>();

                    foreach (Employee employee in employees)
                    {
                        #region Prereq
                        int employeeId = employee.EmployeeId;
                        if (employee == null || !employmentPermission || !payrollPermission || !payrollSalaryPermission)
                            continue;

                        #endregion
                        foreach (var transactionsByProduct in transactions.GroupBy(w => w.ProductId))
                        {
                            PayrollProductDTO product = base.GetPayrollProductFromCache(entities, CacheConfig.Company(reportResult.Input.ActorCompanyId), transactionsByProduct.Key);

                            foreach (TimePayrollTransaction transaction in transactionsByProduct.Where(w => w.EmployeeId == employeeId))
                            {
                                UnionFeeDTO union = unions.FirstOrDefault(f => f.UnionFeeId == transaction.UnionFeeId);

                                EmployeeSalaryUnionFeesItem employeeItem = new EmployeeSalaryUnionFeesItem
                                {
                                    TransactionId = transaction.TimePayrollTransactionId,
                                    EmployeeId = employee.EmployeeId,
                                    EmployeeNr = employee.EmployeeNr,
                                    EmployeeName = employee.Name,
                                    FirstName = employee.FirstName,
                                    LastName = employee.LastName,
                                    SSN = showSocialSec ? employee.SocialSec : string.Empty,

                                    PaymentDate = timePeriods.FirstOrDefault(w => w.TimePeriodId == transaction.TimePeriodId)?.PaymentDate ?? null,
                                    PayrollProductNumber = product.Number,
                                    PayrollProductName = product.Name,
                                    UnitPrice = transaction.UnitPrice.Value,
                                    Quantity = transaction.Quantity,
                                    Amount = transaction.Amount.Value,

                                    UnionName = string.Empty,
                                    PayrollPriceTypeIdPercentName = string.Empty,
                                    PayrollPriceTypeIdPercentCeilingName = string.Empty,
                                    PayrollPriceTypeIdFixedAmountName = string.Empty,

                                    CentRounding = transaction.IsCentRounding,
                                    ManualAdded = transaction.ManuallyAdded,
                                    UnionFeeId = transaction.UnionFeeId,
                                };

                                if (union != null)
                                {
                                    employeeItem.UnionName = union.Name.HasValue() ? union.Name : string.Empty;
                                    employeeItem.PayrollPriceTypeIdPercentName = (union.PayrollPriceTypeIdPercent.HasValue ? priceTypes.FirstOrDefault(w => w.PayrollPriceTypeId == union.PayrollPriceTypeIdPercent.Value)?.Name : null) ?? string.Empty;
                                    employeeItem.PayrollPriceTypeIdPercentCeilingName = (union.PayrollPriceTypeIdPercentCeiling.HasValue ? priceTypes.FirstOrDefault(w => w.PayrollPriceTypeId == union.PayrollPriceTypeIdPercentCeiling.Value)?.Name : null) ?? string.Empty;
                                    employeeItem.PayrollPriceTypeIdFixedAmountName = (union.PayrollPriceTypeIdFixedAmount.HasValue ? priceTypes.FirstOrDefault(w => w.PayrollPriceTypeId == union.PayrollPriceTypeIdFixedAmount.Value)?.Name : null) ?? string.Empty;
                                }
                                transactionList.Add(employeeItem);
                            }
                        }
                    }
                   
                    if (!transactionList.IsNullOrEmpty())
                    {
                        foreach(EmployeeSalaryUnionFeesItem row in transactionList.OrderBy(w => w.PaymentDate).ThenBy(w=> w.UnionFeeId).ToList())
                        {
                            if (row.CentRounding || (row.ManualAdded && row.UnionFeeId.IsNullOrEmpty()))
                            {
                                if (_reportDataOutput.Employees.Any(w => w.EmployeeId == row.EmployeeId && w.PaymentDate == row.PaymentDate && w.TransactionId != row.TransactionId))
                                {
                                    var employeeItem =
                                        _reportDataOutput.Employees.FirstOrDefault(w => w.EmployeeId == row.EmployeeId && w.PaymentDate == row.PaymentDate && w.TransactionId != row.TransactionId && w.UnionFeeId.HasValue) ??
                                        transactionList.FirstOrDefault(w => w.EmployeeId == row.EmployeeId && w.PaymentDate == row.PaymentDate && w.TransactionId != row.TransactionId && w.UnionFeeId.HasValue);
                                    
                                    if (employeeItem != null)
                                    {
                                        employeeItem.Amount += row.Amount;
                                        employeeItem.UnitPrice += row.UnitPrice;
                                    }    
                                }
                                else
                                {
                                    _reportDataOutput.Employees.Add(row);
                                }                                    
                            }
                            else
                            {
                                _reportDataOutput.Employees.Add(row);
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
    }

    public class EmployeeSalaryUnionFeesReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeSalaryUnionFeesMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeSalaryUnionFeesReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeSalaryUnionFeesMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Unknown;
        }
    }

    public class EmployeeSalaryUnionFeesReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeSalaryUnionFeesReportDataField> Columns { get; set; }

        public EmployeeSalaryUnionFeesReportDataInput(CreateReportResult reportResult, List<EmployeeSalaryUnionFeesReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeSalaryUnionFeesReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeSalaryUnionFeesItem> Employees { get; set; }
        public EmployeeSalaryUnionFeesReportDataInput Input { get; set; }

        public EmployeeSalaryUnionFeesReportDataOutput(EmployeeSalaryUnionFeesReportDataInput input)
        {
            this.Employees = new List<EmployeeSalaryUnionFeesItem>();
            this.Input = input;
        }
    }
}