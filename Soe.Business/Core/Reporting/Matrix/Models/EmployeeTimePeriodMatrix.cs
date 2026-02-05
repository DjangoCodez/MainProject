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
    public class EmployeeTimePeriodMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeTimePeriodMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeTimePeriodReportDataField> filter { get; set; }
        List<EmployeeTimePeriodItem> employeeTimePeriodItems { get; set; }
        #endregion

        public EmployeeTimePeriodMatrix(InputMatrix inputMatrix, EmployeeTimePeriodReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            employeeTimePeriodItems = reportDataOutput?.EmployeeTimePeriodItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Payroll_Calculation_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeTimePeriodMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeTimePeriodMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeTimePeriodMatrixColumns.SocialSec));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeTimePeriodMatrixColumns.PaymentDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeTimePeriodMatrixColumns.StartDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeTimePeriodMatrixColumns.StopDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeTimePeriodMatrixColumns.PayrollStartDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeTimePeriodMatrixColumns.PayrollStopDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.Tax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.TableTax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.OneTimeTax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.EmploymentTaxCredit));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.SupplementChargeCredit));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.GrossSalary));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.NetSalary));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.VacationCompensation));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.Benefit));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.Compensation));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.Deduction));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.UnionFee));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.OptionalTax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.SINKTax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.ASINKTax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeTimePeriodMatrixColumns.EmploymentTaxBasis));
            
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeTimePeriodMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in employeeTimePeriodItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeTimePeriodItem employeeTimePeriodItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeTimePeriodMatrixColumns)))
            {
                var type = (TermGroup_EmployeeTimePeriodMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeTimePeriodMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.SocialSec: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.SocialSec, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.PaymentDate: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.PaymentDate, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.StartDate: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.StartDate, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.StopDate: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.StopDate, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.PayrollStartDate: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.PayrollStartDate, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.PayrollStopDate: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.PayrollStopDate, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.Tax: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.Tax, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.TableTax: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.TableTax, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.OneTimeTax: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.OneTimeTax, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.EmploymentTaxCredit: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.EmploymentTaxCredit, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.SupplementChargeCredit: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.SupplementChargeCredit, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.GrossSalary: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.GrossSalary, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.NetSalary: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.NetSalary, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.VacationCompensation: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.VacationCompensation, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.Benefit: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.Benefit, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.Compensation: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.Compensation, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.Deduction: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.Deduction, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.UnionFee: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.UnionFee, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.OptionalTax: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.OptionalTax, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.SINKTax: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.SINKTax, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.ASINKTax: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.ASINKTax, column.MatrixDataType);
                    case TermGroup_EmployeeTimePeriodMatrixColumns.EmploymentTaxBasis: return new MatrixField(rowNumber, column.Key, employeeTimePeriodItem.EmploymentTaxBasis, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
