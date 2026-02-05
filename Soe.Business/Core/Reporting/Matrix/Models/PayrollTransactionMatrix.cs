using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class PayrollTransactionMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "PayrollTransactionMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<PayrollTransactionReportDataReportDataField> filter { get; set; }
                
        #endregion

        #region Permissions

        private bool socialSecPermission;
        private bool disbursementPermission;
        private bool employeeNotePermission;
        private bool employmentPermission;
        private bool payrollSalaryPermission;

        #endregion

        #region Collections

        List<TimePayrollStatisticsDTO> TimePayrollStatistics { get; set; }
        List<EmployeeDTO> Employees { get; set; }
        List<PayrollProductDTO> PayrollProducts { get; set; }
        private Dictionary<int, string> SexDict { get; set; }
        private Dictionary<int, string> Pension { get; set; }
        private List<GenericType> TimeUnits { get; set; }

        #endregion

        public PayrollTransactionMatrix(InputMatrix inputMatrix, PayrollTransactionReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            TimePayrollStatistics = reportDataOutput != null ? reportDataOutput.TimePayrollStatistics : new List<TimePayrollStatisticsDTO>();
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeDTO>();
            PayrollProducts = reportDataOutput != null ? reportDataOutput.PayrollProducts : new List<PayrollProductDTO>();
            filter = reportDataOutput?.Input?.Columns;
            
            SexDict = base.GetTermGroupDict(TermGroup.Sex);
            Pension = base.GetTermGroupDict(TermGroup.PensionCompany);
            TimeUnits = reportDataOutput != null ? reportDataOutput.TimeUnits : new List<GenericType>();
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            socialSecPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
            disbursementPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount);
            employeeNotePermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Note);
            employmentPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
            payrollSalaryPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);

            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeFirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeLastName));
            if (socialSecPermission)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeSocialSec));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeSex));

            if (disbursementPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementClearingNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementAccountNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementCountryCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementBIC));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementIBAN));
            }

            if (payrollSalaryPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.HighRiskProtection));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_PayrollTransactionMatrixColumns.HighRiskProtectionTo));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.MedicalCertificateReminder));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_PayrollTransactionMatrixColumns.MedicalCertificateDays));
            }

            if (employeeNotePermission)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.Note));

            if (employmentPermission)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.SSG));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_PayrollTransactionMatrixColumns.TimeBlockDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_PayrollTransactionMatrixColumns.PaymentDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.AttestState));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.PayrollProductNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.PayrollProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.PayrollProductDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.TimeCodeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.TimeCodeNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.TimeCodeDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.TimeBlockStartTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.TimeBlockStopTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.ScheduleTransaction));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_PayrollTransactionMatrixColumns.ScheduleTransactionType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel1));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel2));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel3));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel4));

            if (payrollSalaryPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.UnitPrice));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.Amount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.VatAmount));
             
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.Formula));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.FormulaPlain));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.FormulaExtracted));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.FormulaNames));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.FormulaOrigin));

                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.Ratio));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.Quantity));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.QuantityWorkDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.QuantityCalendarDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.CalenderDayFactor));
               
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_PayrollTransactionMatrixColumns.TimeUnit));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.TimeUnitName));
                
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.ManuallyAdded));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.AutoAttestFailed));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.Exported));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.IsPreliminary));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.IsRetroactive));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.Pensioncompany));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.Vacationsalarypromoted));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.Unionfeepromoted));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.WorkingTimePromoted));

            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.WorkTimeWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.EmployeeGroupName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollTransactionMatrixColumns.EmployeeGroupWorkTimeWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.PayrollGroupName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollTransactionMatrixColumns.PayrollCalculationPerformed));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.Comment));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_PayrollTransactionMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_PayrollTransactionMatrixColumns.Modified));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.ModifiedBy));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.AccountNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.AccountName));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollTransactionMatrixColumns.AccountString));

            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (definitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();

                List<MatrixLayoutColumn> possibleColumns = GetMatrixLayoutColumns();

                if (filter != null)
                {
                    int columnNumber = 0;
                    foreach (var field in filter.OrderBy(o => o.Sort))
                    {
                        var item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey);

                        if (item != null)
                        {
                            columnNumber++;
                            matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item, columnNumber, field.Selection?.Options != null ? field.Selection.Options : item.Options));
                        }
                    }
                }
                else
                {
                    foreach (MatrixLayoutColumn item in possibleColumns)
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                }

                definitionColumns = matrixDefinitionColumns;
            }
            return definitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_PayrollTransactionMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), string.IsNullOrEmpty(overrideTitle) ? GetText((int)column, EnumUtility.GetName(column)) : overrideTitle, options);
            if (IsAccountInternal(column))
            {
                var name = GetAccountInternalName(column, 1);
                if (!string.IsNullOrEmpty(name))
                    matrixLayoutColumn.Title = name;
            }

            return matrixLayoutColumn;
        }

        public MatrixResult GetMatrixResult()
        {
            socialSecPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
            disbursementPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount);
            employeeNotePermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Note);
            employmentPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
            payrollSalaryPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);
            
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;

            foreach (var transactionsOnEmployee in TimePayrollStatistics.GroupBy(g => g.EmployeeId))
            {
                var employee = Employees.FirstOrDefault(f => f.EmployeeId == transactionsOnEmployee.Key);

                if (employee != null)
                {
                    foreach (var transaction in transactionsOnEmployee)
                    {
                        List<MatrixField> fields = new List<MatrixField>();

                        foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                        {
                            fields.Add(CreateField(rowNumber, column, transaction, employee));
                        }

                        if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                            result.MatrixFields.AddRange(fields);
                        result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                        rowNumber++;
                    }
                }
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, TimePayrollStatisticsDTO transaction, EmployeeDTO employee)
        {
            if (base.GetEnumId<TermGroup_PayrollTransactionMatrixColumns>(column, out int id))
            {
                GenericType timeUnit = TimeUnits?.FirstOrDefault(f => f.Id == transaction.TimeUnit);

                switch ((TermGroup_PayrollTransactionMatrixColumns)id)
                {
                    case TermGroup_PayrollTransactionMatrixColumns.Unknown:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employee.Name, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeFirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeLastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeSocialSec:
                        return new MatrixField(rowNumber, column.Key, socialSecPermission ? employee.SocialSec : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeSex:
                        return new MatrixField(rowNumber, column.Key, GetValueFromDict((int)CalendarUtility.GetSexFromSocialSecNr(employee.SocialSec), SexDict), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementClearingNr:
                        return new MatrixField(rowNumber, column.Key, disbursementPermission ? employee.DisbursementClearingNr : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementAccountNr:
                        return new MatrixField(rowNumber, column.Key, disbursementPermission ? employee.DisbursementAccountNr : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementCountryCode:
                        return new MatrixField(rowNumber, column.Key, disbursementPermission ? employee.DisbursementCountryCode : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementBIC:
                        return new MatrixField(rowNumber, column.Key, disbursementPermission ? employee.DisbursementBIC : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeDisbursementIBAN:
                        return new MatrixField(rowNumber, column.Key, disbursementPermission ? employee.DisbursementIBAN : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.HighRiskProtection:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? employee.HighRiskProtection : false, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.HighRiskProtectionTo:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? employee.HighRiskProtectionTo : null, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.MedicalCertificateReminder:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? employee.MedicalCertificateReminder : false, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.MedicalCertificateDays:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? employee.MedicalCertificateDays : null, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Note:
                        return new MatrixField(rowNumber, column.Key, employeeNotePermission ? employee.Note : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.SSG:
                        return new MatrixField(rowNumber, column.Key, employmentPermission ? employee.CurrentEmploymentPercent : null, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeBlockDate:
                        return new MatrixField(rowNumber, column.Key, transaction.TimeBlockDate, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.PaymentDate:
                        return new MatrixField(rowNumber, column.Key, transaction.PaymentDate, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AttestState:
                        return new MatrixField(rowNumber, column.Key, transaction.AttestStateName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.PayrollProductNumber:
                        return new MatrixField(rowNumber, column.Key, transaction.PayrollProductNumber, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.PayrollProductName:
                        return new MatrixField(rowNumber, column.Key, transaction.PayrollProductName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.PayrollProductDescription:
                        return new MatrixField(rowNumber, column.Key, transaction.PayrollProductDescription, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeCodeName:
                        return new MatrixField(rowNumber, column.Key, transaction.TimeCodeName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeCodeNumber:
                        return new MatrixField(rowNumber, column.Key, transaction.TimeCodeNumber, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeCodeDescription:
                        return new MatrixField(rowNumber, column.Key, transaction.TimeCodeDescription, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeBlockStartTime:
                        return new MatrixField(rowNumber, column.Key, transaction.TimeBlockStartTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeBlockStopTime:
                        return new MatrixField(rowNumber, column.Key, transaction.TimeBlockStopTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.ScheduleTransaction:
                        return new MatrixField(rowNumber, column.Key, transaction.IsScheduleTransaction, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.ScheduleTransactionType:
                        return new MatrixField(rowNumber, column.Key, transaction.ScheduleTransactionType.HasValue ? transaction.ScheduleTransactionType.Value : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel1:
                        return new MatrixField(rowNumber, column.Key, transaction.SysPayrollTypeLevel1Name, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel2:
                        return new MatrixField(rowNumber, column.Key, transaction.SysPayrollTypeLevel2Name, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel3:
                        return new MatrixField(rowNumber, column.Key, transaction.SysPayrollTypeLevel3Name, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.SysPayrollTypeLevel4:
                        return new MatrixField(rowNumber, column.Key, transaction.SysPayrollTypeLevel4Name, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.UnitPrice:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.UnitPrice : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Amount:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.Amount : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.VatAmount:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.VatAmount : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Ratio:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.AbsenceRatio : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Quantity:
                        decimal quantity = payrollSalaryPermission ? transaction.Quantity : 0;
                        if (quantity != 0 && timeUnit != null && timeUnit.Id == (int)TermGroup_PayrollProductTimeUnit.Hours)
                            quantity /= 60;
                        return new MatrixField(rowNumber, column.Key, quantity, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.QuantityWorkDays:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.QuantityWorkDays : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.QuantityCalendarDays:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.QuantityCalendarDays : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.CalenderDayFactor:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.CalenderDayFactor : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeUnit:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.TimeUnit : 0, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.TimeUnitName:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? timeUnit?.Name : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.ManuallyAdded:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission && transaction.ManuallyAdded, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AutoAttestFailed:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission && transaction.AutoAttestFailed, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Exported:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission && transaction.Exported, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.IsPreliminary:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission && transaction.IsPreliminary, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.IsRetroactive:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission && !transaction.RetroactivePayrollOutcomeId.IsNullOrEmpty(), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Formula:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.Formula : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.FormulaPlain:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.FormulaPlain : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.FormulaExtracted:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.FormulaExtracted : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.FormulaNames:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.FormulaNames : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.FormulaOrigin:
                        return new MatrixField(rowNumber, column.Key, payrollSalaryPermission ? transaction.FormulaOrigin : string.Empty, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.WorkTimeWeek:
                        return new MatrixField(rowNumber, column.Key, decimal.Round(decimal.Divide(transaction.WorkTimeWeek, 60), 2), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeGroupName:
                        return new MatrixField(rowNumber, column.Key, transaction.EmployeeGroupName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.EmployeeGroupWorkTimeWeek:
                        return new MatrixField(rowNumber, column.Key, decimal.Round(decimal.Divide(transaction.EmployeeGroupWorkTimeWeek, 60), 2), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.PayrollGroupName:
                        return new MatrixField(rowNumber, column.Key, transaction.PayrollGroupName, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.PayrollCalculationPerformed:
                        return new MatrixField(rowNumber, column.Key, transaction.PayrollCalculationPerformed, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Comment:
                        return new MatrixField(rowNumber, column.Key, transaction.Comment, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, transaction.Created, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, transaction.CreatedBy, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, transaction.Modified, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, transaction.ModifiedBy, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AccountString:
                        return new MatrixField(rowNumber, column.Key, transaction.AccountString, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AccountNr:
                        return new MatrixField(rowNumber, column.Key, transaction.Dim1Nr, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, transaction.Dim1Name, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AccountInternalNames:
                        var accountAnalysisFields1 = transaction.AccountInternals?.Select(s => new AccountAnalysisField(s)).ToList();
                        return new MatrixField(rowNumber, column.Key, accountAnalysisFields1.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.AccountInternalNrs:
                        var accountAnalysisFields2 = transaction.AccountInternals?.Select(s => new AccountAnalysisField(s)).ToList();
                        return new MatrixField(rowNumber, column.Key, accountAnalysisFields2.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Pensioncompany:                        
                        return new MatrixField(rowNumber, column.Key, GetValueFromDict(PayrollProducts?.FirstOrDefault(a => a.ProductId == transaction.PayrollProductId).Settings?.FirstOrDefault()?.PensionCompany, Pension), column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Unionfeepromoted:
                        return new MatrixField(rowNumber, column.Key, PayrollProducts?.FirstOrDefault(a => a.ProductId == transaction.PayrollProductId).Settings?.FirstOrDefault()?.UnionFeePromoted ?? false, column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.Vacationsalarypromoted:
                        return new MatrixField(rowNumber, column.Key, PayrollProducts?.FirstOrDefault(a => a.ProductId == transaction.PayrollProductId).Settings?.FirstOrDefault()?.VacationSalaryPromoted ?? false , column.MatrixDataType);
                    case TermGroup_PayrollTransactionMatrixColumns.WorkingTimePromoted:
                        return new MatrixField(rowNumber, column.Key, PayrollProducts?.FirstOrDefault(a => a.ProductId == transaction.PayrollProductId).Settings?.FirstOrDefault()?.WorkingTimePromoted ?? false, column.MatrixDataType);
                        
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }

    }
}
