using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class GrossProfitCodeIOItem
    {

        #region Collections

        public List<GrossProfitCodeIODTO> grossProfitCodeIOs = new List<GrossProfitCodeIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "GrossProfitCodeIO";
        public const string XML_CHILD_TAG = "GrossProfitCodePeriodIO";

        public string XML_AccountYearDateFrom_TAG = "AccountYearDateFrom";
        public string XML_AccountYear_TAG = "AccountYear";
        public string XML_AccountYearPeriod_TAG = "AccountYearPeriod";

        public string XML_AccountDimNr_TAG = "AccountDimNr";
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_Code_TAG = "Code";
        public string XML_Name_TAG = "Name";
        public string XML_Description_TAG = "Description";
        public string XML_OpeningBalance_TAG = "OpeningBalance";
        public string XML_Period1_TAG = "Period1";
        public string XML_Period2_TAG = "Period2";
        public string XML_Period3_TAG = "Period3";
        public string XML_Period4_TAG = "Period4";
        public string XML_Period5_TAG = "Period5";
        public string XML_Period6_TAG = "Period6";
        public string XML_Period7_TAG = "Period7";
        public string XML_Period8_TAG = "Period8";
        public string XML_Period9_TAG = "Period9";
        public string XML_Period10_TAG = "Period10";
        public string XML_Period11_TAG = "Period11";
        public string XML_Period12_TAG = "Period12";

        #region Child Nodes

        public string XML_CHILD_PeriodNr_TAG = "PeriodNr";
        public string XML_CHILD_Value_TAG = "Value";

        #endregion


        #endregion

        #region Constructors

        public GrossProfitCodeIOItem()
        {
        }

        public GrossProfitCodeIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public GrossProfitCodeIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementGrossProfitCodes = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementGrossProfitCodes, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementGrossProfitCodes, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            int name = 1;

            foreach (var elementGrossProfitCode in elementGrossProfitCodes)
            {
                GrossProfitCodeIODTO grossProfitCodeIODTO = new GrossProfitCodeIODTO();

                grossProfitCodeIODTO.AccountYearDateFrom = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_AccountYearDateFrom_TAG));

                if (grossProfitCodeIODTO.AccountYearDateFrom == CalendarUtility.DATETIME_DEFAULT && !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_AccountYear_TAG)) && !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_AccountYearPeriod_TAG)))
                {
                    DateTime date = new DateTime(XmlUtil.GetElementIntValue(elementGrossProfitCode, XML_AccountYear_TAG), XmlUtil.GetElementIntValue(elementGrossProfitCode, XML_AccountYearPeriod_TAG), 1);
                    grossProfitCodeIODTO.AccountYearDateFrom = date;
                }

                grossProfitCodeIODTO.AccountDimNr  = XmlUtil.GetElementIntValue(elementGrossProfitCode, XML_AccountDimNr_TAG);
                grossProfitCodeIODTO.AccountNr = XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_AccountNr_TAG);
                grossProfitCodeIODTO.Code = XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_Code_TAG);
                grossProfitCodeIODTO.Name = !string.IsNullOrEmpty(XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_Name_TAG)) ? XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_Name_TAG) : name.ToString() ;
                grossProfitCodeIODTO.Description = XmlUtil.GetChildElementValue(elementGrossProfitCode, XML_Description_TAG);
                grossProfitCodeIODTO.OpeningBalance  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_OpeningBalance_TAG);
                grossProfitCodeIODTO.Period1  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period1_TAG);
                grossProfitCodeIODTO.Period2  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period2_TAG);
                grossProfitCodeIODTO.Period3  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period3_TAG);
                grossProfitCodeIODTO.Period4  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period4_TAG);
                grossProfitCodeIODTO.Period5  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period5_TAG);
                grossProfitCodeIODTO.Period6  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period6_TAG);
                grossProfitCodeIODTO.Period7  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period7_TAG);
                grossProfitCodeIODTO.Period8  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period8_TAG);
                grossProfitCodeIODTO.Period9  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period9_TAG);
                grossProfitCodeIODTO.Period10  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period10_TAG);
                grossProfitCodeIODTO.Period11  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period11_TAG);
                grossProfitCodeIODTO.Period12  = XmlUtil.GetElementDecimalValue(elementGrossProfitCode, XML_Period12_TAG);

                name++;

                List<XElement> rowElements = XmlUtil.GetChildElements(elementGrossProfitCode, XML_CHILD_TAG);

                foreach (var rowElement in rowElements)
                {
                    int period = XmlUtil.GetElementIntValue(rowElement, XML_CHILD_PeriodNr_TAG);
                    decimal value = XmlUtil.GetElementDecimalValue(rowElement, XML_CHILD_Value_TAG);

                    if (period == 1) grossProfitCodeIODTO.Period1 = value;
                    else if (period == 2) grossProfitCodeIODTO.Period2 = value;
                    else if (period == 3) grossProfitCodeIODTO.Period3 = value;
                    else if (period == 4) grossProfitCodeIODTO.Period4 = value;
                    else if (period == 5) grossProfitCodeIODTO.Period5 = value;
                    else if (period == 6) grossProfitCodeIODTO.Period6 = value;
                    else if (period == 7) grossProfitCodeIODTO.Period7 = value;
                    else if (period == 8) grossProfitCodeIODTO.Period8 = value;
                    else if (period == 9) grossProfitCodeIODTO.Period9 = value;
                    else if (period == 10) grossProfitCodeIODTO.Period10 = value;
                    else if (period == 11) grossProfitCodeIODTO.Period11 = value;
                    else if (period == 12) grossProfitCodeIODTO.Period12 = value;
                }          

                grossProfitCodeIOs.Add(grossProfitCodeIODTO);
            }

        }
        #endregion
    }
}