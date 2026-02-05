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
    public class EmploymentDaysReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmploymentDaysReportDataInput _reportDataInput;
        private readonly EmploymentDaysReportDataOutput _reportDataOutput;

        private bool loadTotalEmploymentDays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmploymentDaysMatrixColumns.TotalEmploymentDays);
        private bool loadTotalAVADays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeAvaDays);
        private bool loadTotalSVADays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeSvaDays);
        private bool loadTotalVikDays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeVikDays);
        private bool loadTotalOtherDays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeOtherDays);

        public EmploymentDaysReportData(ParameterObject parameterObject, EmploymentDaysReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmploymentDaysReportDataOutput(reportDataInput);
        }

        public EmploymentDaysReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion


            #region Build Data

            using (CompEntities entities = new CompEntities())
            {

                if (selectionEmployeeIds.Any())
                {
                    #region Prereq

                    List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, reportResult.ActorCompanyId);

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: true);

                    List<EmployeeFactor> employeeFactors = null;

                    if (loadTotalAVADays || loadTotalSVADays || loadTotalVikDays || loadTotalOtherDays)
                        employeeFactors = base.GetEmployeeFactorsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));

                    #endregion

                    #region Content

                    DateTime selectionDate = selectionDateTo.Date;

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        List<Employment> employments = employee.GetActiveEmploymentsDesc();
                        if (employments.IsNullOrEmpty())
                            continue;

                        List<EmployeeFactor> employeeFactorsOnEmployee = employeeFactors?.Where(e => e.EmployeeId == employeeId).ToList();

                        #region Initiate list of EmploymentTypesDays
                        Dictionary<int, int> employmentTypesDays = new Dictionary<int, int>();
                        employmentTypes.ForEach(e =>
                        {
                            employmentTypesDays.Add(e.EmploymentTypeId ?? e.Type, 0);
                        });
                        #endregion

                        #region Load LAS information
                        List<EmploymentLASDTO> employeeLASInformation = EmployeeManager.GetLasInformation(entities, reportResult.ActorCompanyId, new List<Employee>() { employee }, CalendarUtility.DATETIME_DEFAULT, selectionDate);
                        #endregion

                        #region Count up days for each employment type
                        employeeLASInformation.ForEach(e =>
                        {
                            if (employmentTypesDays.ContainsKey(e.EmploymentTypeId))
                            {
                                int key = e.EmploymentTypeId != 0 ? e.EmploymentTypeId : (int)e.EmploymentType;
                                employmentTypesDays[key] += e.NumberOfLASDays;
                            }
                        });
                        #endregion

                        int avaDays = 0;
                        int svaDays = 0;
                        int vikDays = 0;
                        int allDays = 0;
                        if ((loadTotalEmploymentDays || loadTotalAVADays))
                        {
                            if (employeeFactorsOnEmployee != null && employeeFactorsOnEmployee.Any(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDaysAva))
                            {
                                var employeeFactor = EmployeeManager.GetEmployeeFactor(employeeFactorsOnEmployee.Where(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDaysAva).ToList(), selectionDate);

                                if (employeeFactor != null)
                                    avaDays = Convert.ToInt32(employeeFactor.Factor);

                                if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Ava))
                                    avaDays += employeeLASInformation.Where(w => w.StartDate >= employeeFactor.FromDate).Where(e => e.EmploymentLASType == EmploymentLASType.Ava).Sum(e => e.NumberOfLASDays);
                            }
                            else if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Ava))
                                avaDays = employeeLASInformation.Where(e => e.EmploymentLASType == EmploymentLASType.Ava).Sum(e => e.NumberOfLASDays);
                        }
                        if (loadTotalEmploymentDays || loadTotalSVADays)
                        {
                            if (employeeFactorsOnEmployee != null && employeeFactorsOnEmployee.Any(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDaysSva))
                            {
                                var employeeFactor = EmployeeManager.GetEmployeeFactor(employeeFactorsOnEmployee.Where(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDaysSva).ToList(), selectionDate);

                                if (employeeFactor != null)
                                    svaDays = Convert.ToInt32(employeeFactor.Factor);

                                if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Sva))
                                    svaDays += employeeLASInformation.Where(w => w.StartDate >= employeeFactor.FromDate).Where(e => e.EmploymentLASType == EmploymentLASType.Sva).Sum(e => e.NumberOfLASDays);
                            }
                            else if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Sva))
                                svaDays = employeeLASInformation.Where(e => e.EmploymentLASType == EmploymentLASType.Sva).Sum(e => e.NumberOfLASDays);
                        }
                        if (loadTotalEmploymentDays || loadTotalVikDays)
                        {
                            if (employeeFactorsOnEmployee != null && employeeFactorsOnEmployee.Any(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDaysVik))
                            {
                                var employeeFactor = EmployeeManager.GetEmployeeFactor(employeeFactorsOnEmployee.Where(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDaysVik).ToList(), selectionDate);

                                if (employeeFactor != null)
                                    vikDays = Convert.ToInt32(employeeFactor.Factor);

                                if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Vik))
                                    vikDays += employeeLASInformation.Where(w => w.StartDate >= employeeFactor.FromDate).Where(e => e.EmploymentLASType == EmploymentLASType.Vik).Sum(e => e.NumberOfLASDays);
                            }
                            else if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Vik))
                                vikDays = employeeLASInformation.Where(e => e.EmploymentLASType == EmploymentLASType.Vik).Sum(e => e.NumberOfLASDays);
                        }
                        if ((loadTotalEmploymentDays || loadTotalOtherDays))
                        {
                            if (employeeFactorsOnEmployee != null && employeeFactorsOnEmployee.Any(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDays))
                            {
                                var employeeFactor = EmployeeManager.GetEmployeeFactor(employeeFactorsOnEmployee.Where(w => w.Type == (int)TermGroup_EmployeeFactorType.BalanceLasDays).ToList(), selectionDate);

                                if (employeeFactor != null)
                                    allDays = Convert.ToInt32(employeeFactor.Factor);

                                if (employeeLASInformation.Any(e => e.EmploymentLASType == EmploymentLASType.Unknown))
                                    allDays += employeeLASInformation.Where(w => w.StartDate >= employeeFactor.FromDate).Sum(e => e.NumberOfLASDays);
                            }
                            else if (employeeFactorsOnEmployee != null && employeeFactorsOnEmployee.Any(w => w.Type == (int)TermGroup_EmployeeFactorType.CurrentLasDays))
                            {
                                var employeeFactor = EmployeeManager.GetEmployeeFactor(employeeFactorsOnEmployee.Where(w => w.Type == (int)TermGroup_EmployeeFactorType.CurrentLasDays).ToList(), selectionDate);

                                if (employeeFactor != null)
                                    allDays = Convert.ToInt32(employeeFactor.Factor);
                            }
                            else if (employeeLASInformation.Any())
                                allDays = employeeLASInformation.Sum(e => e.NumberOfLASDays);
                        }

                        Employment firstEmployment = employments.GetFirstEmployment();
                        Employment lastEmployment = employments.GetLastEmployment();
                        Employment currentEmployment = employments.GetEmployment(selectionDate) ?? firstEmployment.GetNearestEmployment(lastEmployment, selectionDate);
                        if (currentEmployment == null)
                            continue;

                        DateTime currentDate = currentEmployment.GetValidEmploymentDate(selectionDate);

                        #endregion

                        #region Item
                        EmploymentDaysItem empDaysItem = new EmploymentDaysItem();

                        empDaysItem.IsFixedButNotSubstituteDays = employeeLASInformation.Where(e => e.IsFixedButNotSubstitute).Sum(e => e.NumberOfLASDays);
                        empDaysItem.EmploymentNumber = employee.EmployeeNr;
                        empDaysItem.Name = employee.Name;
                        empDaysItem.FirstName = employee.FirstName;
                        empDaysItem.LastName = employee.LastName;
                        empDaysItem.CurrentWorkingPlace = currentEmployment.GetWorkPlace(currentDate);
                        empDaysItem.EmploymentStartDate = firstEmployment.DateFrom.ToValueOrDefault();
                        empDaysItem.EmploymentEndDate = lastEmployment.DateTo;
                        empDaysItem.CurrentEmploymentType = currentEmployment.GetEmploymentTypeName(employmentTypes);
                        empDaysItem.CurrentTimeAgreement = currentEmployment.GetEmployeeGroup().Name;
                        empDaysItem.EmploymentTypesDays = employmentTypesDays;
                        empDaysItem.LASTypeAvaDays = avaDays;
                        empDaysItem.LASTypeSvaDays = svaDays;
                        empDaysItem.LASTypeVikDays = vikDays;
                        empDaysItem.LASTypeOtherDays = allDays - avaDays - svaDays - vikDays;
                        empDaysItem.TotalEmploymentDays = allDays;

                        _reportDataOutput.EmploymentDaysItems.Add(empDaysItem);
                        #endregion
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

    public class EmploymentDaysReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmploymentDaysMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmploymentDaysReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmploymentDaysMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmploymentDaysMatrixColumns.Unknown;
        }
    }

    public class EmploymentDaysReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmploymentDaysReportDataField> Columns { get; set; }

        public EmploymentDaysReportDataInput(CreateReportResult reportResult, List<EmploymentDaysReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmploymentDaysReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmploymentDaysItem> EmploymentDaysItems { get; set; }
        public EmploymentDaysReportDataInput Input { get; set; }

        public EmploymentDaysReportDataOutput(EmploymentDaysReportDataInput input)
        {
            this.EmploymentDaysItems = new List<EmploymentDaysItem>();
            this.Input = input;
        }
    }
}
