using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;

namespace SoftOne.EdiAdmin.Business.Senders
{
    class EdiMoel
    {
        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        private string SOP_Connection = "";

        //Moel
        public bool Moel(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, string Connection, DataSet dsMain)
        {
            SOP_Connection = Connection;
            string InputFileName = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
            string ErrorMessage = "";

            string line = "";
            string xml = "";
            StreamReader sr = new StreamReader(InputFolderFileName, Encoding.UTF8);
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Replace("&", "&amp;");
                xml = xml + line + "\r\n";
            }
            sr.Close();
            StreamWriter sw = new StreamWriter(InputFolderFileName, false, Encoding.UTF8);
            sw.Write(xml);
            sw.Flush();
            sw.Close();

            DataSet dsMeddelandeFil = new DataSet();
            try
            {
                dsMeddelandeFil.ReadXml(InputFolderFileName);
            }
            catch
            {
                string MailSubject = "[MO-1] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " innehåller felaktigt Xml-format";
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["Message"] == null || dsMeddelandeFil.Tables["Message"].Columns["MessageType"] == null)
            {
                string MailSubject = "[MO-2] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'Message' eller 'MessageType'"; ;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            ErrorMessage = MoelMeddelande(dsMeddelandeFil, dsStandardMall);

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
                string MailSubject = "[MO-4] Fel vid överföring från grossist - Meddelandefil: " + InputFileName;
                string MailMessage = ErrorMessage;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            try { File.Delete(OutputFolderFileName); }
            catch
            {
                string MailSubject = "[MO-5] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
            }

            return true;
        }

        private string MoelMeddelande(DataSet dsMeddelandeFil, DataSet dsStandardMall)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            if (dsMeddelandeFil.Tables["Message"].Rows[0]["MessageType"].ToString() == "ORDRSP")
                drMessage["MessageType"] = "ORDERBEKR";
            else
                if (dsMeddelandeFil.Tables["Message"].Rows[0]["MessageType"].ToString() == "INVOICE")
                    drMessage["MessageType"] = "INVOICE";
                else
                    return "Felaktig 'MessageType': " + dsMeddelandeFil.Tables["Message"].Rows[0]["MessageType"].ToString();

            if (dsMeddelandeFil.Tables["Message"].Columns["SenderID"] != null)
                drMessage["MessageSenderId"] = dsMeddelandeFil.Tables["Message"].Rows[0]["SenderID"];
            if (dsMeddelandeFil.Tables["Message"].Columns["MessageDate"] != null)
                drMessage["MessageDate"] = dsMeddelandeFil.Tables["Message"].Rows[0]["MessageDate"];

            if (dsMeddelandeFil.Tables["Seller"].Columns["SellerID"] != null)
                drSeller["SellerId"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["SellerID"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["OrganisationNumber"] != null)
                drSeller["SellerOrganisationNumber"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["OrganisationNumber"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["VATNumber"] != null)
                drSeller["SellerVatNumber"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["VATNumber"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Name"] != null)
                drSeller["SellerName"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Name"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Address"] != null)
                drSeller["SellerAddress"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Address"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["PostalCode"] != null)
                drSeller["SellerPostalCode"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["PostalCode"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["City"] != null)
                drSeller["SellerPostalAddress"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["City"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Phone"] != null)
                drSeller["SellerPhone"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Phone"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Fax"] != null)
                drSeller["SellerFax"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Fax"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Reference"] != null)
                drSeller["SellerReference"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Reference"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["ReferencePhone"] != null)
                drSeller["SellerReferencePhone"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["ReferencePhone"];

            if (dsMeddelandeFil.Tables["Buyer"].Columns["BuyerID"] != null)
                drBuyer["BuyerId"] = dsMeddelandeFil.Tables["Message"].Rows[0]["RecieverID"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["OrganisationNumber"] != null)
                drBuyer["BuyerOrganisationNumber"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["OrganisationNumber"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Name"] != null)
                drBuyer["BuyerName"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Name"];
            else
                if (dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    if (dsMeddelandeFil.Tables["Name"].Rows.Count > 0)
                        drBuyer["BuyerName"] = dsMeddelandeFil.Tables["Name"].Rows[0]["Name_Text"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Address"] != null)
                drBuyer["BuyerAddress"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Address"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["PostalCode"] != null)
                drBuyer["BuyerPostalCode"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["PostalCode"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["City"] != null)
                drBuyer["BuyerPostalAddress"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["City"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Reference"] != null)
                drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Reference"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Phone"] != null)
                drBuyer["BuyerPhone"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Phone"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Fax"] != null)
                drBuyer["BuyerFax"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Fax"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["EmailAddress"] != null)
                drBuyer["BuyerEmailAddress"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["EmailAddress"];

            if (dsMeddelandeFil.Tables["Order"] != null)
                if (dsMeddelandeFil.Tables["Order"].Columns["BuyerOrderNumber"] != null)
                    drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["Order"].Rows[0]["BuyerOrderNumber"];

            if (dsMeddelandeFil.Tables["Invoice"] != null)
            {
                if (dsMeddelandeFil.Tables["Invoice"].Columns["InvoiceNumber"] != null)
                    drHead["HeadInvoiceNumber"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InvoiceNumber"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["InvoiceType"] != null)
                    drHead["HeadInvoiceType"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InvoiceType"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["InvoiceDate"] != null)
                    drHead["HeadInvoiceDate"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InvoiceDate"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["DueDate"] != null)
                    drHead["HeadInvoiceDueDate"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["DueDate"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["BuyerOrderNumber"] != null)
                    drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["BuyerOrderNumber"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["PostalGiro"] != null)
                    drHead["HeadPostalGiro"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["PostalGiro"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["BankGiro"] != null)
                    drHead["HeadBankGiro"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["BankGiro"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["Bank"] != null)
                    drHead["HeadBank"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["Bank"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["CurrencyCode"] != null)
                    drHead["HeadCurrencyCode"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["CurrencyCode"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["PaymentCondition"] != null)
                    drHead["HeadPaymentConditionText"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["PaymentCondition"];
                if (dsMeddelandeFil.Tables["Invoice"].Columns["InterestOverduePaymentText"] != null)
                    drHead["HeadInterestPaymentText"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InterestOverduePaymentText"];
            }

            if (dsMeddelandeFil.Tables["Footer"] != null)
            {
                if (dsMeddelandeFil.Tables["Footer"].Columns["GrossAmount"] != null)
                    drHead["HeadInvoiceNetAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["GrossAmount"];
                if (dsMeddelandeFil.Tables["Footer"].Columns["NetAmount"] != null)
                    drHead["HeadInvoiceGrossAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["NetAmount"];
                if (dsMeddelandeFil.Tables["Footer"].Columns["VATBasis"] != null)
                    drHead["HeadVatBasisAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["VATBasis"];
                if (dsMeddelandeFil.Tables["Footer"].Columns["VATAmount"] != null)
                    drHead["HeadVatAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["VATAmount"];
                if (dsMeddelandeFil.Tables["Footer"].Columns["DispatchFee"] != null)
                    drHead["HeadFreightFeeAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["DispatchFee"];
                if (dsMeddelandeFil.Tables["Footer"].Columns["InvoiceFee"] != null)
                    drHead["HeadRemainingFeeAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["InvoiceFee"];
                if (dsMeddelandeFil.Tables["Footer"].Columns["Equalization"] != null)
                    drHead["HeadRoundingAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["Equalization"];
            }

            if (dsMeddelandeFil.Tables["OrderRows"] != null)
            {
                foreach (DataRow rowOrderRows in dsMeddelandeFil.Tables["OrderRows"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["SellerArticleNumber"] != null)
                        drRow["RowSellerArticleNumber"] = rowOrderRows["SellerArticleNumber"];
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["SellerArticleDescription"] != null)
                        drRow["RowSellerArticleDescription1"] = rowOrderRows["SellerArticleDescription"];
                    if (dsMeddelandeFil.Tables["Order"] != null)
                        if (dsMeddelandeFil.Tables["Order"].Columns["BuyerOrderNumber"] != null)
                            drRow["RowBuyerReference"] = dsMeddelandeFil.Tables["Order"].Rows[0]["BuyerOrderNumber"];
                    foreach (DataRow rowDeliveryPlan in dsMeddelandeFil.Tables["DeliveryPlan"].Rows)
                    {
                        if (rowOrderRows["OrderRows_Id"].ToString() == rowDeliveryPlan["OrderRows_Id"].ToString())
                            if (dsMeddelandeFil.Tables["DeliveryPlan"].Columns["Quantity"] != null)
                                drRow["RowQuantity"] = rowDeliveryPlan["Quantity"];
                    }
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["UnitCode"] != null)
                        drRow["RowUnitCode"] = rowOrderRows["UnitCode"];
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["Price"] != null)
                        drRow["RowUnitPrice"] = rowOrderRows["Price"];
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["RowAmount"] != null)
                        drRow["RowNetAmount"] = rowOrderRows["RowAmount"];
                }
            }

            if (dsMeddelandeFil.Tables["InvoiceRows"] != null)
            {
                foreach (DataRow rowInvoiceRows in dsMeddelandeFil.Tables["InvoiceRows"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["SellerArticleNumber"] != null)
                        drRow["RowSellerArticleNumber"] = rowInvoiceRows["SellerArticleNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["SellerArticleDescription"] != null)
                        drRow["RowSellerArticleDescription1"] = rowInvoiceRows["SellerArticleDescription"];
                    if (dsMeddelandeFil.Tables["Invoice"] != null)
                        if (dsMeddelandeFil.Tables["Invoice"].Columns["BuyerOrderNumber"] != null)
                            drRow["RowBuyerReference"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["BuyerOrderNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["Quantity"] != null)
                        drRow["RowQuantity"] = rowInvoiceRows["Quantity"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["UnitCode"] != null)
                        drRow["RowUnitCode"] = rowInvoiceRows["UnitCode"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["NetAmount"] != null)
                        drRow["RowUnitPrice"] = rowInvoiceRows["NetAmount"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["DiscountPercentage"] != null)
                    {
                        drRow["RowDiscountPercent"] = rowInvoiceRows["DiscountPercentage"];
                        drRow["RowDiscountPercent1"] = rowInvoiceRows["DiscountPercentage"];
                    }
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["RowAmount"] != null)
                        drRow["RowNetAmount"] = rowInvoiceRows["RowAmount"];
                }
            }

            return "";
        }

    }
}
