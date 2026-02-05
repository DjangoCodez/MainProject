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
    public class EmployeeTimePeriodReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeTimePeriodReportDataOutput _reportDataOutput;

        public EmployeeTimePeriodReportData(ParameterObject parameterObject, EmployeeTimePeriodReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new EmployeeTimePeriodReportDataOutput(reportDataInput);
        }

        public static List<EmployeeTimePeriodReportDataField> GetPossibleDataFields()
        {
            List<EmployeeTimePeriodReportDataField> possibleFields = new List<EmployeeTimePeriodReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeTimePeriodMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeTimePeriodReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeTimePeriodReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return new ActionResult(false);

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.Input.ActorCompanyId).Where(o => selectionTimePeriodIds.Contains(o.TimePeriodId)).ToList();
            DateTime selectionDateFrom = timePeriods.OrderBy(o => o.StartDate).FirstOrDefault()?.StartDate ?? CalendarUtility.DATETIME_DEFAULT;
            DateTime selectionDateTo = timePeriods.OrderBy(o => o.StartDate).LastOrDefault()?.PayrollStopDate.Value ?? CalendarUtility.DATETIME_DEFAULT;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            if (employees.IsNullOrEmpty())
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    List<EmployeeTimePeriodItem> employeeTimePeriodItems = new List<EmployeeTimePeriodItem>();
                    bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    var employeeTimePeriods = TimePeriodManager.GetEmployeesTimePeriodsWithValues(entities, selectionTimePeriodIds, selectionEmployeeIds, base.ActorCompanyId);

                    #endregion

                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        var employeeTimePeriodsOnEmployee = employeeTimePeriods.Where(w => w.EmployeeId == employeeId);

                        if (employeeTimePeriodsOnEmployee.IsNullOrEmpty())
                            continue;


                        #region Prereq

                        Employee employee = employees?.GetEmployee(employeeId);
                        if (employee == null)
                            continue;

                        #endregion

                        #region periods

                        foreach (var etp in employeeTimePeriodsOnEmployee)
                        {
                            var timePeriod = timePeriods.FirstOrDefault(f => f.TimePeriodId == etp.TimePeriodId);

                            if (timePeriod?.PaymentDate == null)
                                continue;

                            EmployeeTimePeriodItem item = new EmployeeTimePeriodItem()
                            {
                                EmployeeId = employeeId,
                                EmployeeNr = employee.EmployeeNr,
                                EmployeeName = employee.Name,
                                SocialSec = showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                                StartDate = timePeriod.StartDate,
                                StopDate = timePeriod.StopDate,
                                PayrollStartDate = timePeriod.PayrollStartDate.Value,
                                PayrollStopDate = timePeriod.PayrollStopDate.Value,
                                PaymentDate = timePeriod.PaymentDate.Value,
                                TableTax = etp.GetTableTaxSum(),
                                Tax = etp.GetTaxSum(),
                                OneTimeTax = etp.GetOneTimeTaxSum(),
                                EmploymentTaxCredit = etp.GetEmploymentTaxCreditSum(),
                                SupplementChargeCredit = etp.GetSupplementChargeCreditSum(),
                                GrossSalary = etp.GetGrossSalarySum(),
                                NetSalary = etp.GetNetSum(),
                                VacationCompensation = etp.GetVacationCompensationSum(),
                                Benefit = etp.GetBenefitSum(),
                                Compensation = etp.GetCompensationSum(),
                                Deduction = etp.GetDeductionSum(),
                                UnionFee = etp.GetUnionFeeSum(),
                                OptionalTax = etp.GetOptionalTaxSum(),
                                SINKTax = etp.GetSINKTaxSum(),
                                ASINKTax = etp.GetASINKTaxSum(),
                                EmploymentTaxBasis = etp.GetEmploymentTaxBasisSum()
                            };

                            employeeTimePeriodItems.Add(item);
                        }
                    }

                    _reportDataOutput.EmployeeTimePeriodItems = employeeTimePeriodItems.OrderBy(o => o.PaymentDate).ThenBy(t => t.EmployeeNr).ToList();

                    #endregion

                    #endregion
                }

                #region Close repository

                base.personalDataRepository.GenerateLogs();

                #endregion

                return new ActionResult();
            }
        }
    }

    public class EmployeeTimePeriodReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeTimePeriodMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeTimePeriodReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeTimePeriodMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeTimePeriodMatrixColumns.EmployeeNr;
        }
    }

    public class EmployeeTimePeriodReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeTimePeriodReportDataField> Columns { get; set; }

        public EmployeeTimePeriodReportDataInput(CreateReportResult reportResult, List<EmployeeTimePeriodReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeTimePeriodReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeeTimePeriodReportDataInput Input { get; set; }
        public List<EmployeeTimePeriodItem> EmployeeTimePeriodItems { get; set; }

        public EmployeeTimePeriodReportDataOutput(EmployeeTimePeriodReportDataInput input)
        {
            this.Input = input;
            this.EmployeeTimePeriodItems = new List<EmployeeTimePeriodItem>();
        }
    }
}

