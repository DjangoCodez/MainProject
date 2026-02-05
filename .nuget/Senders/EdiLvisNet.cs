using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.EdiAdmin.Business.Interfaces;
using System.Xml;

namespace SoftOne.EdiAdmin.Business.Senders
{
    class EdiLvisNet : EdiSenderOldBase
    {
        private bool doFileOperations = false;
        List<string> parsedMessages = new List<string>();

        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        private string ErrorMessage = "";
        private int MsgNr = 0;
        private bool ReturnCode = true;

        //LVIS Net
        public bool LvisNet(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, string fileContent)
        {
            string InputFileName; // = InputFolderFileName.Replace(@WholesaleTempFolder + "\\", "");
            var streamReader = EdiDiverse.GetStreamReaderFromContentOrFile(InputFolderFileName, WholesaleTempFolder, fileContent, out this.doFileOperations, out InputFileName);
            
            MsgNr = 0;

            DataSet dsMeddelandeFil = new DataSet();
            try
            {
                dsMeddelandeFil.ReadXml(streamReader);
            }
            catch
            {
                string MailSubject = "[LN-1] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " innehåller felaktigt Xml-format";
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                return false;
            }

            if (dsMeddelandeFil.Tables["Desadv"] == null)
            {
                string MailSubject = "[LN-2] Fel vid överföring från grossist";
                string MailMessage = "Meddelandefilen: " + InputFileName + " Taggen 'Desadv' finns i meddelandet"; ;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                return false;
            }

            //Orderbekräftelse
            if (dsMeddelandeFil.Tables["Desadv"] != null)
            {
                //if (dsMeddelandeFil.Tables["OrderResponse"].Columns["MessageType"] == null)
                //{
                //    string MailSubject = "[LN-2] Fel vid överföring från grossist";
                //    string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'MessageType'";
                //    EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                //    EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                //    return false;
                //}
                //else
                //    if (dsMeddelandeFil.Tables["OrderResponse"].Rows[0]["MessageType"].ToString() != "OrderRespons")
                //    {
                //        string MailSubject = "[LN-2] Fel vid överföring från grossist";
                //        string MailMessage = "Meddelandefilen: " + InputFileName + " Taggen 'MessageType' är inte = 'OrderRespons'";
                //        EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                //        EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                //        return false;
                //    }
                //    else
                {
                    foreach (DataRow rowDesadv in dsMeddelandeFil.Tables["Desadv"].Rows)
                    {
                        foreach (DataTable tb in dsStandardMall.Tables) tb.Rows.Clear();
                        MsgNr++;
                        ErrorMessage = EdiLvisNetOrderbekräftelse(dsMeddelandeFil, dsStandardMall, rowDesadv);
                        EdiLvisNetSkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                    }
                }
            }
            //else
            //Faktura
            if (dsMeddelandeFil.Tables["Invoic"] != null)
            {
                //        if (dsMeddelandeFil.Tables["Invoice"].Columns["MessageType"] == null)
                //        {
                //            string MailSubject = "[LN-2] Fel vid överföring från grossist";
                //            string MailMessage = "Meddelandefilen: " + InputFileName + " Saknar Taggen 'MessageType'";
                //            EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                //            EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                //            return false;
                //        }
                //        else
                //            if (dsMeddelandeFil.Tables["Invoice"].Rows[0]["MessageType"].ToString() != "Invoice")
                //            {
                //                string MailSubject = "[LN-2] Fel vid överföring från grossist";
                //                string MailMessage = "Meddelandefilen: " + InputFileName + " Taggen 'MessageType' är inte = 'Invoice'";
                //                EdiDiverseKlass.SkickaMailInternt(drEdiSettings["EmailAddress"].ToString(), MailSubject, MailMessage);
                //                EdiDiverseKlass.LagraEdiLog(SOP_Connection, dsMain, MailSubject, MailMessage);
                //                return false;
                //            }
                //            else
                {
                    foreach (DataRow rowInvoice in dsMeddelandeFil.Tables["Invoice"].Rows)
                    {
                        foreach (DataTable tb in dsStandardMall.Tables) tb.Rows.Clear();
                        MsgNr++;
                        ErrorMessage = EdiLvisNetFaktura(dsMeddelandeFil, dsStandardMall, rowInvoice);
                        EdiLvisNetSkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                    }
                }
            }

            return ReturnCode;
        }

        private void EdiLvisNetSkrivMeddelande(string InputFolderFileName, string InputFileName, string WholesaleTempFolder, Dictionary<string, string> drEdiSettings, DataSet dsStandardMall)
        {
            string OutputFolderFileName = InputFolderFileName.Replace(".edi", "") + "_" + MsgNr + ".xml";
            if (ErrorMessage == "")
            {
                if (!this.doFileOperations)
                {
                    parsedMessages.Add(dsStandardMall.GetXml());
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
                    string MailSubject = "[LN-4] Fel vid överföring från grossist - Meddelandefil: " + InputFileName + ". Avser meddelande: " + MsgNr + " i filen";
                    string MailMessage = "Det gick inte att kopiera filen " + UploadFileName + " till Temporärmappen: " + drEdiSettings["MsgTempFolder"].ToString();
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    ReturnCode = false;
                }
            }
            else
            {
                string MailSubject = "[LN-5] Fel vid överföring från grossist - Meddelandefil: " + InputFileName + ". Avser meddelande: " + MsgNr + " i filen"; ;
                string MailMessage = ErrorMessage;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                ReturnCode = false;
            }

            try 
            { 
                if(doFileOperations)
                    File.Delete(OutputFolderFileName); 
            }
            catch
            {
                string MailSubject = "[LN-6] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
            }

        }

        private string GetNumeric(string value)
        {
            //Function to get just ordernumber from f.e (200011 / Sakke)
            if (value.Contains("/"))
                value = value.Substring(0, value.LastIndexOf("/"));
            value = value.Replace("/", "");
            value = value.Trim();

            return value;
        }

        private string EdiLvisNetOrderbekräftelse(DataSet dsMeddelandeFil, DataSet dsStandardMall, DataRow drDesadv)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            string Desadv_Id = drDesadv["Desadv_Id"].ToString(); ;

            drMessage["MessageType"] = "ORDERBEKR";

            if (dsMeddelandeFil.Tables["Date"] != null && dsMeddelandeFil.Tables["Date"].Columns["Of"] != null && dsMeddelandeFil.Tables["Date"].Columns["Date_Text"] != null)
            {
                foreach (DataRow rowDate in dsMeddelandeFil.Tables["Date"].Rows)
                {
                    if (Desadv_Id != rowDate["Desadv_Id"].ToString()) continue;
                    if (rowDate["Of"].ToString() == "136")
                    {
                        drMessage["MessageDate"] = rowDate["Date_Text"].ToString();
                        break;
                    }
                }
            }

            if (drMessage["MessageDate"].ToString() == "")
                drMessage["MessageDate"] = DateTime.Now.Date.ToShortDateString();

            //Leverantör
            if (dsMeddelandeFil.Tables["Party"] != null)
            {
                foreach (DataRow rowParty in dsMeddelandeFil.Tables["Party"].Rows)
                {
                    if (Desadv_Id != rowParty["Desadv_Id"].ToString()) continue;
                    if (rowParty["Qualifier"].ToString() != "SE") continue;
                    string Party_Id = rowParty["Party_Id"].ToString(); ;

                    if (dsMeddelandeFil.Tables["PartyID"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["CodeList"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["PartyID_Text"] != null)
                    {
                        foreach (DataRow rowPartyID in dsMeddelandeFil.Tables["PartyID"].Rows)
                        {
                            if (Party_Id != rowPartyID["Party_Id"].ToString()) continue;
                            if (rowPartyID["CodeList"].ToString() != "100") continue;
                            string SellerId = rowPartyID["PartyID_Text"].ToString();
                            if (SellerId.Substring(0, 4) == "0037")
                                SellerId = SellerId.Substring(4);
                            if (SellerId.Length > 1)
                                SellerId = SellerId.Substring(0, SellerId.Length - 1) + "-" + SellerId.Substring(SellerId.Length - 1, 1);
                            drMessage["MessageSenderId"] = SellerId;
                            drSeller["SellerId"] = SellerId;
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["Name"] != null && dsMeddelandeFil.Tables["Name"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    {
                        foreach (DataRow rowName in dsMeddelandeFil.Tables["Name"].Rows)
                        {
                            if (Party_Id != rowName["Party_Id"].ToString()) continue;
                            if (rowName["Qualifier"].ToString() == "1") drSeller["SellerName"] = rowName["Name_Text"].ToString();
                            if (rowName["Qualifier"].ToString() == "2") drSeller["SellerReference"] = rowName["Name_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Address"] != null && dsMeddelandeFil.Tables["Address"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Address"].Columns["Address_Text"] != null)
                    {
                        foreach (DataRow rowAddress in dsMeddelandeFil.Tables["Address"].Rows)
                        {
                            if (Party_Id != rowAddress["Party_Id"].ToString()) continue;
                            if (rowAddress["Qualifier"].ToString() == "Street") drSeller["SellerAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Postcode") drSeller["SellerPostalCode"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "City") drSeller["SellerPostalAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Country") drSeller["SellerCountryCode"] = rowAddress["Address_Text"].ToString();
                        }
                    }

                    if (dsMeddelandeFil.Tables["Contact"] != null && dsMeddelandeFil.Tables["Contact"].Columns["ContactFunction"] != null && dsMeddelandeFil.Tables["Contact"].Columns["Contact_Text"] != null)
                    {
                        foreach (DataRow rowContact in dsMeddelandeFil.Tables["Contact"].Rows)
                        {
                            if (Party_Id != rowContact["Party_Id"].ToString()) continue;
                            if (rowContact["ContactFunction"].ToString() == "IC") drSeller["SellerReference"] = rowContact["Contact_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Communication"] != null && dsMeddelandeFil.Tables["Communication"].Columns["ChannelQualifier"] != null && dsMeddelandeFil.Tables["Communication"].Columns["Communication_Text"] != null)
                    {
                        foreach (DataRow rowCommunication in dsMeddelandeFil.Tables["Communication"].Rows)
                        {
                            if (Party_Id != rowCommunication["Party_Id"].ToString()) continue;
                            if (rowCommunication["ChannelQualifier"].ToString() == "TE") drSeller["SellerPhone"] = rowCommunication["Communication_Text"].ToString();
                            if (rowCommunication["ChannelQualifier"].ToString() == "FX") drSeller["SellerFax"] = rowCommunication["Communication_Text"].ToString();
                            //if (rowCommunication["ChannelQualifier"].ToString() == "EM") drSeller["SellerEmail finns inte"] = rowCommunication["Communication_Text"].ToString();
                        }
                    }
                }
            }

            //Köpare 
            if (dsMeddelandeFil.Tables["Party"] != null)
            {
                foreach (DataRow rowParty in dsMeddelandeFil.Tables["Party"].Rows)
                {
                    if (Desadv_Id != rowParty["Desadv_Id"].ToString()) continue;
                    if (rowParty["Qualifier"].ToString() != "BY") continue;
                    string Party_Id = rowParty["Party_Id"].ToString(); ;

                    if (dsMeddelandeFil.Tables["PartyID"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["CodeList"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["PartyID_Text"] != null)
                    {
                        foreach (DataRow rowPartyID in dsMeddelandeFil.Tables["PartyID"].Rows)
                        {
                            if (Party_Id != rowPartyID["Party_Id"].ToString()) continue;
                            if (rowPartyID["CodeList"].ToString() != "100") continue;
                            string BuyerId = rowPartyID["PartyID_Text"].ToString();
                            if (BuyerId.Substring(0, 4) == "0037")
                                BuyerId = BuyerId.Substring(4);
                            if (BuyerId.Length > 1)
                                BuyerId = BuyerId.Substring(0, BuyerId.Length - 1) + "-" + BuyerId.Substring(BuyerId.Length - 1, 1);
                            drBuyer["BuyerId"] = BuyerId;
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["Name"] != null && dsMeddelandeFil.Tables["Name"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    {
                        foreach (DataRow rowName in dsMeddelandeFil.Tables["Name"].Rows)
                        {
                            if (Party_Id != rowName["Party_Id"].ToString()) continue;
                            if (rowName["Qualifier"].ToString() == "1") drBuyer["BuyerName"] = rowName["Name_Text"].ToString();
                            if (rowName["Qualifier"].ToString() == "2") drBuyer["BuyerReference"] = rowName["Name_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Address"] != null && dsMeddelandeFil.Tables["Address"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Address"].Columns["Address_Text"] != null)
                    {
                        foreach (DataRow rowAddress in dsMeddelandeFil.Tables["Address"].Rows)
                        {
                            if (Party_Id != rowAddress["Party_Id"].ToString()) continue;
                            if (rowAddress["Qualifier"].ToString() == "Street") drBuyer["BuyerAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Postcode") drBuyer["BuyerPostalCode"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "City") drBuyer["BuyerPostalAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Country") drBuyer["BuyerCountryCode"] = rowAddress["Address_Text"].ToString();
                        }
                    }

                    if (dsMeddelandeFil.Tables["Contact"] != null && dsMeddelandeFil.Tables["Contact"].Columns["ContactFunction"] != null && dsMeddelandeFil.Tables["Contact"].Columns["Contact_Text"] != null)
                    {
                        foreach (DataRow rowContact in dsMeddelandeFil.Tables["Contact"].Rows)
                        {
                            if (Party_Id != rowContact["Party_Id"].ToString()) continue;
                            if (rowContact["ContactFunction"].ToString() == "IC") drBuyer["BuyerReference"] = rowContact["Contact_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Communication"] != null && dsMeddelandeFil.Tables["Communication"].Columns["ChannelQualifier"] != null && dsMeddelandeFil.Tables["Communication"].Columns["Communication_Text"] != null)
                    {
                        foreach (DataRow rowCommunication in dsMeddelandeFil.Tables["Communication"].Rows)
                        {
                            if (Party_Id != rowCommunication["Party_Id"].ToString()) continue;
                            if (rowCommunication["ChannelQualifier"].ToString() == "TE") drBuyer["BuyerPhone"] = rowCommunication["Communication_Text"].ToString();
                            if (rowCommunication["ChannelQualifier"].ToString() == "FX") drBuyer["BuyerFax"] = rowCommunication["Communication_Text"].ToString();
                            //if (rowCommunication["ChannelQualifier"].ToString() == "EM") drBuyer["BuyerEmail finns inte"] = rowCommunication["Communication_Text"].ToString();
                        }
                    }
                }
            }

            //Leveransadress
            if (dsMeddelandeFil.Tables["Party"] != null)
            {
                foreach (DataRow rowParty in dsMeddelandeFil.Tables["Party"].Rows)
                {
                    if (Desadv_Id != rowParty["Desadv_Id"].ToString()) continue;
                    if (rowParty["Qualifier"].ToString() != "DP") continue;
                    string Party_Id = rowParty["Party_Id"].ToString(); ;

                    if (dsMeddelandeFil.Tables["Name"] != null && dsMeddelandeFil.Tables["Name"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    {
                        foreach (DataRow rowName in dsMeddelandeFil.Tables["Name"].Rows)
                        {
                            if (Party_Id != rowName["Party_Id"].ToString()) continue;
                            if (rowName["Qualifier"].ToString() == "1") drBuyer["BuyerDeliveryName"] = rowName["Name_Text"].ToString();
                        }
                    }

                    if (dsMeddelandeFil.Tables["Address"] != null && dsMeddelandeFil.Tables["Address"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Address"].Columns["Address_Text"] != null)
                    {
                        foreach (DataRow rowAddress in dsMeddelandeFil.Tables["Address"].Rows)
                        {
                            if (Party_Id != rowAddress["Party_Id"].ToString()) continue;
                            if (rowAddress["Qualifier"].ToString() == "Street") drBuyer["BuyerDeliveryAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Postcode") drBuyer["BuyerDeliveryPostalCode"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "City") drBuyer["BuyerDeliveryPostalAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Country") drBuyer["BuyerDeliveryCountryCode"] = rowAddress["Address_Text"].ToString();
                        }
                    }
                }
            }

            //Huvud
            if (dsMeddelandeFil.Tables["Reference"] != null && dsMeddelandeFil.Tables["Reference"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Reference"].Columns["Reference_Text"] != null)
            {
                foreach (DataRow rowReference in dsMeddelandeFil.Tables["Reference"].Rows)
                {
                    if (Desadv_Id != rowReference["Desadv_Id"].ToString()) continue;
                    //if (rowReference["Qualifier"].ToString() == "CO") drHead["HeadBuyerProjectNumber"] = rowReference["Reference_Text"];
                    //if (rowReference["Qualifier"].ToString() == "AEP") drHead["HeadBuyerOrderNumber"] = rowReference["Reference_Text"];
                    if (rowReference["Qualifier"].ToString() == "JB") drHead["HeadBuyerProjectNumber"] = rowReference["Reference_Text"];
                    if (rowReference["Qualifier"].ToString() == "CR") drHead["HeadBuyerOrderNumber"] = GetNumeric(rowReference["Reference_Text"].ToString());
                    if (rowReference["Qualifier"].ToString() == "VN") drHead["HeadSellerOrderNumber"] = rowReference["Reference_Text"];
                }
            }

            //Orderrader
            if (dsMeddelandeFil.Tables["LineItem"] == null) return "";

            foreach (DataRow rowLineItem in dsMeddelandeFil.Tables["LineItem"].Rows)
            {
                if (Desadv_Id != rowLineItem["Desadv_Id"].ToString()) continue;
                string LineItem_Id = rowLineItem["LineItem_Id"].ToString();
                DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                dsStandardMall.Tables["Row"].Rows.Add(drRow);

                if (dsMeddelandeFil.Tables["LineItem"].Columns["UnitPrice"] != null) drRow["RowUnitPrice"] = rowLineItem["UnitPrice"];

                if (dsMeddelandeFil.Tables["ProductNo"] != null && dsMeddelandeFil.Tables["ProductNo"].Columns["CodeList"] != null && dsMeddelandeFil.Tables["ProductNo"].Columns["ProductNo_Text"] != null)
                {
                    foreach (DataRow rowProductNo in dsMeddelandeFil.Tables["ProductNo"].Rows)
                    {
                        if (LineItem_Id != rowProductNo["LineItem_Id"].ToString())
                            continue;
                        drRow["RowSellerArticleNumber"] = rowProductNo["ProductNo_Text"];
                        break;
                    }
                }

                if (dsMeddelandeFil.Tables["ProductInfo"] != null && dsMeddelandeFil.Tables["ProductInfo"].Columns["ItemNumber"] != null && dsMeddelandeFil.Tables["ProductInfo"].Columns["ProductInfo_Text"] != null)
                {
                    foreach (DataRow rowProductInfo in dsMeddelandeFil.Tables["ProductInfo"].Rows)
                    {
                        if (LineItem_Id != rowProductInfo["LineItem_Id"].ToString()) continue;
                        if (rowProductInfo["ItemNumber"].ToString() == "BP") drRow["RowBuyerArticleNumber"] = rowProductInfo["ProductInfo_Text"];
                        break;
                    }
                }

                if (dsMeddelandeFil.Tables["Article"] != null && dsMeddelandeFil.Tables["Article"].Columns["Article_Text"] != null)
                {
                    foreach (DataRow rowArticle in dsMeddelandeFil.Tables["Article"].Rows)
                    {
                        if (LineItem_Id != rowArticle["LineItem_Id"].ToString()) continue;
                        if (drRow["RowSellerArticleDescription1"].ToString() == "")
                            drRow["RowSellerArticleDescription1"] = rowArticle["Article_Text"];
                        else
                            drRow["RowSellerArticleDescription2"] = rowArticle["Article_Text"];
                    }
                }

                if (dsMeddelandeFil.Tables["Quantity"] != null && dsMeddelandeFil.Tables["Quantity"].Columns["MeasureQualifier"] != null && dsMeddelandeFil.Tables["Quantity"].Columns["Quantity_Text"] != null)
                {
                    foreach (DataRow rowQuantity in dsMeddelandeFil.Tables["Quantity"].Rows)
                    {
                        if (LineItem_Id != rowQuantity["LineItem_Id"].ToString()) continue;
                        if (rowQuantity["Qualifier"].ToString() == "46" || rowQuantity["Qualifier"].ToString() == "113")
                        {
                            drRow["RowUnitCode"] = rowQuantity["MeasureQualifier"];
                            drRow["RowQuantity"] = rowQuantity["Quantity_Text"];
                        }
                    }
                }

                if (dsMeddelandeFil.Tables["Taxes"] != null && dsMeddelandeFil.Tables["Taxes"].Columns["Category"] != null && dsMeddelandeFil.Tables["Taxes"].Columns["TaxesPCD"] != null)
                {
                    foreach (DataRow rowTaxes in dsMeddelandeFil.Tables["Taxes"].Rows)
                    {
                        if (LineItem_Id != rowTaxes["LineItem_Id"].ToString()) continue;
                        if (rowTaxes["Category"].ToString() == "S") drRow["RowVatPercentage"] = rowTaxes["TaxesPCD"];
                    }
                }

                if (dsMeddelandeFil.Tables["Date"] != null && dsMeddelandeFil.Tables["Date"].Columns["Of"] != null && dsMeddelandeFil.Tables["Date"].Columns["Date_Text"] != null)
                {
                    foreach (DataRow rowDate in dsMeddelandeFil.Tables["Date"].Rows)
                    {
                        if (rowDate.Table.Columns["LineItemId"] == null || LineItem_Id != rowDate["LineItem_Id"].ToString()) 
                            continue;
                        if (rowDate["Of"].ToString() == "136") 
                            drRow["RowDeliveryDate"] = rowDate["Date_Text"].ToString();
                        break;
                    }
                }

            }

            return "";
        }

        private string EdiLvisNetFaktura(DataSet dsMeddelandeFil, DataSet dsStandardMall, DataRow drInvoice)
        {

            // OBS! metoden för Faktura är inte klar


            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            string Invoice_Id = drInvoice["Invoice_Id"].ToString(); ;

            drMessage["MessageType"] = "INVOICE";

            if (dsMeddelandeFil.Tables["Date"] != null && dsMeddelandeFil.Tables["Date"].Columns["Of"] != null && dsMeddelandeFil.Tables["Date"].Columns["Date_Text"] != null)
            {
                foreach (DataRow rowDate in dsMeddelandeFil.Tables["Date"].Rows)
                {
                    if (Invoice_Id != rowDate["Invoice_Id"].ToString()) continue;
                    if (rowDate["Of"].ToString() == "136")
                    {
                        drMessage["MessageDate"] = rowDate["Date_Text"].ToString();
                        break;
                    }
                }
            }

            if (drMessage["MessageDate"].ToString() == "")
                drMessage["MessageDate"] = DateTime.Now.Date.ToShortDateString();

            //Leverantör
            if (dsMeddelandeFil.Tables["Party"] != null)
            {
                foreach (DataRow rowParty in dsMeddelandeFil.Tables["Party"].Rows)
                {
                    if (Invoice_Id != rowParty["Invoice_Id"].ToString()) continue;
                    if (rowParty["Qualifier"].ToString() != "SE") continue;
                    string Party_Id = rowParty["Party_Id"].ToString(); ;

                    if (dsMeddelandeFil.Tables["PartyID"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["CodeList"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["PartyID_Text"] != null)
                    {
                        foreach (DataRow rowPartyID in dsMeddelandeFil.Tables["PartyID"].Rows)
                        {
                            if (Party_Id != rowPartyID["Party_Id"].ToString()) continue;
                            //if (rowPartyID["CodeList"].ToString() == "100")
                            //{
                            //    string SellerId = rowPartyID["PartyID_Text"].ToString();
                            //    if (SellerId.Substring(0, 4) == "0037")
                            //        SellerId = SellerId.Substring(4);
                            //    if (SellerId.Length > 1)
                            //        SellerId = SellerId.Substring(0, SellerId.Length - 1) + "-" + SellerId.Substring(SellerId.Length - 1, 1);
                            //    drMessage["MessageSenderId"] = SellerId;
                            //    drSeller["SellerId"] = SellerId;
                            //    break;
                            //}
                            //eller om VAT skall användas
                            if (rowPartyID["CodeList"].ToString() == "VAT")
                            {
                                string SellerId = rowPartyID["PartyID_Text"].ToString().Replace("FI", "");
                                if (SellerId.Length > 1)
                                    SellerId = SellerId.Substring(0, SellerId.Length - 1) + "-" + SellerId.Substring(SellerId.Length - 1, 1);
                                drMessage["MessageSenderId"] = SellerId;
                                drSeller["SellerId"] = SellerId;
                                break;
                            }
                        }
                    }

                    if (dsMeddelandeFil.Tables["Name"] != null && dsMeddelandeFil.Tables["Name"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    {
                        foreach (DataRow rowName in dsMeddelandeFil.Tables["Name"].Rows)
                        {
                            if (Party_Id != rowName["Party_Id"].ToString()) continue;
                            if (rowName["Qualifier"].ToString() == "1") drSeller["SellerName"] = rowName["Name_Text"].ToString();
                            if (rowName["Qualifier"].ToString() == "2") drSeller["SellerReference"] = rowName["Name_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Address"] != null && dsMeddelandeFil.Tables["Address"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Address"].Columns["Address_Text"] != null)
                    {
                        foreach (DataRow rowAddress in dsMeddelandeFil.Tables["Address"].Rows)
                        {
                            if (Party_Id != rowAddress["Party_Id"].ToString()) continue;
                            if (rowAddress["Qualifier"].ToString() == "Street") drSeller["SellerAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Postcode") drSeller["SellerPostalCode"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "City") drSeller["SellerPostalAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Country") drSeller["SellerCountryCode"] = rowAddress["Address_Text"].ToString();
                        }
                    }

                    if (dsMeddelandeFil.Tables["Contact"] != null && dsMeddelandeFil.Tables["Contact"].Columns["ContactFunction"] != null && dsMeddelandeFil.Tables["Contact"].Columns["Contact_Text"] != null)
                    {
                        foreach (DataRow rowContact in dsMeddelandeFil.Tables["Contact"].Rows)
                        {
                            if (Party_Id != rowContact["Party_Id"].ToString()) continue;
                            if (rowContact["ContactFunction"].ToString() == "IC") drSeller["SellerReference"] = rowContact["Contact_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Communication"] != null && dsMeddelandeFil.Tables["Communication"].Columns["ChannelQualifier"] != null && dsMeddelandeFil.Tables["Communication"].Columns["Communication_Text"] != null)
                    {
                        foreach (DataRow rowCommunication in dsMeddelandeFil.Tables["Communication"].Rows)
                        {
                            if (Party_Id != rowCommunication["Party_Id"].ToString()) continue;
                            if (rowCommunication["ChannelQualifier"].ToString() == "TE") drSeller["SellerPhone"] = rowCommunication["Communication_Text"].ToString();
                            if (rowCommunication["ChannelQualifier"].ToString() == "FX") drSeller["SellerFax"] = rowCommunication["Communication_Text"].ToString();
                            //if (rowCommunication["ChannelQualifier"].ToString() == "EM") drSeller["SellerEmail finns inte"] = rowCommunication["Communication_Text"].ToString();
                        }
                    }
                }
            }

            //Köpare 
            if (dsMeddelandeFil.Tables["Party"] != null)
            {
                foreach (DataRow rowParty in dsMeddelandeFil.Tables["Party"].Rows)
                {
                    if (Invoice_Id != rowParty["Invoice_Id"].ToString()) continue;
                    if (rowParty["Qualifier"].ToString() != "BY") continue;
                    string Party_Id = rowParty["Party_Id"].ToString(); ;

                    if (dsMeddelandeFil.Tables["PartyID"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["CodeList"] != null && dsMeddelandeFil.Tables["PartyID"].Columns["PartyID_Text"] != null)
                    {
                        foreach (DataRow rowPartyID in dsMeddelandeFil.Tables["PartyID"].Rows)
                        {
                            if (Party_Id != rowPartyID["Party_Id"].ToString()) continue;
                            if (rowPartyID["CodeList"].ToString() != "100") continue;
                            string BuyerId = rowPartyID["PartyID_Text"].ToString();
                            if (BuyerId.Substring(0, 4) == "0037")
                                BuyerId = BuyerId.Substring(4);
                            if (BuyerId.Length > 1)
                                BuyerId = BuyerId.Substring(0, BuyerId.Length - 1) + "-" + BuyerId.Substring(BuyerId.Length - 1, 1);
                            drBuyer["BuyerId"] = BuyerId;
                            break;
                        }
                    }

                    if (dsMeddelandeFil.Tables["Name"] != null && dsMeddelandeFil.Tables["Name"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    {
                        foreach (DataRow rowName in dsMeddelandeFil.Tables["Name"].Rows)
                        {
                            if (Party_Id != rowName["Party_Id"].ToString()) continue;
                            if (rowName["Qualifier"].ToString() == "1") drBuyer["BuyerName"] = rowName["Name_Text"].ToString();
                            if (rowName["Qualifier"].ToString() == "2") drBuyer["BuyerReference"] = rowName["Name_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Address"] != null && dsMeddelandeFil.Tables["Address"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Address"].Columns["Address_Text"] != null)
                    {
                        foreach (DataRow rowAddress in dsMeddelandeFil.Tables["Address"].Rows)
                        {
                            if (Party_Id != rowAddress["Party_Id"].ToString()) continue;
                            if (rowAddress["Qualifier"].ToString() == "Street") drBuyer["BuyerAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Postcode") drBuyer["BuyerPostalCode"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "City") drBuyer["BuyerPostalAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Country") drBuyer["BuyerCountryCode"] = rowAddress["Address_Text"].ToString();
                        }
                    }

                    if (dsMeddelandeFil.Tables["Contact"] != null && dsMeddelandeFil.Tables["Contact"].Columns["ContactFunction"] != null && dsMeddelandeFil.Tables["Contact"].Columns["Contact_Text"] != null)
                    {
                        foreach (DataRow rowContact in dsMeddelandeFil.Tables["Contact"].Rows)
                        {
                            if (Party_Id != rowContact["Party_Id"].ToString()) continue;
                            if (rowContact["ContactFunction"].ToString() == "IC") drBuyer["BuyerReference"] = rowContact["Contact_Text"].ToString(); //?
                        }
                    }

                    if (dsMeddelandeFil.Tables["Communication"] != null && dsMeddelandeFil.Tables["Communication"].Columns["ChannelQualifier"] != null && dsMeddelandeFil.Tables["Communication"].Columns["Communication_Text"] != null)
                    {
                        foreach (DataRow rowCommunication in dsMeddelandeFil.Tables["Communication"].Rows)
                        {
                            if (Party_Id != rowCommunication["Party_Id"].ToString()) continue;
                            if (rowCommunication["ChannelQualifier"].ToString() == "TE") drBuyer["BuyerPhone"] = rowCommunication["Communication_Text"].ToString();
                            if (rowCommunication["ChannelQualifier"].ToString() == "FX") drBuyer["BuyerFax"] = rowCommunication["Communication_Text"].ToString();
                            //if (rowCommunication["ChannelQualifier"].ToString() == "EM") drBuyer["BuyerEmail finns inte"] = rowCommunication["Communication_Text"].ToString();
                        }
                    }
                }
            }

            //Leveransadress
            if (dsMeddelandeFil.Tables["Party"] != null)
            {
                foreach (DataRow rowParty in dsMeddelandeFil.Tables["Party"].Rows)
                {
                    if (Invoice_Id != rowParty["Invoice_Id"].ToString()) continue;
                    if (rowParty["Qualifier"].ToString() != "DP") continue;
                    string Party_Id = rowParty["Party_Id"].ToString(); ;

                    if (dsMeddelandeFil.Tables["Name"] != null && dsMeddelandeFil.Tables["Name"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Name"].Columns["Name_Text"] != null)
                    {
                        foreach (DataRow rowName in dsMeddelandeFil.Tables["Name"].Rows)
                        {
                            if (Party_Id != rowName["Party_Id"].ToString()) continue;
                            if (rowName["Qualifier"].ToString() == "1") drBuyer["BuyerDeliveryName"] = rowName["Name_Text"].ToString();
                        }
                    }

                    if (dsMeddelandeFil.Tables["Address"] != null && dsMeddelandeFil.Tables["Address"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Address"].Columns["Address_Text"] != null)
                    {
                        foreach (DataRow rowAddress in dsMeddelandeFil.Tables["Address"].Rows)
                        {
                            if (Party_Id != rowAddress["Party_Id"].ToString()) continue;
                            if (rowAddress["Qualifier"].ToString() == "Street") drBuyer["BuyerDeliveryAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Postcode") drBuyer["BuyerDeliveryPostalCode"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "City") drBuyer["BuyerDeliveryPostalAddress"] = rowAddress["Address_Text"].ToString();
                            if (rowAddress["Qualifier"].ToString() == "Country") drBuyer["BuyerDeliveryCountryCode"] = rowAddress["Address_Text"].ToString();
                        }
                    }
                }
            }

            //Huvud
            if (dsMeddelandeFil.Tables["Payment"] != null && dsMeddelandeFil.Tables["Payment"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Payment"].Columns["Payment_Text"] != null)
            {
                foreach (DataRow rowPayment in dsMeddelandeFil.Tables["Payment"].Rows)
                {
                    if (Invoice_Id != rowPayment["Invoice_Id"].ToString()) continue;
                    if (rowPayment["Qualifier"].ToString() == "CO") drHead["HeadBuyerProjectNumber"] = rowPayment["Payment_Text"];
                    if (rowPayment["Qualifier"].ToString() == "AEP") drHead["HeadBuyerOrderNumber"] = rowPayment["Payment_Text"];
                    if (rowPayment["Qualifier"].ToString() == "VN") drHead["HeadSellerOrderNumber"] = rowPayment["Payment_Text"];
                }
            }

            if (dsMeddelandeFil.Tables["Reference"] != null && dsMeddelandeFil.Tables["Reference"].Columns["Qualifier"] != null && dsMeddelandeFil.Tables["Reference"].Columns["Reference_Text"] != null)
            {
                foreach (DataRow rowReference in dsMeddelandeFil.Tables["Reference"].Rows)
                {
                    if (Invoice_Id != rowReference["Invoice_Id"].ToString()) continue;
                    //if (rowReference["Qualifier"].ToString() == "CO") drHead["HeadBuyerProjectNumber"] = rowReference["Reference_Text"];
                    //if (rowReference["Qualifier"].ToString() == "AEP") drHead["HeadBuyerOrderNumber"] = rowReference["Reference_Text"];
                    if (rowReference["Qualifier"].ToString() == "VN") drHead["HeadSellerOrderNumber"] = rowReference["Reference_Text"];
                }
            }

            //Invoicerows
            if (dsMeddelandeFil.Tables["LineItem"] == null) return "";

            foreach (DataRow rowLineItem in dsMeddelandeFil.Tables["LineItem"].Rows)
            {
                if (Invoice_Id != rowLineItem["Invoice_Id"].ToString()) continue;
                string LineItem_Id = rowLineItem["LineItem_Id"].ToString();
                DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
                dsStandardMall.Tables["Row"].Rows.Add(drRow);

                if (dsMeddelandeFil.Tables["LineItem"].Columns["UnitPrice"] != null) drRow["RowUnitPrice"] = rowLineItem["UnitPrice"];

                if (dsMeddelandeFil.Tables["ProductNo"] != null && dsMeddelandeFil.Tables["ProductNo"].Columns["CodeList"] != null && dsMeddelandeFil.Tables["ProductNo"].Columns["ProductNo_Text"] != null)
                {
                    foreach (DataRow rowProductNo in dsMeddelandeFil.Tables["ProductNo"].Rows)
                    {
                        if (LineItem_Id != rowProductNo["LineItem_Id"].ToString())
                            continue;
                        drRow["RowSellerArticleNumber"] = rowProductNo["ProductNo_Text"];
                        break;
                    }
                }

                if (dsMeddelandeFil.Tables["ProductInfo"] != null && dsMeddelandeFil.Tables["ProductInfo"].Columns["ItemNumber"] != null && dsMeddelandeFil.Tables["ProductInfo"].Columns["ProductInfo_Text"] != null)
                {
                    foreach (DataRow rowProductInfo in dsMeddelandeFil.Tables["ProductInfo"].Rows)
                    {
                        if (LineItem_Id != rowProductInfo["LineItem_Id"].ToString()) continue;
                        if (rowProductInfo["ItemNumber"].ToString() == "BP") drRow["RowBuyerArticleNumber"] = rowProductInfo["ProductInfo_Text"];
                        break;
                    }
                }

                if (dsMeddelandeFil.Tables["Article"] != null && dsMeddelandeFil.Tables["Article"].Columns["Article_Text"] != null)
                {
                    foreach (DataRow rowArticle in dsMeddelandeFil.Tables["Article"].Rows)
                    {
                        if (LineItem_Id != rowArticle["LineItem_Id"].ToString()) continue;
                        if (drRow["RowSellerArticleDescription1"].ToString() == "")
                            drRow["RowSellerArticleDescription1"] = rowArticle["Article_Text"];
                        else
                            drRow["RowSellerArticleDescription2"] = rowArticle["Article_Text"];
                    }
                }

                if (dsMeddelandeFil.Tables["Quantity"] != null && dsMeddelandeFil.Tables["Quantity"].Columns["MeasureQualifier"] != null && dsMeddelandeFil.Tables["Quantity"].Columns["Quantity_Text"] != null)
                {
                    foreach (DataRow rowQuantity in dsMeddelandeFil.Tables["Quantity"].Rows)
                    {
                        if (LineItem_Id != rowQuantity["LineItem_Id"].ToString()) continue;
                        if (rowQuantity["Qualifier"].ToString() == "46" || rowQuantity["Qualifier"].ToString() == "113")
                        {
                            drRow["RowUnitCode"] = rowQuantity["MeasureQualifier"];
                            drRow["RowQuantity"] = rowQuantity["Quantity_Text"];
                        }
                    }
                }

                if (dsMeddelandeFil.Tables["Taxes"] != null && dsMeddelandeFil.Tables["Taxes"].Columns["Category"] != null && dsMeddelandeFil.Tables["Taxes"].Columns["TaxesPCD"] != null)
                {
                    foreach (DataRow rowTaxes in dsMeddelandeFil.Tables["Taxes"].Rows)
                    {
                        if (LineItem_Id != rowTaxes["LineItem_Id"].ToString()) continue;
                        if (rowTaxes["Category"].ToString() == "S") drRow["RowVatPercentage"] = rowTaxes["TaxesPCD"];
                    }
                }

                if (dsMeddelandeFil.Tables["Date"] != null && dsMeddelandeFil.Tables["Date"].Columns["Of"] != null && dsMeddelandeFil.Tables["Date"].Columns["Date_Text"] != null)
                {
                    foreach (DataRow rowDate in dsMeddelandeFil.Tables["Date"].Rows)
                    {
                        if (LineItem_Id != rowDate["LineItem_Id"].ToString()) continue;
                        if (rowDate["Of"].ToString() == "136") drRow["RowDeliveryDate"] = rowDate["Date_Text"].ToString();
                        break;
                    }
                }

            }

            return "";
        }

        public IEnumerable<string> ToXmls()
        {
            return this.parsedMessages;
        }

        protected override IEnumerable<string> ConvertMessage(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, DataRow SenderRow, string fileContent)
        {
            this.LvisNet(InputFolderFileName, WholesaleTempFolder, dsStandardMall, drEdiSettings, fileContent);
            return this.parsedMessages;
        }
    }
}
