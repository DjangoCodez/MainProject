using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class DnBNorExport : NorwegianExportBase
    {
        #region Fields
        private const string DNBNOR_ASSIGN_CLAUSE = "Fordringer etter nærværende faktura er overdratt DnB NOR Finans AS , Postboks 6579, Etterstad, 0601 Oslo, til eiendom og befriende betaling kan kun skje til DNB Factoring. Bankgiro 7032.0516038 Postgiro 0813.2016004 Ved betaling vennligst oppgi fakturanummer og leverandør";
        private readonly decimal interestPercent;
        #endregion

        #region Nodes

        private readonly DnBNorBatch batch = new DnBNorBatch();

        #endregion

        #region Constructor

        public DnBNorExport(Company company, decimal interestPercent) : base(company)
        {
            this.company = company;
            this.interestPercent = interestPercent;
            batch.Invoices = new List<DnBNorInvoice>();
        }

        #endregion Constructor

        #region Public Methods

        public XDocument ToXml()
        {
            #region Prereq

            //LineItemCountNumeric = invoiceLines.Count;

            #endregion
            XNamespace xmlns = "http://www.dnbnorfinans.no/Factoring/2004";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace schemalocation = "http://www.dnbnorfinans.no/Factoring/2004 FACTINV-2-2.XSD";

            XElement rootElement = new XElement(xmlns + "BatchCollection", 
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", schemalocation),
                new XAttribute("Version", "2.2"),
                new XAttribute("DefinedBy", "DnB NOR Finans"));
            XDocument doc = new XDocument( rootElement);
            doc.Declaration = new XDeclaration("1.0", "UTF-8", "yes");
            doc.Declaration.Version = "1.0";
            doc.Declaration.Encoding = "UTF-8";
            doc.Declaration.Standalone = "yes";

            rootElement.Add(new XElement(xmlns + "TransferDateTime", DateTime.Now.ToString("s")));
            batch.ToXml(xmlns, ref rootElement);
      
            return doc;
        }

        public bool Validate(out string errorMsg)
        {
            errorMsg = string.Empty;
           
            //foreach (var item in batch.Invoices)
            //{
            //    if (!item.Validate(ref errorMsg))
            //        return false;

            //    foreach (var rows in item.InvItems)
            //    {
            //        if (!rows.Validate(ref errorMsg))
            //            return false;
            //    }
            //}

            errorMsg = this.errorMessage + errorMsg;

            return true;
        }

        #endregion

        #region Private Methods

        protected override bool Populate(CustomerInvoice invoice, Customer customer, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<PaymentInformationRowDTO> paymentInformations, string kidNr, int? paymentSeqNr = null)
        {
            #region Batch

            batch.BatchNbr = 1;
            batch.ClientId = paymentSeqNr ?? 0;
            batch.ClientName = company.Name;
            batch.BatchCcy = "NOK";
            batch.InvSys = Constants.APPLICATION_NAME;
            batch.InvSysVer = Constants.APPLICATION_VERSION;

            #endregion Batch

            #region Invoice

            DnBNorInvoice dnbInvoice = new DnBNorInvoice();

            dnbInvoice.InvType = invoice.IsCredit ? "CreditNote" : "Invoice";
            dnbInvoice.AssignClause = DNBNOR_ASSIGN_CLAUSE;
          
            #region Debtor

            DnBNorDebtor debtor = new DnBNorDebtor();
           
            ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
            ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
            ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.StreetAddress) ?? customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);
            ContactAddressRow customerCountry = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Country);
            ContactECom customerPhone = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob || i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile);
            ContactECom customerFax = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);

            string postalCode = String.Empty;
            if (customerPostalCode != null && !string.IsNullOrEmpty(customerPostalCode.Text))
                postalCode = customerPostalCode.Text.RemoveWhiteSpace('-', '.');

            var pgNumber = paymentInformations.FirstOrDefault(pi => pi.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG);
            int pgNumberInt = 0;
            if (pgNumber != null)
                int.TryParse(pgNumber.PaymentNr.RemoveWhiteSpace('-', '.'), out pgNumberInt);

            var bgNumber = paymentInformations.FirstOrDefault(pi => pi.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG);
            int bgNumberInt = 0;
            if (bgNumber != null)
                int.TryParse(bgNumber.PaymentNr.RemoveWhiteSpace('-', '.'), out bgNumberInt);

            string country = string.Empty;
            if (customer.SysCountryId > 0)
                country = ((TermGroup_Country)customer.SysCountryId).ToString() + (customerCountry != null ? customerCountry.Text ?? string.Empty : string.Empty);

            string customerNr = customer.CustomerNr;
            if (customerNr.Length > 17)
                customerNr = customerNr.Substring(0, 17);
            string customerName = customer.Name.Replace("\x0D0A", " ");
            customerName = customerName.Replace("\x0D", " ");
            customerName = customerName.Replace("\x0A", " ");
            if (customerName.Length > 35)
                customerName = customerName.Substring(0, 35);

            debtor = new DnBNorDebtor()
            {
                DebtorCity = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : String.Empty,
                DebtorCtryCode = customerCountry != null && !string.IsNullOrEmpty(customerCountry.Text) ? customerCountry.Text : "NO",
                DebtorPostalAddr = customerAddressStreetName != null && !string.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : string.Empty,
                DebtorName = customerName,
                DebtorVATNbr = !String.IsNullOrEmpty(customer.VatNr) ? customer.VatNr : "N/A",
                DebtorPhone = customerPhone?.Text ?? string.Empty,
                DebtorFax = customerFax?.Text ?? string.Empty,
                ClientDebtorNbr = customerNr,
            };

            if (!customer.ActorReference.IsLoaded)
                customer.ActorReference.Load();

            if (!String.IsNullOrEmpty(postalCode))
                debtor.DebtorPostalCode = postalCode;

            // Add customer
            dnbInvoice.InvDebtor = debtor;
           
            #endregion

            #region DeliveryDetails

            DnBNorDeliveryDetails dnbDeliveryDetails = new DnBNorDeliveryDetails();
            ContactManager contactManager = new ContactManager(null);
            dnbDeliveryDetails.DeliveryName = customerDeliveryAddress.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Name, invoice.DeliveryAddressId);
            dnbDeliveryDetails.DeliveryAddr = customerDeliveryAddress.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.Address, invoice.DeliveryAddressId);
            dnbDeliveryDetails.DeliveryPostalCode = customerDeliveryAddress.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalCode, invoice.DeliveryAddressId);
            dnbDeliveryDetails.DeliveryCity = customerDeliveryAddress.GetContactAddressRowText(TermGroup_SysContactAddressType.Delivery, TermGroup_SysContactAddressRowType.PostalAddress, invoice.DeliveryAddressId);

            dnbInvoice.InvDeliveryDetails = dnbDeliveryDetails;
            #endregion DeliveryDetails

            #region InvoiceHeader

            DnBNorInvoiceHeader dnbHeader = new DnBNorInvoiceHeader();

            dnbHeader.InvNbr = invoice.SeqNr.ToString();
            dnbHeader.InvDate = invoice.InvoiceDate.Value.ToString("yyyy-MM-dd");
            dnbHeader.DueDate = invoice.DueDate.Value.ToString("yyyy-MM-dd");
            dnbHeader.SellerRef = invoice.ReferenceOur;
            dnbHeader.Kid = kidNr;
            DnBNorPmtTerms pmtTerms = new DnBNorPmtTerms();
            pmtTerms.IntRate = interestPercent;
            pmtTerms.PmtTermsText = invoice.PaymentCondition.Name;
            dnbHeader.PmtTerms = pmtTerms;
            dnbInvoice.InvHeader = dnbHeader;
          
            #endregion InvoiceHeader
            
            #region Items
           
            foreach (var invoiceRow in invoice.CustomerInvoiceRow)
            {
                DnBNorItem dnbItem = new DnBNorItem();
                if (invoiceRow.Type == 2 || invoiceRow.Type == 4)
                {
                    dnbItem.ItemId = invoiceRow.Product.Number;
                    dnbItem.ItemDescr = invoiceRow.Product.Name;
                    dnbItem.NbrOfUnits = invoiceRow.Quantity.HasValue ? invoiceRow.Quantity.Value : 0;
                    dnbItem.Unit = invoiceRow.ProductUnit.Code;
                    dnbItem.UnitPrice = invoiceRow.Amount;
                    dnbItem.DiscPercent = invoiceRow.DiscountPercent;
                    dnbItem.VatPct = invoiceRow.VatRate;
                    dnbItem.Amt = invoiceRow.SumAmount;

                    dnbInvoice.InvItems.Add(dnbItem);
                }
            }

            batch.Invoices.Add(dnbInvoice);
            
            #endregion
            

            #region Total
            DnBNorTotal dnbTotal = new DnBNorTotal();
            dnbTotal.NetAmt = invoice.SumAmount;
            dnbTotal.TotalFreight = invoice.FreightAmount;
            dnbTotal.VATAmt = invoice.VATAmount;
            dnbTotal.TotalAmt = invoice.TotalAmount;
            dnbInvoice.InvTotal = dnbTotal;
            if (invoice.IsCredit)
                batch.TotalBatchAmtCrN += invoice.TotalAmount;
            else
                batch.TotalBatchAmtInv += invoice.TotalAmount;    
            #endregion Total

            #endregion

            return true;
        }

        #endregion

        #region Inner Classes

        #region DnBNorBatch

        internal class DnBNorBatch
        {
 
            #region Properties

            public int ClientId { get; set; }
            public string ClientName { get; set; }
            public DateTime BatchDate { get; set; }
            public int BatchNbr { get; set; }
            public string BatchCcy { get; set; }
            public string InvSys { get; set; }
            public string InvSysVer { get; set; }
            public List<DnBNorInvoice> Invoices { get; set; }
            public decimal TotalBatchAmtInv { get; set; }
            public decimal TotalBatchAmtCrN { get; set; }
   
            #endregion

            #region Constructor

            public DnBNorBatch()
            {
                BatchDate = DateTime.Now;
                Invoices = new List<DnBNorInvoice>(); 
            }
 
#endregion

            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement batchElement = new XElement(xmlns + "Batch");
                batchElement.Add(
                                 new XElement(xmlns + "ClientId", ClientId.ToString()),
                                 new XElement(xmlns + "ClientName", ClientName),
                                 new XElement(xmlns + "BatchDate", BatchDate.ToString("yyyy-MM-dd")),
                                 new XElement(xmlns + "BatchNbr", BatchNbr.ToString()),
                                 new XElement(xmlns + "BatchCcy", BatchCcy),
                                 new XElement(xmlns + "InvSys", InvSys),
                                 new XElement(xmlns + "InvSysVer", InvSysVer)
                                );
                foreach (var item in this.Invoices)
                {
                    item.ToXml(xmlns, ref batchElement);
                }
                batchElement.Add(
                    new XElement(xmlns + "TotalBatchAmtInv", TotalBatchAmtInv),
                    new XElement(xmlns + "TotalBatchAmtCrN", TotalBatchAmtCrN)
                    );
                root.Add(batchElement);
            }
          
        }

        #endregion

        

        #region DnBNorInvoice
        internal class DnBNorInvoice 
        {
        
            #region Properties

            public string InvType { get; set; }
            public DnBNorDebtor InvDebtor { get; set; }
            public DnBNorDeliveryDetails InvDeliveryDetails { get; set; }
            public DnBNorInvoiceHeader InvHeader { get; set; }
            public List<DnBNorItem> InvItems { get; set; }
            public DnBNorTotal InvTotal { get; set; }
            public string AssignClause { get; set; }
            
            #endregion

            #region Constructor

            public DnBNorInvoice()
            {
                InvDebtor = new DnBNorDebtor();
                InvDeliveryDetails = new DnBNorDeliveryDetails();
                InvHeader = new DnBNorInvoiceHeader();
                InvItems = new List<DnBNorItem>();
                InvTotal = new DnBNorTotal();
            }

            #endregion
            
            internal void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement invoiceElement = new XElement(xmlns + "Invoice");
                invoiceElement.Add(new XElement(xmlns + "InvType", InvType));
                InvDebtor.ToXml(xmlns, ref invoiceElement);
                if (!String.IsNullOrEmpty(InvDeliveryDetails.DeliveryName))
                    InvDeliveryDetails.ToXml(xmlns, ref invoiceElement);
                InvHeader.ToXml(xmlns, ref invoiceElement);
                XElement itemsElement = new XElement(xmlns + "Items");
                invoiceElement.Add(itemsElement);
                foreach(var item in InvItems)
                {
                    item.ToXml(xmlns, ref itemsElement);
                }
                InvTotal.ToXml(xmlns, ref invoiceElement);
               
                invoiceElement.Add(new XElement(xmlns + "AssignClause", AssignClause));
                root.Add(invoiceElement);
            }
           
        }
        #endregion

        #region DnBNorDebtor
        internal class DnBNorDebtor 
        {
          
            #region Properties

            public string ClientDebtorNbr { get; set; }
            public string DebtorName { get ; set; }
            public string DebtorVATNbr { get; set; }
            public string DebtorPostalAddr { get; set; }
            public string DebtorSuplAddr { get; set; }
            public string DebtorPostalCode { get; set; }
            public string DebtorCity { get; set; }
            public string DebtorCtryCode { get; set; }
            public string DebtorPhone { get; set; }
            public string DebtorFax { get; set; }

            #endregion

         
            #region Constructor

            public DnBNorDebtor()
            {
                
            }

            #endregion

           
            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement debtorElement = new XElement(xmlns + "Debtor");
                debtorElement.Add(
                    new XElement(xmlns + "DebtorName", DebtorName),
                    new XElement(xmlns + "ClientDebtorNbr", ClientDebtorNbr),
                    new XElement(xmlns + "DebtorVATNbr", DebtorVATNbr),
                    new XElement(xmlns + "DebtorPostalAddr", DebtorPostalAddr),
                    new XElement(xmlns + "DebtorSuplAddr", DebtorSuplAddr),
                    new XElement(xmlns + "DebtorPostalCode", DebtorPostalCode),
                    new XElement(xmlns + "DebtorCity", DebtorCity),
                    new XElement(xmlns + "DebtorCtryCode", DebtorCtryCode),
                    new XElement(xmlns + "DebtorPhone", DebtorPhone),
                    new XElement(xmlns + "DebtorFax", DebtorFax)
                    );
                root.Add(debtorElement);

            }
        }
        #endregion

        #region DnBNorDeliveryDetails
        internal class DnBNorDeliveryDetails 
        {
 
            #region Properties

            public string DeliveryName { get; set; }
            public string DeliveryAddr { get; set; }
            public string DeliveryPostalCode { get; set; }
            public string DeliveryCity { get; set; }
            public string DeliveryCtryCode { get; set; }

            #endregion

            #region Constructor

            public DnBNorDeliveryDetails()
            {

            }

            #endregion

            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement deliveryElement = new XElement(xmlns + "DeliveryDetails");
                deliveryElement.Add(
                    new XElement(xmlns + "DeliveryName", DeliveryName),
                    new XElement(xmlns + "DeliveryAddr", DeliveryAddr),
                    new XElement(xmlns + "DeliveryPostalCode", DeliveryPostalCode),
                    new XElement(xmlns + "DeliveryCity", DeliveryCity),
                    new XElement(xmlns + "DeliveryCtryCode", DeliveryCtryCode)
                    );
                root.Add(deliveryElement);

            }
        }
        #endregion

        #region DnBNorInvoiceHeader
        internal class DnBNorInvoiceHeader 
        {
 

            #region Properties
  

            public string InvNbr { get; set; }
            public string InvDate { get; set; }
            public string DueDate { get; set; }
            public DnBNorPmtTerms PmtTerms { get; set; }
            public string SellerRef { get; set; }
            public string Kid { get; set; }
            #endregion

            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement headerElement = new XElement(xmlns + "InvoiceHeader");
                headerElement.Add(
                    new XElement(xmlns + "InvNbr", InvNbr),
                    new XElement(xmlns + "InvDate", InvDate),
                    new XElement(xmlns + "DueDate", DueDate)
                    );
                PmtTerms.ToXml(xmlns, ref headerElement);
                headerElement.Add(
                 new XElement(xmlns + "SellerRef", SellerRef),
                 new XElement(xmlns + "Kid", Kid)
               
                 );
                root.Add(headerElement);

            }
        }
        #endregion
   
        #region DnBNorPmtTerms
        internal class DnBNorPmtTerms 
        {
            #region Properties

            public decimal IntRate { get; set; }
            public string PmtTermsText { get; set; }

            #endregion

            #region Constructor

            public DnBNorPmtTerms()
            {

            }

            #endregion

            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement debtorElement = new XElement(xmlns + "PmtTerms");
                debtorElement.Add(
                    new XElement(xmlns + "IntRate", IntRate.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "PmtTermsText", PmtTermsText)
                    );
                root.Add(debtorElement);

            }
        }
        #endregion

        #region DnBNorItem


        internal class DnBNorItem 
        {
 
            #region Properties

            public string ItemId { get; set; }
            public string ItemDescr { get; set; }
            public decimal NbrOfUnits { get; set; }
            public string Unit { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal DiscPercent { get; set; }
            public decimal Amt { get; set; }
            public decimal VatPct { get; set; }

            #endregion

            #region Constructor

            public DnBNorItem()
            {

            }

            #endregion
            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement itemElement = new XElement(xmlns + "Item");
                itemElement.Add(
                    new XElement(xmlns + "ItemId", ItemId),
                    new XElement(xmlns + "ItemDescr", ItemDescr),
                    new XElement(xmlns + "NbrOfUnits", NbrOfUnits.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "Unit", Unit),
                    new XElement(xmlns + "UnitPrice", UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "DiscPercent", DiscPercent.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "Amt", Amt.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "VATPct", VatPct.ToString("0.00", CultureInfo.InvariantCulture))
                    );
                root.Add(itemElement);

            }
        }
        #endregion

        #region DnBNorTotal


        internal class DnBNorTotal 
        {
            #region Properties

        

            public decimal NetAmt { get; set; }
            public decimal TotalDiscount { get; set; }
            public decimal TotalFreight { get; set; }
            public decimal VATAmt { get; set; }
            public decimal TotalAmt { get; set; }

 

            #endregion

            #region Constructor

            public DnBNorTotal()
            {

            }

            #endregion

            public void ToXml(XNamespace xmlns, ref XElement root)
            {
                XElement totalElement = new XElement(xmlns + "Total");
                totalElement.Add(
                    new XElement(xmlns + "NetAmt", NetAmt.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "TotalDiscount", TotalDiscount.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "TotalFreight", TotalFreight.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "VATAmt", VATAmt.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(xmlns + "TotalAmt", TotalAmt.ToString("0.00", CultureInfo.InvariantCulture))
                    );
                root.Add(totalElement);

            }
        }
        #endregion

  

        #endregion
    }
}
