using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.SOPExport
{
    public class SOPCustomerInvoiceExport : SOPExportBase
    {
        private List<SOPInvoiceItem> sopInvoices = new List<SOPInvoiceItem>();
        private Company company;

        public SOPCustomerInvoiceExport(Company company)
        {
            this.company = company;
        }

        public bool AddForExport(CustomerInvoice invoice, Customer customer, SysCurrency invoiceCurrency, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerVisitingAddress, List<ContactECom> customerContactEcoms)
        {
            return Populate(invoice, customer, invoiceCurrency, customerBillingAddress, customerVisitingAddress, customerContactEcoms);
        }

        private bool Populate(CustomerInvoice invoice, Customer customer, SysCurrency invoiceCurrency, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerVisitingAddress, List<ContactECom> customerContactEcoms)
        {
            SOPInvoiceItem sopInvoice = new SOPInvoiceItem();

            if (customerBillingAddress.Any(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId))
                customerBillingAddress = customerBillingAddress.Where(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId).ToList();          

            ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);
            ContactAddressRow customerAddressCO = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.AddressCO);
            ContactAddressRow customerVisitingPostalAddress = customerVisitingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerVisitingAddressStreetName = customerVisitingAddress.GetRow(TermGroup_SysContactAddressRowType.StreetAddress);

            //ContactECom
            ContactECom customerMobile = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile);
            ContactECom customerPhoneWork = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob);
            ContactECom customerPhoneHome = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneHome);
            ContactECom customerFax = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);

            //Invoice data
            sopInvoice.InvoiceType = invoice.IsCredit ? CREDIT_ID : DEBIT_ID;
            sopInvoice.InvoiceNr = invoice.InvoiceNr;
            sopInvoice.CustomerID = customer.CustomerNr;
            sopInvoice.YourReferens = invoice.ReferenceYour;
            sopInvoice.InvoiceDate = invoice.InvoiceDate;
            sopInvoice.InvoiceDueDate = invoice.DueDate;
            sopInvoice.InvoiceAmountIncVat = invoice.TotalAmountCurrency;
            sopInvoice.InvoiceVatAmount = invoice.VATAmount;
            sopInvoice.CurrencyCode = (invoiceCurrency != null && !String.IsNullOrEmpty(invoiceCurrency.Code)) ? invoiceCurrency.Code : String.Empty;
            sopInvoice.CurrencyRate = invoice.CurrencyRate;

            //Customer data
            sopInvoice.CustomerName = customer.Name;
            sopInvoice.CustomerAddress1 = customerAddress != null && !String.IsNullOrEmpty(customerAddress.Text) ? customerAddress.Text : String.Empty;
            sopInvoice.CustomerPostalCode = customerPostalCode != null && !String.IsNullOrEmpty(customerPostalCode.Text) ? customerPostalCode.Text : String.Empty;
            sopInvoice.CustomerPostalAddress = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : String.Empty;
            sopInvoice.CustomerVisitingStreetName = customerVisitingAddressStreetName != null && !String.IsNullOrEmpty(customerVisitingAddressStreetName.Text) ? customerVisitingAddressStreetName.Text : String.Empty;
            sopInvoice.CustomerVisitingAddress = customerVisitingPostalAddress != null && !String.IsNullOrEmpty(customerVisitingPostalAddress.Text) ? customerVisitingPostalAddress.Text : String.Empty;
            sopInvoice.CustomerAddressCO = customerAddressCO != null && !String.IsNullOrEmpty(customerAddressCO.Text) ? customerAddressCO.Text : String.Empty;
            sopInvoice.CustomerPhoneNr1 = customerPhoneWork != null && !String.IsNullOrEmpty(customerPhoneWork.Text) ? customerPhoneWork.Text : String.Empty;
            sopInvoice.CustomerPhoneNr2 = customerMobile != null && !String.IsNullOrEmpty(customerMobile.Text) ? customerMobile.Text : String.Empty;
            sopInvoice.CustomerPhoneNr3 = customerPhoneHome != null && !String.IsNullOrEmpty(customerPhoneHome.Text) ? customerPhoneHome.Text : String.Empty;
            sopInvoice.CustomerFaxNr1 = customerFax != null && !String.IsNullOrEmpty(customerFax.Text) ? customerFax.Text : String.Empty;

            String orgNr = customer.OrgNr.Replace("-", "");
            orgNr = orgNr.Replace(" ", "");
            if (StringUtility.GetLong(orgNr, 0) > 0)
                sopInvoice.CustomerOrgNr = orgNr;

            foreach (var row in invoice.ActiveCustomerInvoiceRows)
            {
                foreach (var accountRow in row.ActiveCustomerInvoiceAccountRows)
                {
                    SOPAccountRow sopAccountRow = new SOPAccountRow();

                    if (accountRow.AccountStd != null && accountRow.AccountStd.Account != null)
                        sopAccountRow.Account = accountRow.AccountStd.Account.AccountNr;

                    sopAccountRow.CostCenter = BusinessExportUtil.GetAccountNr(TermGroup_SieAccountDim.CostCentre, accountRow.AccountInternal.ToList());
                    sopAccountRow.Project = BusinessExportUtil.GetAccountNr(TermGroup_SieAccountDim.Project, accountRow.AccountInternal.ToList());
                    sopAccountRow.Amount = accountRow.Amount;
                    sopAccountRow.Quanity = accountRow.Quantity;
                    sopAccountRow.Text = accountRow.Text;
                    sopAccountRow.IsDebit = accountRow.DebitRow;
                    sopAccountRow.IsCredit = accountRow.CreditRow;

                    sopInvoice.originalAccountRows.Add(sopAccountRow);
                }
            }

            sopInvoice.MergeAccountRows();
            sopInvoices.Add(sopInvoice);

            return true;
        }

        public byte[] GenerateExportFile()
        {
            String fileContent = String.Empty;
            foreach (var sopInvoice in sopInvoices)
            {
                fileContent += sopInvoice.AddToExportFile();
            }
            return Constants.ENCODING_LATIN1.GetBytes(fileContent);
        }
    }

    public class SOPInvoiceItem : SOPExportBase
    {
        private String rowType = "F";
        public String InvoiceType { get; set; }
        public String InvoiceNr { get; set; }
        public String CustomerID { get; set; }
        public String YourReferens { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public decimal InvoiceAmountIncVat { get; set; }
        public decimal InvoiceVatAmount { get; set; }
        public String CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public String CustomerName { get; set; }
        public String CustomerAddress1 { get; set; }
        public String CustomerPostalCode { get; set; }
        public String CustomerPostalAddress { get; set; }
        public String CustomerVisitingStreetName { get; set; }
        public String CustomerVisitingAddress { get; set; }
        public String CustomerAddressCO { get; set; }
        public String CustomerPhoneNr1 { get; set; }
        public String CustomerPhoneNr2 { get; set; }
        public String CustomerPhoneNr3 { get; set; }
        public String CustomerFaxNr1 { get; set; }
        public String CustomerOrgNr { get; set; }

        public List<SOPAccountRow> originalAccountRows = new List<SOPAccountRow>();
        public List<SOPAccountRow> mergedAccountRows = new List<SOPAccountRow>();

        public String AddToExportFile()
        {
            StringBuilder content = new StringBuilder();

            content.Append(rowType);
            content.Append(DELIMITER);

            content.Append(GetValue(InvoiceType, 1));
            content.Append(DELIMITER);

            content.Append(GetValue(InvoiceNr, 6));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerID, 12));
            content.Append(DELIMITER);

            content.Append(GetValue(YourReferens, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(InvoiceDate));
            content.Append(DELIMITER);

            content.Append(GetValue(InvoiceDueDate));
            content.Append(DELIMITER);

            content.Append(GetValue(InvoiceAmountIncVat, 2));
            content.Append(DELIMITER);

            content.Append(GetValue(InvoiceVatAmount, 2));
            content.Append(DELIMITER);

            content.Append(GetValue(CurrencyCode, 3));
            content.Append(DELIMITER);

            content.Append(GetValue(CurrencyRate, 6));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerName, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerAddress1, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerPostalCode, 6));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerPostalAddress, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerVisitingStreetName, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerVisitingAddress, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerAddressCO, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerPhoneNr1, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerPhoneNr2, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerPhoneNr3, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerFaxNr1, 30));
            content.Append(DELIMITER);

            content.Append(GetValue(CustomerOrgNr, 10));
            content.Append(DELIMITER);

            content.Append(Environment.NewLine);
            foreach (var accountRow in mergedAccountRows)
            {
                accountRow.AddToExportFile(ref content);
            }

            return content.ToString();
        }

        public void MergeAccountRows()
        {
            while (originalAccountRows.Count > 0)
            {
                SOPAccountRow firstItem = originalAccountRows.FirstOrDefault();

                //find similar SOPAccountRow to merge
                List<SOPAccountRow> matchingTransactions = (from i in originalAccountRows
                                                            where i.Account == firstItem.Account &&
                                                            i.CostCenter == firstItem.CostCenter &&
                                                            i.Project == firstItem.Project &&
                                                            i.IsCredit == firstItem.IsCredit &&
                                                            i.IsDebit == firstItem.IsDebit &&
                                                            i.Text == firstItem.Text
                                                            select i).ToList();

                SOPAccountRow newItem = new SOPAccountRow()
                {
                    Account = firstItem.Account,
                    CostCenter = firstItem.CostCenter,
                    Project = firstItem.Project,
                    Text = firstItem.Text,
                    IsDebit = firstItem.IsDebit,
                    IsCredit = firstItem.IsCredit,
                    Amount = 0, // will get accumulated
                    Quanity = 0, // will get accumulated
                };

                foreach (var tmpItem in matchingTransactions)
                {
                    newItem.Amount += tmpItem.Amount;
                    newItem.Quanity += tmpItem.Quanity.HasValue ? tmpItem.Quanity.Value : 0;
                }
                matchingTransactions.ForEach(i => originalAccountRows.Remove(i));
                mergedAccountRows.Add(newItem);
            }
        }
    }

    public class SOPAccountRow : SOPExportBase
    {
        private String rowType = "K";
        public String Account { get; set; }
        public String CostCenter { get; set; }
        public String Project { get; set; }
        public decimal Amount { get; set; }
        public decimal? Quanity { get; set; }
        public String Text { get; set; }
        public bool IsDebit { get; set; }
        public bool IsCredit { get; set; }


        public void AddToExportFile(ref StringBuilder content)
        {
            content.Append(rowType);
            content.Append(DELIMITER);

            content.Append(GetValue(Account, 6));
            content.Append(DELIMITER);

            content.Append(GetValue(CostCenter, 6));
            content.Append(DELIMITER);

            content.Append(GetValue(Project, 6));
            content.Append(DELIMITER);

            content.Append(GetValue(Amount, 17));
            content.Append(DELIMITER);

            //content.Append(GetValue(Quanity, 17));
            content.Append(DELIMITER);

            content.Append(GetValue(Text, 25));
            content.Append(DELIMITER);

            content.Append(Environment.NewLine);
        }
    }

    public class SOPExportBase
    {
        public const String DELIMITER = ";";
        public const String DEBIT_ID = "1";
        public const String CREDIT_ID = "2";

        public String GetValue(DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToString(CalendarUtility.SHORTDATEMASK);
            else
                return "";
        }

        public String GetValue(String value, int length)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (value.Length > length)
                    return value.Substring(0, length);
                else
                    return value;
            }
            else
                return "";
        }

        public String GetValue(decimal? value, int nrOfDec)
        {
            if (value.HasValue)
            {
                value = Math.Round(value.Value, nrOfDec);
                return value.ToString().Replace(".", ",");
            }
            else
                return "";
        }
    }

}
