using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using SoftOne.EdiAdmin.Business.Util;
using SoftOne.EdiAdmin.Business.Interfaces;

namespace SoftOne.EdiAdmin.Business.Senders
{
    /// <summary>
    /// Specification: <see cref="http://www.elektroskandia.no/admin/common/getImg2.asp?Fileid=1115"/>
    /// </summary>
    class EdiNelfo40 : EdiSenderOldBase
    {
        private bool doFileOperations = false;

        private EdiDiverse EdiDiverseKlass = new EdiDiverse();

        string[] FilKolumn = { "" };
        char[] Avgränsare = { ';' };

        string SellerId = "";
        string MessageDate = "";
        string SellerName = "";
        string SellerAddress = "";
        string SellerPostalCode = "";
        string SellerPostalAddress = "";
        string SellerCountryCode = "";

        StreamReader sr;

        private string ErrorMessage = "";
        private bool ReturnCode = true;
        private string MeddelandeTyp = "";
        private int MsgNr = 0;
        private decimal DecimalTal = 0;

        //Nelfo version 4.0. Semikolonseparerad, det finns även en xml version som vi inte har stöd för?
        public bool Nelfo40(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, string fileContent = null)        
        {
            string InputFileName;
            sr = EdiDiverse.GetStreamReaderFromContentOrFile(InputFolderFileName, WholesaleTempFolder, fileContent, out this.doFileOperations, out InputFileName);
            string filText = "";
            MeddelandeTyp = "";
            MsgNr = 0;

            while ((filText = sr.ReadLine()) != null)
            {
                FilKolumn = filText.Split(Avgränsare);
                if (FilKolumn[1].ToString() != "EFONELFO" | FilKolumn[2].ToString() != "4.0")
                {
                    string MailSubject = "[N4-2] Fel vid överföring från grossist";
                    string MailMessage = "Meddelandefilen: " + InputFileName + " Avser inte EFONELFO version 4.0";
                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    sr.Close();
                    return false;
                }
                if (FilKolumn[0].ToString() == "CH" | FilKolumn[0].ToString() == "DH" | FilKolumn[0].ToString() == "FH")
                {
                    MeddelandeTyp = FilKolumn[0].ToString();
                    break;
                }
                else
                {
                    string MailSubject = "[N4-2] Fel vid överföring från grossist";
                    string MailMessage = "Meddelandefilen: " + InputFileName + " Avser varken 'Orderbekräftelse', 'Leveransbesked' eller 'Faktura'.";

                    if (FilKolumn[0].ToString() == "VH")
                        MailMessage += " Meddelandet är en prisfil.";

                    Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                    sr.Close();
                    return false;
                }
            }
            sr.Close();

            sr = EdiDiverse.GetStreamReaderFromContentOrFile(fileContent, InputFileName);
            foreach (DataTable tab in dsStandardMall.Tables) tab.Clear();

            if (MeddelandeTyp == "CH" | MeddelandeTyp == "DH")
            {
                while ((filText = sr.ReadLine()) != null)
                {
                    FilKolumn = filText.Split(Avgränsare);
                    if (FilKolumn[0].ToString() == "CH" | FilKolumn[0].ToString() == "DH")
                    {
                        if (MsgNr == 0)
                            ErrorMessage = EdiNelfo40OrderbekräftelseHuvud(dsStandardMall);
                        else
                        {
                            MsgNr++;
                            EdiNelfo40SkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                            foreach (DataTable tab in dsStandardMall.Tables) tab.Clear();
                            ErrorMessage = EdiNelfo40OrderbekräftelseHuvud(dsStandardMall);
                        }
                    }
                    else if (FilKolumn[0].ToString() == "CL" | FilKolumn[0].ToString() == "DL")
                        ErrorMessage = EdiNelfo40OrderbekräftelseRad(dsStandardMall);
                }
                if (dsStandardMall.Tables["MessageInfo"].Rows.Count != 0)
                {
                    MsgNr++;
                    EdiNelfo40SkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                    foreach (DataTable tab in dsStandardMall.Tables) tab.Clear();
                }
            }
            else if (MeddelandeTyp == "FH")
            {
                while ((filText = sr.ReadLine()) != null)
                {
                    FilKolumn = filText.Split(Avgränsare);
                    if (FilKolumn[0].ToString() == "FH")
                    {
                        ErrorMessage = EdiNelfo40FakturaAvsändare();
                    }
                    else if (FilKolumn[0].ToString() == "FF")
                    {
                        ErrorMessage = EdiNelfo40FakturaHuvud(dsStandardMall);
                    }
                    else if (FilKolumn[0].ToString() == "FL")
                    {
                        ErrorMessage = EdiNelfo40FakturaRad(dsStandardMall);
                    }
                    else if (FilKolumn[0].ToString() == "FA")
                    {
                        MsgNr++;
                        EdiNelfo40SkrivMeddelande(InputFolderFileName, InputFileName, WholesaleTempFolder, drEdiSettings, dsStandardMall);
                        foreach (DataTable tab in dsStandardMall.Tables) tab.Clear();
                    }
                }
            }

            sr.Close();

            return ReturnCode;
        }

        private void EdiNelfo40SkrivMeddelande(string InputFolderFileName, string InputFileName, string WholesaleTempFolder, Dictionary<string, string> drEdiSettings, DataSet dsStandardMall)
        {
            string OutputFolderFileName = InputFolderFileName.Replace(".edi", "") + "_" + MsgNr + ".xml";
            if (ErrorMessage == "")
            {
                if (!doFileOperations)
                {
                    string xmlContent = dsStandardMall.GetXml();
                    this.ParsedMessages.Add(xmlContent);
                    //System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    //doc.LoadXml(xmlContent);
                    //parsedMessages.Add(doc.DocumentElement.OuterXml);
                    return;

                    // dsStandardMall.GetXml(); // does not work since it includes the schema 
                    //using(var ms = new MemoryStream())
                    //{
                    //    dsStandardMall.WriteXml(ms, XmlWriteMode.IgnoreSchema);
                    //    parsedMessages.Add(Encoding.UTF8.GetString(ms.ToArray()));
                    //    return;
                    //}
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

            try { if(doFileOperations) File.Delete(OutputFolderFileName); }
            catch
            {
                string MailSubject = "[N5-6] Fel vid borttag av meddelande";
                string MailMessage = "Meddelandefilen '" + OutputFolderFileName;
                Console.Error.WriteLine(MailSubject + ": " + MailMessage);
                ReturnCode = false;
            }

        }

        private string EdiNelfo40OrderbekräftelseHuvud(DataSet dsStandardMall)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            if (MeddelandeTyp == "CH")
                drMessage["MessageType"] = "ORDERBEKR";
            else
                drMessage["MessageType"] = "LEVBESKED";
            if (FilKolumn[5].ToString().Length == 8)
                drMessage["MessageDate"] = FilKolumn[5].ToString().Substring(0, 4) + "-" + FilKolumn[5].ToString().Substring(4, 2) + "-" + FilKolumn[5].ToString().Substring(6, 2);
            else
                drMessage["MessageDate"] = FilKolumn[5].ToString();

            //Leverantör
            drMessage["MessageSenderId"] = FilKolumn[3].ToString();
            drSeller["SellerId"] = FilKolumn[3].ToString();
            drSeller["SellerName"] = FilKolumn[45].ToString();
            drSeller["SellerAddress"] = FilKolumn[46].ToString();
            drSeller["SellerPostalCode"] = FilKolumn[48].ToString();
            drSeller["SellerPostalAddress"] = FilKolumn[49].ToString();
            drSeller["SellerCountryCode"] = FilKolumn[50].ToString();
            drSeller["SellerReference"] = FilKolumn[19].ToString();

            //Köpare
            drBuyer["BuyerId"] = FilKolumn[9].ToString();
            drBuyer["BuyerName"] = FilKolumn[32].ToString();
            drBuyer["BuyerAddress"] = FilKolumn[33].ToString();
            drBuyer["BuyerPostalCode"] = FilKolumn[35].ToString();
            drBuyer["BuyerPostalAddress"] = FilKolumn[36].ToString();
            drBuyer["BuyerCountryCode"] = FilKolumn[37].ToString();
            drBuyer["BuyerReference"] = FilKolumn[38].ToString();

            drBuyer["BuyerDeliveryName"] = FilKolumn[26].ToString();
            drBuyer["BuyerDeliveryAddress"] = FilKolumn[27].ToString();
            drBuyer["BuyerDeliveryPostalCode"] = FilKolumn[29].ToString();
            drBuyer["BuyerDeliveryPostalAddress"] = FilKolumn[30].ToString();
            drBuyer["BuyerDeliveryCountryCode"] = FilKolumn[31].ToString();

            //Huvud
            if (FilKolumn[24].ToString().Length == 8)
                drHead["HeadDeliveryDate"] = FilKolumn[24].ToString().Substring(0, 4) + "-" + FilKolumn[24].ToString().Substring(4, 2) + "-" + FilKolumn[24].ToString().Substring(6, 2);
            else
                drHead["HeadDeliveryDate"] = FilKolumn[24].ToString();
            drHead["HeadBuyerProjectNumber"] = FilKolumn[14].ToString();
            drHead["HeadBuyerOrderNumber"] = FilKolumn[12].ToString();
            drHead["HeadSellerOrderNumber"] = FilKolumn[7].ToString();
            drHead["HeadCurrencyCode"] = FilKolumn[51].ToString();

            return "";
        }

        private string EdiNelfo40OrderbekräftelseRad(DataSet dsStandardMall)
        {

            DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
            dsStandardMall.Tables["Row"].Rows.Add(drRow);

            drRow["RowSellerRowNumber"] = FilKolumn[1].ToString();
            drRow["RowSellerArticleNumber"] = FilKolumn[4].ToString();
            drRow["RowSellerArticleDescription1"] = FilKolumn[5].ToString();
            drRow["RowSellerArticleDescription2"] = FilKolumn[6].ToString();
            if (FilKolumn[13].ToString().Length == 8)
                drRow["RowDeliveryDate"] = FilKolumn[13].ToString().Substring(0, 4) + "-" + FilKolumn[13].ToString().Substring(4, 2) + "-" + FilKolumn[13].ToString().Substring(6, 2);
            else
                drRow["RowDeliveryDate"] = FilKolumn[13].ToString();
            if (FilKolumn[7].ToString() == "" | FilKolumn[7].ToString() == "0")
                drRow["RowQuantity"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[7].ToString()) / 100;
                drRow["RowQuantity"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[24].ToString() == "" | FilKolumn[24].ToString() == "0")
                drRow["RowNetAmount"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[24].ToString()) / 100;
                drRow["RowNetAmount"] = string.Format("{0:0.00}", DecimalTal);
            }
            drRow["RowUnitCode"] = FilKolumn[10].ToString();
            if (FilKolumn[15].ToString() == "" | FilKolumn[15].ToString() == "0")
                drRow["RowUnitPrice"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[15].ToString()) / 100;
                drRow["RowUnitPrice"] = string.Format("{0:0.00}", DecimalTal);
            }

            if (FilKolumn[16].ToString() == "" | FilKolumn[16].ToString() == "0")
                drRow["RowDiscountPercent1"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[16].ToString()) / 100;
                drRow["RowDiscountPercent1"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[17].ToString() == "" | FilKolumn[17].ToString() == "0")
                drRow["RowDiscountAmount1"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[17].ToString()) / 100;
                drRow["RowDiscountAmount1"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[18].ToString() == "" | FilKolumn[18].ToString() == "0")
                drRow["RowDiscountPercent2"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[18].ToString()) / 100;
                drRow["RowDiscountPercent2"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[19].ToString() == "" | FilKolumn[19].ToString() == "0")
                drRow["RowDiscountAmount2"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[19].ToString()) / 100;
                drRow["RowDiscountAmount2"] = string.Format("{0:0.00}", DecimalTal);
            }

            if (FilKolumn[16].ToString() != "0,00" & FilKolumn[18].ToString() == "0,00")
                drRow["RowDiscountPercent"] = drRow["RowDiscountPercent1"];

            DecimalTal = Convert.ToDecimal(drRow["RowDiscountAmount1"].ToString()) + Convert.ToDecimal(drRow["RowDiscountAmount2"].ToString());
            drRow["RowDiscountAmount"] = string.Format("{0:0.00}", DecimalTal);

            drRow["RowBuyerReference"] = dsStandardMall.Tables["Head"].Rows[0]["HeadBuyerOrderNumber"];

            return "";
        }

        private string EdiNelfo40FakturaAvsändare()
        {
            SellerId = FilKolumn[3].ToString();
            if (FilKolumn[4].ToString().Length == 8)
                MessageDate = FilKolumn[4].ToString().Substring(0, 4) + "-" + FilKolumn[4].ToString().Substring(4, 2) + "-" + FilKolumn[4].ToString().Substring(6, 2);
            SellerName = FilKolumn[5].ToString();
            SellerAddress = FilKolumn[6].ToString();
            SellerPostalCode = FilKolumn[8].ToString();
            SellerPostalAddress = FilKolumn[9].ToString();
            SellerCountryCode = FilKolumn[10].ToString();

            return "";
        }

        private string EdiNelfo40FakturaHuvud(DataSet dsStandardMall)
        {

            DataRow drMessage = dsStandardMall.Tables["MessageInfo"].NewRow();
            dsStandardMall.Tables["MessageInfo"].Rows.Add(drMessage);
            DataRow drSeller = dsStandardMall.Tables["Seller"].NewRow();
            dsStandardMall.Tables["Seller"].Rows.Add(drSeller);
            DataRow drBuyer = dsStandardMall.Tables["Buyer"].NewRow();
            dsStandardMall.Tables["Buyer"].Rows.Add(drBuyer);
            DataRow drHead = dsStandardMall.Tables["Head"].NewRow();
            dsStandardMall.Tables["Head"].Rows.Add(drHead);

            decimal DecimalTal = 0;

            drMessage["MessageType"] = "INVOICE";
            drMessage["MessageDate"] = MessageDate;

            //Leverantör
            drMessage["MessageSenderId"] = SellerId;
            drSeller["SellerId"] = SellerId;
            drSeller["SellerName"] = SellerName;
            drSeller["SellerAddress"] = SellerAddress;
            drSeller["SellerPostalCode"] = SellerPostalCode;
            drSeller["SellerPostalAddress"] = SellerPostalAddress;
            drSeller["SellerCountryCode"] = SellerCountryCode;
            drSeller["SellerReference"] = FilKolumn[25].ToString();

            //Köpare
            drBuyer["BuyerId"] = FilKolumn[7].ToString();
            drBuyer["BuyerName"] = FilKolumn[39].ToString();
            drBuyer["BuyerAddress"] = FilKolumn[40].ToString();
            drBuyer["BuyerPostalCode"] = FilKolumn[42].ToString();
            drBuyer["BuyerPostalAddress"] = FilKolumn[43].ToString();
            drBuyer["BuyerCountryCode"] = FilKolumn[44].ToString();
            drBuyer["BuyerReference"] = FilKolumn[45].ToString();

            drBuyer["BuyerDeliveryName"] = FilKolumn[33].ToString();
            drBuyer["BuyerDeliveryAddress"] = FilKolumn[34].ToString();
            drBuyer["BuyerDeliveryPostalCode"] = FilKolumn[36].ToString();
            drBuyer["BuyerDeliveryPostalAddress"] = FilKolumn[37].ToString();
            drBuyer["BuyerDeliveryCountryCode"] = FilKolumn[38].ToString();

            //Huvud

            drHead["HeadInvoiceNumber"] = FilKolumn[1].ToString();
            if (FilKolumn[2].ToString() == "-")
                drHead["HeadInvoiceType"] = "2";
            else
                drHead["HeadInvoiceType"] = "1";
            if (FilKolumn[8].ToString().Length == 8)
                drHead["HeadInvoiceDate"] = FilKolumn[8].ToString().Substring(0, 4) + "-" + FilKolumn[8].ToString().Substring(4, 2) + "-" + FilKolumn[8].ToString().Substring(6, 2);
            else
                drHead["HeadInvoiceDate"] = FilKolumn[8].ToString();
            drHead["HeadIbanNumber"] = FilKolumn[53].ToString();
            drHead["HeadBicAddress"] = FilKolumn[54].ToString();
            drHead["HeadBuyerProjectNumber"] = FilKolumn[20].ToString();
            
            // Fetch order nr from 1: Kjøpers ordrenummer, 2: KjøpersRef
            if(string.IsNullOrEmpty(FilKolumn[18].ToString()))
                drHead["HeadBuyerOrderNumber"] = FilKolumn[24].ToString();
            else
                drHead["HeadBuyerOrderNumber"] = FilKolumn[18].ToString();
            
            drHead["HeadSellerOrderNumber"] = FilKolumn[5].ToString();
            if (FilKolumn[9].ToString().Length == 8)
                drHead["HeadInvoiceDueDate"] = FilKolumn[9].ToString().Substring(0, 4) + "-" + FilKolumn[9].ToString().Substring(4, 2) + "-" + FilKolumn[9].ToString().Substring(6, 2);
            else
                drHead["HeadInvoiceDueDate"] = FilKolumn[9].ToString();
            drHead["HeadCurrencyCode"] = FilKolumn[15].ToString();
            drHead["HeadInvoiceOcr"] = FilKolumn[30].ToString();

            if (FilKolumn[10].ToString() == "" | FilKolumn[10].ToString() == "0")
                drHead["HeadInvoiceNetAmount"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[10].ToString()) / 100;
                drHead["HeadInvoiceNetAmount"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[11].ToString() == "" | FilKolumn[11].ToString() == "0")
                drHead["HeadVatAmount"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[11].ToString()) / 100;
                drHead["HeadVatAmount"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[12].ToString() == "" | FilKolumn[12].ToString() == "0")
                drHead["HeadRoundingAmount"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[12].ToString()) / 100;
                if (FilKolumn[13].ToString() == "-") DecimalTal = DecimalTal * -1;
                drHead["HeadRoundingAmount"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[14].ToString() == "" | FilKolumn[14].ToString() == "0")
                drHead["HeadInvoiceGrossAmount"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[14].ToString()) / 100;
                drHead["HeadInvoiceGrossAmount"] = string.Format("{0:0.00}", DecimalTal);
            }

            return "";
        }

        private string EdiNelfo40FakturaRad(DataSet dsStandardMall)
        {

            DataRow drRow = dsStandardMall.Tables["Row"].NewRow();
            dsStandardMall.Tables["Row"].Rows.Add(drRow);

            drRow["RowSellerRowNumber"] = FilKolumn[1].ToString();
            drRow["RowSellerArticleNumber"] = FilKolumn[5].ToString();
            drRow["RowSellerArticleDescription1"] = FilKolumn[20].ToString();
            if (FilKolumn[21].ToString() != "")
                drRow["RowSellerArticleDescription2"] = FilKolumn[21].ToString();
            if (FilKolumn[18].ToString() == "" | FilKolumn[18].ToString() == "0")
                drRow["RowNetAmount"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[18].ToString()) / 100;
                drRow["RowNetAmount"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[6].ToString() == "" | FilKolumn[6].ToString() == "0")
                drRow["RowQuantity"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[6].ToString()) / 100;
                if (FilKolumn[22].ToString() == "-") DecimalTal = DecimalTal * -1;
                drRow["RowQuantity"] = string.Format("{0:0.00}", DecimalTal);
            }
            drRow["RowUnitCode"] = FilKolumn[7].ToString();
            if (FilKolumn[8].ToString() == "" | FilKolumn[8].ToString() == "0")
                drRow["RowUnitPrice"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[8].ToString()) / 100;
                drRow["RowUnitPrice"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[9].ToString() == "" | FilKolumn[9].ToString() == "0")
                drRow["RowVatPercentage"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[9].ToString()) / 100;
                drRow["RowVatPercentage"] = string.Format("{0:0.00}", DecimalTal);
            }

            if (FilKolumn[10].ToString() == "" | FilKolumn[10].ToString() == "0")
                drRow["RowDiscountPercent1"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[10].ToString()) / 100;
                drRow["RowDiscountPercent1"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[11].ToString() == "" | FilKolumn[11].ToString() == "0")
                drRow["RowDiscountAmount1"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[11].ToString()) / 100;
                drRow["RowDiscountAmount1"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[12].ToString() == "" | FilKolumn[12].ToString() == "0")
                drRow["RowDiscountPercent2"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[12].ToString()) / 100;
                drRow["RowDiscountPercent2"] = string.Format("{0:0.00}", DecimalTal);
            }
            if (FilKolumn[13].ToString() == "" | FilKolumn[13].ToString() == "0")
                drRow["RowDiscountAmount2"] = "0,00";
            else
            {
                DecimalTal = Convert.ToDecimal(FilKolumn[13].ToString()) / 100;
                drRow["RowDiscountAmount2"] = string.Format("{0:0.00}", DecimalTal);
            }

            if (drRow["RowDiscountAmount1"].ToString() != "0,00" & drRow["RowDiscountAmount2"].ToString() == "0,00")
                drRow["RowDiscountPercent"] = drRow["RowDiscountPercent1"];

            DecimalTal = Convert.ToDecimal(drRow["RowDiscountAmount1"].ToString()) + Convert.ToDecimal(drRow["RowDiscountAmount2"].ToString());
            drRow["RowDiscountAmount"] = string.Format("{0:0.00}", DecimalTal);

            drRow["RowBuyerReference"] = dsStandardMall.Tables["Head"].Rows[0]["HeadBuyerOrderNumber"];

            return "";
        }

        protected override IEnumerable<string> ConvertMessage(string InputFolderFileName, string WholesaleTempFolder, DataSet dsStandardMall, Dictionary<string, string> drEdiSettings, DataRow SenderRow, string fileContent)
        {
            this.Nelfo40(InputFolderFileName, WholesaleTempFolder, dsStandardMall, drEdiSettings, fileContent);
            return this.ParsedMessages;
        }
    }
}
