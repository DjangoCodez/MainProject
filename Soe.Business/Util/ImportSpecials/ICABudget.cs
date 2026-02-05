using ExcelDataReader;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class IcaBudget
    {
        public string ApplyBudget(string content)
        {
            //<NewDataSet>
            //   <Blad1>
            //     <Datum>20150101</Datum>
            //     <Faktnr xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">67980343</Faktnr>
            //     <Konto xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">2416</Konto>
            //     <Debet xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">26277.86</Debet>
            //     <Kredit xsi:type="xs:string" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
            //   </Blad1>
            //   <Blad1>
            //     <Faktnr xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">67980345</Faktnr>
            //     <Konto xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">2416</Konto>
            //     <Debet xsi:type="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">9661.03</Debet>
            //     <Kredit xsi:type="xs:string" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />
            //   </Blad1>
            //   <Blad1>

            string modifiedContent = string.Empty;
            string budgetNumber = string.Empty;
            string year = string.Empty;
            string konto = string.Empty;
            string kst = string.Empty;
            string proj = string.Empty;
            string saldo = string.Empty;
            string saldo1 = string.Empty;
            string saldo2 = string.Empty;
            string saldo3 = string.Empty;
            string saldo4 = string.Empty;
            string saldo5 = string.Empty;
            string saldo6 = string.Empty;
            string saldo7 = string.Empty;
            string saldo8 = string.Empty;
            string saldo9 = string.Empty;
            string saldo10 = string.Empty;
            string saldo11 = string.Empty;
            string saldo12 = string.Empty;
            string saldo13 = string.Empty;
            string saldo14 = string.Empty;
            string saldo15 = string.Empty;
            string saldo16 = string.Empty;
            string saldo17 = string.Empty;
            string saldo18 = string.Empty;
            string period1 = "1";
            string period2 = "2";
            string period3 = "3";
            string period4 = "4";
            string period5 = "5";
            string period6 = "6";
            string period7 = "7";
            string period8 = "8";
            string period9 = "9";
            string period10 = "10";
            string period11 = "11";
            string period12 = "12";
            string period13 = "13";
            string period14 = "14";
            string period15 = "15";
            string period16 = "16";
            string period17 = "17";
            string period18 = "18";
            List<XElement> budgetHeads = new List<XElement>();
            XElement budgetHeadsElement = new XElement("Budgetar");
            //XElement budgetHead = null;
            XElement xml = null;
            content = content.Replace("Period_x0020_1", "Period1");
            content = content.Replace("Period_x0020_2", "Period2");
            content = content.Replace("Period_x0020_3", "Period3");
            content = content.Replace("Period_x0020_4", "Period4");
            content = content.Replace("Period_x0020_5", "Period5");
            content = content.Replace("Period_x0020_6", "Period6");
            content = content.Replace("Period_x0020_7", "Period7");
            content = content.Replace("Period_x0020_8", "Period8");
            content = content.Replace("Period_x0020_9", "Period9");
            content = content.Replace("Period_x0020_10", "Period10");
            content = content.Replace("Period_x0020_11", "Period11");
            content = content.Replace("Period_x0020_12", "Period12");
            content = content.Replace("Period_x0020_13", "Period13");
            content = content.Replace("Period_x0020_14", "Period14");
            content = content.Replace("Period_x0020_15", "Period15");
            content = content.Replace("Period_x0020_16", "Period16");
            content = content.Replace("Period_x0020_17", "Period17");
            content = content.Replace("Period_x0020_18", "Period18");
            try
            {
                xml = XElement.Parse(content);
            }
            catch
            {
                return modifiedContent;
            }

            List<XElement> rows = xml.Elements("Blad1").ToList();

            foreach (XElement budgetRow in rows)
            {
                budgetNumber = budgetRow.Element("Budgetnr") != null && budgetRow.Element("Budgetnr").Value != null ? budgetRow.Element("Budgetnr").Value : string.Empty;
                year = budgetRow.Element("År") != null && budgetRow.Element("År").Value != null ? budgetRow.Element("År").Value : string.Empty;
                konto = budgetRow.Element("Konto") != null && budgetRow.Element("Konto").Value != null ? budgetRow.Element("Konto").Value : string.Empty;
                kst = budgetRow.Element("Kst") != null && budgetRow.Element("Kst").Value != null ? budgetRow.Element("Kst").Value : string.Empty;
                proj = budgetRow.Element("Projekt") != null && budgetRow.Element("Projekt").Value != null ? budgetRow.Element("Projekt").Value : string.Empty;
                saldo = budgetRow.Element("Saldo") != null && budgetRow.Element("Saldo").Value != null ? budgetRow.Element("Saldo").Value : string.Empty;
                saldo1 = budgetRow.Element("Period1") != null && budgetRow.Element("Period1").Value != null ? budgetRow.Element("Period1").Value : string.Empty;
                saldo2 = budgetRow.Element("Period2") != null && budgetRow.Element("Period2").Value != null ? budgetRow.Element("Period2").Value : string.Empty;
                saldo3 = budgetRow.Element("Period3") != null && budgetRow.Element("Period3").Value != null ? budgetRow.Element("Period3").Value : string.Empty;
                saldo4 = budgetRow.Element("Period4") != null && budgetRow.Element("Period4").Value != null ? budgetRow.Element("Period4").Value : string.Empty;
                saldo5 = budgetRow.Element("Period5") != null && budgetRow.Element("Period5").Value != null ? budgetRow.Element("Period5").Value : string.Empty;
                saldo6 = budgetRow.Element("Period6") != null && budgetRow.Element("Period6").Value != null ? budgetRow.Element("Period6").Value : string.Empty;
                saldo7 = budgetRow.Element("Period7") != null && budgetRow.Element("Period7").Value != null ? budgetRow.Element("Period7").Value : string.Empty;
                saldo8 = budgetRow.Element("Period8") != null && budgetRow.Element("Period8").Value != null ? budgetRow.Element("Period8").Value : string.Empty;
                saldo9 = budgetRow.Element("Period9") != null && budgetRow.Element("Period9").Value != null ? budgetRow.Element("Period9").Value : string.Empty;
                saldo10 = budgetRow.Element("Period10") != null && budgetRow.Element("Period10").Value != null ? budgetRow.Element("Period10").Value : string.Empty;
                saldo11 = budgetRow.Element("Period11") != null && budgetRow.Element("Period11").Value != null ? budgetRow.Element("Period11").Value : string.Empty;
                saldo12 = budgetRow.Element("Period12") != null && budgetRow.Element("Period12").Value != null ? budgetRow.Element("Period12").Value : string.Empty;
                saldo13 = budgetRow.Element("Period13") != null && budgetRow.Element("Period13").Value != null ? budgetRow.Element("Period13").Value : string.Empty;
                saldo14 = budgetRow.Element("Period14") != null && budgetRow.Element("Period14").Value != null ? budgetRow.Element("Period14").Value : string.Empty;
                saldo15 = budgetRow.Element("Period15") != null && budgetRow.Element("Period15").Value != null ? budgetRow.Element("Period15").Value : string.Empty;
                saldo16 = budgetRow.Element("Period16") != null && budgetRow.Element("Period16").Value != null ? budgetRow.Element("Period16").Value : string.Empty;
                saldo17 = budgetRow.Element("Period17") != null && budgetRow.Element("Period17").Value != null ? budgetRow.Element("Period17").Value : string.Empty;
                saldo18 = budgetRow.Element("Period18") != null && budgetRow.Element("Period18").Value != null ? budgetRow.Element("Period18").Value : string.Empty;

                //Create Row period 1
                if (saldo1 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                        new XElement("Budgetnummer", budgetNumber),
                        new XElement("Ar", year),
                        new XElement("Manad", period1),
                        new XElement("Period", period1),
                        new XElement("Konto", konto),
                        new XElement("Kst", kst),
                        new XElement("Projekt", proj),
                        new XElement("Totalt", saldo),
                        new XElement("Saldo", saldo1));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 2
                if (saldo2 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period2),
                    new XElement("Period", period2),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo2));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 3
                if (saldo3 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period3),
                    new XElement("Period", period3),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo3));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 4
                if (saldo4 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period4),
                    new XElement("Period", period4),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo4));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 5
                if (saldo5 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period5),
                    new XElement("Period", period5),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo5));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 6
                if (saldo6 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period6),
                    new XElement("Period", period6),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo6));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 7
                if (saldo7 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period7),
                    new XElement("Period", period7),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo7));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 8
                if (saldo8 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period8),
                    new XElement("Period", period8),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo8));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 9
                if (saldo9 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period9),
                    new XElement("Period", period9),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo9));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 10
                if (saldo10 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period10),
                    new XElement("Period", period10),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo10));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 11
                if (saldo11 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period11),
                    new XElement("Period", period11),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo11));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 12
                if (saldo12 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period12),
                    new XElement("Period", period12),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo12));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 13
                if (saldo13 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period13),
                    new XElement("Period", period13),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo13));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 14
                if (saldo14 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period14),
                    new XElement("Period", period14),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo14));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 15
                if (saldo15 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period15),
                    new XElement("Period", period15),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo15));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 16
                if (saldo16 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period16),
                    new XElement("Period", period16),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo16));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 17
                if (saldo17 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period17),
                    new XElement("Period", period17),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo17));
                    budgetHeadsElement.Add(row);
                }
                //Create Row period 18
                if (saldo18 != string.Empty)
                {
                    XElement row = new XElement("Budget",
                    new XElement("Budgetnummer", budgetNumber),
                    new XElement("Ar", year),
                    new XElement("Manad", period18),
                    new XElement("Period", period18),
                    new XElement("Konto", konto),
                    new XElement("Kst", kst),
                    new XElement("Projekt", proj),
                    new XElement("Totalt", saldo),
                    new XElement("Saldo", saldo18));
                    budgetHeadsElement.Add(row);
                }

            }

            modifiedContent = budgetHeadsElement.ToString();

            return modifiedContent;
        }

        public List<dynamic> GetBudgetHeadDTOs(byte[] content)
        {
            MemoryStream stream = new MemoryStream(content);
            IExcelDataReader excelReader = null;
            BudgetHeadIODTO head = new BudgetHeadIODTO();
            head.Type = (int)DistributionCodeBudgetType.SalesBudgetTime;
            head.UseDimNr2 = true;
            List<BudgetRowIODTO> ioRows = new List<BudgetRowIODTO>();

            try
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }
            catch
            {
                try
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                catch
                {
                    excelReader = ExcelReaderFactory.CreateReader(stream);
                }
            }

            DataSet ds = excelReader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = false } });

            foreach (DataRow row in ds.Tables["sheet1"].Rows)
            {
                try
                {
                    string week = row["Column0"].ToString();

                    if (!week.Contains("20"))
                        continue;

                    string value = row["Column16"].ToString();
                    decimal decimalValue = 0;

                    decimal.TryParse(value, out decimalValue);

                    if (decimalValue == 0)
                        continue;

                    string shiftTypeCode = row["Column1"].ToString();

                    if (string.IsNullOrEmpty(shiftTypeCode))
                        continue;

                    BudgetRowIODTO ioRow = new BudgetRowIODTO();

                    ioRow.ShiftTypeCode = shiftTypeCode;
                    ioRow.Type = (int)DistributionCodeBudgetType.SalesBudgetTime;
                    ioRow.PeriodAmount = decimalValue;
                    ioRow.Week = Convert.ToInt16(week.Substring(4, 2));
                    ioRow.Year = Convert.ToInt16(week.Substring(0, 4));

                    ioRows.Add(ioRow);

                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }

            head.Rows = new List<BudgetRowIODTO>();
            head.Rows.AddRange(ioRows);

            List<dynamic> dtos = new List<dynamic>();
            dtos.Add(head);

            return dtos;
        }
    }


}
