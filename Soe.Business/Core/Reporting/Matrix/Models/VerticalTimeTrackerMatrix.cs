using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class VerticalTimeTrackerMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "VerticalTimeTrackerMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<VerticalTimeTrackerReportDataField> filter { get; set; }
        List<VerticalTimeTrackerItem> verticalTimeTrackerItems { get; set; }
        #endregion

        public VerticalTimeTrackerMatrix(InputMatrix inputMatrix, VerticalTimeTrackerReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            verticalTimeTrackerItems = reportDataOutput?.VerticalTimeTrackerItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VerticalTimeTrackerMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VerticalTimeTrackerMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_VerticalTimeTrackerMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_VerticalTimeTrackerMatrixColumns.StartTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_VerticalTimeTrackerMatrixColumns.StopTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VerticalTimeTrackerMatrixColumns.TimeInterval));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VerticalTimeTrackerMatrixColumns.Time, new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VerticalTimeTrackerMatrixColumns.TimeCost));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VerticalTimeTrackerMatrixColumns.Schedule));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VerticalTimeTrackerMatrixColumns.ScheduleCost));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories)))
            {
                foreach (var dim in inputMatrix.AccountDims.Where(w => w.IsInternal))
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VerticalTimeTrackerMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VerticalTimeTrackerMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_VerticalTimeTrackerMatrixColumns.StartYear));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_VerticalTimeTrackerMatrixColumns.StartMonth));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_VerticalTimeTrackerMatrixColumns.StartDay));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_VerticalTimeTrackerMatrixColumns.StartHour));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_VerticalTimeTrackerMatrixColumns.StartMinute));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_VerticalTimeTrackerMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employee in verticalTimeTrackerItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, VerticalTimeTrackerItem verticalTimeTrackerItem)
        {
            if (base.GetEnumId<TermGroup_VerticalTimeTrackerMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_VerticalTimeTrackerMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_VerticalTimeTrackerMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.Date, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.Time: return new MatrixField(rowNumber, column.Key, decimal.Round(decimal.Divide(verticalTimeTrackerItem.Time, 60), 2), column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.TimeCost: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.TimeCost, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.Schedule: return new MatrixField(rowNumber, column.Key, decimal.Round(decimal.Divide(verticalTimeTrackerItem.Schedule, 60), 2), column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.ScheduleCost: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.ScheduleCost, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StartTime: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StartTime, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StopTime: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StopTime, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StartYear: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StartTime.Year, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StartMonth: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StartTime.Month, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StartDay: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StartTime.Day, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StartHour: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StartTime.Hour, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.StartMinute: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.StartTime.Minute, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.TimeInterval: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.TimeInterval, column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.AccountInternalNames: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_VerticalTimeTrackerMatrixColumns.AccountInternalNrs: return new MatrixField(rowNumber, column.Key, verticalTimeTrackerItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
