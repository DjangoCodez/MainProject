using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class DepreciationMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "DepreciationMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        public List<DepreciationReportDataReportDataField> filter { get; set; }
        DepreciationReportDataOutput _reportDataOutput { get; set; }
        #endregion

        public DepreciationMatrix(InputMatrix inputMatrix, DepreciationReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Economy_Inventory_WriteOffs))
                return possibleColumns;
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_DepreciationMatrixColumns.InventoryNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_DepreciationMatrixColumns.InventoryName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_DepreciationMatrixColumns.RemainingValue));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_DepreciationMatrixColumns.InventoryStatus));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_DepreciationMatrixColumns.InventoryCategories));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_DepreciationMatrixColumns.TotalDepreciationAmount));

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

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumnsForResult(IEnumerable<PeriodUnitDTO> periodUnits)
        {
            var columns = GetMatrixDefinitionColumns();
         
            var lastColumn = columns.LastOrDefault();
            var lastColumnNumber = lastColumn?.ColumnNumber ?? 0;

            if (periodUnits.IsNullOrEmpty())
                return columns;

			var periodColumns = periodUnits.Select(p => p.Name).ToList();

            foreach (var p in periodColumns)
            {
                var column = new MatrixDefinitionColumn()
                {
                    MatrixDataType = MatrixDataType.Decimal,
                    ColumnNumber = lastColumnNumber,
                    Key = Guid.NewGuid(),
                    Field = p,
                    Title = p,
                };

                columns.Add(column);
            }
            return columns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_DepreciationMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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
            var sampleDepreciation = _reportDataOutput.DepreciationInventories.FirstOrDefault();
            var columns = GetMatrixDefinitionColumnsForResult(sampleDepreciation.PeriodUnits);
			result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = columns };

            #region Create matrix

            int rowNumber = 1;

            foreach (var depreciation in _reportDataOutput.DepreciationInventories)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in columns)
                    fields.Add(CreateField(rowNumber, column, depreciation));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(columns));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, DepreciationInventoryDTO inventory)
        {
            var period = inventory.PeriodUnits.FirstOrDefault(p => p.Name == column.Field);

			if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_DepreciationMatrixColumns)))
            {
                var type = (TermGroup_DepreciationMatrixColumns)id;

				switch (type)
                {
                    case TermGroup_DepreciationMatrixColumns.InventoryNumber:
                        return new MatrixField(rowNumber, column.Key, inventory.InventoryNumber, column.MatrixDataType);
                    case TermGroup_DepreciationMatrixColumns.InventoryName:
                        return new MatrixField(rowNumber, column.Key, inventory.InventoryName, column.MatrixDataType);
                    case TermGroup_DepreciationMatrixColumns.TotalDepreciationAmount:
                        return new MatrixField(rowNumber, column.Key, inventory.TotalDepreciationAmount, column.MatrixDataType);
                    case TermGroup_DepreciationMatrixColumns.InventoryCategories:
                        return new MatrixField(rowNumber, column.Key, inventory.InventoryCategories, column.MatrixDataType);
                    case TermGroup_DepreciationMatrixColumns.InventoryStatus:
						return new MatrixField(rowNumber, column.Key, inventory.InventoryStatusName, column.MatrixDataType);
                    case TermGroup_DepreciationMatrixColumns.RemainingValue:
                        return new MatrixField(rowNumber, column.Key, inventory.RemainingValue, column.MatrixDataType);
					default:
                        break;
                }
            }
            else if (period != null)
            {
                return new MatrixField(rowNumber, column.Key, period?.DepreciationAmount ?? 0, column.MatrixDataType);
            } 

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
