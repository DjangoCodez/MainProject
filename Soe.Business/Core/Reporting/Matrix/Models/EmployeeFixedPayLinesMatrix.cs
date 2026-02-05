using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class EmployeeFixedPayLinesMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeFixedPayLinesMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<EmployeeFixedPayLinesReportDataField> Filter { get; set; }

        #endregion

        public EmployeeFixedPayLinesMatrix(InputMatrix inputMatrix, EmployeeFixedPayLinesReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeFixedPayLinesItem>();
            Filter = reportDataOutput?.Input?.Columns;
        }

        List<EmployeeFixedPayLinesItem> Employees { get; set; }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.BirthYear));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.Gender));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.Position));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.SSYKCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.EmploymentTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeFixedPayLinesMatrixColumns.EmploymentStartDate));
            }
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
            { 
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.PayrollGroup));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.ProductNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeFixedPayLinesMatrixColumns.ProuctName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeFixedPayLinesMatrixColumns.FromDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeFixedPayLinesMatrixColumns.ToDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeFixedPayLinesMatrixColumns.Quantity));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeFixedPayLinesMatrixColumns.IsSpecifiedUnitPrice));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeFixedPayLinesMatrixColumns.Distribute));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeFixedPayLinesMatrixColumns.UnitPrice));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeFixedPayLinesMatrixColumns.VatAmount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeFixedPayLinesMatrixColumns.Amount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeFixedPayLinesMatrixColumns.FromPayrollGroup));
            }

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeFixedPayLinesMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeFixedPayLinesItem employee)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeFixedPayLinesMatrixColumns)))
            {
                var type = (TermGroup_EmployeeFixedPayLinesMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employee.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.BirthYear:
                        return new MatrixField(rowNumber, column.Key, employee.BirthYear, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.Position:
                        return new MatrixField(rowNumber, column.Key, employee.Position, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.SSYKCode:
                        return new MatrixField(rowNumber, column.Key, employee.SSYKCode, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.EmploymentStartDate:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentStartDate, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.EmploymentTypeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.PayrollGroup:
                        return new MatrixField(rowNumber, column.Key, employee.Payrollgroup, column.MatrixDataType);

                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.ProductNr:
                        return new MatrixField(rowNumber, column.Key, employee.ProductNr, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.ProuctName:
                        return new MatrixField(rowNumber, column.Key, employee.ProuctName, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.FromDate:
                        return new MatrixField(rowNumber, column.Key, employee.FromDate, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.ToDate:
                        return new MatrixField(rowNumber, column.Key, employee.ToDate, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.Quantity:
                        return new MatrixField(rowNumber, column.Key, employee.Quantity, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.IsSpecifiedUnitPrice:
                        return new MatrixField(rowNumber, column.Key, employee.IsSpecifiedUnitPrice, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.Distribute:
                        return new MatrixField(rowNumber, column.Key, employee.Distribute, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.UnitPrice:
                        return new MatrixField(rowNumber, column.Key, employee.UnitPrice, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.VatAmount:
                        return new MatrixField(rowNumber, column.Key, employee.VatAmount, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.FromPayrollGroup:
                        return new MatrixField(rowNumber, column.Key, employee.FromPayrollGroup, column.MatrixDataType);
                    case TermGroup_EmployeeFixedPayLinesMatrixColumns.Amount:
                        return new MatrixField(rowNumber, column.Key, employee.Amount, column.MatrixDataType);



                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
