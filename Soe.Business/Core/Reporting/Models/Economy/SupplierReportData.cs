using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy
{
    public class SupplierReportData : EconomyReportDataManager, IReportDataModel
    {
        private readonly SupplierReportDataOutput _reportDataOutput;
        private readonly SupplierReportDataInput _reportDataInput;

        bool loadVattypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_SupplierMatrixColumns.VatType);

        public SupplierReportData(ParameterObject parameterObject, SupplierReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new SupplierReportDataOutput(reportDataInput);
        }

        public SupplierReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }
        public ActionResult LoadData()
        {
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeInactive, "includeInactive");

            using (CompEntities entities = new CompEntities())
            {
                if (loadVattypes)
                    _reportDataOutput.VatTypes = GetTermGroupContent(TermGroup.InvoiceVatType);

                List<Supplier> suppliers = SupplierManager.GetSuppliersByCompany(base.ActorCompanyId, !selectionIncludeInactive);
                foreach (Supplier supplier in suppliers)
                {
                    #region Prereq

                    SysCountryDTO sysCountry = (supplier.SysCountryId.HasValue) ? CountryCurrencyManager.GetSysCountry(supplier.SysCountryId.Value) : null;
                    SysCurrency sysCurrency = supplier.CurrencyId > 0 ? CountryCurrencyManager.GetSysCurrencyByCurrency(supplier.CurrencyId, true) : null;
                    PaymentCondition paymentCondition = supplier.PaymentConditionId.HasValue ? PaymentManager.GetPaymentCondition(supplier.PaymentConditionId.Value, base.ActorCompanyId) : null;
                    List<PaymentInformationRow> paymentInformationRows = PaymentManager.GetPaymentInformationRowsForActor(supplier.ActorSupplierId);
                    PaymentInformationRow defaultPaymentInformationRow = paymentInformationRows?.FirstOrDefault(i => i.Default);
                    Supplier factoringSupplier = supplier.FactoringSupplierId.HasValue ? suppliers.FirstOrDefault(i => i.ActorSupplierId == supplier.FactoringSupplierId.Value) : null;
                    Contact contact = ContactManager.GetContactFromActor(supplier.ActorSupplierId, loadActor: true, loadAllContactInfo: true);

                    #endregion

                    #region ECom

                    string phoneJob = "";
                    string email = "";
                    string web = "";
                    string fax = "";
                    string bankgiro = "";
                    string plusgiro = "";
                    string cfp = "";
                    string sepa = "";
                    if (paymentInformationRows != null && paymentInformationRows.Count != 0)
                    {
                        foreach (var payInfRow in paymentInformationRows)
                        {
                            if (payInfRow.SysPaymentTypeId == 1)
                                bankgiro = payInfRow.PaymentNr;
                            else if (payInfRow.SysPaymentTypeId == 2)
                                plusgiro = payInfRow.PaymentNr;
                            else if (payInfRow.SysPaymentTypeId == 5)
                                sepa = payInfRow.PaymentNr;
                            else if (payInfRow.SysPaymentTypeId == 4)
                                supplier.BIC = payInfRow.PaymentNr;
                            else if (payInfRow.SysPaymentTypeId == 9)
                                cfp = payInfRow.PaymentNr;
                        }
                    }
                    if (contact != null && contact.ContactECom != null)
                    {
                        foreach (ContactECom contactEComItem in contact.ContactECom)
                        {
                            if (contactEComItem.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob)
                                phoneJob = contactEComItem.Text;
                            if (contactEComItem.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)
                                email = contactEComItem.Text;
                            if (contactEComItem.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Web)
                                web = contactEComItem.Text;
                            if (contactEComItem.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax)
                                fax = contactEComItem.Text;
                        }
                    }

                    #endregion

                    #region Addresses

                    string deliveryAddressStreet = "";
                    string deliveryAddressCO = "";
                    string deliveryAddressPostalCode = "";
                    string deliveryAddressPostalAddress = "";
                    string distributionAddressStreet = "";
                    string distributionAddressCO = "";
                    string distributionAddressPostalCode = "";
                    string distributionAddressPostalAddress = "";
                    string distributionAddressCountry = "";
                    string visitingAddressStreet = "";
                    string visitingAddressCO = "";
                    string visitingAddressPostalCode = "";
                    string visitingAddressPostalAddress = "";
                    string visitingAddressCountry = "";
                    string invoiceAddressStreet = "";
                    string invoiceAddressCO = "";
                    string invoiceAddressPostalCode = "";
                    string invoiceAddressPostalAddress = "";
                    string invoiceAddressCountry = "";

                    List<ContactAddress> contactAddresses = contact != null ? ContactManager.GetContactAddresses(entities, contact.ContactId) : new List<ContactAddress>();
                    if (!contactAddresses.IsNullOrEmpty())
                    {
                        foreach (ContactAddress contactAddress in contactAddresses)
                        {
                            if (contactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery)
                            {
                                #region Delivery

                                foreach (ContactAddressRow contactAddressRow in contactAddress.ContactAddressRow)
                                {
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                        deliveryAddressStreet = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                        deliveryAddressCO = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                        deliveryAddressPostalCode = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                        deliveryAddressPostalAddress = contactAddressRow.Text;
                                }

                                #endregion
                            }
                            else if (contactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Distribution)
                            {
                                #region Distribution

                                foreach (ContactAddressRow contactAddressRow in contactAddress.ContactAddressRow)
                                {
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                        distributionAddressStreet = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                        distributionAddressCO = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                        distributionAddressPostalCode = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                        distributionAddressPostalAddress = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country)
                                        distributionAddressCountry = contactAddressRow.Text;
                                }

                                #endregion
                            }
                            else if (contactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Visiting)
                            {
                                #region Visiting

                                foreach (ContactAddressRow contactAddressRow in contactAddress.ContactAddressRow)
                                {
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                        visitingAddressStreet = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                        visitingAddressCO = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                        visitingAddressPostalCode = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                        visitingAddressPostalAddress = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country)
                                        visitingAddressCountry = contactAddressRow.Text;
                                }

                                #endregion
                            }
                            else if (contactAddress.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing)
                            {
                                #region Invoice
                                foreach (ContactAddressRow contactAddressRow in contactAddress.ContactAddressRow)
                                {
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                        invoiceAddressStreet = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                        invoiceAddressCO = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                        invoiceAddressPostalCode = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                        invoiceAddressPostalAddress = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country)
                                        invoiceAddressCountry = contactAddressRow.Text;
                                }
                                #endregion
                            }
                        }
                    }

                    #endregion

                    #region Supplier element

                    SupplierItem supplierItem = new SupplierItem()
                    {
                        SupplierName = supplier.Name,
                        SupplierNr = supplier.SupplierNr,
                        SupplierOrgNr = StringUtility.NullToEmpty(supplier.OrgNr),
                        SupplierVatNr = StringUtility.NullToEmpty(supplier.VatNr),
                        Country = sysCountry?.Name ?? string.Empty,
                        Currency = sysCurrency?.Name ?? string.Empty,
                        OurCustomerNr = StringUtility.NullToEmpty(supplier.OurCustomerNr),
                        Reference = StringUtility.NullToEmpty(supplier.InvoiceReference),
                        VatType = supplier.VatType,
                        PaymentCondition = paymentCondition?.Name ?? string.Empty,
                        FactoringSupplier = factoringSupplier?.Name ?? string.Empty,
                        BIC = StringUtility.NullToEmpty(supplier.BIC),
                        StopPayment = supplier.BlockPayment,
                        EDISupplier = supplier.IsEDISupplier,
                        DefaultPaymentInformation = defaultPaymentInformationRow?.PaymentNr ?? string.Empty,
                        PhoneJob = phoneJob,
                        Email = email,
                        Web = web,
                        Fax = fax,
                        Bankgiro = bankgiro,
                        Plusgiro = plusgiro,
                        Cfp = cfp,
                        Sepa = sepa,
                        DeliveryAddress = $"{deliveryAddressStreet}, {deliveryAddressPostalCode}, {deliveryAddressPostalAddress}",
                        DistributionAddress = $"{distributionAddressStreet}, {distributionAddressPostalCode}, {distributionAddressPostalAddress}, {distributionAddressCountry}",
                        VisitingAddress = $"{visitingAddressStreet}, {visitingAddressPostalCode}, {visitingAddressPostalAddress}, {visitingAddressCountry} ",
                        InvoiceAddress = $"{invoiceAddressStreet}, {invoiceAddressPostalCode}, {invoiceAddressPostalAddress}, {invoiceAddressCountry}",
                        DeliveryAddressStreet = deliveryAddressStreet,
                        DeliveryAddressCO = deliveryAddressCO,
                        DeliveryAddressPostalCode = deliveryAddressPostalCode,
                        DeliveryAddressPostalAddress = deliveryAddressPostalAddress,
                        DistributionAddressStreet = distributionAddressStreet,
                        DistributionAddressCO = distributionAddressCO,
                        DistributionAddressPostalCode = distributionAddressPostalCode,
                        DistributionAddressPostalAddress = distributionAddressPostalAddress,
                        DistributionAddressCountry = distributionAddressCountry,
                        VisitingAddressStreet = visitingAddressStreet,
                        VisitingAddressCO = visitingAddressCO,
                        VisitingAddressPostalCode = visitingAddressPostalCode,
                        VisitingAddressPostalAddress = visitingAddressPostalAddress,
                        VisitingAddressCountry = visitingAddressCountry,
                        InvoiceAddressStreet = invoiceAddressStreet,
                        InvoiceAddressCO = invoiceAddressCO,
                        InvoiceAddressPostalCode = invoiceAddressPostalCode,
                        InvoiceAddressPostalAddress = invoiceAddressPostalAddress,
                        InvoiceAddressCountry = invoiceAddressCountry,
                        IsActive = supplier.State == (int)SoeEntityState.Active,
                    };

                    #endregion

                    _reportDataOutput.SupplierItems.Add(supplierItem);

                }
            }

            return new ActionResult();
        }
    }

    public class SupplierReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_SupplierMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public SupplierReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_SupplierMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_SupplierMatrixColumns.Unknown;
        }
    }

    public class SupplierReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<SupplierReportDataReportDataField> Columns { get; set; }

        public SupplierReportDataInput(CreateReportResult reportResult, List<SupplierReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class SupplierReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<SupplierItem> SupplierItems { get; set; }
        public SupplierReportDataInput Input { get; set; }
        public List<GenericType> VatTypes { get; set; }

        public SupplierReportDataOutput(SupplierReportDataInput input)
        {
            this.SupplierItems = new List<SupplierItem>();
            this.Input = input;
            this.VatTypes = new List<GenericType>();
        }
    }
}
