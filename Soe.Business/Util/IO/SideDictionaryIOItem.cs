using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class SideDictionaryIOItem
    {

        #region Collections

        public List<SideDictionaryIODTO> SideDictionaryIOs = new List<SideDictionaryIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "SideDictionary";
        public string XML_Code_TAG = "Code";
        public string XML_Name_TAG = "Name";
        public string XML_Description_TAG = "Description";
        public string XML_Account1Nr_TAG = "Account1Nr";
        public string XML_Account2Nr_TAG = "Account2Nr";
        public string XML_GroupName_TAG = "GroupName";
        public string XML_Percent_TAG = "Percent";
        public string XML_VatIncludedt_TAG = "VatIncluded";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_Quantity2_TAG = "Quantity2";
        public string XML_Discount_TAG = "Discount";
        public string XML_Type_TAG = "Type";
        public string XML_DictionaryType_TAG = "DictionaryType";

        
        #endregion

        #region Constructors

        public SideDictionaryIOItem()
        {
        }

        public SideDictionaryIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public SideDictionaryIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> accountYearElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(accountYearElements, headType, actorCompanyId);

        }

        public void CreateObjects(List<XElement> accountElements, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var accountElement in accountElements)
            {
                SideDictionaryIODTO sideDictionaryIODTO = new SideDictionaryIODTO();

                sideDictionaryIODTO.Type = XmlUtil.GetElementIntValue(accountElement, XML_Type_TAG);
                sideDictionaryIODTO.Code = XmlUtil.GetChildElementValue(accountElement, XML_Code_TAG);
                sideDictionaryIODTO.Name = XmlUtil.GetChildElementValue(accountElement, XML_Name_TAG);
                sideDictionaryIODTO.Description = XmlUtil.GetChildElementValue(accountElement, XML_Description_TAG);
                sideDictionaryIODTO.Account1Nr = XmlUtil.GetChildElementValue(accountElement, XML_Account1Nr_TAG);
                sideDictionaryIODTO.Account2Nr = XmlUtil.GetChildElementValue(accountElement, XML_Account2Nr_TAG);
                sideDictionaryIODTO.GroupName = XmlUtil.GetChildElementValue(accountElement, XML_GroupName_TAG);
                sideDictionaryIODTO.Discount = XmlUtil.GetElementDecimalValue(accountElement, XML_Discount_TAG);
                sideDictionaryIODTO.Percent = XmlUtil.GetElementDecimalValue(accountElement, XML_Percent_TAG);
                sideDictionaryIODTO.Quantity = XmlUtil.GetElementDecimalValue(accountElement, XML_Quantity_TAG);
                sideDictionaryIODTO.Quantity2 = XmlUtil.GetElementDecimalValue(accountElement, XML_Quantity2_TAG);
                sideDictionaryIODTO.VatIncluded = XmlUtil.GetChildElementValue(accountElement, XML_Quantity_TAG) == "1";
                sideDictionaryIODTO.DictionaryType = XmlUtil.GetElementIntValue(accountElement, XML_DictionaryType_TAG);

                SideDictionaryIOs.Add(sideDictionaryIODTO);
            }

        }
        #endregion
    }
}