using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class AgiAbsenceReportData : EconomyReportDataManager, IReportDataModel
    {
        private readonly AgiAbsenceReportDataOutput _reportDataOutput;

        public AgiAbsenceReportData(ParameterObject parameterObject, AgiAbsenceReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new AgiAbsenceReportDataOutput(reportDataInput);
        }

        public static List<AgiAbsenceReportDataField> GetPossibleDataFields()
        {
            List<AgiAbsenceReportDataField> possibleFields = new List<AgiAbsenceReportDataField>();
            EnumUtility.GetValues<TermGroup_AgiAbsenceMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new AgiAbsenceReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public AgiAbsenceReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out _))
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

                    bool socialSecPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec ,Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool payrollPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

                    #endregion

                    #endregion

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees);
                   
                    List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.Input.ActorCompanyId).Where(o => selectionTimePeriodIds.Contains(o.TimePeriodId)).ToList();
                    List<TimePayrollStatisticsSmallDTO> abcenses = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, Company.ActorCompanyId, employees, timePeriods.Select(s => s.TimePeriodId).ToList(), ignoreAccounting: true);

                    #region Content


                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        List<TimePayrollStatisticsSmallDTO> employeeAbsences = abcenses.Where(w => w.EmployeeId == employeeId).ToList();
                        if (!employeeAbsences.Any())
                            continue;

                        List<TimePayrollStatisticsSmallDTO> tempParentalLeave = employeeAbsences.Where(w => w.IsAbsenceTemporaryParentalLeave()).ToList();
                        List<TimePayrollStatisticsSmallDTO> parentalLeave = employeeAbsences.Where(w => w.IsParentalLeave()).ToList();

                        if (!payrollPermission && !tempParentalLeave.Any() && !parentalLeave.Any())
                            continue;

                        if (tempParentalLeave.Any())
                        {
                            foreach (var dayTrans in tempParentalLeave.Where(w => w.Quantity > 0).GroupBy(g => g.Date))
                            {
                                var type = GetText((int)TermGroup_AgiAbsenceMatrixColumns.TemporaryParentalLeave, (int)TermGroup.AgiAbsenceMatrixColumns);
                                _reportDataOutput.AgiAbsenceItems.Add(CreateItem(socialSecPermission, employee, dayTrans.ToList(), type));
                            }
                        }

                        if (parentalLeave.Any())
                        {
                            foreach (var dayTrans in parentalLeave.Where(w => w.Quantity > 0).GroupBy(g => g.Date))
                            {
                                var type = GetText((int)TermGroup_AgiAbsenceMatrixColumns.ParentalLeave, (int)TermGroup.AgiAbsenceMatrixColumns);
                                _reportDataOutput.AgiAbsenceItems.Add(CreateItem(socialSecPermission, employee, dayTrans.ToList(), type));
                            }
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

        private AgiAbsenceItem CreateItem(bool socialSecPermission, Employee employee, List<TimePayrollStatisticsSmallDTO> dayTrans, string type)
        {
            var trans = dayTrans.First();
            return new AgiAbsenceItem
            {   
                EmployeeNr = employee.EmployeeNr,
                EmployeeName = employee.Name,
                SocialSec = socialSecPermission ? employee.SocialSec : string.Empty,
                PaymentDate = trans.PaymentDate.Value,
                Date = trans.Date, 
                ProductNr = trans.PayrollProductNumber, 
                ProductName = trans.PayrollProductName, 
                Type = type,
                Quantity = dayTrans.Sum(s => s.Quantity) / 60,
            };

        }
    }

    public class AgiAbsenceReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_AgiAbsenceMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public AgiAbsenceReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_AgiAbsenceMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_AgiAbsenceMatrixColumns.Unknown;
        }
    }

    public class AgiAbsenceReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AgiAbsenceReportDataField> Columns { get; set; }

        public AgiAbsenceReportDataInput(CreateReportResult reportResult, List<AgiAbsenceReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class AgiAbsenceReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public AgiAbsenceReportDataInput Input { get; set; }
        public List<AgiAbsenceItem> AgiAbsenceItems { get; set; }

        public AgiAbsenceReportDataOutput(AgiAbsenceReportDataInput input)
        {
            this.Input = input;
            this.AgiAbsenceItems = new List<AgiAbsenceItem>();
        }
    }

}

