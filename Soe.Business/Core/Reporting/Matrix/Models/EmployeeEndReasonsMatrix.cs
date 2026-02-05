using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class EmployeeEndReasonsMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeEndReasonsMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeEndReasonsReportDataField> filter { get; set; }
        private List<GenericType> endreason { get; set; }
        #endregion

        public EmployeeEndReasonsMatrix(InputMatrix inputMatrix, EmployeeEndReasonsReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeEndReasonsItem>();
            filter = reportDataOutput?.Input?.Columns;
            endreason = reportDataOutput?.EndReason;
        }

        List<EmployeeEndReasonsItem> Employees { get; set; }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.BirthYear));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.Gender));
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.DefaultRole));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.SSYKCode));
            }
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeEndReasonsMatrixColumns.EmploymentDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeEndReasonsMatrixColumns.EndDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.EmploymentTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.EndReason));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.Comment));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeEndReasonsMatrixColumns.CategoryName));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeEndReasonsMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in Employees)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeEndReasonsItem employee)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeEndReasonsMatrixColumns)))
            {
                var type = (TermGroup_EmployeeEndReasonsMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeEndReasonsMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employee.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.BirthYear:
                        return new MatrixField(rowNumber, column.Key, employee.BirthYear, column.MatrixDataType);
                   case TermGroup_EmployeeEndReasonsMatrixColumns.DefaultRole:
                        return new MatrixField(rowNumber, column.Key, employee.DefaultRole, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.SSYKCode:
                        return new MatrixField(rowNumber, column.Key, employee.SSYKCode, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.EmploymentDate:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentDate, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.EndDate:
                        return new MatrixField(rowNumber, column.Key, employee.EndDate, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.EmploymentTypeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.EndReason:
                        return new MatrixField(rowNumber, column.Key, endreason?.FirstOrDefault(f => f.Id == employee.EndReason)?.Name, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.Comment:
                        return new MatrixField(rowNumber, column.Key, employee.Comment, column.MatrixDataType);
                    case TermGroup_EmployeeEndReasonsMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employee.CategoryName, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
