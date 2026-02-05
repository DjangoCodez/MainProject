using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountYearIOItem
    {

        #region Collections

        public List<AccountYearIODTO> AccountYearIOs = new List<AccountYearIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "AccountYear";
        public string XML_Status_TAG = "Status";
        public string XML_From_TAG = "From";        
        public string XML_To_TAG = "To";
        public string XML_YearFrom_TAG = "YearFrom";
        public string XML_MonthFrom_TAG = "MonthFrom";
        public string XML_YearTo_TAG = "YearTo";
        public string XML_MonthTo_TAG = "MonthTo";

        #endregion

        #region Constructors

        public AccountYearIOItem()
        {
        }

        public AccountYearIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public AccountYearIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

        public void CreateObjects(List<XElement> accountYearElements, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var accountYearElement in accountYearElements)
            {
                AccountYearIODTO accountYearIODTO = new AccountYearIODTO();

                DateTime startDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountYearElement, XML_From_TAG), "yyyyMMdd");
                if (startDate == CalendarUtility.DATETIME_DEFAULT)
                    startDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountYearElement, XML_From_TAG), "yyyy-MM-dd");

                DateTime endDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountYearElement, XML_To_TAG), "yyyyMMdd");
                if (endDate == CalendarUtility.DATETIME_DEFAULT)
                    endDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountYearElement, XML_To_TAG), "yyyy-MM-dd");

                int yearFrom = XmlUtil.GetChildElementValue(accountYearElement, XML_YearFrom_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountYearElement, XML_YearFrom_TAG)) : 0;
                int yearTo = XmlUtil.GetChildElementValue(accountYearElement, XML_YearTo_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountYearElement, XML_YearTo_TAG)) : 0;
                int monthFrom = XmlUtil.GetChildElementValue(accountYearElement, XML_MonthFrom_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountYearElement, XML_MonthFrom_TAG)) : 0;
                int monthTo = XmlUtil.GetChildElementValue(accountYearElement, XML_MonthTo_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountYearElement, XML_MonthTo_TAG)) : 0;

                if (startDate == CalendarUtility.DATETIME_DEFAULT && yearFrom != 0 && monthFrom != 0)
                {
                    yearFrom = yearFrom >= 100 ? yearFrom : (yearFrom > 70 ? yearFrom + 1900 : yearFrom + 2000);

                    startDate = new DateTime(yearFrom, monthFrom, 1);
                }

                if (endDate == CalendarUtility.DATETIME_DEFAULT && yearTo != 0 && monthTo != 0)
                {
                    yearTo = yearTo >= 100 ? yearTo : (yearTo > 70 ? yearTo + 1900 : yearTo + 2000);

                    endDate = new DateTime(yearTo, monthTo, 1);
                    endDate = CalendarUtility.GetLastDateOfMonth(endDate);
                }

                if (endDate == CalendarUtility.DATETIME_DEFAULT || startDate == CalendarUtility.DATETIME_DEFAULT)
                    continue;

                if (DateTime.Now > startDate && DateTime.Now < endDate)
                    accountYearIODTO.Status = (int)TermGroup_AccountStatus.Open;
                else
                    accountYearIODTO.Status = (int)TermGroup_AccountStatus.Closed;

                accountYearIODTO.ActorCompanyId = actorCompanyId;
                accountYearIODTO.From = startDate;
                accountYearIODTO.To = endDate;
                

                //Add periods

                AccountPeriodIOItem periodIOItem = new AccountPeriodIOItem();

                List<XElement> accountPeriodIOElements = XmlUtil.GetChildElements(accountYearElement, periodIOItem.XML_PARENT_TAG);
                periodIOItem.CreateObjects(accountPeriodIOElements, headType, actorCompanyId, accountYearIODTO.Status);

                accountYearIODTO.AccountPeriods.AddRange(periodIOItem.accountPeriodIOs);

                //Add voucherSeries

                VoucherSeriesIOItem voucherSeriesIOItem = new VoucherSeriesIOItem();

                List<XElement> voucherSeriesIOElements = XmlUtil.GetChildElements(accountYearElement, voucherSeriesIOItem.XML_PARENT_TAG);
                voucherSeriesIOItem.CreateObjects(voucherSeriesIOElements, headType, actorCompanyId);

                accountYearIODTO.VoucherSeries.AddRange(voucherSeriesIOItem.voucherSeriesIOs);

                AccountYearIOs.Add(accountYearIODTO);
            }

        }
        #endregion
    }
}
