using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class ReportStatisticsMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "ReportStatisticsMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<ReportStatisticsReportDataField> Filter { get; set; }
        private ReportStatisticsReportDataOutput ReportDataOutput { get; set; }

        #endregion

        public ReportStatisticsMatrix(InputMatrix inputMatrix, ReportStatisticsReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            Filter = reportDataOutput?.Input?.Columns;
            ReportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ReportStatisticsMatrixColumns.ReportName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ReportStatisticsMatrixColumns.SystemReportName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ReportStatisticsMatrixColumns.AmountPrintOut));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ReportStatisticsMatrixColumns.AverageTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ReportStatisticsMatrixColumns.MedianTime));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_ReportStatisticsMatrixColumns.Period));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ReportStatisticsMatrixColumns.AmountOfUniqueUsers));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_ReportStatisticsMatrixColumns.AmountOfFailed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_ReportStatisticsMatrixColumns.Date));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_ReportStatisticsMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var reportStatisticsItem in ReportDataOutput.ReportStatisticsItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, reportStatisticsItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, ReportStatisticsItem reportStatisticsItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_ReportStatisticsMatrixColumns)))
            {
                var type = (TermGroup_ReportStatisticsMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_ReportStatisticsMatrixColumns.ReportName:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.ReportName, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.SystemReportName:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.SystemReportName, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.AmountPrintOut:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.AmountPrintOut, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.AverageTime:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.AverageTime, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.MedianTime:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.MedianTime, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.Period:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.Period, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.AmountOfUniqueUsers:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.AmountOfUniqueUsers, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.AmountOfFailed:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.AmountOfFailed, column.MatrixDataType);
                    case TermGroup_ReportStatisticsMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, reportStatisticsItem.Date, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
