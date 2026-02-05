using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class EmployeeExperienceMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeExperienceMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeExperienceReportDataField> filter { get; set; }
        List<EmployeeExperienceItem> employeeExperienceItems { get; set; }
        #endregion

        public EmployeeExperienceMatrix(InputMatrix inputMatrix, EmployeeExperienceReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            employeeExperienceItems = reportDataOutput?.EmployeeExperienceItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.LastName));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.SSN));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeExperienceMatrixColumns.Age));
            
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeExperienceMatrixColumns.ExperienceIn));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.ExperienceType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeExperienceMatrixColumns.ExperienceTot));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.SalaryType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeExperienceMatrixColumns.SalaryTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeExperienceMatrixColumns.SalaryDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeExperienceMatrixColumns.Salary));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeExperienceMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in employeeExperienceItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeExperienceItem employeeExperienceItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeExperienceMatrixColumns)))
            {
                var type = (TermGroup_EmployeeExperienceMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeExperienceMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.EmployeeName, column.MatrixDataType);
                   
                    case TermGroup_EmployeeExperienceMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.SSN:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.SSN, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.Age:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.Age, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.ExperienceIn:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.ExperienceIn, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.ExperienceTot:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.ExperienceTot, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.ExperienceType:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.ExperienceType, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.SalaryType:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.SalaryType, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.SalaryTypeName:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.SalaryTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.SalaryDate:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.SalaryDate, column.MatrixDataType);
                    case TermGroup_EmployeeExperienceMatrixColumns.Salary:
                        return new MatrixField(rowNumber, column.Key, employeeExperienceItem.Salary, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
