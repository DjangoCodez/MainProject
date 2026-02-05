using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountIOItem
    {

        #region Collections

        public List<AccountIODTO> AccountIOs = new List<AccountIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "Account";
        public string XML_AccountDimNr_TAG = "AccountDimNr";
        public string XML_AccountDimSieNr_TAG = "AccountDimSieNr";
        public string XML_AccountDimName_TAG = "AccountDimName";
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_Name_TAG = "Name";
        public string XML_SysVatAccountNr_TAG = "SysVatAccountNr";
        public string XML_AmountStop_TAG = "AmountStop";
        public string XML_QuantityUnit_TAG = "QuantityUnit";
        public string XML_QuantityStop_TAG = "QuantityStop";
        public string XML_SieKpTyp_TAG = "SieKpTyp";
        public string XML_SruCode1_TAG = "SruCode1";
        public string XML_SruCode2_TAG = "SruCode2";
        public string XML_SruCode3_TAG = "SruCode3";
        public string XML_Created_TAG = "Created";
        public string XML_CreatedBy_TAG = "CreatedBy";
        public string XML_AccountDim2Mandatory_TAG = "AccountDim2Mandatory";
        public string XML_AccountDim3Mandatory_TAG = "AccountDim3Mandatory";
        public string XML_AccountDim4Mandatory_TAG = "AccountDim4Mandatory";
        public string XML_AccountDim5Mandatory_TAG = "AccountDim5Mandatory";
        public string XML_AccountDim6Mandatory_TAG = "AccountDim6Mandatory";
        public string XML_AccountDim2Stop_TAG = "AccountDim2Stop";
        public string XML_AccountDim3Stop_TAG = "AccountDim3Stop";
        public string XML_AccountDim4Stop_TAG = "AccountDim4Stop";
        public string XML_AccountDim5Stop_TAG = "AccountDim5Stop";
        public string XML_AccountDim6Stop_TAG = "AccountDim6Stop";
        public string XML_AccountDim2Default_TAG = "AccountDim2Default";
        public string XML_AccountDim3Default_TAG = "AccountDim3Default";
        public string XML_AccountDim4Default_TAG = "AccountDim4Default";
        public string XML_AccountDim5Default_TAG = "AccountDim5Default";
        public string XML_AccountDim6Default_TAG = "AccountDim6Default";
        public string XML_ExternalCode_TAG = "ExternalCode";
        public string XML_ParentAccountNr_TAG = "ParentAccountNr";

        #endregion

        #region Constructors

        public AccountIOItem()
        {
        }

        public AccountIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public AccountIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

                AccountIODTO accountIODTO = new AccountIODTO();

                DateTime created = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountElement, XML_Created_TAG), "yyyyMMdd");
                if (created == CalendarUtility.DATETIME_DEFAULT)
                    created = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountElement, XML_Created_TAG), "yyyy-MM-dd");

                string amountStopString = XmlUtil.GetChildElementValue(accountElement, XML_AmountStop_TAG);
                int amountStop = 0;

                if (amountStopString == "1" || amountStopString == "D" || amountStopString == "Debet")
                    amountStop = 1;

                int quantityStop = XmlUtil.GetChildElementValue(accountElement, XML_QuantityStop_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_QuantityStop_TAG)) : 0;
                bool accountDim2Mandatory = XmlUtil.GetElementBoolValue(accountElement, XML_AccountDim2Mandatory_TAG);
                bool accountDim3Mandatory = XmlUtil.GetElementBoolValue(accountElement, XML_AccountDim3Mandatory_TAG);
                bool accountDim4Mandatory = XmlUtil.GetElementBoolValue(accountElement, XML_AccountDim4Mandatory_TAG);
                bool accountDim5Mandatory = XmlUtil.GetElementBoolValue(accountElement, XML_AccountDim5Mandatory_TAG);
                bool accountDim6Mandatory = XmlUtil.GetElementBoolValue(accountElement, XML_AccountDim6Mandatory_TAG);
                int accountDim2Stop = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim2Stop_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDim2Stop_TAG)) : 0;
                int accountDim3Stop = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim3Stop_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDim3Stop_TAG)) : 0;
                int accountDim4Stop = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim4Stop_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDim4Stop_TAG)) : 0;
                int accountDim5Stop = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim5Stop_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDim5Stop_TAG)) : 0;
                int accountDim6Stop = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim6Stop_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDim6Stop_TAG)) : 0;
                accountIODTO.AccountDimNr = XmlUtil.GetChildElementValue(accountElement, XML_AccountDimNr_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDimNr_TAG)) : 0;
                accountIODTO.AccountDimSieNr = XmlUtil.GetChildElementValue(accountElement, XML_AccountDimSieNr_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(accountElement, XML_AccountDimSieNr_TAG)) : 0;
                accountIODTO.AccountDimName = XmlUtil.GetChildElementValue(accountElement, XML_AccountDimName_TAG);
                accountIODTO.AccountNr = XmlUtil.GetChildElementValue(accountElement, XML_AccountNr_TAG);
                accountIODTO.Name = XmlUtil.GetChildElementValue(accountElement, XML_Name_TAG);
                accountIODTO.SysVatAccountNr = XmlUtil.GetChildElementValue(accountElement, XML_SysVatAccountNr_TAG);
                accountIODTO.AmountStop = amountStop == 1;
                accountIODTO.QuantityStop = quantityStop == 1;
                accountIODTO.QuantityUnit = XmlUtil.GetChildElementValue(accountElement, XML_QuantityStop_TAG);
                accountIODTO.SieKpTyp = XmlUtil.GetChildElementValue(accountElement, XML_SieKpTyp_TAG);
                accountIODTO.SruCode1 = XmlUtil.GetChildElementValue(accountElement, XML_SruCode1_TAG);
                accountIODTO.SruCode2 = XmlUtil.GetChildElementValue(accountElement, XML_SruCode2_TAG);
                accountIODTO.SruCode3 = XmlUtil.GetChildElementValue(accountElement, XML_SruCode3_TAG);
                accountIODTO.AccountDim2Default = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim2Default_TAG);
                accountIODTO.AccountDim3Default = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim3Default_TAG);
                accountIODTO.AccountDim4Default = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim4Default_TAG);
                accountIODTO.AccountDim5Default = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim5Default_TAG);
                accountIODTO.AccountDim6Default = XmlUtil.GetChildElementValue(accountElement, XML_AccountDim6Default_TAG);
                accountIODTO.AccountDim2Mandatory = accountDim2Mandatory;
                accountIODTO.AccountDim3Mandatory = accountDim3Mandatory;
                accountIODTO.AccountDim4Mandatory = accountDim4Mandatory;
                accountIODTO.AccountDim5Mandatory = accountDim5Mandatory;
                accountIODTO.AccountDim6Mandatory = accountDim6Mandatory;
                accountIODTO.AccountDim2Stop = accountDim2Stop == 1;
                accountIODTO.AccountDim3Stop = accountDim3Stop == 1;
                accountIODTO.AccountDim4Stop = accountDim4Stop == 1;
                accountIODTO.AccountDim5Stop = accountDim5Stop == 1;
                accountIODTO.AccountDim6Stop = accountDim6Stop == 1;
                accountIODTO.Created = created;
                accountIODTO.CreatedBy = XmlUtil.GetChildElementValue(accountElement, XML_CreatedBy_TAG);
                accountIODTO.ExternalCode = XmlUtil.GetChildElementValue(accountElement, XML_ExternalCode_TAG);
                accountIODTO.ParentAccountNr = XmlUtil.GetChildElementValue(accountElement, XML_ParentAccountNr_TAG);

                AccountIOs.Add(accountIODTO);
            }

        }
        #endregion
    }
}
