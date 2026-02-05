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
    public class SwapShiftMatrix(InputMatrix inputMatrix, SwapShiftReportDataOutput reportDataOutput) : BaseMatrix(inputMatrix), IMatrixModel
    {
        #region Column

        public static readonly string Prefix = "SwapShiftMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<SwapShiftReportDataField> Filter { get; set; } = reportDataOutput?.Input?.Columns;
        List<SwapShiftItem> SwapShiftItems { get; set; } = reportDataOutput?.SwapShiftItems;

        #endregion

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = [];
            if (!base.HasReadPermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee))
            {
                return possibleColumns;
            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_SwapShiftMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.HasShift));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_SwapShiftMatrixColumns.ShiftLength, options: new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.AcceptorEmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.AcceptorEmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_SwapShiftMatrixColumns.AcceptorDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.SwappedToEmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.SwappedToEmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.InitiatorEmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.InitiatorEmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_SwapShiftMatrixColumns.InitiatorTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_SwapShiftMatrixColumns.ApprovedDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SwapShiftMatrixColumns.ApprovedBy));
           
            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (DefinitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = [];

                List<MatrixLayoutColumn> possibleColumns = GetMatrixLayoutColumns();

                if (Filter != null)
                {
                    int columnNumber = 0;
                    foreach (var field in Filter.OrderBy(o => o.Sort))
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

                DefinitionColumns = matrixDefinitionColumns;
            }
            return DefinitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_SwapShiftMatrixColumns column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new(dataType, EnumUtility.GetName(column), GetText((int)column, EnumUtility.GetName(column)), options);
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
            MatrixResult result = new()
            {
                MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() }
            };

            #region Create matrix

            int rowNumber = 1;

            foreach (var swapShift in SwapShiftItems)
            {
                List<MatrixField> fields = [];

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, swapShift));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, SwapShiftItem swapShiftItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_SwapShiftMatrixColumns)))
            {

                var type = (TermGroup_SwapShiftMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_SwapShiftMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, swapShiftItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, swapShiftItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, swapShiftItem.Date, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.HasShift: return new MatrixField(rowNumber, column.Key, swapShiftItem.HasShift, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.ShiftLength: return new MatrixField(rowNumber, column.Key, swapShiftItem.ShiftLengthInMinutes, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.AcceptorEmployeeNr: return new MatrixField(rowNumber, column.Key, swapShiftItem.AcceptorEmployeeNr, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.AcceptorEmployeeName: return new MatrixField(rowNumber, column.Key, swapShiftItem.AcceptorEmployeeName, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.AcceptorDate: return new MatrixField(rowNumber, column.Key, swapShiftItem.AcceptedDate, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.SwappedToEmployeeNr: return new MatrixField(rowNumber, column.Key, swapShiftItem.SwappedToEmployeeNr, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.SwappedToEmployeeName: return new MatrixField(rowNumber, column.Key, swapShiftItem.SwappedToEmployeeName, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.InitiatorEmployeeNr: return new MatrixField(rowNumber, column.Key, swapShiftItem.InitiatorEmployeeNr, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.InitiatorEmployeeName: return new MatrixField(rowNumber, column.Key, swapShiftItem.InitiatorEmployeeName, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.InitiatorTime: return new MatrixField(rowNumber, column.Key, swapShiftItem.InitiatedDate, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.ApprovedDate: return new MatrixField(rowNumber, column.Key, swapShiftItem.ApprovedDate, column.MatrixDataType);
                    case TermGroup_SwapShiftMatrixColumns.ApprovedBy: return new MatrixField(rowNumber, column.Key, swapShiftItem.ApprovedBy, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
