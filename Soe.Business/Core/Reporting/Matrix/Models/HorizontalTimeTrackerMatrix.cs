using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class HorizontalTimeTrackerMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "HorizontalTimeTrackerMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<HorizontalTimeTrackerReportDataField> filter { get; set; }
        List<HorizontalTimeTrackerItem> horizontalTimeTrackerItems { get; set; }
        #endregion

        public HorizontalTimeTrackerMatrix(InputMatrix inputMatrix, HorizontalTimeTrackerReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            horizontalTimeTrackerItems = reportDataOutput?.HorizontalTimeTrackerItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_HorizontalTimeTrackerMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_HorizontalTimeTrackerMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_HorizontalTimeTrackerMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_HorizontalTimeTrackerMatrixColumns.Time, new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_HorizontalTimeTrackerMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in horizontalTimeTrackerItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, HorizontalTimeTrackerItem horizontalTimeTrackerItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_HorizontalTimeTrackerMatrixColumns)))
            {
                var type = (TermGroup_HorizontalTimeTrackerMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_HorizontalTimeTrackerMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, horizontalTimeTrackerItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_HorizontalTimeTrackerMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, horizontalTimeTrackerItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_HorizontalTimeTrackerMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, horizontalTimeTrackerItem.Date, column.MatrixDataType);
                    case TermGroup_HorizontalTimeTrackerMatrixColumns.Time: return new MatrixField(rowNumber, column.Key, horizontalTimeTrackerItem.Time, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
