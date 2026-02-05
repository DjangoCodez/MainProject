using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class StaffingneedsFrequencyMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "StaffingneedsFrequencyMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<StaffingneedsFrequencyReportDataReportDataField> filter { get; set; }
        StaffingneedsFrequencyReportDataOutput _reportDataOutput { get; set; }
        #endregion

        public StaffingneedsFrequencyMatrix(InputMatrix inputMatrix, StaffingneedsFrequencyReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Schedule_StaffingNeeds))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.AccountName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.AccountNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.AccountParentName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.AccountParentNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_StaffingneedsFrequencyMatrixColumns.TimeFrom));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_StaffingneedsFrequencyMatrixColumns.TimeTo));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingneedsFrequencyMatrixColumns.Amount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingneedsFrequencyMatrixColumns.Cost));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.FrequencyType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingneedsFrequencyMatrixColumns.NbrOfCustomers));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingneedsFrequencyMatrixColumns.NbrOfItems));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingneedsFrequencyMatrixColumns.NbrOfMinutes));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.ExternalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingneedsFrequencyMatrixColumns.ParentExternalCode));
            
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_StaffingneedsFrequencyMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var StaffingneedsFrequencyItem in _reportDataOutput.StaffingneedsFrequencyItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, StaffingneedsFrequencyItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, StaffingneedsFrequencyItem StaffingneedsFrequencyItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_StaffingneedsFrequencyMatrixColumns)))
            {
                var type = (TermGroup_StaffingneedsFrequencyMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.Unknown:
                        break;
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.AccountName, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.AccountNumber:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.AccountNumber, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.AccountParentName:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.AccountParentName, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.AccountParentNumber:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.AccountParentNumber, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.TimeFrom:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.TimeFrom, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.TimeTo:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.TimeTo, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.NbrOfCustomers:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.NbrOfCustomers, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.NbrOfMinutes:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.NbrOfMinutes, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.NbrOfItems:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.NbrOfItems, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.Amount:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.Amount, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.Cost:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.Cost, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.FrequencyType:


                        return new MatrixField(rowNumber, column.Key, GetFrequencyTypeName(StaffingneedsFrequencyItem.FrequencyType), column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.ExternalCode:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.ExternalCode, column.MatrixDataType);
                    case TermGroup_StaffingneedsFrequencyMatrixColumns.ParentExternalCode:
                        return new MatrixField(rowNumber, column.Key, StaffingneedsFrequencyItem.ParentExternalCode, column.MatrixDataType);
                    default:
                        return new MatrixField(rowNumber, column.Key, string.Empty, MatrixDataType.String);
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }

        private string GetFrequencyTypeName(FrequencyType frequencyType)
        {
            switch (frequencyType)
            {
                case FrequencyType.Actual:
                    return "Actual";
                case FrequencyType.Budget:
                    return "Budget";
                case FrequencyType.Forecast:
                    return "Forecast";
                default:
                    return "Unknown";
            }
        }
    }
}
