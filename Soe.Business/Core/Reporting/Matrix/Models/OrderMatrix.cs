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
    public class OrderMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "OrderMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<OrderReportDataField> filter { get; set; }
        OrderReportDataOutput _reportDataOutput { get; set; }
        #endregion

        public OrderMatrix(InputMatrix inputMatrix, OrderReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Billing_Product_Products_Edit))
                return possibleColumns;

            if (base.HasReadPermission(Feature.Billing_Product_Products_ShowSalesPrice))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_OrderAnalysisMatrixColumns.AmountExVAT));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_OrderAnalysisMatrixColumns.ToInvoiceExVAT));
            }
           
            //Excel - print
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.CustomerNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.CustomerName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.OrderNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_OrderAnalysisMatrixColumns.OrderDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_OrderAnalysisMatrixColumns.DeliveryDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.ProjectNumber));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.ProjectName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.OurReference));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.SalesPriceList));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.AssignmentType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.ReadyStateMy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.ReadyStateAll));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_OrderAnalysisMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_OrderAnalysisMatrixColumns.Changed));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.ChangedBy));

            //name - acc dim
            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? int.MaxValue;
            if (nbrOfAccountDims > 0)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.AccountInternalName1));
                if (nbrOfAccountDims > 1)
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.AccountInternalName2));
                    if (nbrOfAccountDims > 2)
                    {
                        possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.AccountInternalName3));
                        if (nbrOfAccountDims > 3)
                        {
                            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.AccountInternalName4));
                            if (nbrOfAccountDims > 4)
                            {
                                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrderAnalysisMatrixColumns.AccountInternalName5));
                            }
                        }
                    }
                }
            }

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_OrderAnalysisMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var product in _reportDataOutput.OrderItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, OrderItem OrderItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_OrderAnalysisMatrixColumns)))
            {
                var type = (TermGroup_OrderAnalysisMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_OrderAnalysisMatrixColumns.CustomerNumber:
                        return new MatrixField(rowNumber, column.Key, OrderItem.CustomerNumber, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.CustomerName:
                        return new MatrixField(rowNumber, column.Key, OrderItem.CustomerName, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.OrderNumber:
                        return new MatrixField(rowNumber, column.Key, OrderItem.OrderNumber, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.OrderDate:
                        return new MatrixField(rowNumber, column.Key, OrderItem.OrderDate, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.DeliveryDate:
                        return new MatrixField(rowNumber, column.Key, OrderItem.DeliveryDate, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.PurchaseDate:
                        return new MatrixField(rowNumber, column.Key, OrderItem.PurchaseDate, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.ProjectNumber:
                        return new MatrixField(rowNumber, column.Key, OrderItem.ProjectNumber, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.ProjectName:
                        return new MatrixField(rowNumber, column.Key, OrderItem.ProjectName, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AmountExVAT:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AmountExVAT, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.ToInvoiceExVAT:
                        return new MatrixField(rowNumber, column.Key, OrderItem.ToInvoiceExVAT, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AccountInternalName1:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AccountInternalName2, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AccountInternalName2:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AccountInternalName3, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AccountInternalName3:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AccountInternalName4, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AccountInternalName4:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AccountInternalName5, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AccountInternalName5:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AccountInternalName6, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.OurReference:
                        return new MatrixField(rowNumber, column.Key, OrderItem.OurReference, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.SalesPriceList:
                        return new MatrixField(rowNumber, column.Key, OrderItem.SalesPriceList, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.AssignmentType:
                        return new MatrixField(rowNumber, column.Key, OrderItem.AssignmentType, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.ReadyStateMy:
                        return new MatrixField(rowNumber, column.Key, OrderItem.ReadyStateMy, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.ReadyStateAll:
                        return new MatrixField(rowNumber, column.Key, OrderItem.ReadyStateAll, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, OrderItem.Created, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, OrderItem.CreatedBy, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.Changed:
                        return new MatrixField(rowNumber, column.Key, OrderItem.Changed, column.MatrixDataType);
                    case TermGroup_OrderAnalysisMatrixColumns.ChangedBy:
                        return new MatrixField(rowNumber, column.Key, OrderItem.ChangedBy, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
