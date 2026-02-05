using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;


namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EmployeePayrollAdditionsMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string Prefix = "EmployeePayrollAdditionsMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<EmployeePayrollAdditionsReportDataField> Filter { get; set; }
        List<EmployeePayrollAdditionsItem> EmployeePayrollAdditionsItems { get; set; }
        #endregion

        public EmployeePayrollAdditionsMatrix(InputMatrix inputMatrix, EmployeePayrollAdditionsReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            Filter = reportDataOutput?.Input?.Columns;
            EmployeePayrollAdditionsItems = reportDataOutput?.EmployeePayrollAdditionsItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeePayrollAdditionsMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeePayrollAdditionsMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeePayrollAdditionsMatrixColumns.Group));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeePayrollAdditionsMatrixColumns.Type));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeePayrollAdditionsMatrixColumns.FromDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeePayrollAdditionsMatrixColumns.ToDate));

            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (DefinitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeePayrollAdditionsMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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
            MatrixResult result = new MatrixResult
            {
                MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() }
            };

            #region Create matrix

            int rowNumber = 1;

            foreach (var employee in EmployeePayrollAdditionsItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeePayrollAdditionsItem employeePayrollAdditionsItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeePayrollAdditionsMatrixColumns)))
            {

                var type = (TermGroup_EmployeePayrollAdditionsMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeePayrollAdditionsMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, employeePayrollAdditionsItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeePayrollAdditionsMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, employeePayrollAdditionsItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeePayrollAdditionsMatrixColumns.Group: return new MatrixField(rowNumber, column.Key, employeePayrollAdditionsItem.Group, column.MatrixDataType);
                    case TermGroup_EmployeePayrollAdditionsMatrixColumns.Type: return new MatrixField(rowNumber, column.Key, employeePayrollAdditionsItem.Type, column.MatrixDataType);
                    case TermGroup_EmployeePayrollAdditionsMatrixColumns.FromDate: return new MatrixField(rowNumber, column.Key, employeePayrollAdditionsItem.FromDate, column.MatrixDataType);
                    case TermGroup_EmployeePayrollAdditionsMatrixColumns.ToDate: return new MatrixField(rowNumber, column.Key, employeePayrollAdditionsItem.ToDate, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
