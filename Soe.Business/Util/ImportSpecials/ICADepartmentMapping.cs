using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public static class ICADepartmentMapping
    {
        public static string ApplyICADepartmentMapping(string content)
        {
            XElement root = new XElement("Accounts");
            List<XElement> elements = new List<XElement>();

            XDocument xdoc = XDocument.Parse(content);
            XElement stores = XmlUtil.GetChildElement(xdoc, "store");

            foreach(var departments in XmlUtil.GetChildElements(stores, "departments"))
            {
                foreach (var department in XmlUtil.GetChildElements(departments, "department"))
                {
                    foreach (var categories in XmlUtil.GetChildElements(department, "categories"))
                    {
                        foreach (var category in XmlUtil.GetChildElements(categories, "category"))
                        {
                            XElement account = new XElement("Account");
                            account.Add(new XElement("AccountNumber", XmlUtil.GetAttributeStringValue(category, "KategoriId")), new XElement("AccountName", XmlUtil.GetAttributeStringValue(category, "KategoriNamn")), new XElement("AccountDimSieNumber",""), new XElement("ParentAccountNumber", XmlUtil.GetAttributeStringValue(department, "AvdelningsId")));
                            elements.Add(account);  
                        }
                    }
                }
            }

            root.Add(elements);
            return root.ToString();
        }
    }
}
