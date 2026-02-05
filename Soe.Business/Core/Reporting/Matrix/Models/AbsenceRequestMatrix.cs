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
    public class AbsenceRequestMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "AbsenceRequestMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<AbsenceRequestReportDataField> filter { get; set; }
        List<AbsenceRequestItem> shiftRequestItems { get; set; }
        #endregion

        public AbsenceRequestMatrix(InputMatrix inputMatrix, AbsenceRequestReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            shiftRequestItems = reportDataOutput?.AbsenceRequestItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_AbsenceRequestMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AbsenceRequestMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AbsenceRequestMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AbsenceRequestMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AbsenceRequestMatrixColumns.Creator));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AbsenceRequestMatrixColumns.Modifier));

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
                    // Hidden
                    foreach (MatrixLayoutColumn item in possibleColumns.Where(c => c.IsHidden()))
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_AbsenceRequestMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in shiftRequestItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, AbsenceRequestItem shiftRequestItem)
        {
            if (base.GetEnumId<TermGroup_AbsenceRequestMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_AbsenceRequestMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_AbsenceRequestMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.EmployeeId, column.MatrixDataType);
                    case TermGroup_AbsenceRequestMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_AbsenceRequestMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_AbsenceRequestMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Created, column.MatrixDataType);
                    case TermGroup_AbsenceRequestMatrixColumns.Creator:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Creator, column.MatrixDataType);
                    case TermGroup_AbsenceRequestMatrixColumns.Modifier:
                        return new MatrixField(rowNumber, column.Key, shiftRequestItem.Modifier, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
