using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class StaffingNeedsFrequencyIOItem
    {

        #region Collections

        public List<StaffingNeedsFrequencyIODTO> frequencies = new List<StaffingNeedsFrequencyIODTO>();

        #endregion

        #region XML Nodes
        public const string XML_PARENT_TAG = "StaffingNeedsFrequencyIO";

        public const string XML_DateFrom_TAG = "DateFrom";
        public const string XML_TimeFrom_TAG = "TimeFrom";
        public const string XML_DateTo_TAG = "DateTo";
        public const string XML_TimeTo_TAG = "TimeTo";
        public const string XML_NbrOfItems_TAG = "NbrOfItems";
        public const string XML_NbrOfMinutes_TAG = "NbrOfMinutes";
        public const string XML_NbrOfCustomers_TAG = "NbrOfCustomers";
        public const string XML_Amount_TAG = "Amount";
        public const string XML_Cost_TAG = "Cost";
        public const string XML_ExternalCode_TAG = "ExternalCode";
        public const string XML_ParentExternalCode_TAG = "ParentExternalCode";
        public const string XML_FrequencyType_TAG = "FrequencyType";
        public const string XML_Key_TAG = "Key";

        #endregion

        #region Constructors

        public StaffingNeedsFrequencyIOItem()
        {
        }

        public StaffingNeedsFrequencyIOItem(List<string> contents, TermGroup_IOImportHeadType headType, List<dynamic> objects = null)
        {
            if (objects.IsNullOrEmpty() && !contents.IsNullOrEmpty())
            {
                CreateObjects(contents, headType);
            }
            else
            {
                foreach (var item in objects)
                {
                    frequencies.Add(item as StaffingNeedsFrequencyIODTO);
                }
            }
        }

        public StaffingNeedsFrequencyIOItem(string content, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(content, headType);
        }

        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType)
        {

            XDocument xdoc = XDocument.Parse(content);

            List<XElement> staffingNeedsFrequencyIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            foreach (var staffingNeedsFrequencyIOElement in staffingNeedsFrequencyIOElements)
            {
                StaffingNeedsFrequencyIODTO staffingNeedsFrequencyIODTO = new StaffingNeedsFrequencyIODTO();

                staffingNeedsFrequencyIODTO.DateFrom = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_DateFrom_TAG);
                staffingNeedsFrequencyIODTO.TimeFrom = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_TimeFrom_TAG);
                staffingNeedsFrequencyIODTO.DateTo = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_DateTo_TAG);
                staffingNeedsFrequencyIODTO.TimeTo = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_TimeTo_TAG);
                staffingNeedsFrequencyIODTO.NbrOfItems = decimal.Round(XmlUtil.GetElementDecimalValue(staffingNeedsFrequencyIOElement, XML_NbrOfItems_TAG), 2);
                staffingNeedsFrequencyIODTO.NbrOfMinutes = decimal.Round(XmlUtil.GetElementIntValue(staffingNeedsFrequencyIOElement, XML_NbrOfMinutes_TAG), 2);
                staffingNeedsFrequencyIODTO.NbrOfCustomers = decimal.Round(XmlUtil.GetElementDecimalValue(staffingNeedsFrequencyIOElement, XML_NbrOfCustomers_TAG), 2);
                staffingNeedsFrequencyIODTO.Amount = decimal.Round(XmlUtil.GetElementDecimalValue(staffingNeedsFrequencyIOElement, XML_Amount_TAG), 2); 
                staffingNeedsFrequencyIODTO.Cost = decimal.Round(XmlUtil.GetElementDecimalValue(staffingNeedsFrequencyIOElement, XML_Cost_TAG), 2);
                staffingNeedsFrequencyIODTO.ExternalCode = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_ExternalCode_TAG);
                staffingNeedsFrequencyIODTO.ParentExternalCode = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_ParentExternalCode_TAG);
                staffingNeedsFrequencyIODTO.FrequencyType = (FrequencyType)XmlUtil.GetElementIntValue(staffingNeedsFrequencyIOElement, XML_FrequencyType_TAG);
                staffingNeedsFrequencyIODTO.RowKey = XmlUtil.GetElementNullableValue(staffingNeedsFrequencyIOElement, XML_Key_TAG);

                if (staffingNeedsFrequencyIODTO.DateFrom.Contains("2") || staffingNeedsFrequencyIODTO.DateFrom.Contains("1900"))  //Simple check to filter out headers. DateFrom is Mandatory
                    frequencies.Add(staffingNeedsFrequencyIODTO);
            }

        }
        #endregion
    }
}
