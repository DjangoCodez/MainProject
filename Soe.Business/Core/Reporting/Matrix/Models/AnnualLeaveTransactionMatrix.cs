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
    public class AnnualLeaveTransactionMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "AnnualLeaveTransactionMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<AnnualLeaveTransactionReportDataField> filter { get; set; }
        AnnualLeaveTransactionReportDataOutput _reportDataOutput { get; set; }
        #endregion

        public AnnualLeaveTransactionMatrix(InputMatrix inputMatrix, AnnualLeaveTransactionReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {           
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AnnualLeaveTransactionMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AnnualLeaveTransactionMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AnnualLeaveTransactionMatrixColumns.Type));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AnnualLeaveTransactionMatrixColumns.DateEarned));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AnnualLeaveTransactionMatrixColumns.DateSpent));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AnnualLeaveTransactionMatrixColumns.YearEarned, new MatrixDefinitionColumnOptions { DateFormatOption = TermGroup_MatrixDateFormatOption.YearFull }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_AnnualLeaveTransactionMatrixColumns.Hours, new MatrixDefinitionColumnOptions { MinutesToDecimal = true, GroupOption = TermGroup_MatrixGroupAggOption.None, ClearZero = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_AnnualLeaveTransactionMatrixColumns.EarnedHours, new MatrixDefinitionColumnOptions { MinutesToDecimal = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualLeaveTransactionMatrixColumns.EarnedDays));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_AnnualLeaveTransactionMatrixColumns.SpentHours, new MatrixDefinitionColumnOptions { MinutesToDecimal = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualLeaveTransactionMatrixColumns.SpentDays));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_AnnualLeaveTransactionMatrixColumns.BalanceHours, new MatrixDefinitionColumnOptions { MinutesToDecimal = true, GroupOption = TermGroup_MatrixGroupAggOption.Max }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualLeaveTransactionMatrixColumns.BalanceDays, new MatrixDefinitionColumnOptions { GroupOption = TermGroup_MatrixGroupAggOption.Max }));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_AnnualLeaveTransactionMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var annualTransactionItem in _reportDataOutput.AnnualLeaveTransactionItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, annualTransactionItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, AnnualLeaveTransactionItem annualLEaveTransactionItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_AnnualLeaveTransactionMatrixColumns)))
            {
                
                var type = (TermGroup_AnnualLeaveTransactionMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.DateEarned: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.DateEarned, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.YearEarned: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.YearEarned, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.DateSpent: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.DateSpent, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.Hours: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.Hours, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.EarnedHours: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.EarnedHours, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.EarnedDays: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.EarnedDays, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.SpentHours: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.SpentHours, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.SpentDays: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.SpentDays, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.BalanceHours: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.BalanceHours, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.BalanceDays: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.BalanceDays, column.MatrixDataType);
                    case TermGroup_AnnualLeaveTransactionMatrixColumns.Type: return new MatrixField(rowNumber, column.Key, annualLEaveTransactionItem.TypeName, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, ""); 
        }
    }
}
