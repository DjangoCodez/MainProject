using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class SupplierMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "SupplierMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<SupplierReportDataReportDataField> filter { get; set; }
        SupplierReportDataOutput _reportDataOutput { get; set; }
        private List<GenericType> vattypes { get; set; }
        #endregion

        public SupplierMatrix(InputMatrix inputMatrix, SupplierReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            vattypes = reportDataOutput?.VatTypes;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Economy_Supplier_Suppliers_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.SupplierName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.SupplierNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.SupplierOrgNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_SupplierMatrixColumns.IsActive));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.SupplierVatNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Country));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Currency));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.OurCustomerNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Reference));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VatType));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.PaymentCondition));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.FactoringSupplier));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.BIC));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_SupplierMatrixColumns.StopPayment));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_SupplierMatrixColumns.EDISupplier));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DefaultPaymentInformation));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Bankgiro));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Plusgiro));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Cfp));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Sepa));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.PhoneJob));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Email));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Web));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.Fax));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DeliveryAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DistributionAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VisitingAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DeliveryAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DeliveryAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DeliveryAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DeliveryAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DistributionAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DistributionAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DistributionAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DistributionAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.DistributionAddressCountry));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VisitingAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VisitingAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VisitingAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VisitingAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.VisitingAddressCountry));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.InvoiceAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.InvoiceAddressStreet));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.InvoiceAddressCO));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.InvoiceAddressPostalCode));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.InvoiceAddressPostalAddress));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_SupplierMatrixColumns.InvoiceAddressCountry));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_SupplierMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in _reportDataOutput.SupplierItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, SupplierItem SupplierItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_SupplierMatrixColumns)))
            {
                var type = (TermGroup_SupplierMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_SupplierMatrixColumns.SupplierName:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.SupplierName, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.SupplierNr:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.SupplierNr, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.SupplierOrgNr:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.SupplierOrgNr, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.SupplierVatNr:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.SupplierVatNr, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Country:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Country, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Currency:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Currency, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.OurCustomerNr:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.OurCustomerNr, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Reference:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Reference, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VatType:
                        return new MatrixField(rowNumber, column.Key, vattypes?.FirstOrDefault(f => f.Id == SupplierItem.VatType)?.Name, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.PaymentCondition:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.PaymentCondition, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.FactoringSupplier:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.FactoringSupplier, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.BIC:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.BIC, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.StopPayment:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.StopPayment, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.EDISupplier:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.EDISupplier, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DefaultPaymentInformation:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DefaultPaymentInformation, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.PhoneJob:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.PhoneJob, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Email:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Email, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Web:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Web, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Fax:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Fax, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Bankgiro:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Bankgiro, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Plusgiro:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Plusgiro, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Cfp:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Cfp, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.Sepa:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.Sepa, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DeliveryAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DeliveryAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DistributionAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DistributionAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VisitingAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.VisitingAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.InvoiceAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.InvoiceAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DeliveryAddressStreet:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DeliveryAddressStreet, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DeliveryAddressCO:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DeliveryAddressCO, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DeliveryAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DeliveryAddressPostalCode, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DeliveryAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DeliveryAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DistributionAddressStreet:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DistributionAddressStreet, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DistributionAddressCO:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DistributionAddressCO, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DistributionAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DistributionAddressPostalCode, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DistributionAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DistributionAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.DistributionAddressCountry:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.DistributionAddressCountry, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VisitingAddressStreet:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.VisitingAddressStreet, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VisitingAddressCO:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.VisitingAddressCO, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VisitingAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.VisitingAddressPostalCode, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VisitingAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.VisitingAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.VisitingAddressCountry:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.VisitingAddressCountry, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.InvoiceAddressStreet:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.InvoiceAddressStreet, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.InvoiceAddressCO:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.InvoiceAddressCO, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.InvoiceAddressPostalCode:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.InvoiceAddressPostalCode, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.InvoiceAddressPostalAddress:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.InvoiceAddressPostalAddress, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.InvoiceAddressCountry:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.InvoiceAddressCountry, column.MatrixDataType);
                    case TermGroup_SupplierMatrixColumns.IsActive:
                        return new MatrixField(rowNumber, column.Key, SupplierItem.IsActive, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
