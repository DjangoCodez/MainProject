using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class CoopCashVoucher
    {
        public string ApplyCoopCashVoucherSpecialModification(string content, int actorCompanyId)
        {
            //Method to create voucher based on COOP:S different formats
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            string modifiedContent = string.Empty;

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;

            //First we sort the file based on customer number and put them in a list

            XElement voucherHeadElement = new XElement("Verifikat");
            
            List<XElement> vouchers = new List<XElement>();
            XElement voucher = new XElement("Verifikathuvud");
            string shopNr= " ";
            int shopNumber = 0;
            string kst= " ";
            string voucherDate= " ";     
            string artGroup;
            string vatCode;
            string sign;
            decimal quant;
            decimal amount;
            decimal cost;
            decimal totalQuant = 0;
            decimal totalQuantAmount = 0;
            decimal totalexklVatAmountCode0 = 0;
            decimal totalexklVatAmountCode1 = 0;
            decimal totalexklVatAmountCode2 = 0;
            decimal totalexklVatAmountCode3 = 0;
            decimal totalVatAmountCode1 = 0;
            decimal totalVatAmountCode2 = 0;
            decimal totalVatAmountCode3 = 0;
            decimal exlVatAmountCode0;
            decimal exlVatAmountCode1;
            decimal exlVatAmountCode2;
            decimal exlVatAmountCode3;
            decimal vatAmountCode1;
            decimal vatAmountCode2;
            decimal vatAmountCode3;
            decimal totalArtgrp101 = 0;
            decimal totalArtgrp106 = 0;
            decimal totalArtgrp117 = 0;
            decimal totalPmt31 = 0; //dagskassainsättning
            decimal totalPmt32 = 0; // Checkar
            decimal totalPmt33 = 0; // Förskottsinsättning
            decimal totalPmt36 = 0; // Dagskasa Safepay
            decimal totalPmt41 = 0; // Kontanter inkl checkar
            decimal totalPmt42 = 0; // Kreditkort slippar
            decimal totalPmt43 = 0; // Kreditkort, lästa och registrerade
            decimal totalPmt44 = 0; // Direktfakturor
            decimal totalPmt47 = 0; // Presentkort
            decimal totalPmt48 = 0; // Kuponginlösen,manuella
            decimal totalPmt49 = 0; // Rikskuponger
            decimal totalPmt50 = 0; // Internkuponger
            decimal totalPmt54 = 0; // Värdeavi/Plusgirot
            decimal totalPmt55 = 0; // Kupnginlösen,lästa
            decimal totalPmt56 = 0; // Inlösta premiecheckar
            decimal totalPmt57 = 0; // Öresavrundning
            decimal totalPmt58 = 0; // Restaurangcheckar
            decimal totalPmt59 = 0; // Övriga kuponger
            decimal totalPmt71 = 0; // Kassadifferends
            decimal totalPmt80 = 0; // Inköpstjänst    
            decimal totalPricechangelocal = 0; // LH= prishöjning LS=prissänkning
            decimal totalPhysicaldestruction = 0; // FF= fysisk förstörsle
            decimal totalPricechangecentral = 0; // CH= prishöjning CS=prissänkning
            decimal totalPassedDate = 0; // UD= utgående datum
            decimal totalVGPLMMIKEFLF = 0; // VGPLMMIKEFLF
            decimal totalDK = 0; // DK=Diverse konstnader
            decimal totalDM = 0; // DM= Demonstration
            decimal totalFM = 0; // FM= Förbrukningsmaterial
            decimal totalGA = 0; // GA= Gåvor anställda
            decimal totalKF = 0; // KF= Kaffe o frukt, personal
            decimal totalKM = 0; // KM= Kontorsmaterial
            decimal totalRA = 0; // RA= Representation anställda
            decimal totalRE = 0; // RE= Reparationer
            decimal totalSP = 0; // GA= Gåvor anställda
            decimal totalTM = 0; // TM= Tvättmedel

            bool write02 = false;
            bool write03 = false;
            bool write04 = false;
            bool write06 = false;
            bool write07 = false;
            bool write09 = false;

            line = reader.ReadLine();
            while (line != null)
            {
               
                if (line == "") continue;
               
                   
                
                if (line.StartsWith("01"))
                {
                    string[] inputRow = line.Split(delimiter);

                    shopNumber = inputRow[1] != string.Empty ? Convert.ToInt32(inputRow[1]) : 0;
                    shopNr = shopNumber.ToString();
                    voucherDate = inputRow[2] != null ? inputRow[2] : string.Empty;
                    
                    voucher.Add(
                           new XElement("Namn", "Kassafil butik:" + shopNr),
                           new XElement("Datum", voucherDate));
                    switch (shopNr)
                    {
                        case "26737": kst = "410"; break;  //Stenhamra
                        case "26711": kst = "510"; break;  //Kungsberga
                        case "196231": kst = "410"; break; //Bjursås
                        case "196235": kst = "510"; break; //Grycksbo
                        case "146475": kst = "410"; break; //Dalsjöfors
                        case "205200": kst = "410"; break; //Forsbacka
                        case "76528": kst = "410"; break;  //Älghult
                        case "26731": kst = "410"; break;  //Berg
                        case "26732": kst = "510"; break;  //Långvik
                        case "26733": kst = "610"; break;  //Ingmarsö
                        case "26734": kst = "710"; break;  //Bränsleavdelning
                        case "116385": kst = "410"; break;  //Getinge
                        case "116418": kst = "410"; break;  //Knäred
                        case "58500": kst = "110"; break;  //Fastighet Stora Coop Finspång
                        case "736512": kst = "310"; break;  //Föreningsledning
                        case "54000": kst = "410"; break;  //Stora Coop Finspång
                        case "54029": kst = "420"; break;  //Cafe(Stora Coop)
                        case "54091": kst = "430"; break;  //Restaurang(Stora Coop)
                        case "54050": kst = "440"; break;  //Bageri
                        case "56301": kst = "510"; break;  //Bergslagshallen
                        case "56309": kst = "610"; break;  //Coop Rejmyre
                        case "56310": kst = "710"; break;  //Coop Skärblacka
                        case "56313": kst = "810"; break;  //Östermalmshallen
                    }

                }
                if (line.StartsWith("02"))               
                {
                    write02 = true;
                    string[] inputRow = line.Split(delimiter);
                    
                    artGroup = inputRow[1] != null ? inputRow[1] : string.Empty;
                    vatCode = inputRow[2] != null ? inputRow[2] : string.Empty;
                    sign = inputRow[3] != null ? inputRow[3] : string.Empty;
                    amount = inputRow[4] != string.Empty ? Convert.ToDecimal(inputRow[4]) / 100 : 0;                   
                    if (sign == "-")
                    {
                        amount = amount * -1;
                    }
                    if (vatCode == "0")
                    {
                        exlVatAmountCode0 = amount;
                        totalexklVatAmountCode0 += exlVatAmountCode0;
                    }
                    if (vatCode == "1")
                    {
                        exlVatAmountCode1 = Math.Round(amount / 1.25M, 2);
                        vatAmountCode1 = Math.Round(amount - exlVatAmountCode1, 2);
                        totalVatAmountCode1 += vatAmountCode1;
                        totalexklVatAmountCode1 += amount;
                    }
                    if (vatCode == "2")
                    {
                        exlVatAmountCode2 = Math.Round(amount / 1.12M, 2);
                        vatAmountCode2 = Math.Round(amount - exlVatAmountCode2, 2);
                        totalVatAmountCode2 += vatAmountCode2;
                        totalexklVatAmountCode2 += amount;
                    }
                    if (vatCode == "3")
                    {
                        exlVatAmountCode3 = Math.Round(amount / 1.06M, 2);
                        vatAmountCode3 = Math.Round(amount - exlVatAmountCode3, 2);
                        totalVatAmountCode3 += vatAmountCode3;
                        totalexklVatAmountCode3 += amount;
                    }        
                }
                if (line.StartsWith("03"))
                {
                    
                    write03 = true;
                    if (write02)
                    {
                        totalexklVatAmountCode1 = (totalexklVatAmountCode1 * -1);
                        voucher.Add(CreateVoucherRow("3010", "Försäljning 25% moms", totalexklVatAmountCode1, kst));

                        if (totalVatAmountCode1 > 0 || totalVatAmountCode1 < 0)
                        {
                            voucher.Add(CreateVoucherRow("3009", "Momsdel av försäljningen", totalVatAmountCode1, kst));
                            totalVatAmountCode1 = (totalVatAmountCode1 * -1);
                            voucher.Add(CreateVoucherRow("2611", "Utgående moms 25 %", totalVatAmountCode1, kst));
                        }
                        if (totalexklVatAmountCode2 > 0 || totalexklVatAmountCode2 < 0) 
                        {
                            totalexklVatAmountCode2 = (totalexklVatAmountCode2 * -1);
                            voucher.Add(CreateVoucherRow("3011", "Försäljning 12% moms", totalexklVatAmountCode2, kst));
                        }
                        if (totalVatAmountCode2 > 0 || totalVatAmountCode2 < 0)
                        {
                            voucher.Add(CreateVoucherRow("3009", "Momsdel av försäljningen", totalVatAmountCode2, kst));
                            totalVatAmountCode2 = (totalVatAmountCode2 * -1);
                            voucher.Add(CreateVoucherRow("2621", "Utgående moms 12 %", totalVatAmountCode2, kst));
                        }
                        if (totalexklVatAmountCode3 > 0 || totalexklVatAmountCode3 < 0)
                        {
                            totalexklVatAmountCode3 = (totalexklVatAmountCode3 * -1);
                            voucher.Add(CreateVoucherRow("3012", "Försäljning 6% moms", totalexklVatAmountCode3, kst));
                        }
                        if (totalVatAmountCode3 > 0 || totalVatAmountCode3 < 0)
                        {
                            voucher.Add(CreateVoucherRow("3009", "Momsdel av försäljningen", totalVatAmountCode3, kst));
                            totalVatAmountCode3 = (totalVatAmountCode3 * -1);
                            voucher.Add(CreateVoucherRow("2631", "Utgående moms 6 %", totalVatAmountCode3, kst));
                        }
                        if (totalexklVatAmountCode0 > 0 || totalexklVatAmountCode0 < 0)
                        {
                            totalexklVatAmountCode0 = (totalexklVatAmountCode0 * -1);
                            voucher.Add(CreateVoucherRow("3014", "Försäljning 0% moms", totalexklVatAmountCode0, kst));
                        }

                        write02 = false;
                    }
                    string[] inputRow = line.Split(delimiter);

                    artGroup = inputRow[1] != null ? inputRow[1] : string.Empty;
                    vatCode = inputRow[2] != null ? inputRow[2] : string.Empty;
                    sign = inputRow[3] != null ? inputRow[3] : string.Empty;
                    amount = inputRow[4] != string.Empty ? Convert.ToDecimal(inputRow[4]) / 100 : 0;
                     if(sign == "-")
                    {
                        amount = amount * -1;
                    }
                    switch (vatCode)
                    {
                        case "101": totalArtgrp101 += amount; break;
                        case "106": totalArtgrp106 += amount; break;
                        default: totalArtgrp117 += amount; break;
                    }
                }
                if (line.StartsWith("04"))
                {
                    write04 = true;
                    if (write02)
                    {
                        totalexklVatAmountCode1 = (totalexklVatAmountCode1 * -1);
                        voucher.Add(CreateVoucherRow("3010", "Försäljning 25% moms", totalexklVatAmountCode1, kst));

                        if (totalVatAmountCode1 > 0 || totalVatAmountCode1 < 0)
                        {
                            voucher.Add(CreateVoucherRow("3009", "Momsdel av försäljningen", totalVatAmountCode1, kst));
                            totalVatAmountCode1 = (totalVatAmountCode1 * -1);
                            voucher.Add(CreateVoucherRow("2611", "Utgående moms 25 %", totalVatAmountCode1, kst));
                        }
                        if (totalexklVatAmountCode2 > 0 || totalexklVatAmountCode2 < 0)
                        {
                            totalexklVatAmountCode2 = (totalexklVatAmountCode2 * -1);
                            voucher.Add(CreateVoucherRow("3011", "Försäljning 12% moms", totalexklVatAmountCode2, kst));
                        }
                        if (totalVatAmountCode2 > 0 || totalVatAmountCode2 < 0)
                        {
                            voucher.Add(CreateVoucherRow("3009", "Momsdel av försäljningen", totalVatAmountCode2, kst));
                            totalVatAmountCode2 = (totalVatAmountCode2 * -1);
                            voucher.Add(CreateVoucherRow("2621", "Utgående moms 12 %", totalVatAmountCode2, kst));
                        }
                        if (totalexklVatAmountCode3 > 0 || totalexklVatAmountCode3 < 0)
                        {
                            totalexklVatAmountCode3 = (totalexklVatAmountCode3 * -1);
                            voucher.Add(CreateVoucherRow("3012", "Försäljning 6% moms", totalexklVatAmountCode3, kst));
                        }
                        if (totalVatAmountCode3 > 0 || totalVatAmountCode3 < 0)
                        {
                            voucher.Add(CreateVoucherRow("3009", "Momsdel av försäljningen", totalVatAmountCode3, kst));
                            totalVatAmountCode3 = (totalVatAmountCode3 * -1);
                            voucher.Add(CreateVoucherRow("2631", "Utgående moms 6 %", totalVatAmountCode3, kst));
                        }
                        if (totalexklVatAmountCode0 > 0 || totalexklVatAmountCode0 < 0)
                        {
                            totalexklVatAmountCode0 = (totalexklVatAmountCode0 * -1);
                            voucher.Add(CreateVoucherRow("3014", "Försäljning 0% moms", totalexklVatAmountCode0, kst));
                        }

                        write02 = false;
                    }
                    if (write03)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");
                        if (totalArtgrp101 > 0 || totalArtgrp101 < 0)
                        {
                            totalArtgrp101 = (totalArtgrp101 * -1);
                            voucher.Add(CreateVoucherRow("1505", "Nonsale artgrp101", totalArtgrp101, kst));
                        }
                        if (totalArtgrp106 > 0 || totalArtgrp106 < 0)
                        {
                            totalArtgrp106 = (totalArtgrp106 * -1);
                            voucher.Add(CreateVoucherRow("2401", "Nonsale artgrp106", totalArtgrp106, kst));
                        }
                        if (totalArtgrp117 > 0 || totalArtgrp117 < 0)
                        {
                            totalArtgrp117 = (totalArtgrp117 * -1);
                            voucher.Add(CreateVoucherRow("2890", "Nonsale artgrp117", totalArtgrp117, kst));
                        }
                        write03 = false;
                    }
                    string[] inputRow = line.Split(delimiter);
                    artGroup = "";
                    sign = "";
                    amount = 0;
                    artGroup = inputRow[1] != null ? inputRow[1] : string.Empty;
                    sign = inputRow[2] != null ? inputRow[2] : string.Empty;
                    amount = inputRow[3] != string.Empty ? Convert.ToDecimal(inputRow[3]) / 100 : 0;                   
                    if (sign == "-")
                    {
                        amount = amount * -1;
                    }
                    switch (artGroup)
                    {
                        case "31": totalPmt31 += amount; break;
                        case "32": totalPmt32 += amount; break;
                        case "33": totalPmt33 += amount; break;
                        case "36": totalPmt36 += amount; break;
                        case "41": totalPmt41 += amount; break;
                        case "42": totalPmt42 += amount; break;
                        case "43": totalPmt43 += amount; break;
                        case "44": totalPmt44 += amount; break;
                        case "47": totalPmt47 += amount; break;
                        case "48": totalPmt48 += amount; break;
                        case "49": totalPmt49 += amount; break;
                        case "50": totalPmt50 += amount; break;
                        case "54": totalPmt54 += amount; break;
                        case "55": totalPmt55 += amount; break;
                        case "56": totalPmt56 += amount; break;
                        case "57": totalPmt57 += amount; break;
                        case "58": totalPmt58 += amount; break;
                        case "59": totalPmt59 += amount; break;
                        case "71": totalPmt71 += amount; break;
                        case "80": totalPmt80 += amount; break;
                    }
                }
                if (line.StartsWith("06"))
                {
                    write06 = true;
                    if (write04)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");
                         //{
                        //    voucher.Add(CreateVoucherRow("1960", "Dagskasseinsättning", totalPmt31));
                        //}
                        //if (totalPmt32 > 0 | totalPmt32 < 0)
                        //{
                        //    totalPmt32 = (totalPmt32 * -1);
                        //    voucher.Add(CreateVoucherRow("1930", "Checkar", totalPmt32, kst));
                        //}
                        if (totalPmt33 > 0 || totalPmt33 < 0)
                        {                          
                            voucher.Add(CreateVoucherRow("1910", "Förskottsinsättning", totalPmt33, kst));
                        }
                        if (totalPmt36 > 0 || totalPmt36 < 0)
                        {                         
                            voucher.Add(CreateVoucherRow("1914", "Dagskassa Safepay", totalPmt36, kst));
                        }
                        if (totalPmt41 > 0 || totalPmt41 < 0)
                        {                          
                            voucher.Add(CreateVoucherRow("1910", "Kontant", totalPmt41, kst));
                        }
                        if (totalPmt42 > 0 || totalPmt42 < 0)
                        {                          
                            voucher.Add(CreateVoucherRow("1930", "Kreditkort,slippar", totalPmt42, kst));
                        }
                        if (totalPmt43 > 0 || totalPmt43 < 0)
                        {                          
                            voucher.Add(CreateVoucherRow("1930", "Kreditkort, lästa", totalPmt43, kst));
                        }
                        if (totalPmt44 > 0 || totalPmt44 < 0)
                        {                         
                            voucher.Add(CreateVoucherRow("1518", "Direktfakturor", totalPmt44, kst));
                        }
                        if (totalPmt47 > 0 || totalPmt47 < 0)
                        {
                            totalPmt47 = (totalPmt47 * -1);
                            voucher.Add(CreateVoucherRow("1507", "Presentkort", totalPmt47, kst));
                        }
                        if (totalPmt48 > 0 || totalPmt48 < 0)
                        {
                            totalPmt48 = (totalPmt48 * -1);
                            voucher.Add(CreateVoucherRow("1508", "Kuponginlösen, manuella", totalPmt48, kst));
                        }
                        if (totalPmt49 > 0 || totalPmt49 < 0)
                        {                         
                            voucher.Add(CreateVoucherRow("1506", "Rikskuponger", totalPmt49, kst));
                        }
                        if (totalPmt50 > 0 || totalPmt50 < 0)
                        {                         
                            voucher.Add(CreateVoucherRow("1504", "Internkuponger", totalPmt50, kst));
                        }
                        if (totalPmt54 > 0 || totalPmt54 < 0)
                        {                           
                            voucher.Add(CreateVoucherRow("1501", "Värdeavi", totalPmt54, kst));
                        }
                        if (totalPmt55 > 0 || totalPmt55 < 0)
                        {                           
                            voucher.Add(CreateVoucherRow("1508", "Kuponginlösen", totalPmt55, kst));
                        }
                        if (totalPmt56 > 0 || totalPmt56 < 0)
                        {                          
                            voucher.Add(CreateVoucherRow("2993", "Premiecheckar", totalPmt56, kst));
                        }
                        if (totalPmt57 > 0 || totalPmt57 < 0)
                        {
                            totalPmt57 = (totalPmt57 * -1);
                            voucher.Add(CreateVoucherRow("3741", "Öresavrundning", totalPmt57, kst));
                        }
                        if (totalPmt58 > 0 || totalPmt58 < 0)
                        {                           
                            voucher.Add(CreateVoucherRow("1506", "Restaurangcheckar", totalPmt58, kst));
                        }
                        if (totalPmt59 > 0 || totalPmt59 < 0)
                        {                           
                            voucher.Add(CreateVoucherRow("1504", "Övriga kuponger", totalPmt59, kst));
                        }
                        if (totalPmt71 > 0 || totalPmt71 < 0)
                        {
                            totalPmt71 = (totalPmt71 * -1);
                            voucher.Add(CreateVoucherRow("6381", "Kassadifferens", totalPmt71, kst));
                        }
                        if (totalPmt80 > 0 || totalPmt80 < 0)
                        {
                           voucher.Add(CreateVoucherRow("1689", "Inköpstjänst", totalPmt80, kst));
                        }
                        vouchers.Add(voucher);
                        voucher = new XElement("Verifikathuvud");
                        voucher.Add(
                          new XElement("Namn", "Kassafil Övriga tjänster butik:" + shopNr),
                          new XElement("Datum", voucherDate));
                        write04 = false;
                    }
                    string[] inputRow = line.Split(delimiter);
                    artGroup = "";
                    amount = 0;
                    quant = 0;
                    artGroup = inputRow[1] != null ? inputRow[1] : string.Empty;
                    quant = inputRow[2] != string.Empty ? Convert.ToDecimal(inputRow[2]): 0;
                    totalQuant += quant;
                }
                if (line.StartsWith("07"))
                {
                    write07 = true;
                    if (write04)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");
                         //{
                        //    voucher.Add(CreateVoucherRow("1960", "Dagskasseinsättning", totalPmt31));
                        //}
                        //if (totalPmt32 > 0 | totalPmt32 < 0)
                        //{
                        //    totalPmt32 = (totalPmt32 * -1);
                        //    voucher.Add(CreateVoucherRow("1930", "Checkar", totalPmt32, kst));
                        //}
                        if (totalPmt33 > 0 || totalPmt33 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1910", "Förskottsinsättning", totalPmt33, kst));
                        }
                        if (totalPmt36 > 0 || totalPmt36 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1914", "Dagskassa Safepay", totalPmt36, kst));
                        }
                        if (totalPmt41 > 0 || totalPmt41 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1910", "Kontant", totalPmt41, kst));
                        }
                        if (totalPmt42 > 0 || totalPmt42 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1930", "Kreditkort,slippar", totalPmt42, kst));
                        }
                        if (totalPmt43 > 0 || totalPmt43 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1930", "Kreditkort, lästa", totalPmt43, kst));
                        }
                        if (totalPmt44 > 0 || totalPmt44 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1518", "Direktfakturor", totalPmt44, kst));
                        }
                        if (totalPmt47 > 0 || totalPmt47 < 0)
                        {
                            totalPmt47 = (totalPmt47 * -1);
                            voucher.Add(CreateVoucherRow("1507", "Presentkort", totalPmt47, kst));
                        }
                        if (totalPmt48 > 0 || totalPmt48 < 0)
                        {
                            totalPmt48 = (totalPmt48 * -1);
                            voucher.Add(CreateVoucherRow("1508", "Kuponginlösen, manuella", totalPmt48, kst));
                        }
                        if (totalPmt49 > 0 || totalPmt49 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1506", "Rikskuponger", totalPmt49, kst));
                        }
                        if (totalPmt50 > 0 || totalPmt50 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1504", "Internkuponger", totalPmt50, kst));
                        }
                        if (totalPmt54 > 0 || totalPmt54 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1501", "Värdeavi", totalPmt54, kst));
                        }
                        if (totalPmt55 > 0 || totalPmt55 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1508", "Kuponginlösen", totalPmt55, kst));
                        }
                        if (totalPmt56 > 0 || totalPmt56 < 0)
                        {
                            voucher.Add(CreateVoucherRow("2993", "Premiecheckar", totalPmt56, kst));
                        }
                        if (totalPmt57 > 0 || totalPmt57 < 0)
                        {
                            totalPmt57 = (totalPmt57 * -1);
                            voucher.Add(CreateVoucherRow("3741", "Öresavrundning", totalPmt57, kst));
                        }
                        if (totalPmt58 > 0 || totalPmt58 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1506", "Restaurangcheckar", totalPmt58, kst));
                        }
                        if (totalPmt59 > 0 || totalPmt59 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1504", "Övriga kuponger", totalPmt59, kst));
                        }
                        if (totalPmt71 > 0 || totalPmt71 < 0)
                        {
                            totalPmt71 = (totalPmt71 * -1);
                            voucher.Add(CreateVoucherRow("6381", "Kassadifferens", totalPmt71, kst));
                        }
                        if (totalPmt80 > 0 || totalPmt80 < 0)
                        {
                            voucher.Add(CreateVoucherRow("1689", "Inköpstjänst", totalPmt80, kst));
                        }
                        vouchers.Add(voucher);
                        voucher = new XElement("Verifikathuvud");
                        voucher.Add(
                          new XElement("Namn", "Kassafil Övriga tjänster butik:" + shopNr),
                          new XElement("Datum", voucherDate));
                        write04 = false;
                    }
                    if (write06)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");
                        if (totalQuant > 0 )
                        {
                            totalQuantAmount = totalQuant;
                            voucher.Add(CreateVoucherRow("9000", "Antal kunder", totalQuantAmount, kst));
                            totalQuantAmount = totalQuant * -1;
                            voucher.Add(CreateVoucherRow("9001", "Antal kunder", totalQuantAmount, kst));
                        }
                        write06 = false;
                    }
                    string[] inputRow = line.Split(delimiter);
                    artGroup = "";
                    vatCode = "";
                    sign = "";
                    amount = 0;
                    cost = 0;
                    vatCode = inputRow[1] != null ? inputRow[1] : string.Empty;
                    artGroup = inputRow[2] != null ? inputRow[2] : string.Empty;
                    sign = inputRow[2] != null ? inputRow[3] : string.Empty;
                    amount = inputRow[4] != string.Empty ? Convert.ToDecimal(inputRow[4]) / 100 : 0;
                    cost = inputRow[5] != string.Empty ? Convert.ToDecimal(inputRow[5]) / 100 : 0;
                    if (amount == 0)
                    {
                        amount = cost;
                    }
                    if (sign == "-")
                    {
                        amount = amount * -1;
                    }
                    switch (vatCode)
                    {
                        case "LH": totalPricechangelocal += amount; break;
                        case "LS": totalPricechangelocal += amount; break;
                        case "FF": totalPhysicaldestruction += amount; break;
                        case "CH": totalPricechangecentral += amount; break;
                        case "CS": totalPricechangecentral += amount; break;
                        case "UD": totalPassedDate += amount; break;
                        case "VG": totalVGPLMMIKEFLF += amount; break;
                        case "PL": totalVGPLMMIKEFLF += amount; break;
                        case "MM": totalVGPLMMIKEFLF += amount; break;
                        case "IK": totalVGPLMMIKEFLF += amount; break;
                        case "EF": totalVGPLMMIKEFLF += amount; break;
                        case "LF": totalVGPLMMIKEFLF += amount; break;                       
                    }

                }
                if (line.StartsWith("09"))
                {
                    write09 = true;
                    if (write07)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");
                        
                        if (totalPricechangelocal > 0 || totalPricechangelocal < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Lokal prisändring", totalPricechangelocal, kst));
                            totalPricechangelocal = totalPricechangelocal * -1;
                            voucher.Add(CreateVoucherRow("1462", "Lokal prisändring", totalPricechangelocal, kst));
                        }
                        if (totalPhysicaldestruction > 0 || totalPhysicaldestruction < 0)
                        {
                            voucher.Add(CreateVoucherRow("4191", "Fysisk förstörelse", totalPhysicaldestruction, kst));
                            totalPhysicaldestruction = totalPhysicaldestruction * -1;
                            voucher.Add(CreateVoucherRow("1462", "Fysisk förstörelse", totalPhysicaldestruction, kst));
                        }
                        if (totalPricechangecentral > 0 || totalPricechangecentral < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Central prisändring", totalPricechangecentral, kst));
                            totalPricechangecentral = totalPricechangecentral * -1;
                            voucher.Add(CreateVoucherRow("1462", "Central prisändring", totalPricechangecentral, kst));
                        }
                        if (totalPassedDate > 0 || totalPassedDate < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Utgående datum", totalPassedDate, kst));
                            totalPassedDate = totalPassedDate * -1;
                            voucher.Add(CreateVoucherRow("1462", "Utgående datum", totalPassedDate, kst));
                        }
                        if (totalVGPLMMIKEFLF > 0 || totalVGPLMMIKEFLF < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Övriga Lagerändringar", totalVGPLMMIKEFLF, kst));
                            totalVGPLMMIKEFLF = totalVGPLMMIKEFLF * -1;
                            voucher.Add(CreateVoucherRow("1462", "Övriga Lagerändringar", totalVGPLMMIKEFLF, kst));
                        }
                        write07 = false;
                    }
                    string[] inputRow = line.Split(delimiter);
                    artGroup = "";
                    vatCode = "";
                    sign = "";
                    amount = 0;
                    cost = 0;
                    vatCode = inputRow[1] != null ? inputRow[1] : string.Empty;
                    artGroup = inputRow[2] != null ? inputRow[2] : string.Empty;
                    sign = inputRow[2] != null ? inputRow[3] : string.Empty;
                    amount = inputRow[4] != string.Empty ? Convert.ToDecimal(inputRow[4]) / 100 : 0;
                    cost = inputRow[5] != string.Empty ? Convert.ToDecimal(inputRow[5]) / 100 : 0;
                    if (amount == 0)
                    {
                        amount = cost;
                    }
                    if (sign == "-")
                    {
                        amount = amount * -1;
                    }
                    switch (vatCode)
                    {
                        case "DK": totalDK += amount; break;
                        case "DM": totalDM += amount; break;
                        case "FM": totalFM += amount; break;
                        case "GA": totalGA += amount; break;
                        case "KF": totalKF += amount; break;
                        case "KM": totalKM += amount; break;
                        case "RA": totalRA += amount; break;
                        case "RE": totalRE += amount; break;
                        case "SP": totalSP += amount; break;
                        case "TM": totalTM += amount; break;
                       
                    }

                }
                if (line.StartsWith("99"))
                {
                    if (write07)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");

                        if (totalPricechangelocal > 0 || totalPricechangelocal < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Lokal prisändring", totalPricechangelocal, kst));
                            totalPricechangelocal = totalPricechangelocal * -1;
                            voucher.Add(CreateVoucherRow("1462", "Lokal prisändring", totalPricechangelocal, kst));
                        }
                        if (totalPhysicaldestruction > 0 || totalPhysicaldestruction < 0)
                        {
                            voucher.Add(CreateVoucherRow("1462", "Fysisk förstörelse", totalPhysicaldestruction, kst));
                            totalPhysicaldestruction = totalPhysicaldestruction * -1;
                            voucher.Add(CreateVoucherRow("4191", "Fysisk förstörelse", totalPhysicaldestruction, kst));
                        }
                        if (totalPricechangecentral > 0 || totalPricechangecentral < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Central prisändring", totalPricechangecentral, kst));
                            totalPricechangecentral = totalPricechangecentral * -1;
                            voucher.Add(CreateVoucherRow("1462", "Central prisändring", totalPricechangecentral, kst));
                        }
                        if (totalPassedDate > 0 || totalPassedDate < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Utgående datum", totalPassedDate, kst));
                            totalPassedDate = totalPassedDate * -1;
                            voucher.Add(CreateVoucherRow("1462", "Utgående datum", totalPassedDate, kst));
                        }
                        if (totalVGPLMMIKEFLF > 0 || totalVGPLMMIKEFLF < 0)
                        {
                            voucher.Add(CreateVoucherRow("4195", "Övriga Lagerändringar", totalVGPLMMIKEFLF, kst));
                            totalVGPLMMIKEFLF = totalVGPLMMIKEFLF * -1;
                            voucher.Add(CreateVoucherRow("1462", "Övriga Lagerändringar", totalVGPLMMIKEFLF, kst));
                        }
                        write07 = false;
                    }
                    if (write09)
                    {
                        XElement voucherrow = new XElement("Verifikatrad");
                        
                        if (totalDK > 0 || totalDK < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Diverse kostnader", totalDK, kst));
                            totalDK = totalDK * -1;
                            voucher.Add(CreateVoucherRow("6190", "Diverse kostnader", totalDK, kst));
                        }
                        if (totalDM > 0 || totalDM < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Demonstration", totalDM, kst));
                            totalDM = totalDM * -1;
                            voucher.Add(CreateVoucherRow("5993", "Demonstration", totalDM, kst));
                        }
                        if (totalFM > 0 || totalFM < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Förbrukningsmaterial", totalFM, kst));
                            totalFM = totalFM * -1;
                            voucher.Add(CreateVoucherRow("5460", "Förbrukningsmaterial", totalFM, kst));
                        }
                        if (totalGA > 0 || totalGA < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Gåvor anställda", totalGA, kst));
                            totalGA = totalGA * -1;
                            voucher.Add(CreateVoucherRow("7661", "Gåvor anställda", totalGA, kst));
                        }
                        if (totalKF > 0 || totalKF < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Kaffe o frukt,personal", totalKF, kst));
                            totalKF = totalKF * -1;
                            voucher.Add(CreateVoucherRow("7661", "Kaffe o frukt,personal", totalKF, kst));
                        }
                        if (totalKM > 0 || totalKM < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Kontorsmaterial", totalKM, kst));
                            totalKM = totalKM * -1;
                            voucher.Add(CreateVoucherRow("6110", "Kontorsmaterial", totalKM, kst));
                        }
                        if (totalRA > 0 || totalRA < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Representation anställda", totalRA, kst));
                            totalRA = totalRA * -1;
                            voucher.Add(CreateVoucherRow("7631", "Representation anställda", totalRA, kst));
                        }
                        if (totalRE > 0 || totalRE < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Reparationer", totalRE, kst));
                            totalRE = totalRE * -1;
                            voucher.Add(CreateVoucherRow("5510", "Reparationer", totalRE, kst));
                        }
                        if (totalSP > 0 || totalSP < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Sponsring", totalSP, kst));
                            totalSP = totalSP * -1;
                            voucher.Add(CreateVoucherRow("5980", "Sponsring", totalSP, kst));
                        }
                        if (totalTM > 0 || totalTM < 0)
                        {
                            voucher.Add(CreateVoucherRow("4114", "Tvättmedel", totalTM, kst));
                            totalTM = totalTM * -1;
                            voucher.Add(CreateVoucherRow("5580", "Tvättmedel", totalTM, kst));
                        }
                        write09 = false;
                    }
                    string[] inputRow = line.Split(delimiter);
                    artGroup = "";
                    amount = 0;
                    quant = 0;
                    artGroup = inputRow[1] != null ? inputRow[1] : string.Empty;

                }
                line = reader.ReadLine();
            }
            vouchers.Add(voucher);
            voucherHeadElement.Add(vouchers);

            modifiedContent = voucherHeadElement.ToString();

         return modifiedContent;
        }


        private static XElement CreateVoucherRow(String account, String text, decimal amount, string kst)
        {
            XElement voucherrow = new XElement("Verifikatrad");
            voucherrow.Add(
                                new XElement("Konto", account),
                                new XElement("Text", text),
                                new XElement("Kst", kst),
                                new XElement("Belopp", amount));
            return voucherrow;
        }
        private static XElement CreateVoucherRow(String account, String text, decimal amount, decimal quant)
        {
            XElement voucherrow = new XElement("Verifikatrad");
            voucherrow.Add(
                                new XElement("Konto", account),
                                new XElement("Text", text),
                                new XElement("Belopp", amount),
                                new XElement("Kvantitet", quant));
            return voucherrow;
        }
    }
    
    
}
