using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.EdiAdmin.Business.Interfaces;

namespace SoftOne.EdiAdmin.Business.Senders
{
    public class EdiSelga : EdiSenderOldBase
    {
        private bool doFileOperations = false;
        List<string> parsedMessages = new List<string>();

        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        //Selga and Storel
        public bool Selga(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, DataRow SenderRow, string fileContent = null)
        {
            string InputFileName; // = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
            string ErrorMessage = "";
            var streamReader = EdiDiverse.GetStreamReaderFromContentOrFile(InputFolderFileName, WholesaleTempFolder, fileContent, out this.doFileOperations, out InputFileName);

            DataSet dsMeddelandeFil = new DataSet();
            try
            {
                dsMeddelandeFil.ReadXml(streamReader);
            }
            catch
            {
                string MailSubject = "[SE-1] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " innehåller felaktigt Xml-format";
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["Routing"] == null || dsMeddelandeFil.Tables["Routing"].Columns["DocumentName"] == null)
            {
                string MailSubject = "[SE-2] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'Message' eller 'MessageType'";
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                return false;
            }

            foreach (DataTable tab in dsStandardMall.Tables) tab.Clear();
            ErrorMessage = SelgaMeddelande(dsMeddelandeFil, dsStandardMall, SenderRow);

            string OutputFolderFileName = InputFolderFileName.Replace(".edi", ".xml");
            if (ErrorMessage == "" || !this.doFileOperations)
            {
                if (!this.doFileOperations)
                {
                    // Needed to not get schema inside the xml
                    parsedMessages.Add(dsStandardMall.GetXml());
                    return true;
                }
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
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    //EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    //EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                    return false;
                }
            }
            else
            {
                string MailSubject = "[SE-4] Fel vid överföring från grossist - Meddelandefil: " + InputFileName;
                string MailMessage = ErrorMessage;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                //EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                //EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            try { File.Delete(OutputFolderFileName); }
            catch
            {
                string MailSubject = "[SE-5] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
            }

            return true;
        }

        private string SelgaMeddelande(DataSet dsMeddelandeFil, DataSet dsStandardMall, DataRow SenderRow)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            if (dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString() == "SoftoneOrderResp")
                drMessage["MessageType"] = "ORDERBEKR";
            else
                if (dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString() == "SoftoneDesAdv")
                    drMessage["MessageType"] = "ORDERBEKR";
                else
                    if (dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString() == "SyncitInvoice")
                        drMessage["MessageType"] = "INVOICE";
                    else
                        return "Felaktig 'MessageType': " + dsMeddelandeFil.Tables["Routing"].Rows[0]["DocumentName"].ToString();

            if (dsMeddelandeFil.Tables["Routing"].Columns["SourceValue"] != null)
                drSeller["SellerName"] = dsMeddelandeFil.Tables["Routing"].Rows[0]["SourceValue"];
            else
                if (SenderRow["SenderId"].ToString() == "ST")
                    drSeller["SellerName"] = "Storel";
                else
                    drSeller["SellerName"] = "Selga";

            #region Orderbekräftelse
            //Orderbekräftelse huvud
            if (dsMeddelandeFil.Tables["tbl_OrderHead"] != null)
            {
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["OrderCreatedDate"] != null) drMessage["MessageDate"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["OrderCreatedDate"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["SupplierCompanyCode"] != null)
                {
                    drMessage["MessageSenderId"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["SupplierCompanyCode"];
                    if (SenderRow["SenderId"].ToString() == "ST") drMessage["MessageSenderId"] = "STOREL";
                    drSeller["SellerId"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["SupplierCompanyCode"];
                }
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["SupplierReference"] != null) drSeller["SellerReference"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["SupplierReference"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["PurchaserIdentification"] != null) drBuyer["BuyerId"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["PurchaserIdentification"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["DeliveryReceiver"] != null) drBuyer["BuyerName"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["DeliveryReceiver"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["DeliveryAddress"] != null) drBuyer["BuyerAddress"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["DeliveryAddress"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["DeliveryPostalCode"] != null) drBuyer["BuyerPostalCode"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["DeliveryPostalCode"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["DeliveryCity"] != null) drBuyer["BuyerPostalAddress"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["DeliveryCity"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["PurchaserReference"] != null) drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["PurchaserReference"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["ProjectNo"] != null) drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["ProjectNo"];
                if (dsMeddelandeFil.Tables["tbl_OrderHead"].Columns["SupplierOrderNo"] != null) drHead["HeadSellerOrderNumber"] = dsMeddelandeFil.Tables["tbl_OrderHead"].Rows[0]["SupplierOrderNo"];
            }

            //Orderbekräftelse rad
            if (dsMeddelandeFil.Tables["tbl_OrderLine"] != null)
            {
                foreach (DataRow rowOrderRows in dsMeddelandeFil.Tables["tbl_OrderLine"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["tbl_OrderLine"].Columns["ArticleNo"] != null) drRow["RowSellerArticleNumber"] = rowOrderRows["ArticleNo"];
                    if (dsMeddelandeFil.Tables["tbl_OrderLine"].Columns["ArticleName"] != null) drRow["RowSellerArticleDescription1"] = rowOrderRows["ArticleName"];
                    if (dsMeddelandeFil.Tables["tbl_OrderLine"].Columns["Quantity"] != null) drRow["RowQuantity"] = rowOrderRows["Quantity"];
                    if (dsMeddelandeFil.Tables["tbl_OrderLine"].Columns["QuantityMeasurement"] != null) drRow["RowUnitCode"] = rowOrderRows["QuantityMeasurement"];
                    if (dsMeddelandeFil.Tables["tbl_OrderLine"].Columns["NetPrice"] != null) drRow["RowUnitPrice"] = rowOrderRows["NetPrice"];
                    if (dsMeddelandeFil.Tables["tbl_OrderLine"].Columns["DeliveryDate"] != null) drRow["RowDeliveryDate"] = rowOrderRows["DeliveryDate"];
                    drRow["RowBuyerReference"] = drHead["HeadBuyerOrderNumber"];
                }
            }

            #endregion

            #region Leveransbesked
            //Leveransbesked huvud
            if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"] != null)
            {
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["DeliveryNoteCreatedDate"] != null) drMessage["MessageDate"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["DeliveryNoteCreatedDate"];
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["SupplierCompanyCode"] != null)
                {
                    drMessage["MessageSenderId"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["SupplierCompanyCode"];
                    if (SenderRow["SenderId"].ToString() == "ST") drMessage["MessageSenderId"] = "STOREL";
                    drSeller["SellerId"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["SupplierCompanyCode"];
                }
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["SupplierReference"] != null) drSeller["SellerReference"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["SupplierReference"];
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["PurchaserIdentification"] != null) drBuyer["BuyerId"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["PurchaserIdentification"];
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["PurchaserCompanyName"] != null) drBuyer["BuyerName"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["PurchaserCompanyName"];
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["PurchaserReference"] != null) drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["PurchaserReference"];
                if (dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Columns["ProjectNo"] != null) drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["tbl_DeliveryNoteHead"].Rows[0]["ProjectNo"];
            }

            //Leveransbesked rad
            if (dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"] != null)
            {
                foreach (DataRow rowOrderRows in dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"].Columns["ArticleNo"] != null) drRow["RowSellerArticleNumber"] = rowOrderRows["ArticleNo"];
                    if (dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"].Columns["ArticleName"] != null) drRow["RowSellerArticleDescription1"] = rowOrderRows["ArticleName"];
                    if (dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"].Columns["Quantity"] != null) drRow["RowQuantity"] = rowOrderRows["Quantity"];
                    if (dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"].Columns["QuantityMeasurement"] != null) drRow["RowUnitCode"] = rowOrderRows["QuantityMeasurement"];
                    if (dsMeddelandeFil.Tables["tbl_DeliveryNoteLine"].Columns["NetPrice"] != null) drRow["RowUnitPrice"] = rowOrderRows["NetPrice"];
                    drRow["RowBuyerReference"] = drHead["HeadBuyerOrderNumber"];
                }
            }

            #endregion

            #region Faktura

            //Faktura huvud
            if (dsMeddelandeFil.Tables["tbl_InvoiceHead"] != null)
            {
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierCompanyCode"] != null) drMessage["MessageSenderId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierCompanyCode"];
                if (SenderRow["SenderId"].ToString() == "ST") drMessage["MessageSenderId"] = "STOREL";
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceDate"] != null) drMessage["MessageDate"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceDate"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierIdentification"] != null)
                    drSeller["SellerId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierIdentification"];
                else
                    if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierCompanyCode"] != null)
                        drSeller["SellerId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierCompanyCode"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierVatNo"] != null && dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"].ToString().Length > 12)
                    drSeller["SellerOrganisationNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"].ToString().Substring(2, 10);
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierVatNo"] != null) drSeller["SellerVatNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierVatNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierCompanyName"] != null) drSeller["SellerName"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierCompanyName"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierAddress"] != null) drSeller["SellerAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierAddress"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierPostalCode"] != null) drSeller["SellerPostalCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierPostalCode"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierCity"] != null) drSeller["SellerPostalAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierCity"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierReference"] != null) drSeller["SellerReference"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierReference"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierReferencePhone"] != null) drSeller["SellerReferencePhone"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierReferencePhone"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserIdentification"] != null) drBuyer["BuyerId"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserIdentification"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserCompanyName"] != null) drBuyer["BuyerName"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserCompanyName"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserAddress"] != null) drBuyer["BuyerAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserAddress"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["DeliveryAddress"] != null) drBuyer["BuyerDeliveryAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["DeliveryAddress"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["DeliveryPostalCode"] != null) drBuyer["BuyerDeliveryPostalCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["DeliveryPostalCode"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["DeliveryCity"] != null) drBuyer["BuyerDeliveryPostalAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["DeliveryCity"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["DeliveryCountry"] != null) drBuyer["BuyerDeliveryCountryCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["DeliveryCountry"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserPostalCode"] != null) drBuyer["BuyerPostalCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserPostalCode"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserCity"] != null) drBuyer["BuyerPostalAddress"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserCity"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserReference"] != null) drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserReference"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserReferencePhone"] != null) drBuyer["BuyerPhone"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserReferencePhone"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PurchaserReferenceFax"] != null) drBuyer["BuyerFax"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PurchaserReferenceFax"];
                
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["OrderHeadID"] != null) 
                    drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["OrderHeadID"];
                else if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["ProjectNo"] != null)
                    drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["ProjectNo"];

                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["ProjectNo"] != null) drHead["HeadBuyerProjectNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["ProjectNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierInvoiceNo"] != null) drHead["HeadInvoiceNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierInvoiceNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["SupplierOrderNo"] != null) drHead["HeadSellerOrderNumber"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["SupplierOrderNo"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceType"] != null) drHead["HeadInvoiceType"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceType"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceDate"] != null) drHead["HeadInvoiceDate"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceDate"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["InvoiceDueDate"] != null) drHead["HeadInvoiceDueDate"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["InvoiceDueDate"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["AccountHolder2"] != null) drHead["HeadPostalGiro"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["AccountHolder2"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["AccountHolder1"] != null) drHead["HeadBankGiro"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["AccountHolder1"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PaymentReference"] != null) drHead["HeadInvoiceOcr"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PaymentReference"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["Currency"] != null) drHead["HeadCurrencyCode"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["Currency"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["PenaltyInterest"] != null) drHead["HeadInterestPaymentPercent"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["PenaltyInterest"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["TermsOfPayment"] != null) drHead["HeadInterestPaymentText"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["TermsOfPayment"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["NetSum"] != null) drHead["HeadInvoiceNetAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["NetSum"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["TotalSum"] != null) drHead["HeadInvoiceGrossAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["TotalSum"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["VatSum"] != null) drHead["HeadVatAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["VatSum"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["NetAdjustment"] != null) drHead["HeadRoundingAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["NetAdjustment"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["EDIBonus"] != null && dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["EDIBonus"].ToString() != "")
                    drHead["HeadBonusAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["EDIBonus"];
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["Frakt"] != null) drHead["HeadFreightFeeAmount"] = dsMeddelandeFil.Tables["tbl_InvoiceHead"].Rows[0]["Frakt"];
                double chargeFeeAmount = 0;
                double toConvert;
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["ExpAvg"] != null && double.TryParse(dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["ExpAvg"].ToString(), out toConvert))
                    chargeFeeAmount += toConvert;
                if (dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["FaktAvg"] != null && double.TryParse(dsMeddelandeFil.Tables["tbl_InvoiceHead"].Columns["FaktAvg"].ToString(), out toConvert))
                    chargeFeeAmount += toConvert;
                if(chargeFeeAmount > 0)
                    drHead["HeadHandlingChargeFeeAmount"] = chargeFeeAmount;
            }

            //Faktura rad
            if (dsMeddelandeFil.Tables["tbl_InvoiceLine"] != null)
            {
                foreach (DataRow rowInvoiceRows in dsMeddelandeFil.Tables["tbl_InvoiceLine"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["ArticleNo"] != null) drRow["RowSellerArticleNumber"] = rowInvoiceRows["ArticleNo"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["ArticleName"] != null) drRow["RowSellerArticleDescription1"] = rowInvoiceRows["ArticleName"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceHead"] != null)
                        if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["OrderHeadID"] != null)
                            drRow["RowBuyerReference"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["OrderHeadID"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["Quantity"] != null) drRow["RowQuantity"] = rowInvoiceRows["Quantity"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["QuantityMeasurement"] != null) drRow["RowUnitCode"] = rowInvoiceRows["QuantityMeasurement"];
                    // Row unitprice
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["NetPrice"] != null) drRow["RowUnitPrice"] = rowInvoiceRows["NetPrice"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["GrossPrice"] != null && dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["NetPrice"] == null) drRow["RowUnitPrice"] = rowInvoiceRows["GrossPrice"];
                    //if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["GrossPrice"] != null) drRow["RowUnitPrice"] = rowInvoiceRows["GrossPrice"];
                    //if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["NetPrice"] != null && dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["GrossPrice"] == null) drRow["RowUnitPrice"] = rowInvoiceRows["NetPrice"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["GrossPrice"] != null && dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["NetPrice"] != null)
                    {
                        if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["GrossPrice"].ToString() != "0" && dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["NetPrice"].ToString() == "0")
                            drRow["RowSellerArticleDescription1"] += " (" + dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["NetPrice"].ToString() + ")";
                    }

                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["Discount"] != null)
                    {
                        drRow["RowDiscountPercent"] = rowInvoiceRows["Discount"];
                        drRow["RowDiscountPercent1"] = rowInvoiceRows["Discount"];
                    }
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["Discount-2"] != null) drRow["RowDiscountPercent2"] = rowInvoiceRows["Discount-2"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["LineSum"] != null) drRow["RowNetAmount"] = rowInvoiceRows["LineSum"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["VAT"] != null) drRow["RowVatPercentage"] = rowInvoiceRows["VAT"].ToString().Replace(".", "");
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["DeliveryNote"] != null && drHead["HeadSellerOrderNumber"].ToString() == "") drHead["HeadSellerOrderNumber"] = rowInvoiceRows["DeliveryNote"];
                    if (dsMeddelandeFil.Tables["tbl_InvoiceLine"].Columns["DeliveryDate"] != null)
                    {
                        drRow["RowDeliveryDate"] = rowInvoiceRows["DeliveryDate"];
                        // Add the delivery date from the row to the head (the head doesn't have a delivery date)
                        drHead["HeadDeliveryDate"] = rowInvoiceRows["DeliveryDate"];
                    }
                }
            }

            #endregion

            return "";
        }

        protected override IEnumerable<string> ConvertMessage(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, DataRow SenderRow, string fileContent)
        {
            this.Selga(InputFolderFileName, WholesaleTempFolder, dsStandardMall, drEdiSettings, SenderRow, fileContent);
            return this.parsedMessages;
        }
    }
}
