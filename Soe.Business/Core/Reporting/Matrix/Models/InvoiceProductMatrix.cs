using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class InvoiceProductMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "InvoiceProductMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<InvoiceProductReportDataReportDataField> filter { get; set; }
        InvoiceProductReportDataOutput _reportDataOutput { get; set; }
        private List<GenericType> productCalculationTypes { get; set; }
        private List<GenericType> productVatTypes { get; set; }
        public  List<VatCodeDTO> productVatCodes { get; set; }
        public List<ProductUnit> productUnits { get; set; }
        public List<SysHouseholdType> householdDeductionTypes { get; set; }
        public List<ProductGroup> productGroups { get; set; }
        private List<InvoiceProductStatisticsDTO> productStatistics { get; set; }

        #endregion

        public InvoiceProductMatrix(InputMatrix inputMatrix, InvoiceProductReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            productCalculationTypes = reportDataOutput?.ProductCalculationTypes;
            productVatTypes = reportDataOutput?.ProductVatTypes;
            productVatCodes = reportDataOutput?.ProductVatCodes;
            productUnits = reportDataOutput?.ProductUnits;
            householdDeductionTypes = reportDataOutput?.HouseholdDeductionTypes;
            productGroups = reportDataOutput?.ProductGroups;
            productStatistics = reportDataOutput?.ProductStatistics;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Billing_Product_Products_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.Description));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_InvoiceProductMatrixColumns.IsActive));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductGroupCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductGroupName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductCategoryNames));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductEAN));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_InvoiceProductMatrixColumns.IsImported));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.CalculationType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.ProductUnitName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.VatCodeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceProductMatrixColumns.HouseholdDeductionType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceProductMatrixColumns.HouseholdDeductionPercentage));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceProductMatrixColumns.Weight));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceProductMatrixColumns.SalesQuantity));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceProductMatrixColumns.SalesAmount));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_InvoiceProductMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var product in _reportDataOutput.InvoiceProductItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, InvoiceProductItem InvoiceProductItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_InvoiceProductMatrixColumns)))
            {
                var type = (TermGroup_InvoiceProductMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_InvoiceProductMatrixColumns.ProductNr:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductNr, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductName:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductName, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.Description:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductDescription, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.IsActive:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.IsActive, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductGroupName:
                        return new MatrixField(rowNumber, column.Key, productGroups?.FirstOrDefault(f => f.ProductGroupId == InvoiceProductItem.ProductGroupId)?.Name, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductGroupCode:
                        return new MatrixField(rowNumber, column.Key, productGroups?.FirstOrDefault(f => f.ProductGroupId == InvoiceProductItem.ProductGroupId)?.Code, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductCategoryNames:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductCategoryNames, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductEAN:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.ProductEAN, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.IsImported:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.IsImported, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.CalculationType:
                        return new MatrixField(rowNumber, column.Key, productCalculationTypes?.FirstOrDefault(f => f.Id == InvoiceProductItem.CalculationType)?.Name, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductType:
                        return new MatrixField(rowNumber, column.Key, productVatTypes?.FirstOrDefault(f => f.Id == InvoiceProductItem.VatType)?.Name, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.ProductUnitName:
                        return new MatrixField(rowNumber, column.Key, productUnits?.FirstOrDefault(f => f.ProductUnitId == InvoiceProductItem.ProductUnit)?.Name, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.VatCodeName:
                        return new MatrixField(rowNumber, column.Key, productVatCodes?.FirstOrDefault(f => f.VatCodeId == InvoiceProductItem.VatCodeId)?.Name, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.HouseholdDeductionType:
                        return new MatrixField(rowNumber, column.Key, householdDeductionTypes?.FirstOrDefault(f => f.SysHouseholdTypeId == InvoiceProductItem.HouseholdDeductionType)?.Name, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.HouseholdDeductionPercentage:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.HouseholdDeductionPercentage, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.Weight:
                        return new MatrixField(rowNumber, column.Key, InvoiceProductItem.Weight, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.SalesAmount:
                        return new MatrixField(rowNumber, column.Key, productStatistics?.FirstOrDefault(p => p.ProductId == InvoiceProductItem.ProductId)?.SalesAmount ?? 0, column.MatrixDataType);
                    case TermGroup_InvoiceProductMatrixColumns.SalesQuantity:
                        return new MatrixField(rowNumber, column.Key, productStatistics?.FirstOrDefault(p => p.ProductId == InvoiceProductItem.ProductId)?.SalesQuantity ?? 0, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
