using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class PirateAccountCostPlace
    {
        public string ApplyPirateAccountCostPlaceSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            XElement accountsHeadElement = new XElement("Accounts");
            string modifiedContent = string.Empty;
            string rowCostPlace = string.Empty;
            string line;
            string sieDimNr = "1";
            List<string> Accounts = new List<string>();
            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                if (line.Length <= 90) continue;

                //Parse information
                if (line.Length == 100)
                {
                    rowCostPlace = line.Substring(97, 3);
                }
                if (line.Length == 99)
                {
                    rowCostPlace = line.Substring(96, 3);
                }
                if (line.Length == 98)
                {
                    rowCostPlace = line.Substring(95, 3);
                }
                if(line.Length == 97)
                {
                    rowCostPlace = line.Substring(95, 2);
                }

                if (Accounts.Contains(rowCostPlace))
                    continue;

                Accounts.Add(rowCostPlace);
                string rowText1 = line.Substring(11, 30);
                string rowText2 = line.Substring(41, 30);
 
                if (!rowCostPlace.IsNullOrEmpty())
                {
                    XElement accountCostplace = new XElement("Kst");
                    accountCostplace.Add(
                        new XElement("Kstid", rowCostPlace.Trim()),
                        new XElement("Name", rowText1),
                        new XElement("SieDimNr", sieDimNr));
                    accountsHeadElement.Add(accountCostplace);
                }
            }
              modifiedContent = accountsHeadElement.ToString();

            return modifiedContent;
        }

    }
}
