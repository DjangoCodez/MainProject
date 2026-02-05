using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class SysImportDefinitionColumnParser
    {
        private const string PARENT_TAG = "NewDataSet";
        private const string CHILD_TAG = "Columns";

        private const string CHILD_COLUMN_TAG = "Column";
        private const string CHILD_TEXT_TAG = "Text";
        private const string CHILD_UPDATE_TAG = "Update";
        private const string CHILD_XMLTAG_TAG = "XmlTag";
        private const string CHILD_POSITION_TAG = "Position";
        private const string CHILD_FROM_TAG = "From";
        private const string CHILD_CHARACTERS_TAG = "Characters";
        private const string CHILD_CONVERT_TAG = "Convert";
        private const string CHILD_STANDARD_TAG = "Standard";

        public List<SysImportDefinitionLevelColumnSettings> Columns = new List<SysImportDefinitionLevelColumnSettings>();
        public string XML;

        public SysImportDefinitionColumnParser(string settings, int level)
        {
            CreateObjects(settings, level);
        }

        public SysImportDefinitionColumnParser(List<SysImportDefinitionLevelColumnSettings> items, TermGroup_SysImportDefinitionType definitionType)
        {
            CreateXML(items, definitionType);
        }

        private void CreateObjects(string settings, int level)
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

            List<XElement> columnElements = XmlUtil.GetChildElements(xdoc, CHILD_TAG);
            foreach (XElement columnElement in columnElements)
            {
                string column = XmlUtil.GetChildElementValue(columnElement, CHILD_COLUMN_TAG);
                string text = XmlUtil.GetChildElementValue(columnElement, CHILD_TEXT_TAG);      
                string position = XmlUtil.GetChildElementValue(columnElement, CHILD_POSITION_TAG);

                string update = XmlUtil.GetChildElementValue(columnElement, CHILD_UPDATE_TAG);
                string xmlTag = XmlUtil.GetChildElementValue(columnElement, CHILD_XMLTAG_TAG);
                string fromStr = XmlUtil.GetChildElementValue(columnElement, CHILD_FROM_TAG);
                string charactersStr = XmlUtil.GetChildElementValue(columnElement, CHILD_CHARACTERS_TAG);
                string convert = XmlUtil.GetChildElementValue(columnElement, CHILD_CONVERT_TAG);
                string standard = XmlUtil.GetChildElementValue(columnElement,CHILD_STANDARD_TAG );                

                SysImportDefinitionLevelColumnSettings columnSettings = new SysImportDefinitionLevelColumnSettings();

                columnSettings.Level = level;
                columnSettings.Column = column;
                columnSettings.Text = text;
                columnSettings.XmlTag = xmlTag;                
                columnSettings.Convert = convert;
                columnSettings.Standard = standard;
                
                int pos = 0;
                Int32.TryParse(position, out pos);
                columnSettings.Position = pos;

                int updateTypeId = 0;
                Int32.TryParse(update, out updateTypeId);
                columnSettings.UpdateTypeId = updateTypeId;

                int from = 0;
                Int32.TryParse(fromStr, out from);
                columnSettings.From = from;

                int characters = 0;
                Int32.TryParse(charactersStr, out characters);
                columnSettings.Characters = characters;


                Columns.Add(columnSettings);

            }
        }

        private void CreateXML(List<SysImportDefinitionLevelColumnSettings> items, TermGroup_SysImportDefinitionType definitionType)
        {
            if (items.Count == 0)
            {
                XML = string.Empty;
                return;
            }

            if (definitionType == TermGroup_SysImportDefinitionType.Separator)
                items = items.OrderBy(x => x.Position).ToList();
            else if (definitionType == TermGroup_SysImportDefinitionType.Fixed)
                items = items.OrderBy(x => x.From).ThenBy(x => x.Characters).ToList();

            XElement rootElement = new XElement(PARENT_TAG);

            foreach (var item in items)
            {
                XElement childElement = new XElement(CHILD_TAG);
                if (item.Column=="xmltag")
                {
                    item.Column = "XmlTag";
                }
                childElement.Add(new XElement(CHILD_COLUMN_TAG, item.Column));
                childElement.Add(new XElement(CHILD_TEXT_TAG, item.Text));
                childElement.Add(new XElement(CHILD_UPDATE_TAG, item.UpdateTypeId.ToString()));

                if (definitionType == TermGroup_SysImportDefinitionType.XML)
                {
                    childElement.Add(new XElement(CHILD_XMLTAG_TAG, item.XmlTag));
                }
                else if (definitionType == TermGroup_SysImportDefinitionType.Fixed)
                {
                    childElement.Add(new XElement(CHILD_FROM_TAG, item.From.ToString()));
                    childElement.Add(new XElement(CHILD_CHARACTERS_TAG, item.Characters.ToString()));                    
                }
                else if (definitionType == TermGroup_SysImportDefinitionType.Separator)
                {
                    childElement.Add(new XElement(CHILD_POSITION_TAG, item.Position.ToString()));
                }

                if (!string.IsNullOrEmpty(item.Convert))                
                    childElement.Add(new XElement(CHILD_CONVERT_TAG, item.Convert));

                if (!string.IsNullOrEmpty(item.Standard))
                    childElement.Add(new XElement(CHILD_STANDARD_TAG, item.Standard));

                rootElement.Add(childElement);
            }
            
            XML = rootElement.ToString();
        }
    }
}
