using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;

namespace SoftOne.EdiAdmin.Business.Senders
{
    class EdiElgrossn
    {
        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        private string SOP_Connection = "";

        //Elgross´n
        public bool Elgrossn(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, string Connection, DataSet dsMain, DataRow SenderRow)
        {
            SOP_Connection = Connection;
            string InputFileName = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
            string ErrorMessage = "";

            DataSet dsMeddelandeFil = new DataSet();
            try
            {
                dsMeddelandeFil.ReadXml(InputFolderFileName);
            }
            catch
            {
                string MailSubject = "[EG-1] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " innehåller felaktigt Xml-format";
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["Routing"] == null || dsMeddelandeFil.Tables["Routing"].Columns["DocumentName"] == null)
            {
                string MailSubject = "[EG-2] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'Message' eller 'MessageType'"; ;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            ErrorMessage = ElgrossnMeddelande(dsMeddelandeFil, dsStandardMall, SenderRow);

            string OutputFolderFileName = InputFolderFileName.Replace(".edi", ".xml");
            if (ErrorMessage == "")
            {
                dsStandardMall.WriteXml(OutputFolderFileName, System.Data.XmlWriteMode.IgnoreSchema);
                string UploadFileName = OutputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
                try
                {
                    File.Copy(OutputFolderFileName, OutputFolderFileName.Replace(@WholesaleTempFolder, drEdiSettings["MsgTempFolder"].ToString()), true);
                }
                catch
                {
                    string MailSubject = "[ES-4] Fel vid överföring från grossist - Meddelandefil: " + InputFileName;
                    string MailMessage = "Det gick inte att kopiera filen " + UploadFileName + " till Temporärmappen: " + drEdiSettings["MsgTempFolder"].ToString();
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                    return false;
                }
            }
            else
            {
                string MailSubject = "[EG-4] Fel vid överföring från grossist - Meddelandefil: " + InputFileName;
                string MailMessage = ErrorMessage;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            try
            {
                File.Delete(OutputFolderFileName);
            }
            catch
            {
                string MailSubject = "[EG-5] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
            }

            return true;
        }

        private string ElgrossnMeddelande(DataSet dsMeddelandeFil, DataSet dsStandardMall, DataRow SenderRow)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            if (dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString().ToUpper() == "ORDRSP")
                drMessage["MessageType"] = "ORDERBEKR";
            else
                if (dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString().ToUpper() == "INVOICE")
                    drMessage["MessageType"] = "INVOICE";
                else
                    return "Felaktig 'MessageType': " + dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString();

            if (dsMeddelandeFil.Tables["tbl_InvoiceHead"] != null)
            {
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["ProjectNo"] != null)
                    drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["ProjectNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierOrderNo"] != null)
                    drHead["HeadSellerOrderNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierOrderNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierVatNo"] != null)
                    drMessage["MessageSenderId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceDate"] != null)
                    drMessage["MessageDate"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceDate"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierVatNo"] != null)
                    drSeller["SellerId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierVatNo"] != null &&
                   dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"].ToString().Length > 12)
                    drSeller["SellerOrganisationNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"].ToString().Substring(2, 10);
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierVatNo"] != null)
                    drSeller["SellerVatNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierCompanyName"] != null)
                    drSeller["SellerName"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierCompanyName"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierAddress"] != null)
                    drSeller["SellerAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierAddress"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierPostalCode"] != null)
                    drSeller["SellerPostalCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierPostalCode"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierCity"] != null)
                    drSeller["SellerPostalAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierCity"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierReference"] != null)
                    drSeller["SellerReference"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierReference"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierReferencePhone"] != null)
                    drSeller["SellerReferencePhone"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierReferencePhone"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserIdentification"] != null)
                    drBuyer["BuyerId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserIdentification"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserCompanyName"] != null)
                    drBuyer["BuyerName"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserCompanyName"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserAddress"] != null)
                    drBuyer["BuyerAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserAddress"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserPostalCode"] != null)
                    drBuyer["BuyerPostalCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserPostalCode"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserCity"] != null)
                    drBuyer["BuyerPostalAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserCity"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserReference"] != null)
                    drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserReference"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierInvoiceNo"] != null)
                    drHead["HeadInvoiceNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierInvoiceNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceType"] != null)
                    drHead["HeadInvoiceType"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceType"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceDate"] != null)
                    drHead["HeadInvoiceDate"] = EdiDiverseKlass.Blanka(dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceDate"].ToString());
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceDueDate"] != null)
                    drHead["HeadInvoiceDueDate"] = EdiDiverseKlass.Blanka(dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceDueDate"].ToString());
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["AccountHolder2"] != null)
                    drHead["HeadPostalGiro"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["AccountHolder2"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["AccountHolder1"] != null)
                    drHead["HeadBankGiro"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["AccountHolder1"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PaymentReference"] != null)
                    drHead["HeadInvoiceOcr"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PaymentReference"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["Currency"] != null)
                    drHead["HeadCurrencyCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["Currency"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PaymentCondition"] != null)
                    drHead["HeadPaymentConditionText"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PaymentCondition"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InterestOverduePaymentText"] != null)
                    drHead["HeadInterestPaymentText"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InterestOverduePaymentText"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["NetSum"] != null)
                    drHead["HeadInvoiceNetAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["NetSum"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["TotalSum"] != null)
                    drHead["HeadInvoiceGrossAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["TotalSum"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["VatSum"] != null)
                    drHead["HeadVatAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["VatSum"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["NetAdjustment"] != null)
                    drHead["HeadRoundingAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["NetAdjustment"];
            }

            if (dsMeddelandeFil.Tables["tbl_InvoiceLine"] != null)
            {
                foreach (DataRow rowInvoiceRows in dsMeddelandeFil.Tables["tbl_InvoiceLine"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["ArticleNo"] != null)
                        drRow["RowSellerArticleNumber"] = rowInvoiceRows["ArticleNo"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["ArticleName"] != null)
                        drRow["RowSellerArticleDescription1"] = rowInvoiceRows["ArticleName"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceHead"] != null)
                        if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["ProjectNo"] != null)
                            drRow["RowBuyerReference"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["ProjectNo"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["Quantity"] != null)
                        drRow["RowQuantity"] = rowInvoiceRows["Quantity"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["QuantityMeasurement"] != null)
                        drRow["RowUnitCode"] = rowInvoiceRows["QuantityMeasurement"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["GrossPrice"] != null)
                        drRow["RowUnitPrice"] = rowInvoiceRows["GrossPrice"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["Discount"] != null)
                    {
                        drRow["RowDiscountPercent"] = rowInvoiceRows["Discount"];
                        drRow["RowDiscountPercent1"] = rowInvoiceRows["Discount"];
                    }
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["LineSum"] != null)
                        drRow["RowNetAmount"] = rowInvoiceRows["LineSum"];
                }
            }

            return "";
        }

    }
}
