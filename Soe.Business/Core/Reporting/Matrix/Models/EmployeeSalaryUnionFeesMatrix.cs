using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class EmployeeSalaryUnionFeesMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeSalaryUnionFeesMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<EmployeeSalaryUnionFeesReportDataField> Filter { get; set; }

        #endregion

        public EmployeeSalaryUnionFeesMatrix(InputMatrix inputMatrix, EmployeeSalaryUnionFeesReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeSalaryUnionFeesItem>();
            Filter = reportDataOutput?.Input?.Columns;
        }

        List<EmployeeSalaryUnionFeesItem> Employees { get; set; }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.SSN));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PaymentDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollProductNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.UnitPrice));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Quantity));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Amount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.UnionName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollPriceTypeIdPercentName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollPriceTypeIdPercentCeilingName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollPriceTypeIdFixedAmountName));
            
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeSalaryUnionFeesMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeSalaryUnionFeesItem employee)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeSalaryUnionFeesMatrixColumns)))
            {
                var type = (TermGroup_EmployeeSalaryUnionFeesMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.SSN:
                        return new MatrixField(rowNumber, column.Key, employee.SSN, column.MatrixDataType);

                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.UnionName:
                        return new MatrixField(rowNumber, column.Key, employee.UnionName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollPriceTypeIdPercentName:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollPriceTypeIdPercentName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollPriceTypeIdPercentCeilingName:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollPriceTypeIdPercentCeilingName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollPriceTypeIdFixedAmountName:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollPriceTypeIdFixedAmountName, column.MatrixDataType);

                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PaymentDate:
                        return new MatrixField(rowNumber, column.Key, employee.PaymentDate, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollProductNumber:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollProductNumber, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.PayrollProductName:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollProductName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.UnitPrice:
                        return new MatrixField(rowNumber, column.Key, employee.UnitPrice, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Quantity:
                        return new MatrixField(rowNumber, column.Key, employee.Quantity, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryUnionFeesMatrixColumns.Amount:
                        return new MatrixField(rowNumber, column.Key, employee.Amount, column.MatrixDataType);

                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
