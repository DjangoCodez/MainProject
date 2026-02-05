using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class InvoiceMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "InvoiceMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<InvoiceReportDataField> filter { get; set; }
        InvoiceReportDataOutput _reportDataOutput { get; set; }
        #endregion

        public InvoiceMatrix(InputMatrix inputMatrix, InvoiceReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Billing_Invoice_Invoices))
                return possibleColumns;

            var percentageColDef = new MatrixDefinitionColumnOptions();
            percentageColDef.Decimals = 2;

            //Excel - print
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.CustomerNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.CustomerName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.InvoiceNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InvoiceAnalysisMatrixColumns.InvoiceDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InvoiceAnalysisMatrixColumns.DueDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InvoiceAnalysisMatrixColumns.OrderDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InvoiceAnalysisMatrixColumns.DeliveryDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.InvoiceType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.Status));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.ProjectNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.ProjectName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceAnalysisMatrixColumns.AmountExVAT, percentageColDef));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_InvoiceAnalysisMatrixColumns.ToInvoiceExVAT, percentageColDef));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.VATType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.Currency));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.OurReference));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.OriginDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.InvoiceLabel));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.SalesPriceList));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.InvoiceAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.DeliveryAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InvoiceAnalysisMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_InvoiceAnalysisMatrixColumns.Modified));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.ModifiedBy));

            //name - acc dim
            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? int.MaxValue;
            if (nbrOfAccountDims > 0)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName1));
                if (nbrOfAccountDims > 1)
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName2));
                    if (nbrOfAccountDims > 2)
                    {
                        possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName3));
                        if (nbrOfAccountDims > 3)
                        {
                            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName4));
                            if (nbrOfAccountDims > 4)
                            {
                                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName5));
                            }
                        }
                    }
                }
            }

            return possibleColumns;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, InvoiceItem invoiceItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_InvoiceAnalysisMatrixColumns)))
            {
                var type = (TermGroup_InvoiceAnalysisMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_InvoiceAnalysisMatrixColumns.CustomerNumber:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.CustomerNumber, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.CustomerName:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.CustomerName, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.InvoiceNumber:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.InvoiceNumber, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.InvoiceDate:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.InvoiceDate, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.DueDate:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.DueDate, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.OrderDate:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.OrderDate, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.DeliveryDate:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.DeliveryDate, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.InvoiceType:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.InvoiceType, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.Status:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.Status, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.ProjectNumber:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.ProjectNumber, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.ProjectName:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.ProjectName, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.AmountExVAT:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.AmountExVAT, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.ToInvoiceExVAT:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.ToInvoiceExVAT, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.VATType:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.VATType, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.Currency:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.Currency, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName1:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.AccountInternalName2, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName2:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.AccountInternalName3, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName3:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.AccountInternalName4, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName4:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.AccountInternalName5, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.AccountInternalName5:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.AccountInternalName6, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.OurReference:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.OurReference, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.OriginDescription:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.OriginDescription, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.InvoiceLabel:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.InvoiceLabel, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.InvoiceAddress:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.InvoiceAddress, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.DeliveryAddress:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.DeliveryAddress, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.SalesPriceList:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.SalesPriceList, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.Created, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.CreatedBy, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.Changed, column.MatrixDataType);
                    case TermGroup_InvoiceAnalysisMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, invoiceItem.ChangedBy, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_InvoiceAnalysisMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var product in _reportDataOutput.InvoiceItems)
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
    }
}
