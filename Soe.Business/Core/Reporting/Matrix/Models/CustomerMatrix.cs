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
    public class CustomerMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "CustomerMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        public List<CustomerReportDataReportDataField> filter { get; set; }
        CustomerReportDataOutput _reportDataOutput { get; set; }
        private List<GenericType> vattypes { get; set; }
        private Dictionary<int, string> InvoiceDeliveryType { get; set; }
        #endregion

        public CustomerMatrix(InputMatrix inputMatrix, CustomerReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            vattypes = reportDataOutput?.VatTypes;

            InvoiceDeliveryType = base.GetTermGroupDict(TermGroup.InvoiceDeliveryType);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Economy_Customer_Customers_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.CustomerName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.CustomerOrgNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.CustomerNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.CustomerVatNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_CustomerMatrixColumns.IsActive));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_CustomerMatrixColumns.CustomerSupNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.Country));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.Currency));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.PhoneJob));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.Email));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.Web));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.Fax));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_CustomerMatrixColumns.DiscountMerchandise));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.InvoiceReference));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_CustomerMatrixColumns.DisableInvoiceFee));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.InvoiceDeliveryType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.ContactGLN));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.InvoiceLabel));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.PaymentCondition));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_CustomerMatrixColumns.ImportInvoicesDetailed));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DeliveryAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DistributionAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.VisitingAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.BillingAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.BillingAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.BillingAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.BillingAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.BillingAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.BillingAddressCountry));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DeliveryAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DeliveryAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DeliveryAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DeliveryAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DistributionAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DistributionAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DistributionAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DistributionAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.DistributionAddressCountry));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.VisitingAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.VisitingAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.VisitingAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.VisitingAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_CustomerMatrixColumns.VisitingAddressCountry));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_CustomerMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in _reportDataOutput.CustomerItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, CustomerItem CustomerItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_CustomerMatrixColumns)))
            {
                var type = (TermGroup_CustomerMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_CustomerMatrixColumns.CustomerName:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.CustomerName, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.CustomerOrgNr:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.CustomerOrgNr, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.CustomerNr:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.CustomerNr, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.CustomerVatNr:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.CustomerVatNr, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.CustomerSupNr:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.CustomerSupNr, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.Country:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.Country, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.Currency:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.Currency, column.MatrixDataType);

                    case TermGroup_CustomerMatrixColumns.PhoneJob:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.PhoneJob, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.Email:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.Email, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.Web:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.Web, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.Fax:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.Fax, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DeliveryAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DeliveryAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DistributionAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DistributionAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.VisitingAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.VisitingAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.BillingAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.BillingAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DeliveryAddressStreet:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DeliveryAddressStreet, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DeliveryAddressCO:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DeliveryAddressCO, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DeliveryAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DeliveryAddressPostalCode, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DeliveryAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DeliveryAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DistributionAddressStreet:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DistributionAddressStreet, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DistributionAddressCO:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DistributionAddressCO, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DistributionAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DistributionAddressPostalCode, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DistributionAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DistributionAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DistributionAddressCountry:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DistributionAddressCountry, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.VisitingAddressStreet:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.VisitingAddressStreet, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.VisitingAddressCO:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.VisitingAddressCO, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.VisitingAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.VisitingAddressPostalCode, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.VisitingAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.VisitingAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.VisitingAddressCountry:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.VisitingAddressCountry, column.MatrixDataType);

                    case TermGroup_CustomerMatrixColumns.BillingAddressStreet:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.BillingAddressStreet, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.BillingAddressCO:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.BillingAddressCO, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.BillingAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.BillingAddressPostalCode, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.BillingAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.BillingAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.BillingAddressCountry:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.BillingAddressCountry, column.MatrixDataType);

                    case TermGroup_CustomerMatrixColumns.DiscountMerchandise:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DiscountMerchandise, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.InvoiceReference:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.InvoiceReference, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.DisableInvoiceFee:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.DisableInvoiceFee, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.InvoiceDeliveryType:
                        return new MatrixField(rowNumber, column.Key, !CustomerItem.InvoiceDeliveryType.IsNullOrEmpty() ? GetValueFromDict((int)CustomerItem.InvoiceDeliveryType, InvoiceDeliveryType) : string.Empty, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.ContactGLN:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.ContactGLN, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.InvoiceLabel:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.InvoiceLabel, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.PaymentCondition:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.PaymentCondition, column.MatrixDataType);
                    case TermGroup_CustomerMatrixColumns.ImportInvoicesDetailed:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.ImportInvoicesDetailed, column.MatrixDataType);

                    case TermGroup_CustomerMatrixColumns.IsActive:
                        return new MatrixField(rowNumber, column.Key, CustomerItem.IsActvie, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
