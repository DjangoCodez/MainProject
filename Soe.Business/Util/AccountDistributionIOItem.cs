using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class AccountDistributionIOItem
    {

        #region Collections
        public List<AccountDistributionHeadIODTO> AccountDistributionHeads = new List<AccountDistributionHeadIODTO>();

        #endregion

        #region XML Nodes
        public const string XML_PARENT_TAG = "AccountDistributionHeadIO";

        public const string XML_VoucherSeriesType_TAG = "VoucherSeriesType";
        public const string XML_Type_TAG = "Type";
        public const string XML_Name_TAG = "Name";
        public const string XML_Description_TAG = "Description";
        public const string XML_TriggerType_TAG = "TriggerType";
        public const string XML_CalculationType_TAG = "CalculationType";
        public const string XML_Calculate_TAG = "Calculate";
        public const string XML_PeriodType_TAG = "PeriodType";
        public const string XML_PeriodValue_TAG = "PeriodValue";
        public const string XML_Sort_TAG = "Sort";
        public const string XML_StartDate_TAG = "StartDate";
        public const string XML_EndDate_TAG = "EndDate";
        public const string XML_DayNumber_TAG = "DayNumber";
        public const string XML_Amount_TAG = "Amount";
        public const string XML_AmountOperator_TAG = "AmountOperator";
        public const string XML_KeepRow_TAG = "KeepRow";
        public const string XML_UseInVoucher_TAG = "UseInVoucher";
        public const string XML_UseInSupplierInvoice_TAG = "UseInSupplierInvoice";
        public const string XML_UseInCustomerInvoice_TAG = "UseInCustomerInvoice";
        public const string XML_Dim1Expression_TAG = "Dim1Expression";
        public const string XML_Dim2Expression_TAG = "Dim2Expression";
        public const string XML_Dim3Expression_TAG = "Dim3Expression";
        public const string XML_Dim4Expression_TAG = "Dim4Expression";
        public const string XML_Dim5Expression_TAG = "Dim5Expression";
        public const string XML_Dim6Expression_TAG = "Dim6Expression";
        public const string XML_DimExpressionSieDim1_TAG = "DimExpressionSieDim1";
        public const string XML_DimExpressionSieDim6_TAG = "DimExpressionSieDim6";
        public const string XML_Dim1Nr_TAG = "Dim1Nr";
        public const string XML_Dim2Nr_TAG = "Dim2Nr";
        public const string XML_Dim3Nr_TAG = "Dim3Nr";
        public const string XML_Dim4Nr_TAG = "Dim4Nr";
        public const string XML_Dim5Nr_TAG = "Dim5Nr";
        public const string XML_Dim6Nr_TAG = "Dim6Nr";
        public const string XML_DimSieDim1_TAG = "DimSieDim1";
        public const string XML_DimSieDim6_TAG = "DimSieDim6";

        #endregion

        #region Constructors

        public AccountDistributionIOItem()
        {
        }

        public AccountDistributionIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public AccountDistributionIOItem(string content, TermGroup_IOImportHeadType headType)
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

            List<XElement> accountDistributionIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            foreach (var accountDistributionIOElement in accountDistributionIOElements)
            {
                AccountDistributionHeadIODTO accountDistributionIODTO = new AccountDistributionHeadIODTO();

                //Try different dateformats
                DateTime? startDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_StartDate_TAG), "yyyyMMdd");
                if (startDate == CalendarUtility.DATETIME_DEFAULT)
                    startDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_StartDate_TAG), "yyyy-MM-dd");

                DateTime? endDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_EndDate_TAG), "yyyyMMdd");
                if (endDate == CalendarUtility.DATETIME_DEFAULT)
                    endDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_EndDate_TAG), "yyyy-MM-dd");

                accountDistributionIODTO.VoucherSeriesType = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_VoucherSeriesType_TAG);
                accountDistributionIODTO.Amount = XmlUtil.GetElementDecimalValue(accountDistributionIOElement, XML_Amount_TAG);
                accountDistributionIODTO.AmountOperator = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_AmountOperator_TAG);
                accountDistributionIODTO.Calculate = XmlUtil.GetElementDecimalValue(accountDistributionIOElement, XML_Calculate_TAG);

                accountDistributionIODTO.CalculationType = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_CalculationType_TAG);
                accountDistributionIODTO.DayNumber = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_DayNumber_TAG);
                accountDistributionIODTO.Description = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Description_TAG);
                accountDistributionIODTO.Dim1Expression = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Dim1Expression_TAG);
                accountDistributionIODTO.Dim2Expression = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Dim2Expression_TAG);
                accountDistributionIODTO.Dim3Expression = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Dim3Expression_TAG);
                accountDistributionIODTO.Dim4Expression = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Dim4Expression_TAG);
                accountDistributionIODTO.Dim5Expression = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Dim5Expression_TAG);
                accountDistributionIODTO.Dim6Expression = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Dim6Expression_TAG);
                accountDistributionIODTO.DimExpressionSieDim1 = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_DimExpressionSieDim1_TAG);
                accountDistributionIODTO.DimExpressionSieDim6 = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_DimExpressionSieDim6_TAG);

                accountDistributionIODTO.Dim1Nr = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Dim1Nr_TAG);
                accountDistributionIODTO.Dim2Nr = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Dim2Nr_TAG);
                accountDistributionIODTO.Dim3Nr = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Dim3Nr_TAG);
                accountDistributionIODTO.Dim4Nr = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Dim4Nr_TAG);
                accountDistributionIODTO.Dim5Nr = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Dim5Nr_TAG);
                accountDistributionIODTO.Dim6Nr = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Dim6Nr_TAG);
                accountDistributionIODTO.DimSieDim1 = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_DimSieDim1_TAG);
                accountDistributionIODTO.DimSieDim6 = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_DimSieDim6_TAG);
                accountDistributionIODTO.EndDate = endDate;
                accountDistributionIODTO.StartDate = startDate;
                accountDistributionIODTO.KeepRow = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_KeepRow_TAG) == "1";
                accountDistributionIODTO.Name = XmlUtil.GetChildElementValue(accountDistributionIOElement, XML_Name_TAG);
                accountDistributionIODTO.PeriodType = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_PeriodType_TAG);
                accountDistributionIODTO.PeriodValue = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_PeriodValue_TAG);
                accountDistributionIODTO.Sort = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Sort_TAG);
                accountDistributionIODTO.TriggerType = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_TriggerType_TAG);
                accountDistributionIODTO.Type = XmlUtil.GetElementIntValue(accountDistributionIOElement, XML_Type_TAG);
                accountDistributionIODTO.UseInCustomerInvoice = XmlUtil.GetElementBoolValue(accountDistributionIOElement, XML_UseInCustomerInvoice_TAG);
                accountDistributionIODTO.UseInSupplierInvoice = XmlUtil.GetElementBoolValue(accountDistributionIOElement, XML_UseInSupplierInvoice_TAG);
                accountDistributionIODTO.UseInVoucher = XmlUtil.GetElementBoolValue(accountDistributionIOElement, XML_UseInVoucher_TAG);
                accountDistributionIODTO.Calculate = XmlUtil.GetElementDecimalValue(accountDistributionIOElement, XML_Calculate_TAG);

                if (accountDistributionIODTO.PeriodType == 0)
                    accountDistributionIODTO.PeriodType = 1;

                if (accountDistributionIODTO.TriggerType == 0)
                    accountDistributionIODTO.TriggerType = 1;

                AccountDistributionRowIOItem rowIOItem = new AccountDistributionRowIOItem();

                List<XElement> accountDistributionRowIOElements = XmlUtil.GetChildElements(accountDistributionIOElement, rowIOItem.XML_PARENT_TAG);
                rowIOItem.CreateObjects(accountDistributionRowIOElements, headType);

                if (accountDistributionIODTO.Rows == null)
                    accountDistributionIODTO.Rows = new List<AccountDistributionRowIODTO>();

                accountDistributionIODTO.Rows.AddRange(rowIOItem.accountDistributionRowIOs);

                AccountDistributionHeads.Add(accountDistributionIODTO);
            }

        }
        #endregion
    }
}
