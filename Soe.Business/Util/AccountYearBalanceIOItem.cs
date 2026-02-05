using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountYearBalanceIOItem
    {

        #region Collections

        public List<AccountYearBalanceIODTO> accountYearBalanceIOs = new List<AccountYearBalanceIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "AccountYearBalance";
        public string XML_AccountYearStartDate_TAG = "AccountYearStartDate";
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_Balance_TAG = "Balance";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_Year_TAG = "Year";
        public string XML_Month_TAG = "Month";
        public string XML_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_AccountDim6Nr_TAG = "AccountDim6Nr";
        public string XML_AccountNrSieDim1_TAG = "AccountNrSieDim1";
        public string XML_AccountNrSieDim6_TAG = "AccountNrSieDim6";

        #endregion

        #region Constructors

        public AccountYearBalanceIOItem()
        {
        }

        public AccountYearBalanceIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public AccountYearBalanceIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

        public void CreateObjects(List<XElement> accountYearBalanceElements, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {

            foreach (var accountYearBalanceElement in accountYearBalanceElements)
            {

                AccountYearBalanceIODTO accountYearBalanceIODTO = new AccountYearBalanceIODTO();

                DateTime accountYearStartDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountYearStartDate_TAG), "yyyyMMdd");
                if (accountYearStartDate == CalendarUtility.DATETIME_DEFAULT)
                    accountYearStartDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountYearStartDate_TAG), "yyyy-MM-dd");

                int year = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Year_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Year_TAG)) : 0;
                int month = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Month_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Month_TAG)) : 0;

                if (year != 0 && month == 0) 
                    month = 1;

                if (accountYearStartDate == CalendarUtility.DATETIME_DEFAULT && year != 0)
                    accountYearStartDate = new DateTime(year, month, 1);

                accountYearBalanceIODTO.AccountNr = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountNr_TAG);
                accountYearBalanceIODTO.AccountDim2Nr = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountDim2Nr_TAG);
                accountYearBalanceIODTO.AccountDim3Nr = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountDim3Nr_TAG);
                accountYearBalanceIODTO.AccountDim4Nr = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountDim4Nr_TAG);
                accountYearBalanceIODTO.AccountDim5Nr = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountDim5Nr_TAG);
                accountYearBalanceIODTO.AccountDim6Nr = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountDim6Nr_TAG);
                accountYearBalanceIODTO.AccountNrSieDim1 = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountNrSieDim1_TAG);
                accountYearBalanceIODTO.AccountNrSieDim6 = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_AccountNrSieDim6_TAG);
                accountYearBalanceIODTO.Balance = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Balance_TAG) != "" ? Convert.ToDecimal(XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Balance_TAG).Replace(".",",")) : 0;
                accountYearBalanceIODTO.Quantity = XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Quantity_TAG) != "" ? Convert.ToDecimal(XmlUtil.GetChildElementValue(accountYearBalanceElement, XML_Quantity_TAG).Replace(".", ",")) : 0;
                accountYearBalanceIODTO.AccountYearStartDate = accountYearStartDate;

                accountYearBalanceIOs.Add(accountYearBalanceIODTO);
            }

        }
        #endregion
    }
}
