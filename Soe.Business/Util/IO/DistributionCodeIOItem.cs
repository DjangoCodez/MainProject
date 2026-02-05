using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class DistributionCodeIOItem
    {
        #region Collections

        public List<DistributionCodeHeadIODTO> distributionCodeIOs = new List<DistributionCodeHeadIODTO>();
        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "DistributionCodeHeadIO";
        public const string XML_CHILD_TAG = "DistributionCodePeriodIO";

        #region Parent

        public string XML_Type_TAG = "Type";
        public string XML_Name_TAG = "Name";
        public string XML_NoOfPeriods_TAG = "NoOfPeriods";

        #endregion

        #region Child Nodes

        public string XML_Number_TAG = "Number";
        public string XML_Percent_TAG = "Percent";
        public string XML_Comment_TAG = "Comment";


        #endregion

        #endregion

        #region Constructors

        public DistributionCodeIOItem()
        {
        }

        public DistributionCodeIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public DistributionCodeIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementDistributionCodes = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            CreateObjects(elementDistributionCodes, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementDistributionCodes, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementDistributionCode in elementDistributionCodes)
            {
                DistributionCodeHeadIODTO headDTO = new DistributionCodeHeadIODTO();
                List<DistributionCodePeriodIODTO> rowDTOs = new List<DistributionCodePeriodIODTO>();

                headDTO.Type = XmlUtil.GetElementIntValue(elementDistributionCode, XML_Type_TAG);
                headDTO.Name = XmlUtil.GetChildElementValue(elementDistributionCode, XML_Name_TAG);
                headDTO.NoOfPeriods = XmlUtil.GetElementIntValue(elementDistributionCode, XML_NoOfPeriods_TAG);
                headDTO.Periods = new List<DistributionCodePeriodIODTO>();

                List<XElement> rowElements = XmlUtil.GetChildElements(elementDistributionCode, XML_CHILD_TAG);

                foreach (var rowElement in rowElements)
                {
                    DistributionCodePeriodIODTO rowDTO = new DistributionCodePeriodIODTO();

                    rowDTO.Number = XmlUtil.GetElementIntValue(rowElement, XML_Number_TAG);
                    rowDTO.Percent = XmlUtil.GetElementDecimalValue(rowElement, XML_Percent_TAG);
                    rowDTO.Comment = XmlUtil.GetChildElementValue(rowElement, XML_Comment_TAG);


                    if (headDTO.Type == 0)
                        headDTO.Type = (int)TermGroup_ChecklistHeadType.Order;

                    headDTO.Periods.Add(rowDTO);
                }

                #region Fix

                if (headDTO.Type == 0)
                    headDTO.Type = (int)DistributionCodeBudgetType.AccountingBudget;

                if (headDTO.NoOfPeriods == 0)
                    headDTO.NoOfPeriods = headDTO.Periods.Count;

                #endregion

                distributionCodeIOs.Add(headDTO);


            }
        }

        #endregion

    }

}