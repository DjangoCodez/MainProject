using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class TextBlockIOItem
    {

        #region Collections

        public List<TextBlockIODTO> textBlockIOs = new List<TextBlockIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "TextBlockIO";

        public string XML_Entity_TAG = "Entity";
        public string XML_Text_TAG = "Text";
        public string XML_IsCustomerInvoice_TAG = "IsCustomerInvoice";
        public string XML_IsOrder_TAG = "IsOrder";
        public string XML_IsContract_TAG = "IsContract";
        public string XML_LanguageCode_TAG = "ILanguageCode";

        #endregion

        #region Constructors

        public TextBlockIOItem()
        {
        }

        public TextBlockIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public TextBlockIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(content, headType, actorCompanyId);
        }


        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType, actorCompanyId);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementTextBlocks = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementTextBlocks, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementTextBlocks, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementTextBlock in elementTextBlocks)
            {
                TextBlockIODTO textBlockIODTO = new TextBlockIODTO();

                textBlockIODTO.Entity = XmlUtil.GetElementIntValue(elementTextBlock, XML_Entity_TAG);
                textBlockIODTO.Text = XmlUtil.GetChildElementValue(elementTextBlock, XML_Text_TAG);
                textBlockIODTO.IsCustomerInvoice = XmlUtil.GetElementBoolValue(elementTextBlock, XML_IsCustomerInvoice_TAG);
                textBlockIODTO.IsOrder = XmlUtil.GetElementBoolValue(elementTextBlock, XML_IsOrder_TAG);
                textBlockIODTO.IsContract = XmlUtil.GetElementBoolValue(elementTextBlock, XML_IsContract_TAG);
                textBlockIODTO.LanguageCode = XmlUtil.GetChildElementValue(elementTextBlock, XML_LanguageCode_TAG);

                textBlockIOs.Add(textBlockIODTO);
            }
        }
        #endregion
    }
}