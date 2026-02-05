using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class PirateInvoiceProduct
    {
        public string ApplyPirateInvoiceProductSpecialModification(string content, int actorCompanyId, ParameterObject parameterObject)
        {
            SettingManager sm = new SettingManager(parameterObject);
            ProductManager pm = new ProductManager(parameterObject);
            AccountManager am = new AccountManager(parameterObject);
             //Get AccountStd from CompanySetting
            int accountId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountInvoiceProductSales, 0, actorCompanyId, 0);
            var productSalesAccount = am.GetAccount(actorCompanyId, accountId, onlyActive: false);
            char[] delimiter = new char[1];
            delimiter[0] = ';';
            List<string> Products = new List<string>();
            XElement invoiceProductsHeadElement = new XElement("InvoiceProducts");
            string modifiedContent = string.Empty;
            string rowCostPlace = string.Empty;
            string line;
            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                if (line.Length <= 90) continue;

                //Parse information              
                string productNr = line.Substring(0, 5);

                if (Products.Contains(productNr))
                    continue;

                Products.Add(productNr);

                string rowProject = line.Substring(0, 4);
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
                string rowText1 = line.Substring(11, 30);
                string rowText2 = line.Substring(41, 30);
 
                if (!productNr.IsNullOrEmpty())
                {
                    XElement invoiceProduct = new XElement("Artikel");
                    invoiceProduct.Add(
                        new XElement("Artikelid", productNr),
                        new XElement("Name1", rowText1),
                        new XElement("Name2", rowText2),
                        new XElement("Konto", productSalesAccount.AccountNr),
                        new XElement("Projekt", rowProject),
                        new XElement("Kst", rowCostPlace.Trim()));
                    invoiceProductsHeadElement.Add(invoiceProduct);
                }
            }
              modifiedContent = invoiceProductsHeadElement.ToString();

            return modifiedContent;
        }

    }
}
