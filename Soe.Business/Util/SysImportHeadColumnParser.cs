using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class SysImportHeadColumnParser
    {
        public List<SysImportSelectColumnSettings> Columns = new List<SysImportSelectColumnSettings>();
        public string XML;

        public SysImportHeadColumnParser(string settings)
        {
            CreateObjects(settings);
        }

        public SysImportHeadColumnParser(List<SysImportSelectColumnSettings> items)
        {
            CreateXML(items);
        }

        private void CreateObjects(string settings)
        {
            #region Special treatment because of strange characters from Professional

            const String startTag = "<Columns>";
            const String endTag = "</Columns>";

            int startIndex = settings.IndexOf(startTag);
            if (startIndex == -1)
                return;

            settings = settings.Substring(startIndex);
            int stopIndex = settings.LastIndexOf(endTag) + endTag.Length;
            settings = settings.Substring(0, stopIndex);

            settings = "<Root>" + settings + "</Root>";
            #endregion


            XDocument xdoc = XDocument.Parse(settings);

             List<XElement> columnElements = XmlUtil.GetChildElements(xdoc, "Columns");
             foreach (XElement columnElement in columnElements)
             {
                 string column = XmlUtil.GetChildElementValue(columnElement, "Column");

                 string text = XmlUtil.GetChildElementValue(columnElement, "Text");
                 string datatype = XmlUtil.GetChildElementValue(columnElement, "DataType");                                  
                 string mandatory = XmlUtil.GetChildElementValue(columnElement, "Mandatory");                 
                 string position = XmlUtil.GetChildElementValue(columnElement, "Position");

                 SysImportSelectColumnSettings columnSettings = new SysImportSelectColumnSettings();

                 columnSettings.Column = column;                 
                 columnSettings.Text = text;
                 columnSettings.DataType = datatype;

                 if (mandatory.ToLower().Equals("true"))
                     columnSettings.Mandatory = true;
                 else
                     columnSettings.Mandatory = false;

                 int pos = 0;
                 Int32.TryParse(position, out pos);
                 columnSettings.Position = pos;

                 Columns.Add(columnSettings);
             }
        }

        private void CreateXML(List<SysImportSelectColumnSettings> items)
        {
            //Future implementation : Create XML from items
         
        }
    }

   
}
