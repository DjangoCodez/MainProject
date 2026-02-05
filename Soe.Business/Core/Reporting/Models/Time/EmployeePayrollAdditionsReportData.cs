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
    public class EmployeePayrollAdditionsReportData : EconomyReportDataManager, IReportDataModel
    {
        private readonly EmployeePayrollAdditionsReportDataOutput _reportDataOutput;

        public EmployeePayrollAdditionsReportData(ParameterObject parameterObject, EmployeePayrollAdditionsReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new EmployeePayrollAdditionsReportDataOutput(reportDataInput);
        }

        public static List<EmployeePayrollAdditionsReportDataField> GetPossibleDataFields()
        {
            List<EmployeePayrollAdditionsReportDataField> possibleFields = new List<EmployeePayrollAdditionsReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeePayrollAdditionsMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeePayrollAdditionsReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeePayrollAdditionsReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out _))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out bool onlyWithAdditions, "onlyEmployeesWithAdditions");

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Prereq

                    #region Permissions

                    bool EmployeePayrollAdditionsPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

                    if (!EmployeePayrollAdditionsPermission)
                        return new ActionResult(false);

                    #endregion

                    #endregion

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees);

                    var termDict = base.GetTermGroupDict(TermGroup.EmployeeSettingType, GetLangId(), includeKey: false);

                    List<EmployeeSetting> settings = EmployeeManager.GetEmployeeSettings(reportResult.ActorCompanyId, employees.Select(s => s.EmployeeId).ToList(), selectionDateFrom, selectionDateTo, TermGroup_EmployeeSettingType.Additions, null, null);
                    
                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        var employeeSettings = settings.Where(i => i.EmployeeId == employeeId).ToList();

                        if (onlyWithAdditions && !employeeSettings.Any())
                        {
                            continue;
                        }
                        else if (!employeeSettings.Any())
                        {
                            _reportDataOutput.EmployeePayrollAdditionsItems.Add(CreateEmployeePayrollAddition(employee));
                        }

                        foreach (var employeeSetting in employeeSettings)
                        {
                            if (employeeSetting == null)
                                continue;

                            _reportDataOutput.EmployeePayrollAdditionsItems.Add(CreateEmployeePayrollAddition(employee, termDict[employeeSetting.EmployeeSettingGroupType], employeeSetting.Name, employeeSetting.ValidFromDate, employeeSetting.ValidToDate));
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

        private EmployeePayrollAdditionsItem CreateEmployeePayrollAddition(Employee employee, string group = "", string type = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            return new EmployeePayrollAdditionsItem
            {
                EmployeeNr = employee.EmployeeNr,
                EmployeeName = employee.Name,
                Group = group,
                Type = type,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

    }

    public class EmployeePayrollAdditionsReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeePayrollAdditionsMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeePayrollAdditionsReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeePayrollAdditionsMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeePayrollAdditionsMatrixColumns.Unknown;
        }
    }

    public class EmployeePayrollAdditionsReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeePayrollAdditionsReportDataField> Columns { get; set; }

        public EmployeePayrollAdditionsReportDataInput(CreateReportResult reportResult, List<EmployeePayrollAdditionsReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeePayrollAdditionsReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeePayrollAdditionsReportDataInput Input { get; set; }
        public List<EmployeePayrollAdditionsItem> EmployeePayrollAdditionsItems { get; set; }

        public EmployeePayrollAdditionsReportDataOutput(EmployeePayrollAdditionsReportDataInput input)
        {
            this.Input = input;
            this.EmployeePayrollAdditionsItems = new List<EmployeePayrollAdditionsItem>();
        }
    }

}


