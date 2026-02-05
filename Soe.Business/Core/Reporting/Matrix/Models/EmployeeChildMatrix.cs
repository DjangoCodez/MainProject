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
    public class EmployeeChildMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string Prefix = "EmployeeChildMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<EmployeeChildReportDataField> Filter { get; set; }
        List<EmployeeChildItem> EmployeeChildItems { get; set; }
        #endregion

        public EmployeeChildMatrix(InputMatrix inputMatrix, EmployeeChildReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            Filter = reportDataOutput?.Input?.Columns;
            EmployeeChildItems = reportDataOutput?.EmployeeChildItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeChildMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeChildMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeChildMatrixColumns.ChildFirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeChildMatrixColumns.ChildLastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeChildMatrixColumns.ChildDateOfBirth));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeChildMatrixColumns.ChildSingelCustody));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeChildMatrixColumns.AmountOfDays));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeChildMatrixColumns.Openingbalanceuseddays));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeChildMatrixColumns.AmountOfDaysUsed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeChildMatrixColumns.AmountOfDaysLeft));
           
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeChildMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in EmployeeChildItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeChildItem employeeChildItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeChildMatrixColumns)))
            {

                var type = (TermGroup_EmployeeChildMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeChildMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, employeeChildItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, employeeChildItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.ChildFirstName: return new MatrixField(rowNumber, column.Key, employeeChildItem.ChildFirstName, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.ChildLastName: return new MatrixField(rowNumber, column.Key, employeeChildItem.ChildLastName, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.ChildDateOfBirth: return new MatrixField(rowNumber, column.Key, employeeChildItem.ChildDateOfBirth, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.ChildSingelCustody: return new MatrixField(rowNumber, column.Key, employeeChildItem.ChildSingelCustody, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.AmountOfDays: return new MatrixField(rowNumber, column.Key, employeeChildItem.AmountOfDays, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.AmountOfDaysUsed: return new MatrixField(rowNumber, column.Key, employeeChildItem.AmountOfDaysUsed, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.AmountOfDaysLeft: return new MatrixField(rowNumber, column.Key, employeeChildItem.AmountOfDaysLeft, column.MatrixDataType);
                    case TermGroup_EmployeeChildMatrixColumns.Openingbalanceuseddays: return new MatrixField(rowNumber, column.Key, employeeChildItem.Openingbalanceuseddays, column.MatrixDataType);


                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
