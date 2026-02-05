using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class EmployeeSalaryDistressMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeSalaryDistressMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<EmployeeSalaryDistressReportDataField> Filter { get; set; }

        #endregion

        public EmployeeSalaryDistressMatrix(InputMatrix inputMatrix, EmployeeSalaryDistressReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeSalaryDistressItem>();
            Filter = reportDataOutput?.Input?.Columns;
        }

        List<EmployeeSalaryDistressItem> Employees { get; set; }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.Gender));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.SSN));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeSalaryDistressMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeSalaryDistressMatrixColumns.PaymentDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.PayrollProductNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.PayrollProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryDistressMatrixColumns.UnitPrice));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryDistressMatrixColumns.Quantity));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryDistressMatrixColumns.Amount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeSalaryDistressMatrixColumns.ManualAdded));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryDistressMatrixColumns.SalaryDistressResAmount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.CaseNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.SeizureAmountType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryDistressMatrixColumns.SalaryDistressAmount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryDistressMatrixColumns.Absence));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeSalaryDistressMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeSalaryDistressItem employee)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeSalaryDistressMatrixColumns)))
            {
                var type = (TermGroup_EmployeeSalaryDistressMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employee.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.SSN:
                        return new MatrixField(rowNumber, column.Key, employee.SSN, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, employee.Date, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.PaymentDate:
                        return new MatrixField(rowNumber, column.Key, employee.PaymentDate, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.PayrollProductNumber:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollProductNumber, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.PayrollProductName:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollProductName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.UnitPrice:
                        return new MatrixField(rowNumber, column.Key, employee.UnitPrice, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.Quantity:
                        return new MatrixField(rowNumber, column.Key, employee.Quantity, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.Amount:
                        return new MatrixField(rowNumber, column.Key, employee.Amount, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.ManualAdded:
                        return new MatrixField(rowNumber, column.Key, employee.ManualAdded, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.SalaryDistressResAmount:
                        return new MatrixField(rowNumber, column.Key, employee.ReservedAmounts, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.CaseNumber:
                        return new MatrixField(rowNumber, column.Key, employee.CaseNumber, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.SeizureAmountType:
                        return new MatrixField(rowNumber, column.Key, employee.SeizureAmountType, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.SalaryDistressAmount:
                        return new MatrixField(rowNumber, column.Key, employee.SalaryDistressAmount, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryDistressMatrixColumns.Absence:
                        return new MatrixField(rowNumber, column.Key, employee.Absence, column.MatrixDataType);

                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
