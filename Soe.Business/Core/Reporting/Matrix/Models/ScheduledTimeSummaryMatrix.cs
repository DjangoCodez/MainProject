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
    public class ScheduledTimeSummaryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "ScheduledTimeSummaryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<ScheduledTimeSummaryReportDataField> filter { get; set; }
        List<ScheduledTimeSummaryItem> scheduledTimeSummaryItems { get; set; }
        #endregion

        public ScheduledTimeSummaryMatrix(InputMatrix inputMatrix, ScheduledTimeSummaryReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            scheduledTimeSummaryItems = reportDataOutput?.ScheduledTimeSummaryItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduledTimeSummaryMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduledTimeSummaryMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_ScheduledTimeSummaryMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_ScheduledTimeSummaryMatrixColumns.Time, new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ScheduledTimeSummaryMatrixColumns.Type));
            
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ScheduledTimeSummaryMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in scheduledTimeSummaryItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, ScheduledTimeSummaryItem scheduledTimeSummaryItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_ScheduledTimeSummaryMatrixColumns)))
            {
                var type = (TermGroup_ScheduledTimeSummaryMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_ScheduledTimeSummaryMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, scheduledTimeSummaryItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_ScheduledTimeSummaryMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, scheduledTimeSummaryItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_ScheduledTimeSummaryMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, scheduledTimeSummaryItem.Date, column.MatrixDataType);
                    case TermGroup_ScheduledTimeSummaryMatrixColumns.Time: return new MatrixField(rowNumber, column.Key, scheduledTimeSummaryItem.Time, column.MatrixDataType);
                    case TermGroup_ScheduledTimeSummaryMatrixColumns.Type: return new MatrixField(rowNumber, column.Key, scheduledTimeSummaryItem.Type, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
