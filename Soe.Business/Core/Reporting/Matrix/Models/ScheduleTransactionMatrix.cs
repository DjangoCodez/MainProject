using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class ScheduleTransactionMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "timeTransactionMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<ScheduleTransactionReportDataReportDataField> filter { get; set; }
        private List<TimeScheduleTypeDTO> timeScheduleTypes { get; set; }
        private List<ShiftTypeDTO> shiftTypes { get; set; }
        Dictionary<int, List<TimeScheduleTransactionItem>> ScheduleTransactions { get; set; }
        List<EmployeeDTO> Employees { get; set; }
        List<TimeCode> TimeCodes { get; set; }
        List<TimeRule> TimeRules { get; set; }

        #endregion

        public ScheduleTransactionMatrix(InputMatrix inputMatrix, ScheduleTransactionReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            ScheduleTransactions = reportDataOutput != null ? reportDataOutput.ScheduleTransactions : new Dictionary<int, List<TimeScheduleTransactionItem>>();
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeDTO>();
            filter = reportDataOutput?.Input?.Columns;
            shiftTypes = reportDataOutput?.ShiftTypes;
            timeScheduleTypes = reportDataOutput?.TimeScheduleTypes;
            TimeCodes = reportDataOutput?.TimeCodes;
            TimeRules = reportDataOutput?.TimeRules;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            bool socialSecPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);

            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.EmployeeName));
            if (socialSecPermission)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.SocialSec));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_ScheduleTransactionMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.StartTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.StopTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduleTransactionMatrixColumns.NetMinutes));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduleTransactionMatrixColumns.GrossMinutes));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduleTransactionMatrixColumns.NetHours, new MatrixDefinitionColumnOptions { MinutesToDecimal = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduleTransactionMatrixColumns.GrossHours, new MatrixDefinitionColumnOptions { MinutesToDecimal = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduleTransactionMatrixColumns.NetHoursString, new MatrixDefinitionColumnOptions { MinutesToTimeSpan = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduleTransactionMatrixColumns.GrossHoursString, new MatrixDefinitionColumnOptions { MinutesToTimeSpan = true }));
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ScheduleTransactionMatrixColumns.NetCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ScheduleTransactionMatrixColumns.GrossCost));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeScheduleTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.ScheduleTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.Description));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ScheduleTransactionMatrixColumns.IsBreak));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ScheduleTransactionMatrixColumns.IsPreliminary));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ScheduleTransactionMatrixColumns.ExtraShift));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ScheduleTransactionMatrixColumns.SubstituteShift));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_ScheduleTransactionMatrixColumns.SubstituteShiftCalculated));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.EmployeeGroup));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_ScheduleTransactionMatrixColumns.EmploymentPercent));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.TimeCodeCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.TimeCodeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.TimeRuleName));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ScheduleTransactionMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_ScheduleTransactionMatrixColumns.Modified));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduleTransactionMatrixColumns.ModifiedBy));

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_ScheduleTransactionMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
                }
            }

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
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }
                }

                definitionColumns = matrixDefinitionColumns;
            }
            return definitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ScheduleTransactionMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;
            bool socialSecPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);

            foreach (var transactionsOnEmployee in ScheduleTransactions)
            {
                var employee = Employees.FirstOrDefault(f => f.EmployeeId == transactionsOnEmployee.Key);

                if (employee != null)
                {
                    foreach (var transaction in transactionsOnEmployee.Value)
                    {
                        List<MatrixField> fields = new List<MatrixField>();

                        foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                        {
                            fields.Add(CreateField(rowNumber, column, transaction, employee, socialSecPermission));
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, TimeScheduleTransactionItem scheduleTransaction, EmployeeDTO employee, bool socialSecPermission = false)
        {
            if (base.GetEnumId<TermGroup_ScheduleTransactionMatrixColumns>(column, out int id))
            {
                switch ((TermGroup_ScheduleTransactionMatrixColumns)id)
                {
                    case TermGroup_ScheduleTransactionMatrixColumns.Unknown:
                        break;
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeNr:
                        if (employee != null && column.MatrixDataType == MatrixDataType.String)
                            return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                        else
                            return new MatrixField(rowNumber, column.Key, "");
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeName:
                        if (employee != null && column.MatrixDataType == MatrixDataType.String)
                            return new MatrixField(rowNumber, column.Key, employee.Name, column.MatrixDataType);
                        else
                            return new MatrixField(rowNumber, column.Key, "");
                    case TermGroup_ScheduleTransactionMatrixColumns.SocialSec:
                        if (employee != null && socialSecPermission && column.MatrixDataType == MatrixDataType.String)
                            return new MatrixField(rowNumber, column.Key, employee.SocialSec, column.MatrixDataType);
                        else
                            return new MatrixField(rowNumber, column.Key, "");
                    case TermGroup_ScheduleTransactionMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Date, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.StartTime:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.StartTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.StopTime:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.StopTime.ToString("HH:mm"), column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.NetMinutes:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.NetLength, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossMinutes:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Length, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.NetCost:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Amount, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossCost:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.GrossAmount, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.IsBreak:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.IsBreak, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.IsPreliminary:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.IsPreliminary, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.ExtraShift:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.ExtraShift, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.SubstituteShift:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.SubstituteShift, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.EmployeeGroup:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.EmployeeGroup, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.SubstituteShiftCalculated:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.SubstituteShiftCalculated, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.EmploymentPercent:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.EmploymentPercent, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeCodeCode:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.TimeCodeId.ToString(), column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeCodeName:
                        return new MatrixField(rowNumber, column.Key, TimeCodes.FirstOrDefault(f => f.TimeCodeId == scheduleTransaction.TimeCodeId).Name, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.TimeRuleName:
                        var timeRuleList = GetTimeRulesList(scheduleTransaction);
                        return new MatrixField(rowNumber, column.Key, timeRuleList, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.Description:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Description, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.NetHoursString:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.NetQuantity, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.NetHours:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.NetQuantity, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossHoursString:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Quantity, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.GrossHours:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Quantity, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.ScheduleTypeName:
                        return new MatrixField(rowNumber, column.Key, timeScheduleTypes?.FirstOrDefault(f => f.TimeScheduleTypeId == scheduleTransaction.ScheduleTypeId)?.Name, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeName:
                        return new MatrixField(rowNumber, column.Key, shiftTypes?.FirstOrDefault(f => f.ShiftTypeId == scheduleTransaction.ShiftTypeId)?.Name, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeScheduleTypeName:
                        return new MatrixField(rowNumber, column.Key, shiftTypes?.FirstOrDefault(f => f.ShiftTypeId == scheduleTransaction.ShiftTypeId)?.TimeScheduleTypeName, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Created, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.CreatedBy, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.Modified, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.ModifiedBy, column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_ScheduleTransactionMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, scheduleTransaction.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);
                    default:
                        return new MatrixField(rowNumber, column.Key, "", column.MatrixDataType);
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }

        public string GetTimeRulesList(TimeScheduleTransactionItem scheduleTransaction)
        {
            var ruleIds = scheduleTransaction.GrossTimeRules != null ? scheduleTransaction.GrossTimeRules.Select(s => s.TimeRuleId).ToList() : new List<int>();
            var timeRules = ruleIds.Count > 0 ? TimeRules.Where(w => ruleIds.Contains(w.TimeRuleId)).ToList() : new List<TimeRule>();
            return timeRules.Count > 0 ? String.Join(", ", timeRules.Select(s => s.Name).ToList()) : "";
        }
    }
}
