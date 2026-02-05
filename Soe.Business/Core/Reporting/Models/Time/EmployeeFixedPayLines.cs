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
    public class EmployeeFixedPayLinesReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeFixedPayLinesReportDataInput _reportDataInput;
        private readonly EmployeeFixedPayLinesReportDataOutput _reportDataOutput;

        public EmployeeFixedPayLinesReportData(ParameterObject parameterObject, EmployeeFixedPayLinesReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeFixedPayLinesReportDataOutput(reportDataInput);
        }

        bool LoadEmployments
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeFixedPayLinesMatrixColumns.EmploymentStartDate ||
                       a.Column == TermGroup_EmployeeFixedPayLinesMatrixColumns.EmploymentTypeName);
            }
        }
        bool LoadPositions
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                    a.Column == TermGroup_EmployeeFixedPayLinesMatrixColumns.SSYKCode);
            }
        }

        public static List<EmployeeFixedPayLinesReportDataField> GetPossibleDataFields()
        {
            List<EmployeeFixedPayLinesReportDataField> possibleFields = new List<EmployeeFixedPayLinesReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeFixedPayLinesMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeFixedPayLinesReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeFixedPayLinesReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Permissions

                    bool employmentPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollSalaryPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool userPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    
                    #endregion

                    #region Terms and dictionaries

                    int langId = GetLangId();
                    Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);
                   
                    #endregion

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: LoadEmployments && employmentPermission, loadUser: userPermission, loadEmploymentVacactionGroup: LoadEmployments && employmentPermission);

                    #region Content
                    Dictionary<int, List<EmployeeFixedPayrollRowChangesDTO>> payrollAllRows = EmployeeManager.GetEmployeeFixedPayrollRowsChanges(base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                   

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        if (userPermission && !employee.UserReference.IsLoaded)
                            employee.UserReference.Load();

                        EmployeePosition defaultEmployeePosition = null;
                        if (LoadPositions)
                        {
                            List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true);
                            defaultEmployeePosition = employeePositions.FirstOrDefault(f => f.Default);
                        }
                        List<EmployeeFixedPayrollRowChangesDTO> payrollRows = payrollAllRows.ContainsKey(employee.EmployeeId) ? payrollAllRows[employee.EmployeeId] : new List<EmployeeFixedPayrollRowChangesDTO>();

                        #endregion

                        foreach (var payrollRow in payrollRows)
                        {
                            #region Item

                            EmployeeFixedPayLinesItem employeeItem = new EmployeeFixedPayLinesItem();

                            employeeItem.EmployeeNr = employee.EmployeeNr;
                            employeeItem.EmployeeName = employee.Name;
                            employeeItem.FirstName = employee.FirstName;
                            employeeItem.LastName = employee.LastName;
                            employeeItem.BirthYear = CalendarUtility.GetBirthYearFromSecurityNumber(employee.SocialSec);
                            employeeItem.Gender = GetValueFromDict((int)employee.Sex, sexDict);

                            if (userPermission)
                            {

                                employeeItem.Position = defaultEmployeePosition?.Position?.Name ?? string.Empty;
                                employeeItem.SSYKCode = defaultEmployeePosition?.Position?.SysPositionCode ?? string.Empty;
                            }

                            if (LoadEmployments && employmentPermission)
                            {
                                if (payrollRow.FromPayrollGroup)
                                {
                                    employeeItem.EmploymentStartDate = payrollRow.EmploymentStartDate;
                                    employeeItem.EmploymentTypeName = payrollRow.EmploymentTypeName;
                                }
                                else
                                {
                                    employeeItem.EmploymentStartDate = null;
                                    employeeItem.EmploymentTypeName = "";
                                }

                            }
                            if (payrollPermission && payrollSalaryPermission)
                            {
                                employeeItem.ProductNr = payrollRow.ProductNr;
                                employeeItem.ProuctName = payrollRow.ProductName;
                                employeeItem.FromDate = payrollRow.FromDate;
                                employeeItem.ToDate = payrollRow.ToDate;
                                employeeItem.Quantity = payrollRow.Quantity;
                                employeeItem.IsSpecifiedUnitPrice = payrollRow.IsSpecifiedUnitPrice;
                                employeeItem.Distribute = payrollRow.Distribute;
                                employeeItem.UnitPrice = payrollRow.UnitPrice;
                                employeeItem.VatAmount = payrollRow.VatAmount;
                                employeeItem.Amount = payrollRow.Amount;
                                employeeItem.FromPayrollGroup = payrollRow.FromPayrollGroup;
                                if (payrollRow.FromPayrollGroup)
                                    employeeItem.Payrollgroup = payrollRow.PayrollGroupName;
                                else
                                    employeeItem.Payrollgroup = "";

                            }

                            _reportDataOutput.Employees.Add(employeeItem);
                        }

                        #endregion
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

    public class EmployeeFixedPayLinesReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeFixedPayLinesMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeFixedPayLinesReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeFixedPayLinesMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeFixedPayLinesMatrixColumns.Unknown;
        }
    }

    public class EmployeeFixedPayLinesReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeFixedPayLinesReportDataField> Columns { get; set; }

        public EmployeeFixedPayLinesReportDataInput(CreateReportResult reportResult, List<EmployeeFixedPayLinesReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeFixedPayLinesReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeFixedPayLinesItem> Employees { get; set; }
        public EmployeeFixedPayLinesReportDataInput Input { get; set; }
        public List<GenericType> EndReason { get; set; }

        public EmployeeFixedPayLinesReportDataOutput(EmployeeFixedPayLinesReportDataInput input)
        {
            this.Employees = new List<EmployeeFixedPayLinesItem>();
            this.Input = input;
            this.EndReason = new List<GenericType>();
        }
    }
}
