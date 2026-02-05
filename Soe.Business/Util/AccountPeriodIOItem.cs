using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountPeriodIOItem
    {

        #region Collections

        public List<AccountPeriodIODTO> accountPeriodIOs = new List<AccountPeriodIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "AccountPeriod";
        public string XML_PeriodStatus_TAG = "PeriodStatus";
        public string XML_PeriodNr_TAG = "PeriodNr";
        public string XML_PeriodFrom_TAG = "PeriodFrom";
        public string XML_PeriodTo_TAG = "PeriodTo";

        public string XML_YearFrom_TAG = "YearFrom";
        public string XML_MonthFrom_TAG = "MonthFrom";
        public string XML_YearTo_TAG = "YearTo";
        public string XML_MonthTo_TAG = "MonthTo";

        #endregion

        #region Constructors

        public AccountPeriodIOItem()
        {
        }

        public AccountPeriodIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public AccountPeriodIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> accountDistributionRowElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(accountDistributionRowElements, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> accountPeriodElements, TermGroup_IOImportHeadType headType, int actorCompanyId, int YearStatus = (int)TermGroup_AccountStatus.Closed)
        {
            foreach (var accountPeriodElement in accountPeriodElements)
            {
                AccountPeriodIODTO accountPeriodIODTO = new AccountPeriodIODTO();

                    DateTime from = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodFrom_TAG), "yyyyMMdd");
                    if (from == CalendarUtility.DATETIME_DEFAULT)
                        from = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodFrom_TAG), "yyyy-MM-dd");

                    DateTime to = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodTo_TAG), "yyyyMMdd");
                    if (to == CalendarUtility.DATETIME_DEFAULT)
                        to = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodTo_TAG), "yyyy-MM-dd");

                    int yearFrom = XmlUtil.GetChildElementValue(accountPeriodElement, XML_YearFrom_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountPeriodElement, XML_YearFrom_TAG)) : 0;
                    int yearTo = XmlUtil.GetChildElementValue(accountPeriodElement, XML_YearTo_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountPeriodElement, XML_YearTo_TAG)) : 0;
                    int monthFrom = XmlUtil.GetChildElementValue(accountPeriodElement, XML_MonthFrom_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountPeriodElement, XML_MonthFrom_TAG)) : 0;
                    int monthTo = XmlUtil.GetChildElementValue(accountPeriodElement, XML_MonthTo_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountPeriodElement, XML_MonthTo_TAG)) : 0;

                    if (from == CalendarUtility.DATETIME_DEFAULT && yearFrom != 0 && monthFrom != 0)
                    {
                        yearFrom = yearFrom >= 100 ? yearFrom : (yearFrom > 70 ? yearFrom + 1900 : yearFrom + 2000);

                        from = new DateTime(yearFrom, monthFrom, 1);
                    }

                    if (to == CalendarUtility.DATETIME_DEFAULT && yearTo != 0 && monthTo != 0)
                    {
                        yearTo = yearTo >= 100 ? yearTo : (yearTo > 70 ? yearTo + 1900 : yearTo + 2000);

                        to = new DateTime(yearFrom, monthFrom, 1);
                        to = CalendarUtility.GetLastDateOfMonth(to);
                    }

                    if (from != CalendarUtility.DATETIME_DEFAULT && to == CalendarUtility.DATETIME_DEFAULT)
                    {
                        to = CalendarUtility.GetLastDateOfMonth(from);
                    }

                    if (from == CalendarUtility.DATETIME_DEFAULT || to == CalendarUtility.DATETIME_DEFAULT)
                        continue;

                    accountPeriodIODTO.Status = XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodStatus_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodStatus_TAG)) : 0;

                    if (YearStatus != (int)TermGroup_AccountStatus.Closed && accountPeriodIODTO.Status == 0)
                        accountPeriodIODTO.Status = YearStatus;
                    else
                        accountPeriodIODTO.Status = (int)TermGroup_AccountStatus.Closed;

                    accountPeriodIODTO.From = from;
                    accountPeriodIODTO.To = to;
                    accountPeriodIODTO.PeriodNr = XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodNr_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountPeriodElement, XML_PeriodNr_TAG)) : 0;
                    
                
                accountPeriodIOs.Add(accountPeriodIODTO);
            }
        }
        #endregion
    }
}
