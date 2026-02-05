using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class AggregatedTimeStatisticsMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "AggregatedTimeStatisticsMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<AggregatedTimeStatisticsReportDataField> filter { get; set; }
        List<AggregatedTimeStatisticsItem> aggregatedTimeStatisticsItems { get; set; }
        #endregion

        public AggregatedTimeStatisticsMatrix(InputMatrix inputMatrix, AggregatedTimeStatisticsReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            aggregatedTimeStatisticsItems = reportDataOutput?.AggregatedTimeStatisticsItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (this.inputMatrix == null)
                return possibleColumns;

            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountDimName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeePosition));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeWeekWorkHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetQuantity));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossQuantity));
                if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetAmount));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossAmount));
                }
            }
            if (base.HasReadPermission(Feature.Time_Time_Attest) || base.HasReadPermission(Feature.Time_Payroll_Calculation_Edit))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.WorkHoursTotal));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel40));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel50));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel57));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel70));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel79));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel100));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel113));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel35));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel70));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel100));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel35));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel50));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel70));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel100));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceHours));

            }
            if (base.HasReadPermission(Feature.Time_Payroll_Calculation_Edit))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeSalary));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.CostTotal));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.CostCalenderDayWeek));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.CostCalenderDay));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.CostNetHours));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel40));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel50));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel57));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel70));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel79));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel100));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel113));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel35));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel70));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel100));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel35));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel50));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel70));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel100));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceCost));

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
                        MatrixLayoutColumn item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey);

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_AggregatedTimeStatisticsMatrixColumns column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), GetText((int)column, EnumUtility.GetName(column)), options);
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

            foreach (var employee in aggregatedTimeStatisticsItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employee));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, AggregatedTimeStatisticsItem aggregatedTimeStatisticsItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_AggregatedTimeStatisticsMatrixColumns)))
            {
                var type = (TermGroup_AggregatedTimeStatisticsMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.Unknown:
                        break;
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountDimName:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AccountNr, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountNr:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AccountNr, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AccountName, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeePosition:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.EmployeePosition, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeWeekWorkHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.EmployeeWeekWorkHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeSalary:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.EmployeeSalary, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.WorkHoursTotal:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.WorkHoursTotal, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel40:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel40, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel50:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel50, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel57:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel57, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel70:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel70, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel79:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel79, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel100:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel100, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel113:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingHoursLevel113, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel35:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeHoursLevel35, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel70:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeHoursLevel70, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel100:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeHoursLevel100, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel35:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeHoursLevel35, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel50:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeHoursLevel50, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel70:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeHoursLevel70, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel100:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeHoursLevel100, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.SicknessHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.VacationHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AbsenceHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostTotal:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.CostTotal, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostCalenderDayWeek:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.CostCalenderDayWeek, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostCalenderDay:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.CostCalenderDay, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostNetHours:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.CostNetHours, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCost:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCost, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel40:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel40, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel50:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel50, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel57:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel57, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel70:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel70, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel79:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel79, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel100:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel100, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel113:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.InconvinientWorkingCostLevel113, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCost:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeCost, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel35:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeCostLevel35, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel70:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeCostLevel70, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel100:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AddedTimeCostLevel100, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCost:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeCost, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel35:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeCostLevel35, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel50:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeCostLevel50, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel70:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeCostLevel70, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel100:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.OverTimeCostLevel100, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessCost:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.SicknessCost, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationCost:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.VacationCost, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceCost:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AbsenceCost, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetQuantity:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.ScheduleNetQuantity, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossQuantity:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.ScheduleGrossQuantity, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetAmount:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.ScheduleNetAmount, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossAmount:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.ScheduleGrossAmount, column.MatrixDataType);
                    default:
                        break;
                }

                switch (type)
                {
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountNr:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AccountNr, column.MatrixDataType);
                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, aggregatedTimeStatisticsItem.AccountName, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
