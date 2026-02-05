using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy
{
    public class CustomerReportData : EconomyReportDataManager, IReportDataModel
    {
        private readonly CustomerReportDataOutput _reportDataOutput;

        public CustomerReportData(ParameterObject parameterObject, CustomerReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new CustomerReportDataOutput(reportDataInput);
        }

        public CustomerReportDataOutput CreateOutput(CreateReportResult reportResult)
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

                List<PaymentCondition> PaymentCondions = PaymentManager.GetPaymentConditions(base.ActorCompanyId);

                List<Customer> customers = CustomerManager.GetCustomersByCompany(base.ActorCompanyId, !selectionIncludeInactive);
                foreach (Customer customer in customers)
                {

                    #region Prereq

                    SysCountryDTO sysCountry = (customer.SysCountryId.HasValue) ? CountryCurrencyManager.GetSysCountry(customer.SysCountryId.Value) : null;
                    SysCurrency sysCurrency = customer.CurrencyId > 0 ? CountryCurrencyManager.GetSysCurrencyByCurrency(customer.CurrencyId, true) : null;
                    Contact contact = ContactManager.GetContactFromActor(customer.ActorCustomerId, loadActor: true, loadAllContactInfo: true);

                    #endregion

                    #region ECom

                    string phoneJob = "";
                    string email = "";
                    string web = "";
                    string fax = "";

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
                    string billingAddressStreet = "";
                    string billingAddressCO = "";
                    string billingAddressPostalCode = "";
                    string billingAddressPostalAddress = "";
                    string billingAddressCountry = "";
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
                                #region Billing

                                foreach (ContactAddressRow contactAddressRow in contactAddress.ContactAddressRow)
                                {
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                        billingAddressStreet = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                        billingAddressCO = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                        billingAddressPostalCode = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                        billingAddressPostalAddress = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country)
                                        billingAddressCountry = contactAddressRow.Text;
                                }

                                #endregion
                            }
                        }
                    }

                    #endregion
                    
                    CustomerItem customerItem = new CustomerItem()
                    {
                        CustomerName = customer.Name,
                        CustomerOrgNr = customer.OrgNr,
                        CustomerNr = customer.CustomerNr,
                        CustomerVatNr = customer.VatNr,
                        Country = sysCountry?.Name ?? string.Empty,
                        Currency = sysCurrency?.Name ?? string.Empty,
                        PhoneJob = phoneJob,
                        Email = email,
                        Web = web,
                        Fax = fax,
                        DeliveryAddress = $"{deliveryAddressStreet}, {deliveryAddressPostalCode}, {deliveryAddressPostalAddress}",
                        DistributionAddress = $"{distributionAddressStreet}, {distributionAddressPostalCode}, {distributionAddressPostalAddress}, {distributionAddressCountry}",
                        VisitingAddress = $"{visitingAddressStreet}, {visitingAddressPostalCode}, {visitingAddressPostalAddress}, {visitingAddressCountry} ",
                        BillingAddress = $"{billingAddressStreet}, {billingAddressPostalCode}, {billingAddressPostalAddress}, {billingAddressCountry} ",

                        DiscountMerchandise = customer.DiscountMerchandise,
                        InvoiceReference = customer.InvoiceReference,
                        DisableInvoiceFee = customer.DisableInvoiceFee,
                        InvoiceDeliveryType = customer.InvoiceDeliveryType,
                        ContactGLN = customer.ContactGLNId.HasValue ? ContactManager.GetContactEComText(entities, (int)customer.ContactGLNId) : string.Empty,
                        InvoiceLabel = customer.InvoiceLabel,
                        PaymentCondition = PaymentCondions.FirstOrDefault(a => a.PaymentConditionId == customer.PaymentConditionId)?.Name ?? string.Empty,
                        ImportInvoicesDetailed = customer.ImportInvoicesDetailed,

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
                        BillingAddressStreet = billingAddressStreet,
                        BillingAddressCO = billingAddressCO,
                        BillingAddressPostalCode = billingAddressPostalCode,
                        BillingAddressPostalAddress = billingAddressPostalAddress,
                        BillingAddressCountry = billingAddressCountry,

                        IsActvie = customer.State == (int)SoeEntityState.Active,
                    };

                    int supplierNr;
                    if (Int32.TryParse(customer.SupplierNr, out supplierNr))
                        customerItem.CustomerSupNr = supplierNr;

                    _reportDataOutput.CustomerItems.Add(customerItem);
                }


            }
            return new ActionResult();
        }
    }

    public class CustomerReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_CustomerMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public CustomerReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_CustomerMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_CustomerMatrixColumns.Unknown;
        }
    }

    public class CustomerReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<CustomerReportDataReportDataField> Columns { get; set; }

        public CustomerReportDataInput(CreateReportResult reportResult, List<CustomerReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class CustomerReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<CustomerItem> CustomerItems { get; set; }
        public CustomerReportDataInput Input { get; set; }
        public List<GenericType> VatTypes { get; set; }

        public CustomerReportDataOutput(CustomerReportDataInput input)
        {
            this.CustomerItems = new List<CustomerItem>();
            this.Input = input;
            this.VatTypes = new List<GenericType>();
        }
    }    
}
