using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class ICACustomer : ExportFilesBase
    {
        #region Ctor

        public ICACustomer(ParameterObject parameterObject, CreateReportResult ReportResult) : base(parameterObject, ReportResult) { }

        #endregion

        #region Public methods

        public string CreateFile(int actorCompanyId)
        {
            #region Init

            if (ReportResult == null)
                return null;

            var cm = new CompanyManager(parameterObject);
            var sb = new StringBuilder();

            #endregion

            #region Prereq

            var company = cm.GetCompany(actorCompanyId);
            if (company == null)
                return null;

            var customers = CustomerManager.GetCustomersByCompany(actorCompanyId, true, loadContact: true, loadContactAddresses: true); 
            var defaultCustomerPaymentCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, 0, actorCompanyId, 0);
            var paymentConditions = PaymentManager.GetPaymentConditions(actorCompanyId);

            #endregion

            #region Column headers

            List<string> headerNames = new List<string>();
            headerNames.Add("BMSID");
            headerNames.Add("Butiksnummer");
            headerNames.Add("Butiksnamn");
            headerNames.Add("Kundnummer");
            headerNames.Add("Organisationsnummer");
            headerNames.Add("Personnummer");
            headerNames.Add("Kundnamn");
            headerNames.Add("Övrigt id");
            headerNames.Add("Namn");
            headerNames.Add("Telefon 1");
            headerNames.Add("Telefon 2");
            headerNames.Add("E-post");
            headerNames.Add("C/o");
            headerNames.Add("Adress");
            headerNames.Add("Postnr");
            headerNames.Add("Postort");
            headerNames.Add("Telefon 1");
            headerNames.Add("Telefon 2");
            headerNames.Add("E-post");
            headerNames.Add("Faktureringssätt");
            headerNames.Add("GLN");
            headerNames.Add("Mottagare för epost faktura");
            headerNames.Add("Detaljerad faktura");
            headerNames.Add("Samlingsfaktura");
            headerNames.Add("Faktureringsfrekvens");
            headerNames.Add("Referens");
            headerNames.Add("Kreditgräns");
            headerNames.Add("Utnyttjad kredit");
            headerNames.Add("Betalningsvillkor");
            headerNames.Add("Faktureringsavgift");
            headerNames.Add("Påminnelse dagar");
            headerNames.Add("Privatperson");

            ExcelHelper helper = new ExcelHelper(GetText(48, "Kunder"), headerNames);

            #endregion

            #region Content

            int rowNr = 2;
            foreach (Customer customer in customers)
            {
                if (es.SL_HasActorNrInterval && !StringUtility.IsInIntervalLong(customer.CustomerNr, es.SL_ActorNrFrom, es.SL_ActorNrTo))
                    continue;

                List<object> row = new List<object>();
                row.Add(null); //BMSID
                row.Add(String.Empty); // Butiksnummer
                row.Add(String.Empty); // Butiksnamn
                row.Add(customer.CustomerNr); // Kundnr
                row.Add(!customer.IsPrivatePerson.HasValue || !customer.IsPrivatePerson.Value ? customer.OrgNr : string.Empty); // Orgnr 
                row.Add(customer.IsPrivatePerson.HasValue && customer.IsPrivatePerson.Value ? customer.OrgNr : string.Empty); // Personnr
                row.Add(customer.Name); // Kundnamn
                row.Add(null); // Övrigt id
                row.Add(String.Empty); // Namn
                row.Add(String.Empty); // Telefon 1
                row.Add(String.Empty); // Telefon 2
                row.Add(String.Empty); // Epost

                List<ContactAddressItem> contactAddresses = ContactManager.GetContactAddressItems(customer.ActorCustomerId, ContactAddressItemType.AddressBilling);

                bool addressAdded = false;
                if (contactAddresses != null)
                {
                    ContactAddressItem billingAddress = contactAddresses.FirstOrDefault();

                    if (billingAddress != null)
                    {
                        row.Add(billingAddress.AddressCO?.Replace("\n", ","));
                        row.Add(billingAddress.Address?.Replace("\n", ","));
                        row.Add(billingAddress.PostalCode);
                        row.Add(billingAddress.PostalAddress?.Replace("\n", ","));
                        addressAdded = true;
                    }
                }

                if (!addressAdded)
                {
                    row.Add(String.Empty); // C/o
                    row.Add(String.Empty); // Adress
                    row.Add(String.Empty); // Postr
                    row.Add(String.Empty); // Postadress
                }

                List<ContactECom> eComs = ContactManager.GetContactEComsFromActor(customer.ActorCustomerId, true);

                ContactECom phoneNumber = eComs?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
                if (phoneNumber != null)
                    row.Add(phoneNumber.Text);
                else
                    row.Add(String.Empty); // Telefon 1

                row.Add(String.Empty); // Telefon 2

                ContactECom email = eComs?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
                if (email != null)
                    row.Add(email.Text);
                else
                    row.Add(String.Empty); // Email

                if (customer.InvoiceDeliveryType.HasValue)
                {
                    switch((SoeInvoiceDeliveryType)customer.InvoiceDeliveryType.Value){
                        case SoeInvoiceDeliveryType.Electronic:
                            row.Add(2); // Faktureringssätt
                            break;
                        case SoeInvoiceDeliveryType.Email:
                            row.Add(1); // Faktureringssätt
                            break;
                        default:
                            row.Add(3); // Faktureringssätt
                            break;
                    }
                }
                else
                {
                    row.Add(3); // Faktureringssätt
                }

                if (customer.ContactGLNId.HasValue)
                {
                    ContactECom gln = eComs?.FirstOrDefault(c => c.ContactEComId == customer.ContactGLNId.Value);
                    if (gln != null)
                        row.Add(gln.Text); // GLN
                    else
                        row.Add(String.Empty); // GLN
                }
                else
                {
                    row.Add(String.Empty); // GLN
                }

                if (customer.ContactEComId.HasValue)
                {
                    ContactECom invoiceEmail = eComs?.FirstOrDefault(c => c.ContactEComId == customer.ContactEComId.Value);
                    if(invoiceEmail != null)
                        row.Add(invoiceEmail.Text); // Mottagare för e-post faktura
                    else
                        row.Add(String.Empty); // Mottagare för e-post faktura
                }
                else
                {
                    row.Add(String.Empty); // Mottagare för e-post faktura
                }

                row.Add(1); // Detaljerad faktura
                row.Add(1); // Samlingsfaktura
                row.Add(4); // Faktureringsfrekvens
                row.Add(customer.InvoiceReference); // Referens
                row.Add(customer.CreditLimit); // Kreditgräns
                row.Add(String.Empty); // Utnyttjad kredit

                var paymentConditionSet = false;
                if (customer.PaymentConditionId.HasValue)
                {
                    var paymentCondition = paymentConditions.FirstOrDefault(p => p.PaymentConditionId == customer.PaymentConditionId.Value);
                    if(paymentCondition != null)
                    {
                        row.Add(setPaymentCondition(paymentCondition));
                        paymentConditionSet = true;
                    }
                }

                if (!paymentConditionSet)
                {
                    var defaultPaymenCondition = paymentConditions.FirstOrDefault(p => p.PaymentConditionId == defaultCustomerPaymentCondition);
                    if (defaultPaymenCondition != null)
                        row.Add(setPaymentCondition(defaultPaymenCondition));
                    else
                        row.Add(String.Empty);
                }

                row.Add(String.Empty); // Faktureringsavgift
                row.Add(1); // Påminnelsedagar
                row.Add(customer.IsPrivatePerson.HasValue && customer.IsPrivatePerson.Value ? 1 : 0); // Privatperson

                helper.AddDataRow(rowNr, row);
                rowNr++;
            }

            #endregion

            #region Formatting

            //helper.FormatColumnsAsEditable(new List<string>() { "A", "D", "H", "I", "J" }, true);
            //helper.FormatColumnsAsDate(new List<string>() { "C", "F", "G" }, true);
            //helper.FormatColumnsAsNumber(new List<string>() { "H", "J" }, true);

            helper.FormatRowAsHeader(headerNames.Count);
            helper.AutoFitColumns();

            #endregion

            #region Create file

            byte[] bytes = helper.GetData();
            if (bytes == null || bytes.Length == 0)
                return String.Empty;

            string filename = GetText(48, "Kunder") + ".xlsx";
            string url = GeneralManager.GetUrlForDownload(bytes, filename);

            #endregion

            return url;
        }

        public string CreateFileForSafilo(int actorCompanyId)
        {
            #region Init

            if (ReportResult == null)
                return null;

            var cm = new CompanyManager(parameterObject);
            var sb = new StringBuilder();

            #endregion

            #region Prereq

            var company = cm.GetCompany(actorCompanyId);
            if (company == null)
                return null;

            var customers = CustomerManager.GetCustomersByCompany(actorCompanyId, true, loadContact: true, loadContactAddresses: true);
            var defaultCustomerPaymentCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, 0, actorCompanyId, 0);
            var paymentConditions = PaymentManager.GetPaymentConditions(actorCompanyId);

            #endregion

            #region Content

            foreach (Customer customer in customers)
            {
                if (es.SL_HasActorNrInterval && !StringUtility.IsInIntervalLong(customer.CustomerNr, es.SL_ActorNrFrom, es.SL_ActorNrTo))
                    continue;

                string line = customer.CustomerNr + ";";
                line += customer.Name + ";";

                var billingAddress = ContactManager.GetContactAddressItems(customer.ActorCustomerId, ContactAddressItemType.AddressBilling)?.FirstOrDefault();

                // Billing address
                if (billingAddress != null)
                {
                    line += billingAddress.Address?.Replace("\n", ",") + ";";
                    line += billingAddress.PostalCode + ";";
                    line += billingAddress.PostalAddress?.Replace("\n", ",") + ";";
                }
                else{
                    line += ";;;";
                }

                // Distriktskod ??
                line += ";";

                List<ContactECom> eComs = ContactManager.GetContactEComsFromActor(customer.ActorCustomerId, true);

                // Phone nr
                var phoneNumber = eComs?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
                if (phoneNumber != null)
                {
                    line += phoneNumber.Text;
                }
                else
                {
                    phoneNumber = eComs?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile);
                    if (phoneNumber != null)
                        line += phoneNumber.Text;
                    else
                        line += ";";
                }

                // Fax nr
                var faxNr = eComs?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);
                if (faxNr != null)
                    line += faxNr + ";";
                else
                    line += ";";

                // Org nr
                line += customer.OrgNr + ";";

                // Vat nr
                line += customer.VatNr + ";";

                // Payment condition
                var paymentConditionSet = false;
                if (customer.PaymentConditionId.HasValue)
                {
                    var paymentCondition = paymentConditions.FirstOrDefault(p => p.PaymentConditionId == customer.PaymentConditionId.Value);
                    if (paymentCondition != null)
                    {
                        line += setPaymentCondition(paymentCondition).ToString() + ";";
                        paymentConditionSet = true;
                    }
                }

                if (!paymentConditionSet)
                {
                    var defaultPaymenCondition = paymentConditions.FirstOrDefault(p => p.PaymentConditionId == defaultCustomerPaymentCondition);
                    if (defaultPaymenCondition != null)
                        line += setPaymentCondition(defaultPaymenCondition).ToString() + ";";
                    else
                        line += ";";
                }

                var categories = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customer.ActorCustomerId, actorCompanyId);

                if (categories != null && categories.Count > 0)
                    line += categories.FirstOrDefault().Category.Name + ";";
                else
                    line += ";";

                // Add customer
                sb.AppendLine(line);
            }

            #endregion

            #region Create File

            byte[] bytes = Encoding.Unicode.GetBytes(sb.ToString());
            if (bytes == null || bytes.Length == 0)
                return String.Empty;

            string filename = GetText(48, "Kunder") + ".csv";
            string url = GeneralManager.GetUrlForDownload(bytes, filename);

            #endregion

            return url;
        }

        private int setPaymentCondition(PaymentCondition paymentCondition)
        {
            //20,30,35,40, 45, 60
            if (paymentCondition.Days < 11)
                return 10;
            else if (paymentCondition.Days >= 11 && paymentCondition.Days < 16)
                return 15;
            else if (paymentCondition.Days >= 16 && paymentCondition.Days < 21)
                return 20;
            else if (paymentCondition.Days >= 21 && paymentCondition.Days < 31)
                return 30;
            else if (paymentCondition.Days >= 31 && paymentCondition.Days < 36)
                return 35;
            else if (paymentCondition.Days >= 36 && paymentCondition.Days < 41)
                return 40;
            else if (paymentCondition.Days >= 41 && paymentCondition.Days < 46)
                return 45;
            else
                return 60;
        }

        #endregion
    }
}



