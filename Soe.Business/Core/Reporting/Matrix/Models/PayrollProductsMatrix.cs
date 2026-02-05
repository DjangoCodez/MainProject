using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class PayrollProductsMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "PayrollProductsMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<PayrollProductsReportDataField> Filter { get; set; }

        #endregion

        public PayrollProductsMatrix(InputMatrix inputMatrix, PayrollProductsReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            PayrollProducts = reportDataOutput != null ? reportDataOutput.PayrollProducts : new List<PayrollProductsItem>();
            Filter = reportDataOutput?.Input?.Columns;
        }

        List<PayrollProductsItem> PayrollProducts { get; set; }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_PayrollProductsMatrixColumns.PayrollProductId, new MatrixDefinitionColumnOptions() { Hidden = true }));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Number));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.ShortName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.ExternalNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel1));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel2));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel3));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel4));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_PayrollProductsMatrixColumns.ProductFactor));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.ResultType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.PayrollProductPayed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.ExcludeInWorkTimeSummary));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.AverageCalculated));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.UseInPayroll));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.DontUseFixedAccounting));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.ProductExport));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.IncludeAmountInExport));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Payrollgroup));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.CentroundingType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.CentroundingLevel));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.TaxCalculationType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.PensionCompany));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.TimeUnit));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.QuantityRoundingType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_PayrollProductsMatrixColumns.QuantityRoundingMinutes));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.ChildProduct));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.PrintOnSalaryspecification));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.DontPrintOnSalarySpecificationWhenZeroAmount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.ShowPrintDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.DontIncludeInRetroactivePayroll));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.VacationSalaryPromoted));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.UnionFeePromoted));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.WorkingTimePromoted));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.CalculateSupplementCharge));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_PayrollProductsMatrixColumns.CalculateSicknessSalary));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Payrollpricetypes));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.Payrollpriceformulas));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.AccountingPurchase));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_PayrollProductsMatrixColumns.AccountingPrioName));

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
                    // Hidden
                    foreach (MatrixLayoutColumn item in possibleColumns.Where(c => c.IsHidden()))
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }

                    foreach (var field in Filter.OrderBy(o => o.Sort))
                    {
                        MatrixLayoutColumn item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey && !w.IsHidden());

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_PayrollProductsMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var payrollProduct in PayrollProducts)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, payrollProduct));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, PayrollProductsItem payrollProduct)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_PayrollProductsMatrixColumns)))
            {
                var type = (TermGroup_PayrollProductsMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_PayrollProductsMatrixColumns.PayrollProductId:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.PayrollProductId, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Number:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.Number, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.Name, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ShortName:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ShortName, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ExternalNumber:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ExternalNumber, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ResultType:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ResultType, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ProductFactor:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ProductFactor, column.MatrixDataType);

                    case TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel1:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.syspayrolltypelevel1, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel2:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.syspayrolltypelevel2, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel3:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.syspayrolltypelevel3, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Syspayrolltypelevel4:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.syspayrolltypelevel4, column.MatrixDataType);

                    case TermGroup_PayrollProductsMatrixColumns.PayrollProductPayed:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.PayrollProductPayed, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ExcludeInWorkTimeSummary:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ExcludeInWorkTimeSummary, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.AverageCalculated:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.AverageCalculated, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.UseInPayroll:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.UseInPayroll, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.DontUseFixedAccounting:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.DontUseFixedAccounting, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ProductExport:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ProductExport, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.IncludeAmountInExport:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.IncludeAmountInExport, column.MatrixDataType);

                    case TermGroup_PayrollProductsMatrixColumns.Payrollgroup:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.Payrollgroup, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.CentroundingType:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.CentroundingType, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.CentroundingLevel:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.CentroundingLevel, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.TaxCalculationType:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.TaxCalculationType, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.PensionCompany:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.PensionCompany, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.TimeUnit:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.TimeUnit, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.QuantityRoundingType:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.QuantityRoundingType, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.QuantityRoundingMinutes:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.QuantityRoundingMinutes, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ChildProduct:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ChildProduct, column.MatrixDataType);

                    case TermGroup_PayrollProductsMatrixColumns.PrintOnSalaryspecification:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.PrintOnSalaryspecification, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.DontPrintOnSalarySpecificationWhenZeroAmount:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.DontPrintOnSalarySpecificationWhenZeroAmount, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.ShowPrintDate:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.ShowPrintDate, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.DontIncludeInRetroactivePayroll:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.DontIncludeInRetroactivePayroll, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.VacationSalaryPromoted:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.VacationSalaryPromoted, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.UnionFeePromoted:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.UnionFeePromoted, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.WorkingTimePromoted:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.WorkingTimePromoted, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.CalculateSupplementCharge:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.CalculateSupplementCharge, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.CalculateSicknessSalary:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.CalculateSicknessSalary, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Payrollpricetypes:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.Payrollpricetypes, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.Payrollpriceformulas:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.Payrollpriceformulas, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.AccountingPurchase:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.AccountingPurchase, column.MatrixDataType);
                    case TermGroup_PayrollProductsMatrixColumns.AccountingPrioName:
                        return new MatrixField(rowNumber, column.Key, payrollProduct.AccountingPrioName, column.MatrixDataType);
                   
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
