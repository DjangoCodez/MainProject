using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;


namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class InventoryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "InventoryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        public List<InventoryReportDataReportDataField> filter { get; set; }
        InventoryReportDataOutput _reportDataOutput { get; set; }
        //private List<GenericType> vattypes { get; set; }
        //private Dictionary<int, string> InvoiceDeliveryType { get; set; }
        #endregion

        public InventoryMatrix(InputMatrix inputMatrix, InventoryReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            //vattypes = reportDataOutput?.VatTypes;

            //InvoiceDeliveryType = base.GetTermGroupDict(TermGroup.InvoiceDeliveryType);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Economy_Inventory_Inventories_Edit))
                return possibleColumns;
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.InventoryNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.InventoryName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String,TermGroup_InventoryMatrixColumns.InventoryNumberName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String,TermGroup_InventoryMatrixColumns.InventoryStatus));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String,TermGroup_InventoryMatrixColumns.InventoryDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String,TermGroup_InventoryMatrixColumns.InventoryAccount));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InventoryMatrixColumns.AcquisitionDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.AcquisitionValue));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.AcquisitionsForThePeriod));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.DepreciationValue));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.BookValue));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.DepreciationForThePeriod));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.Disposals));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.Scrapped));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_InventoryMatrixColumns.AccumulatedDepreciationTotal));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.DepriciationMethod));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InventoryMatrixColumns.InventoryCategories));
            

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_InventoryMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in _reportDataOutput.InventoryItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, InventoryItem inventoryItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_InventoryMatrixColumns)))
            {
                var type = (TermGroup_InventoryMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_InventoryMatrixColumns.InventoryNumber:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryNumber, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.InventoryName:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryName, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.InventoryNumberName:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryNumberName, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.InventoryStatus:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryStatus, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.InventoryDescription:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryDescription, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.InventoryAccount:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryAccount, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.AcquisitionDate:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.AcquisitionDate, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.AcquisitionValue:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.AcquisitionValue, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.AcquisitionsForThePeriod:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.AcquisitionsForThePeriod, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.DepreciationValue:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.DepreciationValue, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.BookValue:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.BookValue, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.DepreciationForThePeriod:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.DepreciationForThePeriod, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.Disposals:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.Disposals, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.Scrapped:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.Scrapped, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.AccumulatedDepreciationTotal:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.AccumulatedDepreciationTotal, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.DepriciationMethod:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.DepriciationMethod, column.MatrixDataType);
                    case TermGroup_InventoryMatrixColumns.InventoryCategories:
                        return new MatrixField(rowNumber, column.Key, inventoryItem.InventoryCategories, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
