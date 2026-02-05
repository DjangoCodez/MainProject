using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SoftOne.Soe.Business.Core.Util.ICA;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class ICACustomerBalance : ExportFilesBase
    {
        #region Ctor

        public ICACustomerBalance(ParameterObject parameterObject, CreateReportResult ReportResult) : base(parameterObject, ReportResult) { }

        #endregion

        #region Public methods

        public string CreateFile(int? actorCompanyId = null, bool myStore = false)
        {
            #region Init

            if (ReportResult == null)
                return null;

            var cm = new CompanyManager(parameterObject);
            var sb = new StringBuilder();

            #endregion

            #region Prereq

            var company = cm.GetCompany(actorCompanyId.HasValue ? actorCompanyId.Value : ReportResult.ActorCompanyId); 

            List<Customer> customers = CustomerManager.GetCustomersByCompany(company.ActorCompanyId, true);

            var invoices = InvoiceManager.GetCustomerInvoicesForGrid(SoeOriginStatusClassification.CustomerInvoicesOpen, (int)SoeOriginType.CustomerInvoice, company.ActorCompanyId, 0, true, false, false, false, TermGroup_ChangeStatusGridAllItemsSelection.All, false);
            invoices = invoices.Where(x => x.Status == 2 || x.Status == 3).ToList();

            #endregion

            #region Content

            foreach (Customer customer in customers)
            {
                if (es.SL_HasActorNrInterval && !StringUtility.IsInIntervalLong(customer.CustomerNr, es.SL_ActorNrFrom, es.SL_ActorNrTo))
                    continue;

                #region Balance Item

                decimal balance = invoices.Where(i => i.ActorCustomerId == customer.ActorCustomerId).Sum(i => i.TotalAmount - i.PaidAmount);

                var balanceItem = new ICACustomerBalanceItem
                {
                    CustomerNr = customer.CustomerNr,
                    Name = customer.Name.Replace("\n", ","),
                    Discount = 0,
                    InLedger = balance,
                    CancellationFlag = false,
                    IsMyStore = myStore,
                };

                #endregion

                #region Billing Address

                List<ContactAddressItem> contactAddresses = ContactManager.GetContactAddressItems(customer.ActorCustomerId, ContactAddressItemType.AddressBilling);

                if (contactAddresses != null)
                {
                    ContactAddressItem billingAddress = contactAddresses.FirstOrDefault();

                    if (billingAddress != null)
                    {
                        balanceItem.Address = billingAddress.Address?.Replace("\n", ",");
                        balanceItem.PostalCode = billingAddress.PostalCode;
                        balanceItem.PostalAddress = billingAddress.PostalAddress?.Replace("\n", ",");
                    }
                }

                #endregion

                #region Phone Number

                List<ContactECom> contactPhoneNumbers = ContactManager.GetContactEComsFromActor(customer.ActorCustomerId, true);

                ContactECom phoneNumber = contactPhoneNumbers?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
                if (phoneNumber != null)
                    balanceItem.PhoneNr = phoneNumber.Text;

                ContactECom faxNumber = contactPhoneNumbers?.FirstOrDefault(c => c.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);
                if (faxNumber != null)
                    balanceItem.FaxNr = faxNumber.Text;

                sb.AppendLine(balanceItem.ToString());

                #endregion
            }

            #endregion

            #region Create File

            var guid = Guid.NewGuid();
            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_ICABALANCE_PHYSICAL + guid.ToString());
            string fileName = "KundSaldo"; // + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".dat";

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.Default);
            }
            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            // Temporary returns file content
            //return sb.ToString();
            return guid.ToString();
        }

        public string CreateFileForSafilo(int? actorCompanyId = null)
        {
            #region Init

            if (ReportResult == null)
                return null;

            var cm = new CompanyManager(parameterObject);
            var sb = new StringBuilder();

            #endregion

            #region Prereq

            var company = cm.GetCompany(actorCompanyId.HasValue ? actorCompanyId.Value : ReportResult.ActorCompanyId);

            List<Customer> customers = CustomerManager.GetCustomersByCompany(company.ActorCompanyId, true);

            var invoices = InvoiceManager.GetCustomerInvoicesForGrid(SoeOriginStatusClassification.CustomerInvoicesOpen, (int)SoeOriginType.CustomerInvoice, company.ActorCompanyId, 0, true, false, false, false, TermGroup_ChangeStatusGridAllItemsSelection.All, false);
            invoices = invoices.Where(x => x.Status == 2 || x.Status == 3).ToList();

            #endregion

            #region Content

            foreach (var invoice in invoices)
            {
                if (es.SL_HasActorNrInterval && !StringUtility.IsInIntervalLong(invoice.ActorCustomerNr, es.SL_ActorNrFrom, es.SL_ActorNrTo))
                    continue;
                
                string line = "1;";
                line += invoice.InvoiceNr + ";";
                line += invoice.ActorCustomerNr + ";";
                line += invoice.InvoiceDate.ToShortDateString() + ";";
                line += invoice.DueDate.ToShortDateString() + ";";
                line += invoice.TotalAmountCurrency + ";";
                line += (invoice.TotalAmountCurrency - invoice.PaidAmountCurrency) + ";";

                sb.AppendLine(line);
            }

            #endregion

            #region Create File

            byte[] bytes = Encoding.Unicode.GetBytes(sb.ToString());
            if (bytes == null || bytes.Length == 0)
                return String.Empty;

            string filename = GetText(1809, "Fakturor") + ".csv";
            string url = GeneralManager.GetUrlForDownload(bytes, filename);

            #endregion

            return url;
        }

        #endregion
    }
}



