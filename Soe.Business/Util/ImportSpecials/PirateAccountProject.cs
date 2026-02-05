using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class PirateAccountProject
    {
        public string ApplyPirateAccountProjectSpecialModification(string content, int actorCompanyId)
        {
            SettingManager settingManager = new SettingManager(null);
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            XElement accountsHeadElement = new XElement("Accounts");
            string modifiedContent = string.Empty;
            string rowCostPlace = string.Empty;
            string line;
            string sieDimNr = "6";
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
                string rowProject = line.Substring(0, 4);

                if (Accounts.Contains(rowProject))
                    continue;

                Accounts.Add(rowProject);

                string rowText1 = line.Substring(41, 30);
 
                if (!rowProject.IsNullOrEmpty())
                {
                    XElement accountCostplace = new XElement("Prj");
                    accountCostplace.Add(
                        new XElement("Prjid", rowProject),
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
