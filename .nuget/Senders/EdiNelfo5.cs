using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;

namespace SoftOne.EdiAdmin.Business.Senders
{
    class EdiNelfo5 : EdiSenderOldBase
    {
        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        private string ErrorMessage = "";
        private int MsgNr = 0;
        private bool ReturnCode = true;
        private bool doFileOperations;

        //Nelfo version 5
        public bool Nelfo5(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, string content = null)
        {
            string InputFileName; // = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
            var reader = Util.EdiDiverse.GetStreamReaderFromContentOrFile(InputFolderFileName, WholesaleTempFolder, content, out this.doFileOperations, out InputFileName);
            MsgNr = 0;

            DataSet dsMeddelandeFil = new DataSet();
            try
            {
                dsMeddelandeFil.ReadXml(reader);
            }
            catch
            {
                string MailSubject = "[N5-1] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " innehåller felaktigt Xml-format";
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["OrderResponse"] == null & dsMeddelandeFil.Tables["Invoice"] == null)
            {
                string MailSubject = "[N5-2] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " Varken Taggen 'OrderRespons' eller 'Invoice' finns i meddelandet"; ;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                return false;
            }

            //Orderbekräftelse
            if (dsMeddelandeFil.Tables["OrderResponse"] != null)
            {
                if (dsMeddelandeFil.Tables["OrderResponse"].Columns["MessageType"] == null)
                {
                    string MailSubject = "[N5-2] Fel vid överföring från grossist";
                    string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'MessageType'";
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    return false;
                }
                else if (dsMeddelandeFil.Tables["OrderResponse"].Rows[0]["MessageType"].ToString() != "OrderRespons")
                {
                    string MailSubject = "[N5-2] Fel vid överföring från grossist";
                    string MailMessage = "Meddelandefilen: " + InputFileName + " Taggen 'MessageType' är inte = 'OrderRespons'";
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    return false;
                }
                else
                {
                    foreach (DataRow rowOrderResponse in dsMeddelandeFil.Tables["OrderResponse"].Rows)
                    {
                        foreach (DataTable tb in dsStandardMall.Tables) tb.Rows.Clear();
                        MsgNr++;
                        ErrorMessage = EdiNelfo5Orderbekräftelse(dsMeddelandeFil, dsStandardMall, rowOrderResponse);
                        EdiNelfo5SkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                    }
                }
            }
            else if (dsMeddelandeFil.Tables["Invoice"] != null)
            {
                //Faktura
                if (dsMeddelandeFil.Tables["Invoice"].Columns["MessageType"] == null)
                {
                    string MailSubject = "[N5-2] Fel vid överföring från grossist";
                    string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'MessageType'";
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    return false;
                }
                else
                    if (dsMeddelandeFil.Tables["Invoice"].Rows[0]["MessageType"].ToString() != "Invoice")
                    {
                        string MailSubject = "[N5-2] Fel vid överföring från grossist";
                        string MailMessage = "Meddelandefilen: " + InputFileName + " Taggen 'MessageType' är inte = 'Invoice'";
                        Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                        return false;
                    }
                    else
                    {
                        foreach (DataRow rowInvoice in dsMeddelandeFil.Tables["Invoice"].Rows)
                        {
                            foreach (DataTable tb in dsStandardMall.Tables) tb.Rows.Clear();
                            MsgNr++;
                            ErrorMessage = EdiNelfo5Faktura(dsMeddelandeFil, dsStandardMall, rowInvoice);
                            EdiNelfo5SkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                        }
                    }
            }

            return ReturnCode;
        }

        private void EdiNelfo5SkrivMeddelande(string InputFolderFileName, string InputFileName, string WholesaleTempFolder, Dictionary<string, string> drEdiSettings, DataSet dsStandardMall)
        {
            string OutputFolderFileName = InputFolderFileName.Replace(".edi", "") + "_" + MsgNr + ".xml";
            if (ErrorMessage == "")
            {
                if (!doFileOperations)
                {
                    string xmlContent = dsStandardMall.GetXml();
                    this.ParsedMessages.Add(xmlContent);
                    return;
                }
                dsStandardMall.WriteXml(OutputFolderFileName, System.Data.XmlWriteMode.IgnoreSchema);
                string UploadFileName = OutputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
                try
                {
                    File.Copy(OutputFolderFileName, OutputFolderFileName.Replace(@WholesaleTempFolder, drEdiSettings["MsgTempFolder"].ToString()), true);
                }
                catch
                {
                    string MailSubject = "[N5-4] Fel vid överföring från grossist - Meddelandefil: " + InputFileName + ". Avser meddelande: " + MsgNr + " i filen";
                    string MailMessage = "Det gick inte att kopiera filen " + UploadFileName + " till Temporärmappen: " + drEdiSettings["MsgTempFolder"].ToString();
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    ReturnCode = false;
                }
            }
            else
            {
                string MailSubject = "[N5-5] Fel vid överföring från grossist - Meddelandefil: " + InputFileName + ". Avser meddelande: " + MsgNr + " i filen"; ;
                string MailMessage = ErrorMessage;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                ReturnCode = false;
            }

            try { File.Delete(OutputFolderFileName); }
            catch
            {
                string MailSubject = "[N5-6] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
            }

        }

        private string EdiNelfo5Orderbekräftelse(DataSet dsMeddelandeFil, DataSet dsStandardMall, DataRow drOrderRespons)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            drMessage["MessageType"] = "ORDERBEKR";
            if (drOrderRespons["MessageTimeStamp"] != null && drOrderRespons["MessageTimeStamp"].ToString().Length >= 10)
                drMessage["MessageDate"] = drOrderRespons["MessageTimeStamp"].ToString().Substring(0, 10);

            string OrderResponseHeader_Id = "";
            foreach (DataRow rowOrderResponseHeader in dsMeddelandeFil.Tables["OrderResponseHeader"].Rows)
            {
                if (drOrderRespons["OrderResponse_Id"].ToString() != rowOrderResponseHeader["OrderResponse_Id"].ToString()) continue;
                OrderResponseHeader_Id = drOrderRespons["OrderResponse_Id"].ToString();
                break;
            }

            //Leverantör
            if (dsMeddelandeFil.Tables["Supplier"] != null)
            {
                foreach (DataRow rowSupplier in dsMeddelandeFil.Tables["Supplier"].Rows)
                {
                    if (OrderResponseHeader_Id != rowSupplier["OrderResponseHeader_Id"].ToString()) continue;

                    if (dsMeddelandeFil.Tables["Supplier"].Columns["PartyId"] != null)
                    {
                        drMessage["MessageSenderId"] = rowSupplier["PartyId"].ToString().Replace(" ", "");
                        drSeller["SellerId"] = rowSupplier["PartyId"].ToString().Replace(" ", "");
                    }
                    if (dsMeddelandeFil.Tables["Supplier"].Columns["Name"] != null) drSeller["SellerName"] = rowSupplier["Name"];
                    if (dsMeddelandeFil.Tables["Supplier"].Columns["VatId"] != null) drSeller["SellerVatNumber"] = rowSupplier["VatId"];

                    if (dsMeddelandeFil.Tables["StreetAddress"] != null && dsMeddelandeFil.Tables["StreetAddress"].Columns["Supplier_Id"] != null)
                    {
                        foreach (DataRow rowStreetAddress in dsMeddelandeFil.Tables["StreetAddress"].Rows)
                        {
                            if (rowSupplier["Supplier_Id"].ToString() != rowStreetAddress["Supplier_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Name"] != null && drSeller["SellerName"].ToString() == "") drSeller["SellerName"] = rowStreetAddress["Name"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Address1"] != null) drSeller["SellerAddress"] = rowStreetAddress["Address1"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalCode"] != null) drSeller["SellerPostalCode"] = rowStreetAddress["PostalCode"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalDistrict"] != null) drSeller["SellerPostalAddress"] = rowStreetAddress["PostalDistrict"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["CountryCode"] != null) drSeller["SellerCountryCode"] = rowStreetAddress["CountryCode"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactInformation"] != null && dsMeddelandeFil.Tables["ContactInformation"].Columns["Supplier_Id"] != null)
                    {
                        foreach (DataRow rowContactInformation in dsMeddelandeFil.Tables["ContactInformation"].Rows)
                        {
                            if (rowSupplier["Supplier_Id"].ToString() != rowContactInformation["Supplier_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["PhoneNumber"] != null) drSeller["SellerPhone"] = rowContactInformation["PhoneNumber"];
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["FaxNumber"] != null) drSeller["SellerFax"] = rowContactInformation["FaxNumber"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactPerson"] != null && dsMeddelandeFil.Tables["ContactPerson"].Columns["Supplier_Id"] != null)
                    {
                        foreach (DataRow rowContactPerson in dsMeddelandeFil.Tables["ContactPerson"].Rows)
                        {
                            if (rowSupplier["Supplier_Id"].ToString() != rowContactPerson["Supplier_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactPerson"].Columns["Name"] != null) drSeller["SellerReference"] = rowContactPerson["Name"];
                            if (dsMeddelandeFil.Tables["ContactInformation"] != null && dsMeddelandeFil.Tables["ContactInformation"].Columns["ContactPerson_Id"] != null)
                            {
                                foreach (DataRow rowContactInformation in dsMeddelandeFil.Tables["ContactInformation"].Rows)
                                {
                                    if (rowContactPerson["ContactPerson_Id"].ToString() != rowContactInformation["ContactPerson_Id"].ToString()) continue;
                                    if (dsMeddelandeFil.Tables["ContactInformation"].Columns["PhoneNumber"] != null) drSeller["SellerReferencePhone"] = rowContactInformation["PhoneNumber"];
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            //Köpare 
            if (dsMeddelandeFil.Tables["Buyer"] != null)
            {
                foreach (DataRow rowBuyer in dsMeddelandeFil.Tables["Buyer"].Rows)
                {
                    if (OrderResponseHeader_Id != rowBuyer["OrderResponseHeader_Id"].ToString()) continue;
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["PartyId"] != null) drBuyer["BuyerId"] = rowBuyer["PartyId"].ToString().Replace(" ", "");
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["Name"] != null) drBuyer["BuyerName"] = rowBuyer["Name"];
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["VatId"] != null) drBuyer["BuyerVatNumber"] = rowBuyer["VatId"];

                    if (dsMeddelandeFil.Tables["StreetAddress"] != null && dsMeddelandeFil.Tables["StreetAddress"].Columns["Buyer_Id"] != null)
                    {
                        foreach (DataRow rowStreetAddress in dsMeddelandeFil.Tables["StreetAddress"].Rows)
                        {
                            if (rowBuyer["Buyer_Id"].ToString() != rowStreetAddress["Buyer_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Name"] != null && drBuyer["BuyerName"].ToString() == "") drBuyer["BuyerName"] = rowStreetAddress["Name"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Address1"] != null) drBuyer["BuyerAddress"] = rowStreetAddress["Address1"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalCode"] != null) drBuyer["BuyerPostalCode"] = rowStreetAddress["PostalCode"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalDistrict"] != null) drBuyer["BuyerPostalAddress"] = rowStreetAddress["PostalDistrict"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["CountryCode"] != null) drBuyer["BuyerCountryCode"] = rowStreetAddress["CountryCode"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactInformation"] != null && dsMeddelandeFil.Tables["ContactInformation"].Columns["Buyer_Id"] != null)
                    {
                        foreach (DataRow rowContactInformation in dsMeddelandeFil.Tables["ContactInformation"].Rows)
                        {
                            if (rowBuyer["Buyer_Id"].ToString() != rowContactInformation["Buyer_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["PhoneNumber"] != null) drBuyer["BuyerPhone"] = rowContactInformation["PhoneNumber"];
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["FaxNumber"] != null) drBuyer["BuyerFax"] = rowContactInformation["FaxNumber"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactPerson"] != null && dsMeddelandeFil.Tables["ContactPerson"].Columns["Buyer_Id"] != null)
                    {
                        foreach (DataRow rowContactPerson in dsMeddelandeFil.Tables["ContactPerson"].Rows)
                        {
                            if (rowBuyer["Buyer_Id"].ToString() != rowContactPerson["Buyer_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactPerson"].Columns["Name"] != null) drBuyer["BuyerReference"] = rowContactPerson["Name"];
                            break;
                        }
                    }
                    break;
                }
            }

            //Leveransadress
            if (dsMeddelandeFil.Tables["DeliveryPart"] != null && dsMeddelandeFil.Tables["StreetAddress"] != null && dsMeddelandeFil.Tables["StreetAddress"].Columns["DeliveryPart_Id"] != null)
            {
                foreach (DataRow rowDeliveryPart in dsMeddelandeFil.Tables["DeliveryPart"].Rows)
                {
                    if (OrderResponseHeader_Id != rowDeliveryPart["OrderResponseHeader_Id"].ToString()) continue;
                    foreach (DataRow rowStreetAddress in dsMeddelandeFil.Tables["StreetAddress"].Rows)
                    {
                        if (rowDeliveryPart["DeliveryPart_Id"].ToString() != rowStreetAddress["DeliveryPart_Id"].ToString()) continue;
                        if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Name"] != null) drBuyer["BuyerDeliveryName"] = rowStreetAddress["Name"];
                        if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Address1"] != null) drBuyer["BuyerDeliveryAddress"] = rowStreetAddress["Address1"];
                        if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalCode"] != null) drBuyer["BuyerDeliveryPostalCode"] = rowStreetAddress["PostalCode"];
                        if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalDistrict"] != null) drBuyer["BuyerDeliveryPostalAddress"] = rowStreetAddress["PostalDistrict"];
                        if (dsMeddelandeFil.Tables["StreetAddress"].Columns["CountryCode"] != null) drBuyer["BuyerDeliveryCountryCode"] = rowStreetAddress["CountryCode"];
                        break;
                    }
                    break;
                }
            }

            //Huvud
            if (dsMeddelandeFil.Tables["References"] != null)
            {
                foreach (DataRow rowReferences in dsMeddelandeFil.Tables["References"].Rows)
                {
                    if (OrderResponseHeader_Id != rowReferences["OrderResponseHeader_Id"].ToString()) continue;
                    if (dsMeddelandeFil.Tables["References"].Columns["BuyersProjectCode"] != null) drHead["HeadBuyerProjectNumber"] = rowReferences["BuyersProjectCode"];
                    if (dsMeddelandeFil.Tables["References"].Columns["BuyersOrderNumber"] != null) drHead["HeadBuyerOrderNumber"] = rowReferences["BuyersOrderNumber"];
                }
            }

            if (dsMeddelandeFil.Tables["OrderTotals"] != null)
            {
                foreach (DataRow rowOrderResponseSummary in dsMeddelandeFil.Tables["OrderResponseSummary"].Rows)
                {
                    if (OrderResponseHeader_Id != rowOrderResponseSummary["OrderResponse_Id"].ToString()) continue;
                    foreach (DataRow rowOrderTotals in dsMeddelandeFil.Tables["OrderTotals"].Rows)
                    {
                        if (rowOrderResponseSummary["OrderResponseSummary_Id"].ToString() != rowOrderTotals["OrderResponseSummary_Id"].ToString()) continue;
                        if (dsMeddelandeFil.Tables["OrderTotals"].Columns["NetAmount"] != null) drHead["HeadInvoiceNetAmount"] = rowOrderTotals["NetAmount"];
                        break;
                    }
                    break;
                }
            }

            //Orderrader
            if (dsMeddelandeFil.Tables["OrderResponseDetails"] == null) return "";

            string OrderResponseDetails = "";
            foreach (DataRow rowOrderResponseDetails in dsMeddelandeFil.Tables["OrderResponseDetails"].Rows)
            {
                if (drOrderRespons["OrderResponse_Id"].ToString() != rowOrderResponseDetails["OrderResponse_Id"].ToString()) continue;
                OrderResponseDetails = drOrderRespons["OrderResponse_Id"].ToString();
                break;
            }

            if (dsMeddelandeFil.Tables["BaseItemDetails"] != null && dsMeddelandeFil.Tables["BaseItemDetails"].Columns["OrderResponseDetails_Id"] != null)
            {
                foreach (DataRow rowBaseItemDetails in dsMeddelandeFil.Tables["BaseItemDetails"].Rows)
                {
                    if (OrderResponseDetails != rowBaseItemDetails["OrderResponseDetails_Id"].ToString()) continue;
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);

                    if (dsMeddelandeFil.Tables["ProductIdentification"] != null && dsMeddelandeFil.Tables["ProductIdentification"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowProductIdentification in dsMeddelandeFil.Tables["ProductIdentification"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowProductIdentification["BaseItemDetails_Id"].ToString()) continue;
                            if (rowProductIdentification["ProductIdCode"].ToString() == "ELNR")
                                drRow["RowSellerArticleNumber"] = rowProductIdentification["ProductIdentification_Text"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["BaseItemDetails"].Columns["Description"] != null) drRow["RowSellerArticleDescription1"] = rowBaseItemDetails["Description"];
                    if (dsMeddelandeFil.Tables["BaseItemDetails"].Columns["PriceQuantityUnit"] != null) drRow["RowUnitCode"] = rowBaseItemDetails["PriceQuantityUnit"];
                    if (dsMeddelandeFil.Tables["BaseItemDetails"].Columns["UnitPrice"] != null) drRow["RowUnitPrice"] = rowBaseItemDetails["UnitPrice"];
                    if (dsMeddelandeFil.Tables["BaseItemDetails"].Columns["LineItemAmount"] != null) drRow["RowNetAmount"] = rowBaseItemDetails["LineItemAmount"];

                    if (dsMeddelandeFil.Tables["ConfirmedQuantity"] != null && dsMeddelandeFil.Tables["ConfirmedQuantity"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowConfirmedQuantity in dsMeddelandeFil.Tables["ConfirmedQuantity"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowConfirmedQuantity["BaseItemDetails_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ConfirmedQuantity"].Columns["Quantity"] != null)
                                drRow["RowQuantity"] = rowConfirmedQuantity["Quantity"];
                            if (dsMeddelandeFil.Tables["ConfirmedQuantity"].Columns["UnitOfMeasure"] != null)
                                drRow["RowUnitCode"] = rowConfirmedQuantity["UnitOfMeasure"];
                            if (dsMeddelandeFil.Tables["QuantityDate"] != null && dsMeddelandeFil.Tables["QuantityDate"].Columns["DateType"] != null &&
                                dsMeddelandeFil.Tables["QuantityDate"].Columns["ConfirmedQuantity_Id"] != null)
                            {
                                foreach (DataRow rowQuantityDate in dsMeddelandeFil.Tables["QuantityDate"].Rows)
                                {
                                    if (rowConfirmedQuantity["ConfirmedQuantity_Id"].ToString() != rowQuantityDate["ConfirmedQuantity_Id"].ToString()) continue;
                                    if (rowQuantityDate["DateType"].ToString() != "69") continue;
                                    drRow["RowDeliveryDate"] = rowQuantityDate["Date"];
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["BuyersOrderInfo"] != null && dsMeddelandeFil.Tables["BuyersOrderInfo"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowBuyersOrderInfo in dsMeddelandeFil.Tables["BuyersOrderInfo"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowBuyersOrderInfo["BaseItemDetails_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["BuyersOrderInfo"].Columns["BuyersOrderNumber"] != null)
                                drRow["RowBuyerReference"] = rowBuyersOrderInfo["BuyersOrderNumber"];
                            break;
                        }
                    }
                }
            }

            return "";
        }

        private string EdiNelfo5Faktura(DataSet dsMeddelandeFil, DataSet dsStandardMall, DataRow drInvoice)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            drMessage["MessageType"] = "INVOICE";
            if (drInvoice["MessageTimeStamp"] != null && drInvoice["MessageTimeStamp"].ToString().Length >= 10)
                drMessage["MessageDate"] = drInvoice["MessageTimeStamp"].ToString().Substring(0, 10);

            string InvoiceHeader_Id = "";
            foreach (DataRow rowInvoiceHeader in dsMeddelandeFil.Tables["InvoiceHeader"].Rows)
            {
                if (drInvoice["Invoice_Id"].ToString() != rowInvoiceHeader["Invoice_Id"].ToString()) continue;
                InvoiceHeader_Id = drInvoice["Invoice_Id"].ToString();
                if (dsMeddelandeFil.Tables["InvoiceHeader"].Columns["InvoiceNumber"] != null) drHead["HeadInvoiceNumber"] = rowInvoiceHeader["InvoiceNumber"];
                drHead["HeadInvoiceType"] = "1";
                if (dsMeddelandeFil.Tables["InvoiceHeader"].Columns["InvoiceType"] != null && rowInvoiceHeader["InvoiceType"].ToString() == "381") drHead["HeadInvoiceType"] = "2";
                if (dsMeddelandeFil.Tables["InvoiceHeader"].Columns["InvoiceDate"] != null) drHead["HeadInvoiceDate"] = rowInvoiceHeader["InvoiceDate"];
                break;
            }

            //Leverantör
            if (dsMeddelandeFil.Tables["Supplier"] != null)
            {
                foreach (DataRow rowSupplier in dsMeddelandeFil.Tables["Supplier"].Rows)
                {
                    if (InvoiceHeader_Id != rowSupplier["InvoiceHeader_Id"].ToString()) continue;
                    if (dsMeddelandeFil.Tables["Supplier"].Columns["PartyId"] != null)
                    {
                        drMessage["MessageSenderId"] = rowSupplier["PartyId"].ToString().Replace(" ", "");
                        drSeller["SellerId"] = rowSupplier["PartyId"].ToString().Replace(" ", "");
                    }
                    if (dsMeddelandeFil.Tables["Supplier"].Columns["Name"] != null) drSeller["SellerName"] = rowSupplier["Name"];
                    if (dsMeddelandeFil.Tables["Supplier"].Columns["OrgNumber"] != null) drSeller["SellerOrganisationNumber"] = rowSupplier["OrgNumber"];
                    if (dsMeddelandeFil.Tables["Supplier"].Columns["VatId"] != null) drSeller["SellerVatNumber"] = rowSupplier["VatId"];

                    if (dsMeddelandeFil.Tables["PostalAddress"] != null && dsMeddelandeFil.Tables["PostalAddress"].Columns["Supplier_Id"] != null)
                    {
                        foreach (DataRow rowPostalAddress in dsMeddelandeFil.Tables["PostalAddress"].Rows)
                        {
                            if (rowSupplier["Supplier_Id"].ToString() != rowPostalAddress["Supplier_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["Name"] != null && drSeller["SellerName"].ToString() == "") drSeller["SellerName"] = rowPostalAddress["Name"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["Address1"] != null) drSeller["SellerAddress"] = rowPostalAddress["Address1"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["PostalCode"] != null) drSeller["SellerPostalCode"] = rowPostalAddress["PostalCode"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["PostalDistrict"] != null) drSeller["SellerPostalAddress"] = rowPostalAddress["PostalDistrict"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["CountryCode"] != null) drSeller["SellerCountryCode"] = rowPostalAddress["CountryCode"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactInformation"] != null && dsMeddelandeFil.Tables["ContactInformation"].Columns["Supplier_Id"] != null)
                    {
                        foreach (DataRow rowContactInformation in dsMeddelandeFil.Tables["ContactInformation"].Rows)
                        {
                            if (rowSupplier["Supplier_Id"].ToString() != rowContactInformation["Supplier_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["PhoneNumber"] != null) drSeller["SellerPhone"] = rowContactInformation["PhoneNumber"];
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["FaxNumber"] != null) drSeller["SellerFax"] = rowContactInformation["FaxNumber"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactPerson"] != null && dsMeddelandeFil.Tables["ContactPerson"].Columns["Supplier_Id"] != null)
                    {
                        foreach (DataRow rowContactPerson in dsMeddelandeFil.Tables["ContactPerson"].Rows)
                        {
                            if (rowSupplier["Supplier_Id"].ToString() != rowContactPerson["Supplier_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactPerson"].Columns["Name"] != null) drSeller["SellerReference"] = rowContactPerson["Name"];
                            if (dsMeddelandeFil.Tables["ContactInformation"] != null && dsMeddelandeFil.Tables["ContactInformation"].Columns["ContactPerson_Id"] != null)
                            {
                                foreach (DataRow rowContactInformation in dsMeddelandeFil.Tables["ContactInformation"].Rows)
                                {
                                    if (rowContactPerson["ContactPerson_Id"].ToString() != rowContactInformation["ContactPerson_Id"].ToString()) continue;
                                    if (dsMeddelandeFil.Tables["ContactInformation"].Columns["PhoneNumber"] != null) drSeller["SellerReferencePhone"] = rowContactInformation["PhoneNumber"];
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            //Köpare 
            if (dsMeddelandeFil.Tables["Buyer"] != null)
            {
                foreach (DataRow rowBuyer in dsMeddelandeFil.Tables["Buyer"].Rows)
                {
                    if (InvoiceHeader_Id != rowBuyer["InvoiceHeader_Id"].ToString()) continue;
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["PartyId"] != null) drBuyer["BuyerId"] = rowBuyer["PartyId"].ToString().Replace(" ", "");
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["Name"] != null) drBuyer["BuyerName"] = rowBuyer["Name"];
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["VatId"] != null) drBuyer["BuyerVatNumber"] = rowBuyer["VatId"];
                    if (dsMeddelandeFil.Tables["Buyer"].Columns["OrgNumber"] != null) drBuyer["BuyerOrganisationNumber"] = rowBuyer["OrgNumber"];

                    if (dsMeddelandeFil.Tables["PostalAddress"] != null && dsMeddelandeFil.Tables["PostalAddress"].Columns["Buyer_Id"] != null)
                    {
                        foreach (DataRow rowPostalAddress in dsMeddelandeFil.Tables["PostalAddress"].Rows)
                        {
                            if (rowBuyer["Buyer_Id"].ToString() != rowPostalAddress["Buyer_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["Name"] != null && drBuyer["BuyerName"].ToString() == "") drBuyer["BuyerName"] = rowPostalAddress["Name"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["Address1"] != null) drBuyer["BuyerAddress"] = rowPostalAddress["Address1"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["PostalCode"] != null) drBuyer["BuyerPostalCode"] = rowPostalAddress["PostalCode"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["PostalDistrict"] != null) drBuyer["BuyerPostalAddress"] = rowPostalAddress["PostalDistrict"];
                            if (dsMeddelandeFil.Tables["PostalAddress"].Columns["CountryCode"] != null) drBuyer["BuyerCountryCode"] = rowPostalAddress["CountryCode"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactInformation"] != null && dsMeddelandeFil.Tables["ContactInformation"].Columns["Buyer_Id"] != null)
                    {
                        foreach (DataRow rowContactInformation in dsMeddelandeFil.Tables["ContactInformation"].Rows)
                        {
                            if (rowBuyer["Buyer_Id"].ToString() != rowContactInformation["Buyer_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["PhoneNumber"] != null) drBuyer["BuyerPhone"] = rowContactInformation["PhoneNumber"];
                            if (dsMeddelandeFil.Tables["ContactInformation"].Columns["FaxNumber"] != null) drBuyer["BuyerFax"] = rowContactInformation["FaxNumber"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["ContactPerson"] != null && dsMeddelandeFil.Tables["ContactPerson"].Columns["Buyer_Id"] != null)
                    {
                        foreach (DataRow rowContactPerson in dsMeddelandeFil.Tables["ContactPerson"].Rows)
                        {
                            if (rowBuyer["Buyer_Id"].ToString() != rowContactPerson["Buyer_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["ContactPerson"].Columns["Name"] != null) drBuyer["BuyerReference"] = rowContactPerson["Name"];
                            break;
                        }
                    }
                    break;
                }
            }

            //Leveransadress
            if (dsMeddelandeFil.Tables["DeliveryPart"] != null && dsMeddelandeFil.Tables["DeliveryPart"].Columns["InvoiceHeader_Id"] != null &&
                dsMeddelandeFil.Tables["StreetAddress"] != null && dsMeddelandeFil.Tables["StreetAddress"].Columns["DeliveryPart_Id"] != null)
            {
                foreach (DataRow rowDeliveryPart in dsMeddelandeFil.Tables["DeliveryPart"].Rows)
                {
                    if (InvoiceHeader_Id != rowDeliveryPart["InvoiceHeader_Id"].ToString()) continue;
                    {
                        foreach (DataRow rowStreetAddress in dsMeddelandeFil.Tables["StreetAddress"].Rows)
                        {
                            if (rowDeliveryPart["DeliveryPart_Id"].ToString() != rowStreetAddress["DeliveryPart_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Name"] != null) drBuyer["BuyerDeliveryName"] = rowStreetAddress["Name"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["Address1"] != null) drBuyer["BuyerDeliveryAddress"] = rowStreetAddress["Address1"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalCode"] != null) drBuyer["BuyerDeliveryPostalCode"] = rowStreetAddress["PostalCode"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["PostalDistrict"] != null) drBuyer["BuyerDeliveryPostalAddress"] = rowStreetAddress["PostalDistrict"];
                            if (dsMeddelandeFil.Tables["StreetAddress"].Columns["CountryCode"] != null) drBuyer["BuyerDeliveryCountryCode"] = rowStreetAddress["CountryCode"];
                            break;
                        }
                    }
                    break;
                }
            }

            //Huvud
            if (dsMeddelandeFil.Tables["Invoicee"] != null && dsMeddelandeFil.Tables["AccountInformation"] != null && dsMeddelandeFil.Tables["AccountInformation"].Columns["Invoicee_Id"] != null)
            {
                foreach (DataRow rowInvoicee in dsMeddelandeFil.Tables["Invoicee"].Rows)
                {
                    if (InvoiceHeader_Id != rowInvoicee["InvoiceHeader_Id"].ToString()) continue;
                    foreach (DataRow rowAccountInformation in dsMeddelandeFil.Tables["AccountInformation"].Rows)
                    {
                        if (rowInvoicee["Invoicee_Id"].ToString() != rowAccountInformation["Invoicee_Id"].ToString()) continue;
                        if (dsMeddelandeFil.Tables["AccountInformation"].Columns["IbanNumber"] != null) drHead["HeadIbanNumber"] = rowAccountInformation["IbanNumber"];
                        if (dsMeddelandeFil.Tables["AccountInformation"].Columns["SwiftNumber"] != null) drHead["HeadBicAddress"] = rowAccountInformation["SwiftNumber"];
                        break;
                    }
                    break;
                }
            }

            if (dsMeddelandeFil.Tables["InvoiceReferences"] != null && dsMeddelandeFil.Tables["InvoiceReferences"].Columns["InvoiceHeader_Id"] != null)
            {
                foreach (DataRow rowInvoiceReferences in dsMeddelandeFil.Tables["InvoiceReferences"].Rows)
                {
                    if (InvoiceHeader_Id != rowInvoiceReferences["InvoiceHeader_Id"].ToString()) continue;
                    if (dsMeddelandeFil.Tables["InvoiceReferences"].Columns["BuyersProjectCode"] != null) drHead["HeadBuyerProjectNumber"] = rowInvoiceReferences["BuyersProjectCode"];
                    if (dsMeddelandeFil.Tables["InvoiceReferences"].Columns["BuyersReference"] != null) drHead["HeadBuyerOrderNumber"] = rowInvoiceReferences["BuyersReference"];
                    if (dsMeddelandeFil.Tables["InvoiceReferences"].Columns["SuppliersOrderNumber"] != null) drHead["HeadSellerOrderNumber"] = rowInvoiceReferences["SuppliersOrderNumber"];
                    break;
                }
            }

            //Leveransvillkor
            if (dsMeddelandeFil.Tables["Payment"] != null)
            {
                foreach (DataRow rowPayment in dsMeddelandeFil.Tables["Payment"].Rows)
                {
                    if (InvoiceHeader_Id != rowPayment["InvoiceHeader_Id"].ToString()) continue;
                    if (dsMeddelandeFil.Tables["Payment"].Columns["DueDate"] != null) drHead["HeadInvoiceDueDate"] = rowPayment["DueDate"];
                    if (dsMeddelandeFil.Tables["Payment"].Columns["Currency"] != null) drHead["HeadCurrencyCode"] = rowPayment["Currency"];
                    if (dsMeddelandeFil.Tables["Payment"].Columns["KidNumber"] != null) drHead["HeadInvoiceOcr"] = rowPayment["KidNumber"];
                    if (dsMeddelandeFil.Tables["Payment"].Columns["PaymentTerms"] != null) drHead["HeadPaymentConditionText"] = rowPayment["PaymentTerms"];
                    if (dsMeddelandeFil.Tables["Payment"].Columns["OverDuePercent"] != null) drHead["HeadInterestPaymentPercent"] = rowPayment["OverDuePercent"];
                    break;
                }
            }

            if (dsMeddelandeFil.Tables["InvoiceSummary"] != null && dsMeddelandeFil.Tables["InvoiceTotals"] != null &&
                dsMeddelandeFil.Tables["InvoiceTotals"].Columns["InvoiceSummary_Id"] != null)
            {
                foreach (DataRow rowInvoiceSummary in dsMeddelandeFil.Tables["InvoiceSummary"].Rows)
                {
                    if (InvoiceHeader_Id != rowInvoiceSummary["Invoice_Id"].ToString()) continue;
                    foreach (DataRow rowInvoiceTotals in dsMeddelandeFil.Tables["InvoiceTotals"].Rows)
                    {
                        if (rowInvoiceSummary["InvoiceSummary_Id"].ToString() != rowInvoiceTotals["InvoiceSummary_Id"].ToString()) continue;
                        if (dsMeddelandeFil.Tables["InvoiceTotals"].Columns["NetAmount"] != null) drHead["HeadInvoiceNetAmount"] = rowInvoiceTotals["NetAmount"];
                        if (dsMeddelandeFil.Tables["InvoiceTotals"].Columns["VatTotalsAmount"] != null) drHead["HeadVatAmount"] = rowInvoiceTotals["VatTotalsAmount"];
                        if (dsMeddelandeFil.Tables["InvoiceTotals"].Columns["DiscountTotalsAmount"] != null) drHead["HeadDiscountAmount"] = rowInvoiceTotals["DiscountTotalsAmount"];
                        if (dsMeddelandeFil.Tables["InvoiceTotals"].Columns["RoundingAmount"] != null) drHead["HeadRoundingAmount"] = rowInvoiceTotals["RoundingAmount"];
                        if (dsMeddelandeFil.Tables["InvoiceTotals"].Columns["GrossAmount"] != null) drHead["HeadInvoiceGrossAmount"] = rowInvoiceTotals["GrossAmount"];
                        break;
                    }
                    break;
                }
            }

            if (dsMeddelandeFil.Tables["InvoiceDiscountChargesAndTax"] != null && dsMeddelandeFil.Tables["InvoiceChargesInfo"] != null &&
                dsMeddelandeFil.Tables["InvoiceChargesInfo"].Columns["InvoiceDiscountChargesAndTax_Id"] != null &&
                dsMeddelandeFil.Tables["InvoiceChargesInfo"].Columns["Code"] != null && dsMeddelandeFil.Tables["InvoiceChargesInfo"].Columns["Amount"] != null)
            {
                foreach (DataRow rowInvoiceDiscountChargesAndTax in dsMeddelandeFil.Tables["InvoiceDiscountChargesAndTax"].Rows)
                {
                    if (InvoiceHeader_Id != rowInvoiceDiscountChargesAndTax["Invoice_Id"].ToString()) continue;
                    {
                        foreach (DataRow rowInvoiceChargesInfo in dsMeddelandeFil.Tables["InvoiceChargesInfo"].Rows)
                        {
                            if (rowInvoiceDiscountChargesAndTax["InvoiceDiscountChargesAndTax_Id"].ToString() != rowInvoiceChargesInfo["InvoiceDiscountChargesAndTax_Id"].ToString()) continue;
                            if (rowInvoiceChargesInfo["Code"].ToString() == "FC")
                                drHead["HeadFreightFeeAmount"] = rowInvoiceChargesInfo["Amount"];
                            else
                                if (rowInvoiceChargesInfo["Code"].ToString() == "IS")
                                    drHead["HeadDespatchFeeAmount"] = rowInvoiceChargesInfo["Amount"];
                                else
                                    if (rowInvoiceChargesInfo["Code"].ToString() == "IN")
                                        drHead["HeadInsuranceFeeAmount"] = rowInvoiceChargesInfo["Amount"];
                        }
                    }
                    break;
                }
            }

            //Rader
            if (dsMeddelandeFil.Tables["InvoiceDetails"] == null)
                return "";

            string InvoiceDetails = "";
            foreach (DataRow rowInvoiceDetails in dsMeddelandeFil.Tables["InvoiceDetails"].Rows)
            {
                if (drInvoice["Invoice_Id"].ToString() != rowInvoiceDetails["Invoice_Id"].ToString()) continue;
                InvoiceDetails = drInvoice["Invoice_Id"].ToString();
                break;
            }

            if (dsMeddelandeFil.Tables["BaseItemDetails"] != null && dsMeddelandeFil.Tables["BaseItemDetails"].Columns["InvoiceDetails_Id"] != null)
            {
                foreach (DataRow rowBaseItemDetails in dsMeddelandeFil.Tables["BaseItemDetails"].Rows)
                {
                    if (InvoiceDetails != rowBaseItemDetails["InvoiceDetails_Id"].ToString()) continue;
                    DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                    dsStandardMall.Tables["Row"].Rows.Add(drRow);

                    if (dsMeddelandeFil.Tables["ProductIdentification"] != null && dsMeddelandeFil.Tables["ProductIdentification"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowProductIdentification in dsMeddelandeFil.Tables["ProductIdentification"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowProductIdentification["BaseItemDetails_Id"].ToString()) continue;
                            if (rowProductIdentification["ProductIdCode"].ToString() == "ELNR") drRow["RowSellerArticleNumber"] = rowProductIdentification["ProductIdentification_Text"];
                            break;
                        }
                    }
                    if (dsMeddelandeFil.Tables["BaseItemDetails"].Columns["Description"] != null) drRow["RowSellerArticleDescription1"] = rowBaseItemDetails["Description"];
                    if (dsMeddelandeFil.Tables["BaseItemDetails"].Columns["LineItemAmount"] != null) drRow["RowNetAmount"] = rowBaseItemDetails["LineItemAmount"];

                    if (dsMeddelandeFil.Tables["InvoicedQuantity"] != null && dsMeddelandeFil.Tables["InvoicedQuantity"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowInvoicedQuantity in dsMeddelandeFil.Tables["InvoicedQuantity"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowInvoicedQuantity["BaseItemDetails_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["InvoicedQuantity"].Columns["Quantity"] != null) drRow["RowQuantity"] = rowInvoicedQuantity["Quantity"];
                            if (dsMeddelandeFil.Tables["InvoicedQuantity"].Columns["UnitOfMeasure"] != null) drRow["RowUnitCode"] = rowInvoicedQuantity["UnitOfMeasure"];
                            if (dsMeddelandeFil.Tables["InvoicedQuantity"].Columns["UnitPrice"] != null) drRow["RowUnitPrice"] = rowInvoicedQuantity["UnitPrice"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["DeliveryInformation"] != null && dsMeddelandeFil.Tables["DeliveredQuantity"] != null && dsMeddelandeFil.Tables["DeliveredQuantity"].Columns["DeliveryInformation_Id"] != null &&
                        dsMeddelandeFil.Tables["QuantityDate"] != null && dsMeddelandeFil.Tables["QuantityDate"].Columns["DateType"] != null && dsMeddelandeFil.Tables["QuantityDate"].Columns["DeliveredQuantity_Id"] != null &&
                        dsMeddelandeFil.Tables["QuantityDate"].Columns["Date"] != null)
                    {
                        foreach (DataRow rowDeliveryInformation in dsMeddelandeFil.Tables["DeliveryInformation"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowDeliveryInformation["BaseItemDetails_Id"].ToString()) continue;
                            foreach (DataRow rowDeliveredQuantity in dsMeddelandeFil.Tables["DeliveredQuantity"].Rows)
                            {
                                if (rowDeliveryInformation["DeliveryInformation_Id"].ToString() != rowDeliveredQuantity["DeliveryInformation_Id"].ToString()) continue;
                                foreach (DataRow rowQuantityDate in dsMeddelandeFil.Tables["QuantityDate"].Rows)
                                {
                                    if (rowDeliveredQuantity["DeliveredQuantity_Id"].ToString() != rowQuantityDate["DeliveredQuantity_Id"].ToString()) continue;
                                    if (rowQuantityDate["DateType"].ToString() != "69") continue;
                                    drRow["RowDeliveryDate"] = rowQuantityDate["Date"];
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["VatInfo"] != null && dsMeddelandeFil.Tables["VatInfo"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowVatInfo in dsMeddelandeFil.Tables["VatInfo"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowVatInfo["BaseItemDetails_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["VatInfo"].Columns["VatPercent"] != null) drRow["RowVatPercentage"] = rowVatInfo["VatPercent"];
                            if (dsMeddelandeFil.Tables["VatInfo"].Columns["VatAmount"] != null) drRow["RowVatAmount"] = rowVatInfo["VatAmount"];
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["Discount"] != null && dsMeddelandeFil.Tables["Discount"].Columns["BaseItemDetails_Id"] != null)
                    {
                        decimal BaseAmount = 0;
                        decimal Amount = 0;
                        int Antal = 0;
                        bool BaseAmountFinns = true;
                        foreach (DataRow rowDiscount in dsMeddelandeFil.Tables["Discount"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowDiscount["BaseItemDetails_Id"].ToString()) continue;
                            Antal++;
                            if (dsMeddelandeFil.Tables["Discount"].Columns["BaseAmount"] != null)
                                if (rowDiscount["BaseAmount"].ToString() == "")
                                    BaseAmountFinns = false;
                                else
                                    BaseAmount = BaseAmount + Convert.ToDecimal(rowDiscount["BaseAmount"].ToString().Replace(".", ","));
                            if (dsMeddelandeFil.Tables["Discount"].Columns["Amount"] != null && rowDiscount["Amount"].ToString() != "")
                                Amount = Amount + Convert.ToDecimal(rowDiscount["Amount"].ToString().Replace(".", ","));
                            if (Antal == 1)
                            {
                                if (dsMeddelandeFil.Tables["Discount"].Columns["Percent"] != null) drRow["RowDiscountPercent1"] = rowDiscount["Percent"];
                                if (dsMeddelandeFil.Tables["Discount"].Columns["Amount"] != null) drRow["RowDiscountAmount1"] = rowDiscount["Amount"];
                            }
                            else
                                if (Antal == 2)
                                {
                                    if (dsMeddelandeFil.Tables["Discount"].Columns["Percent"] != null) drRow["RowDiscountPercent2"] = rowDiscount["Percent"];
                                    if (dsMeddelandeFil.Tables["Discount"].Columns["Amount"] != null) drRow["RowDiscountAmount2"] = rowDiscount["Amount"];
                                }
                        }
                        if (Antal == 1)
                        {
                            drRow["RowDiscountPercent"] = drRow["RowDiscountPercent1"];
                            drRow["RowDiscountAmount"] = drRow["RowDiscountAmount1"];
                        }
                        else
                            if (Antal > 1)
                            {
                                if (BaseAmountFinns == true) drRow["RowDiscountPercent"] = Amount / BaseAmount;
                                drRow["RowDiscountAmount"] = Amount;
                            }
                    }

                    if (dsMeddelandeFil.Tables["OrderInformation"] != null && dsMeddelandeFil.Tables["OrderInformation"].Columns["BaseItemDetails_Id"] != null)
                    {
                        foreach (DataRow rowOrderInformation in dsMeddelandeFil.Tables["OrderInformation"].Rows)
                        {
                            if (rowBaseItemDetails["BaseItemDetails_Id"].ToString() != rowOrderInformation["BaseItemDetails_Id"].ToString()) continue;
                            if (dsMeddelandeFil.Tables["OrderInformation"].Columns["BuyersOrderNumber"] != null) drRow["RowBuyerReference"] = rowOrderInformation["BuyersOrderNumber"];
                            break;
                        }
                    }
                }
            }

            return "";
        }



        protected override IEnumerable<string> ConvertMessage(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, DataRow SenderRow, string fileContent)
        {
            this.Nelfo5(InputFolderFileName, WholesaleTempFolder, dsStandardMall, drEdiSettings, fileContent);
            return this.ParsedMessages;
        }
    }
}
