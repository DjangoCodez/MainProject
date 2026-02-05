using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class TimeCodeTransactionSimpleIOItem
    {

        #region Collections

        public List<TimeCodeTransactionSimpleIODTO> timeCodeTransactionSimpleIOs = new List<TimeCodeTransactionSimpleIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "TimeCodeTransactionSimple";

        public string XML_TimeCodeCode_TAG = "TimeCodeCode";
        public string XML_TimeCodeName_TAG = "TimeCodeName";
        public string XML_ProjectNr_TAG = "ProjectNr";
        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_Type_TAG = "Type";
        public string XML_Amount_TAG = "Amount";
        public string XML_AmountCurrency_TAG = "AmountCurrency";
        public string XML_AmountEntCurrency_TAG = "AmountEntCurrency";
        public string XML_AmountLedgerCurrency_TAG = "AmountLedgerCurrency";
        public string XML_Vat_TAG = "Vat";
        public string XML_VatCurrency_TAG = "VatCurrency";
        public string XML_VatEntCurrency_TAG = "VatEntCurrency";
        public string XML_VatLedgerCurrency_TAG = "VatLedgerCurrency";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_InvoiceQuantity_TAG = "InvoiceQuantity";
        public string XML_Start_TAG = "Start";
        public string XML_Stop_TAG = "Stop";
        public string XML_Comment_TAG = "Comment";
        public string XML_ExternalComment_TAG = "ExternalComment";
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_AccountNrSieDim1_TAG = "AccountNrSieDim1";
        public string XML_AccountNrSieDim2_TAG = "AccountNrSieDim2";
        public string XML_AccountNrSieDim6_TAG = "AccountNrSieDim6";
        public string XML_AccountNrSieDim7_TAG = "AccountNrSieDim7";
        public string XML_AccountNrSieDim8_TAG = "AccountNrSieDim8";
        public string XML_AccountNrSieDim9_TAG = "AccountNrSieDim9";
        public string XML_AccountNrSieDim10_TAG = "AccountNrSieDim10";
        public string XML_AccountNrSieDim30_TAG = "AccountNrSieDim30";
        public string XML_AccountNrSieDim40_TAG = "AccountNrSieDim40";
        public string XML_AccountNrSieDim50_TAG = "AccountNrSieDim50";

        #endregion

        #region Constructors

        public TimeCodeTransactionSimpleIOItem()
        {
        }

        public TimeCodeTransactionSimpleIOItem(List<string> contents)
        {
            CreateObjects(contents);
        }

        public TimeCodeTransactionSimpleIOItem(string content)
        {
            CreateObjects(content);
        }


        #endregion

        #region Parse

        private void CreateObjects(List<string> contents)
        {
            foreach (var content in contents)
            {
                CreateObjects(content);
            }
        }

        private void CreateObjects(string content)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementTimeCodeTransactionSimples = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementTimeCodeTransactionSimples);
        }

        public void CreateObjects(List<XElement> elementTimeCodeTransactionSimples)
        {
            foreach (var elementTimeCodeTransactionSimple in elementTimeCodeTransactionSimples)
            {
                TimeCodeTransactionSimpleIODTO timeCodeTransactionSimpleIODTO = new TimeCodeTransactionSimpleIODTO()
                {
                    TimeCodeCode = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_TimeCodeCode_TAG),
                    TimeCodeName = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_TimeCodeName_TAG),
                    ProjectNr = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_ProjectNr_TAG),
                    EmployeeNr = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_EmployeeNr_TAG),
                    Type = (SoeTimeCodeType)XmlUtil.GetElementIntValue(elementTimeCodeTransactionSimple, XML_Type_TAG),
                    Amount = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_Amount_TAG),
                    AmountCurrency = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_AmountCurrency_TAG),
                    AmountEntCurrency = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_AmountEntCurrency_TAG),
                    AmountLedgerCurrency = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_AmountLedgerCurrency_TAG),
                    Vat = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_Vat_TAG),
                    VatCurrency = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_VatCurrency_TAG),
                    VatEntCurrency = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_VatEntCurrency_TAG),
                    VatLedgerCurrency = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_VatLedgerCurrency_TAG),
                    Quantity = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_Quantity_TAG),
                    InvoiceQuantity = XmlUtil.GetElementDecimalValue(elementTimeCodeTransactionSimple, XML_InvoiceQuantity_TAG),
                    Start = XmlUtil.GetElementDateTimeValue(elementTimeCodeTransactionSimple, XML_Start_TAG),
                    Stop = XmlUtil.GetElementDateTimeValue(elementTimeCodeTransactionSimple, XML_Stop_TAG),
                    Comment = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_Comment_TAG),
                    ExternalComment = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_ExternalComment_TAG),
                    AccountNr = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNr_TAG),
                    AccountNrSieDim1 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim1_TAG),
                    AccountNrSieDim2 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim2_TAG),
                    AccountNrSieDim6 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim6_TAG),
                    AccountNrSieDim7 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim7_TAG),
                    AccountNrSieDim8 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim8_TAG),
                    AccountNrSieDim9 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim9_TAG),
                    AccountNrSieDim10 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim10_TAG),
                    AccountNrSieDim30 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim30_TAG),
                    AccountNrSieDim40 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim40_TAG),
                    AccountNrSieDim50 = XmlUtil.GetElementNullableValue(elementTimeCodeTransactionSimple, XML_AccountNrSieDim50_TAG),

                };

                if (!string.IsNullOrEmpty(timeCodeTransactionSimpleIODTO.EmployeeNr) && timeCodeTransactionSimpleIODTO.Type != SoeTimeCodeType.None)
                    timeCodeTransactionSimpleIOs.Add(timeCodeTransactionSimpleIODTO);
            }
        }
        #endregion
    }
}