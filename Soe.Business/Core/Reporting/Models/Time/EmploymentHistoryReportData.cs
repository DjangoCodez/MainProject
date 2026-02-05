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
    public class EmploymentHistoryReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmploymentHistoryReportDataInput _reportDataInput;
        private readonly EmploymentHistoryReportDataOutput _reportDataOutput;

        public EmploymentHistoryReportData(ParameterObject parameterObject, EmploymentHistoryReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmploymentHistoryReportDataOutput(reportDataInput);
        }

        public EmploymentHistoryReportDataOutput CreateOutput(CreateReportResult reportResult)
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
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);
            
            List<int> employmentTypes = new List<int>();
            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out bool selectionOnlyInactive, out bool? selectionActiveEmployees);
            TryGetIdsFromSelection(reportResult, out employmentTypes, "employmentTypes");

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Build XML

            using (CompEntities entities = new CompEntities())
            {

                if (selectionEmployeeIds.Any())
                {
                    #region Prereq

                    int langId = GetLangId();
                    Dictionary<int, string> endReasonsDict = EmployeeManager.GetSystemEndReasons(entities, reportResult.ActorCompanyId, includeCompanyEndReasons: true);
                    List<EmploymentTypeDTO> employmentTypeDTOs = EmployeeManager.GetEmploymentTypes(entities, reportResult.ActorCompanyId);

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: true);

                    #endregion

                    #region Content

                    DateTime selectionDate = selectionDateFrom.Date == selectionDateTo.Date ? selectionDateFrom.Date : selectionDateTo.Date;

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        List<Employment> employments = employee.GetActiveEmploymentsDesc();
                        if (employments.IsNullOrEmpty())
                            continue;

                        Employment firstEmployment = employments.GetFirstEmployment();
                        Employment lastEmployment = employments.GetLastEmployment();
                        Employment currentEmployment = employments.GetEmployment(selectionDate) ?? firstEmployment.GetNearestEmployment(lastEmployment, selectionDate);
                        if (currentEmployment == null)
                            continue;

                        DateTime currentDate = currentEmployment.GetValidEmploymentDate(selectionDate);

                        #endregion

                        if (employmentTypes.Count > 0)
                        {
                            if (!selectionIncludeInactive && !selectionOnlyInactive)
                                employments = employments.Where(w => employmentTypes.Contains(w.OriginalType) && (w.DateTo == null || w.DateTo >= selectionDate)).ToList();
                            else if (!selectionIncludeInactive && selectionOnlyInactive)
                                employments = employments.Where(w => employmentTypes.Contains(w.OriginalType) && (w.DateTo != null && w.DateTo < selectionDate)).ToList();
                            else
                                employments = employments.Where(w => employmentTypes.Contains(w.OriginalType)).ToList();
                        }
                        else
                        {
                            if (!selectionIncludeInactive && !selectionOnlyInactive)
                                employments = employments.Where(w => (w.DateTo == null || w.DateTo >= selectionDate)).ToList();
                            else if (!selectionIncludeInactive && selectionOnlyInactive)
                                employments = employments.Where(w => (w.DateTo != null && w.DateTo < selectionDate)).ToList();
                        }

                        var isLASDaysColumnIncluded = _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmploymentHistoryMatrixColumns.LASDays);

                        #region Initiate list of EmploymentTypesDays
                        Dictionary<int, int> employmentTypesDays = new Dictionary<int, int>();
                        employmentTypeDTOs.ForEach(e =>
                        {
                            employmentTypesDays.Add(e.EmploymentTypeId ?? e.Type, 0);
                        });
                        #endregion

                        foreach (var employmentHistory in employments)
                        {
                            DateTime? date = CalendarUtility.GetDateTimeInInterval(currentDate, employmentHistory.DateFrom, employmentHistory.DateTo);
                            DateTime startDate = employmentHistory.DateFrom.ToValueOrDefault();
                            DateTime stopDate = employmentHistory.DateTo.HasValue ? employmentHistory.DateTo.Value : selectionDateTo;
                            var employmentType = employmentHistory.GetEmploymentTypeName(employmentTypeDTOs);
                            var employmentTypeId = employmentHistory.GetEmploymentType(employmentTypeDTOs);
                            var empTypeDTO = new EmploymentTypeDTO();
                            empTypeDTO.Type = employmentTypeId;

                            var empTypeList = new List<EmploymentTypeDTO>();
                            empTypeList.Add(empTypeDTO);

                            EmploymentHistoryItem empHistoryItem = new EmploymentHistoryItem();

                            empHistoryItem.EmploymentNumber = employmentHistory.Employee.EmployeeNr;
                            empHistoryItem.FirstName = employmentHistory.Employee.FirstName;
                            empHistoryItem.LastName = employmentHistory.Employee.LastName;
                            empHistoryItem.WorkingPlace = employmentHistory.GetWorkPlace(date);
                            empHistoryItem.EmploymentStartDate = employmentHistory.DateFrom.ToValueOrDefault();
                            empHistoryItem.EmploymentEndDate = employmentHistory.DateTo;
                            empHistoryItem.ReasonForEndingEmployment = endReasonsDict[employmentHistory.GetEndReason()];
                            empHistoryItem.EmploymentType = employmentType;
                            empHistoryItem.TotalEmploymentDays = employmentHistory.GetEmploymentDays(stopDate: currentDate);
                            empHistoryItem.EmploymentPercentage = employmentHistory.GetPercent(date);
                            if (isLASDaysColumnIncluded)
                            {
                                if (employmentTypeId != (int)TermGroup_EmploymentType.Unknown)
                                    empHistoryItem.LASDays = EmployeeManager.GetLasDays(entities, reportResult.ActorCompanyId, employee, startDate, stopDate);
                            }
                            _reportDataOutput.EmploymentHistoryItems.Add(empHistoryItem);
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }
    }

    public class EmploymentHistoryReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmploymentHistoryMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmploymentHistoryReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmploymentHistoryMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmploymentHistoryMatrixColumns.Unknown;
        }
    }

    public class EmploymentHistoryReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmploymentHistoryReportDataField> Columns { get; set; }

        public EmploymentHistoryReportDataInput(CreateReportResult reportresult, List<EmploymentHistoryReportDataField> columns)
        {
            this.ReportResult = reportresult;
            this.Columns = columns;
        }
    }

    public class EmploymentHistoryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmploymentHistoryItem> EmploymentHistoryItems { get; set; }
        public EmploymentHistoryReportDataInput Input { get; set; }

        public EmploymentHistoryReportDataOutput(EmploymentHistoryReportDataInput input)
        {
            this.EmploymentHistoryItems = new List<EmploymentHistoryItem>();
            this.Input = input;
        }
    }
}
