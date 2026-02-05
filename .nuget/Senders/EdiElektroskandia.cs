using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;
using System.Collections.Generic;

namespace SoftOne.EdiAdmin.Business.Senders
{
    class EdiElektroskandia
    {
        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        //Elektroskandia
        public bool Elektroskandia(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, string SOP_Connection, DataSet dsMain)
        {
            string InputFileName = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
            string ErrorMessage = "";

            DataSet dsMeddelandeFil = new DataSet();
            try
            {
                dsMeddelandeFil.ReadXml(InputFolderFileName);
            }
            catch
            {
                string MailSubject = "[ES-1] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " innehåller felaktigt Xml-format";
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["Message"] == null || dsMeddelandeFil.Tables["Message"].Columns["MessageType"] == null)
            {
                string MailSubject = "[ES-2] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'Message' eller 'MessageType'"; ;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["Message"].Rows[0]["MessageType"].ToString() == "ORDRSP")
                ErrorMessage = ElektroskandiaOrderbekräftelse(dsMeddelandeFil, dsStandardMall);
            else
                if (dsMeddelandeFil.Tables["Message"].Rows[0]["MessageType"].ToString() == "INVOIC")
                    ErrorMessage = ElektroskandiaFaktura(dsMeddelandeFil, dsStandardMall);
                else
                {
                    string MailSubject = "[ES-3] Fel vid överföring från grossist";
                    string MailMessage = "Meddelandefilen: " + InputFileName + " Felaktig 'MessageType': " + dsMeddelandeFil.Tables["Message"].Rows[0]["MessageType"].ToString();
                    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                    EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                    return false;
                }

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
                string MailSubject = "[ES-5] Fel vid överföring från grossist - Meddelandefil: " + InputFileName;
                string MailMessage = ErrorMessage;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                return false;
            }

            try { File.Delete(OutputFolderFileName); }
            catch
            {
                string MailSubject = "[ES-6] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
            }

            return true;
        }

        private string ElektroskandiaOrderbekräftelse(DataSet dsMeddelandeFil, DataSet dsStandardMall)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            drMessage["MessageSenderId"] = "ELEKTROSKANDIA";
            drMessage["MessageType"] = "ORDERBEKR";
            if (dsMeddelandeFil.Tables["Message"].Columns["MessageDate"] != null)
                drMessage["MessageDate"] = dsMeddelandeFil.Tables["Message"].Rows[0]["MessageDate"];

            if (dsMeddelandeFil.Tables["Seller"].Columns["SellerID"] != null)
                drSeller["SellerId"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["SellerID"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Name"] != null)
                drSeller["SellerName"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Name"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Handler"] != null)
                drSeller["SellerReference"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Handler"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Phone"] != null)
                drSeller["SellerPhone"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Phone"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Fax"] != null)
                drSeller["SellerFax"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Fax"];

            if (dsMeddelandeFil.Tables["Buyer"].Columns["BuyerID"] != null)
                drBuyer["BuyerId"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["BuyerID"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Name"] != null)
                drBuyer["BuyerName"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Name"];
            else
                if (dsMeddelandeFil.Tables["DeliveryAddress"] != null && dsMeddelandeFil.Tables["DeliveryAddress"].Columns["Name"] != null)
                    if (dsMeddelandeFil.Tables["DeliveryAddress"].Rows.Count > 0)
                        drBuyer["BuyerName"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["Name"];
            if (dsMeddelandeFil.Tables["DeliveryAddress"] != null)
            {
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["Street"] != null)
                    drBuyer["BuyerAddress"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["Street"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["PostalCode"] != null)
                    drBuyer["BuyerPostalCode"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["PostalCode"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["City"] != null)
                    drBuyer["BuyerPostalAddress"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["City"];
            }
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Handler"] != null)
                drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Handler"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Phone"] != null)
                drBuyer["BuyerPhone"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Phone"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Fax"] != null)
                drBuyer["BuyerFax"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Fax"];

            if (dsMeddelandeFil.Tables["Order"] != null && dsMeddelandeFil.Tables["Order"].Columns["BuyerOrderNumber"] != null &&
                dsMeddelandeFil.Tables["Order"].Rows[0]["BuyerOrderNumber"].ToString() != "")
                drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["Order"].Rows[0]["BuyerOrderNumber"];
            else
                if (dsMeddelandeFil.Tables["Buyer"].Columns["Reference"] != null)
                    drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Reference"];

            if (dsMeddelandeFil.Tables["Order"] != null && dsMeddelandeFil.Tables["Order"].Columns["SellerOrderNumber"] != null &&
                dsMeddelandeFil.Tables["Order"].Rows[0]["SellerOrderNumber"].ToString() != "")
                drHead["HeadSellerOrderNumber"] = dsMeddelandeFil.Tables["Order"].Rows[0]["SellerOrderNumber"];

            if (dsMeddelandeFil.Tables["Message"].Columns["TotalPrice"] != null & dsMeddelandeFil.Tables["Message"].Rows.Count == 2)
                drHead["HeadInvoiceNetAmount"] = dsMeddelandeFil.Tables["Message"].Rows[1]["TotalPrice"];

            if (dsMeddelandeFil.Tables["Order"].Columns["RegistrationDate"] != null)
                drHead["HeadInvoiceDate"] = dsMeddelandeFil.Tables["Order"].Rows[0]["RegistrationDate"];

            if (dsMeddelandeFil.Tables["OrderRows"] != null)
            {
                foreach (DataRow row in dsMeddelandeFil.Tables["OrderRows"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["SellerArticleNumber"] != null)
                        drRow["RowSellerArticleNumber"] = row["SellerArticleNumber"];
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["SellerArticleDescription"] != null)
                        drRow["RowSellerArticleDescription1"] = row["SellerArticleDescription"];
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["Reference"] != null)
                        drRow["RowBuyerReference"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Reference"]; ;
                    foreach (DataRow rowDeliveryPlan in dsMeddelandeFil.Tables["DeliveryPlan"].Rows)
                    {
                        if (row["OrderRows_Id"].ToString() == rowDeliveryPlan["OrderRows_Id"].ToString())
                            if (dsMeddelandeFil.Tables["DeliveryPlan"].Columns["Quantity"] != null)
                                drRow["RowQuantity"] = rowDeliveryPlan["Quantity"];
                    }
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["UnitCode"] != null)
                        drRow["RowUnitCode"] = row["UnitCode"];
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["Price"] != null)
                        drRow["RowUnitPrice"] = row["Price"];
                    if (dsMeddelandeFil.Tables["OrderRows"].Columns["RowAmount"] != null)
                        drRow["RowNetAmount"] = row["RowAmount"];
                }
            }

            return "";
        }

        private string ElektroskandiaFaktura(DataSet dsMeddelandeFil, DataSet dsStandardMall)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            drMessage["MessageSenderId"] = "ELEKTROSKANDIA";
            drMessage["MessageType"] = "INVOICE";
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
            if (dsMeddelandeFil.Tables["Seller"].Columns["CountryCode"] != null)
                drSeller["SellerCountryCode"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["CountryCode"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Phone"] != null)
                drSeller["SellerPhone"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Phone"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Fax"] != null)
                drSeller["SellerFax"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Fax"];
            if (dsMeddelandeFil.Tables["Seller"].Columns["Reference"] != null)
                drSeller["SellerReference"] = dsMeddelandeFil.Tables["Seller"].Rows[0]["Reference"];

            if (dsMeddelandeFil.Tables["Buyer"].Columns["BuyerID"] != null)
                drBuyer["BuyerId"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["BuyerID"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["OrganisationNumber"] != null)
                drBuyer["BuyerOrganisationNumber"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["OrganisationNumber"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Name"] != null)
                drBuyer["BuyerName"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Name"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Address"] != null)
                drBuyer["BuyerAddress"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Address"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["PostalCode"] != null)
                drBuyer["BuyerPostalCode"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["PostalCode"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["City"] != null)
                drBuyer["BuyerPostalAddress"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["City"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["CountryCode"] != null)
                drBuyer["BuyerCountryCode"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["CountryCode"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Reference"] != null)
                drBuyer["BuyerReference"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Reference"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Phone"] != null)
                drBuyer["BuyerPhone"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Phone"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["Fax"] != null)
                drBuyer["BuyerFax"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["Fax"];
            if (dsMeddelandeFil.Tables["Buyer"].Columns["EmailAddress"] != null)
                drBuyer["BuyerEmailAddress"] = dsMeddelandeFil.Tables["Buyer"].Rows[0]["EmailAddress"];
            if (dsMeddelandeFil.Tables["DeliveryAddress"] != null && dsMeddelandeFil.Tables["DeliveryAddress"].Rows.Count > 0)
            {
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["Name"] != null)
                    drBuyer["BuyerDeliveryName"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["Name"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["Street"] != null)
                    drBuyer["BuyerDeliveryAddress"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["Street"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["PostalCode"] != null)
                    drBuyer["BuyerDeliveryPostalCode"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["PostalCode"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["City"] != null)
                    drBuyer["BuyerDeliveryPostalAddress"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["City"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["CountryCode"] != null)
                    drBuyer["BuyerDeliveryCountryCode"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["CountryCode"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["DeliveryNoteText"] != null)
                    drBuyer["BuyerDeliveryNoteText"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["DeliveryNoteText"];
                if (dsMeddelandeFil.Tables["DeliveryAddress"].Columns["GoodsMarking"] != null)
                    drBuyer["BuyerDeliveryGoodsMarking"] = dsMeddelandeFil.Tables["DeliveryAddress"].Rows[0]["GoodsMarking"];
            }

            if (dsMeddelandeFil.Tables["Invoice"].Columns["InvoiceNumber"] != null)
                drHead["HeadInvoiceNumber"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InvoiceNumber"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["InvoiceType"] != null)
                drHead["HeadInvoiceType"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InvoiceType"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["InvoiceDate"] != null)
                drHead["HeadInvoiceDate"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InvoiceDate"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["DueDate"] != null)
                drHead["HeadInvoiceDueDate"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["DueDate"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["DispatchDate"] != null)
                drHead["HeadDeliveryDate"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["DispatchDate"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["BuyerOrderNumber"] != null)
                drHead["HeadBuyerOrderNumber"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["BuyerOrderNumber"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["PostalGiro"] != null)
                drHead["HeadPostalGiro"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["PostalGiro"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["BankGiro"] != null)
                drHead["HeadBankGiro"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["BankGiro"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["Bank"] != null)
                drHead["HeadBank"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["Bank"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["SwiftAddress"] != null)
                drHead["HeadBicAddress"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["SwiftAddress"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["IBANNumber"] != null)
                drHead["HeadIbanNumber"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["IBANNumber"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["CurrencyCode"] != null)
                drHead["HeadCurrencyCode"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["CurrencyCode"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["VATPercentage"] != null)
                drHead["HeadVatPercentage"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["VATPercentage"];
            if (dsMeddelandeFil.Tables["Invoice"].Columns["PaymentCondition"] != null)
            {
                drHead["HeadPaymentConditionText"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["PaymentCondition"];
                if (dsMeddelandeFil.Tables["Invoice"].Rows[0]["PaymentCondition"].ToString().Length >= 2)
                    drHead["HeadPaymentConditionDays"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["PaymentCondition"].ToString().Substring(0, 2);
            }
            if (dsMeddelandeFil.Tables["Invoice"].Columns["InterestOverduePaymentText"] != null)
                drHead["HeadInterestPaymentText"] = dsMeddelandeFil.Tables["Invoice"].Rows[0]["InterestOverduePaymentText"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["NetAmount"] != null)
                drHead["HeadInvoiceGrossAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["NetAmount"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["GrossAmount"] != null)
                drHead["HeadInvoiceNetAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["GrossAmount"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["VATBasis"] != null)
                drHead["HeadVatBasisAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["VATBasis"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["VATAmount"] != null)
                drHead["HeadVatAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["VATAmount"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["Freight"] != null)
                drHead["HeadFreightFeeAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["Freight"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["InvoiceFee"] != null)
                drHead["HeadRemainingFeeAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["InvoiceFee"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["OrderBonus"] != null && dsMeddelandeFil.Tables["Footer"].Rows[0]["OrderBonus"].ToString() != "")
                drHead["HeadBonusAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["OrderBonus"];
            if (dsMeddelandeFil.Tables["Footer"].Columns["ReturnReduction"] != null && dsMeddelandeFil.Tables["Footer"].Rows[0]["ReturnReduction"].ToString() != "")
                drHead["HeadDiscountAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["ReturnReduction"].ToString().Replace("-", "");
            if (dsMeddelandeFil.Tables["Footer"].Columns["Equalization"] != null)
                drHead["HeadRoundingAmount"] = dsMeddelandeFil.Tables["Footer"].Rows[0]["Equalization"];
            drHead["HeadInvoiceArrival"] = "0";
            drHead["HeadInvoiceAuthorized"] = "0";

            if (dsMeddelandeFil.Tables["InvoiceRows"] != null)
            {
                foreach (DataRow row in dsMeddelandeFil.Tables["InvoiceRows"].Rows)
                {
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["SellerArticleNumber"] != null)
                        drRow["RowSellerArticleNumber"] = row["SellerArticleNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["SellerArticleDescription"] != null)
                        drRow["RowSellerArticleDescription1"] = row["SellerArticleDescription"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["SellerRowNumber"] != null)
                        drRow["RowSellerRowNumber"] = row["SellerRowNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["BuyerArticleNumber"] != null)
                        drRow["RowBuyerArticleNumber"] = row["BuyerArticleNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["BuyerRowNumber"] != null)
                        drRow["RowBuyerRowNumber"] = row["BuyerRowNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["BuyerReference"] != null)
                        drRow["RowBuyerReference"] = row["BuyerReference"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["BuyerObjectID"] != null)
                        drRow["RowBuyerObjectId"] = row["BuyerObjectID"]; ;
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["Quantity"] != null)
                        drRow["RowQuantity"] = row["Quantity"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["UnitCode"] != null)
                        drRow["RowUnitCode"] = row["UnitCode"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["GrossAmount"] != null)
                        drRow["RowUnitPrice"] = row["GrossAmount"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["DiscountPercentage"] != null)
                    {
                        drRow["RowDiscountPercent"] = row["DiscountPercentage"];
                        drRow["RowDiscountPercent1"] = row["DiscountPercentage"];
                    }
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["RowAmount"] != null)
                        drRow["RowNetAmount"] = row["RowAmount"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["VATAmount"] != null)
                        drRow["RowVatAmount"] = row["VATAmount"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["SellerOrderNumber"] != null)
                        drHead["HeadSellerOrderNumber"] = row["SellerOrderNumber"];
                    if (dsMeddelandeFil.Tables["InvoiceRows"].Columns["BuyerObjectID"] != null)
                        drHead["HeadBuyerProjectNumber"] = row["BuyerObjectID"];
                }
            }

            return "";
        }

    }
}
