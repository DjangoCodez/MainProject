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
    public class EmployeeChildReportData : EconomyReportDataManager, IReportDataModel
    {
        private readonly EmployeeChildReportDataOutput _reportDataOutput;

        public EmployeeChildReportData(ParameterObject parameterObject, EmployeeChildReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new EmployeeChildReportDataOutput(reportDataInput);
        }

        public static List<EmployeeChildReportDataField> GetPossibleDataFields()
        {
            List<EmployeeChildReportDataField> possibleFields = new List<EmployeeChildReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeChildMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeChildReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeChildReportDataOutput CreateOutput(CreateReportResult reportResult)
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

                    bool employeeChildPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    
                    if(!employeeChildPermission)
                        return new ActionResult(false);

                    #endregion

                    #endregion

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees);

                    List<EmployeeChild> employeeChilds = base.GetEmployeeChildsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));

                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        if (employeeChilds.Any(w=> w.EmployeeId == employeeId))
                        {
                            var children = EmployeeManager.GetEmployeeChilds(entities, employeeId, reportResult.ActorCompanyId, includeUsedDays: true).ToDTOs();
                            if (!children.Any())
                                continue;

                            foreach (EmployeeChildDTO child in children)
                            {
                                var item = new EmployeeChildItem
                                {
                                    EmployeeNr = employee.EmployeeNr,
                                    EmployeeName = employee.Name,
                                    ChildFirstName = child.FirstName,
                                    ChildLastName = child.LastName,
                                    ChildDateOfBirth = child.BirthDate,
                                    ChildSingelCustody = child.SingleCustody,
                                    AmountOfDays = child.NbrOfDays,
                                    AmountOfDaysUsed = child.UsedDays,
                                    AmountOfDaysLeft = child.DaysLeft,
                                    Openingbalanceuseddays = child.OpeningBalanceUsedDays,
                                };
                                _reportDataOutput.EmployeeChildItems.Add(item);
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

    public class EmployeeChildReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeChildMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeChildReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeChildMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeChildMatrixColumns.Unknown;
        }
    }

    public class EmployeeChildReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeChildReportDataField> Columns { get; set; }

        public EmployeeChildReportDataInput(CreateReportResult reportResult, List<EmployeeChildReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeChildReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeeChildReportDataInput Input { get; set; }
        public List<EmployeeChildItem> EmployeeChildItems { get; set; }

        public EmployeeChildReportDataOutput(EmployeeChildReportDataInput input)
        {
            this.Input = input;
            this.EmployeeChildItems = new List<EmployeeChildItem>();
        }
    }

}


