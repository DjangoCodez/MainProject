using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class InvoiceProductUnitConvertMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "InvoiceProductUnitConversionMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<InvoiceProductUnitConvertReportDataField> filter { get; set; }
        InvoiceProductUnitConvertReportDataOutput _reportDataOutput { get; set; }

        #endregion

        public InvoiceProductUnitConvertMatrix(InputMatrix inputMatrix, InvoiceProductUnitConvertReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Billing_Product_Products_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductUnitConvertMatrixColumns.ProductNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductUnitConvertMatrixColumns.ProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductUnitConvertMatrixColumns.ProductUnitName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductUnitConvertMatrixColumns.ConvertUnitName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceProductUnitConvertMatrixColumns.ConvertUnitFactor));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductUnitConvertMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_InvoiceProductUnitConvertMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductUnitConvertMatrixColumns.ModifiedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_InvoiceProductUnitConvertMatrixColumns.Modified));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_InvoiceProductUnitConvertMatrixColumns column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), GetText((int)column, EnumUtility.GetName(column)), options);
            return matrixLayoutColumn;
        }

        public MatrixResult GetMatrixResult()
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;

            foreach (var product in _reportDataOutput.Items)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, product));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        static private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, InvoiceProductUnitConvertItem InvoiceProductItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_InvoiceProductUnitConvertMatrixColumns)))
            {
                var type = (TermGroup_InvoiceProductUnitConvertMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.ProductNr:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductNr, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.ProductName:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductName, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.ProductUnitName:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductUnitName, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.ConvertUnitName:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductConvertUnitName, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.ConvertUnitFactor:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ConvertFactor, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.CreatedBy, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.Created, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ModifiedBy, column.MatrixDataType);
                    case TermGroup_InvoiceProductUnitConvertMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.Modified, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
