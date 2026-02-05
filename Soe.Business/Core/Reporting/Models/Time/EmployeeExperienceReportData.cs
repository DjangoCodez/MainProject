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
    public class EmployeeExperienceReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeExperienceReportDataInput _reportDataInput;
        private readonly EmployeeExperienceReportDataOutput _reportDataOutput;

        private bool LoadExperience
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.ExperienceIn ||
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.ExperienceType ||
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.ExperienceTot);
            }
        }
        private bool LoadPriceTypes
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.SalaryType ||
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.Salary ||
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.SalaryTypeName ||
                       a.Column == TermGroup_EmployeeExperienceMatrixColumns.SalaryDate);
            }
        }

        public EmployeeExperienceReportData(ParameterObject parameterObject, EmployeeExperienceReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeExperienceReportDataOutput(reportDataInput);
        }

        public static List<EmployeeExperienceReportDataField> GetPossibleDataFields()
        {
            List<EmployeeExperienceReportDataField> possibleFields = new List<EmployeeExperienceReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeExperienceMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeExperienceReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeExperienceReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, Company.ActorCompanyId, 0);

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
                        #region Prereq                  

                        #endregion

                        #region Collections

                        Dictionary<int, List<EmploymentPriceTypeChangeDTO>> employmentPayrollPriceTypeChanges = EmployeeManager.GetEmploymentPriceTypeChangesForEmployees(base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                        List<PayrollPriceType> companyPayrollPriceTypes = PayrollManager.GetPayrollPriceTypes(base.ActorCompanyId, null);

                        #endregion
                        var loadEmployment = true;
                        if (selectionEmployeeIds.Count == 0)
                            employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: loadEmployment, loadContact: false);
                        else
                            employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: loadEmployment, loadContact: false);

                        #region Content


                        foreach (var employee in employees)
                        {
                            var experienceIn = 0;
                            var experienceTot = 0;
                            var experienceType = "";

                            var employmentsEntities = employee.GetEmployments(selectionDateFrom, selectionDateTo, includeSecondary: true);
                            List<EmploymentDTO> employments = employmentsEntities.ToSplittedDTOs();
                            EmploymentDTO lastEmployment = employments.OrderBy(o => o.DateFrom).LastOrDefault();
                            if (lastEmployment != null && !employments.Any(e => e.EmploymentId == lastEmployment.EmploymentId))
                            {
                                employments.Add(lastEmployment);
                                employments = employments.OrderBy(o => o.DateFrom).ToList();
                            }

                            if (!employments.IsNullOrEmpty())
                            {

                                var age = CalendarUtility.GetYearsBetweenDates(EmployeeManager.GetEmployeeBirthDate(employee).ToValueOrToday(), DateTime.Today);
                                if (LoadExperience)
                                {
                                    var last = employmentsEntities.FirstOrDefault(f => f.EmploymentId == lastEmployment.EmploymentId);
                                    DateTime df = (DateTime)lastEmployment.DateFrom;
                                    experienceTot = EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, last, useExperienceMonthsOnEmploymentAsStartValue, selectionDateTo);
                                    experienceIn = EmployeeManager.GetExperienceMonthsForEmployment(reportResult.ActorCompanyId, lastEmployment.EmploymentId, df.AddDays(-1));
                                    experienceType = (last.GetExperienceAgreedOrEstablished(selectionDateTo).ToInt() == 1) ? GetText(11707, "Överenskommen") : GetText(11708, "Konstaterad");
                                }
                                if (LoadPriceTypes)
                                {
                                    List<EmploymentPriceTypeChangeDTO> changeList = employmentPayrollPriceTypeChanges.ContainsKey(employee.EmployeeId) ? employmentPayrollPriceTypeChanges[employee.EmployeeId] : new List<EmploymentPriceTypeChangeDTO>();


                                    foreach (var changeItem in changeList.Where(c => c.Amount > 0).ToList())
                                    {

                                        PayrollPriceType payrollPriceType = companyPayrollPriceTypes.FirstOrDefault(t => t.PayrollPriceTypeId == changeItem.PayrollPriceTypeId);

                                        #region Item
                                        var employeeItem = new EmployeeExperienceItem()
                                        {
                                            FirstName = employee.FirstName,
                                            LastName = employee.LastName,
                                            EmployeeName = employee.Name,
                                            EmployeeNr = employee.EmployeeNr,
                                            SSN = employee.SocialSec,
                                            Age = age,
                                            ExperienceIn = experienceIn,
                                            ExperienceTot = experienceTot,
                                            ExperienceType = experienceType,
                                            SalaryDate = changeItem.FromDate.HasValue ? changeItem.FromDate : null,
                                            Salary = changeItem.Amount,
                                            SalaryType = payrollPriceType?.Code ?? string.Empty,
                                            SalaryTypeName = payrollPriceType?.Name ?? string.Empty,

                                        };
                                        _reportDataOutput.EmployeeExperienceItems.Add(employeeItem);
                                    }

                                }
                                else
                                {
                                    var employeeItem = new EmployeeExperienceItem()
                                    {
                                        FirstName = employee.FirstName,
                                        LastName = employee.LastName,
                                        EmployeeName = employee.Name,
                                        EmployeeNr = employee.EmployeeNr,
                                        SSN = employee.SocialSec,
                                        Age = age,
                                        ExperienceIn = experienceIn,
                                        ExperienceTot = experienceTot,
                                        ExperienceType = experienceType,
                                        SalaryDate = null,
                                        Salary = 0,
                                        SalaryType = string.Empty,
                                        SalaryTypeName = string.Empty,

                                    };
                                    _reportDataOutput.EmployeeExperienceItems.Add(employeeItem);
                                }


                                #endregion
                            }
                        }

                        #endregion
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

    public class EmployeeExperienceReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeExperienceMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeExperienceReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeExperienceMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeExperienceMatrixColumns.EmployeeNr;
        }
    }

    public class EmployeeExperienceReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeExperienceReportDataField> Columns { get; set; }

        public EmployeeExperienceReportDataInput(CreateReportResult reportResult, List<EmployeeExperienceReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeExperienceReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeeExperienceReportDataInput Input { get; set; }
        public List<EmployeeExperienceItem> EmployeeExperienceItems { get; set; }

        public EmployeeExperienceReportDataOutput(EmployeeExperienceReportDataInput input)
        {
            this.Input = input;
            this.EmployeeExperienceItems = new List<EmployeeExperienceItem>();
        }
    }
}
