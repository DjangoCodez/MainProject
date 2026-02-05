using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class PaymentIOItem
    {

        #region Collections

        public List<PaymentRowImportIODTO> PaymentIOs = new List<PaymentRowImportIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "Payment";
        public string XML_Type_TAG = "Type";
        public string XML_InvoiceNr_TAG = "InvoiceNr";
        public string XML_InvoiceSeqNr_TAG = "InvoiceSeqNr";
        public string XML_SupplierNr_TAG = "SupplierNr";
        public string XML_PaymentInvoiceNr_TAG = "PaymentInvoiceNr";
        public string XML_PaymentVoucherNr_TAG = "VoucherNr";
        public string XML_VoucherDate_TAG = "VoucherDate";
        public string XML_PaymentVoucherSeriesNr_TAG = "VoucherSeriesNr";
        public string XML_PayDate_TAG = "PayDate";
        public string XML_PaymentNr_TAG = "PaymentNr";
        public string XML_SeqNr_TAG = "SeqNr";
        public string XML_SysPaymentTypeId_TAG = "SysPaymentTypeId";
        public string XML_CurrencyCode_TAG = "CurrencyCode";
        public string XML_CurrencyRate_TAG = "CurrencyRate";
        public string XML_CurrencyDate_TAG = "CurrencyDate";
        public string XML_Amount_TAG = "Amount";
        public string XML_AmountCurrency_TAG = "AmountCurrency";
        public string XML_PaymentMethodCode_TAG = "PaymentMethodCode";
        public string XML_FullyPaid_TAG = "FullyPaid";
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_AccountDim6Nr_TAG = "AccountDim6Nr";
        public string XML_Status_TAG = "Status";

        #endregion

        #region Constructors

        public PaymentIOItem()
        {
        }

        public PaymentIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public PaymentIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> paymentYearElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(paymentYearElements, headType, actorCompanyId);

        }

        public void CreateObjects(List<XElement> paymentElements, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var paymentElement in paymentElements)
            {
                PaymentRowImportIODTO paymentIODTO = new PaymentRowImportIODTO();

                DateTime paymentDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(paymentElement, XML_PayDate_TAG), "yyyyMMdd");
                if (paymentDate == CalendarUtility.DATETIME_DEFAULT)
                    paymentDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(paymentElement, XML_PayDate_TAG), "yyyy-MM-dd");

                DateTime paymentVoucherDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(paymentElement, XML_VoucherDate_TAG), "yyyyMMdd");
                if (paymentVoucherDate == CalendarUtility.DATETIME_DEFAULT)
                    paymentVoucherDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(paymentElement, XML_VoucherDate_TAG), "yyyy-MM-dd");

                DateTime currencyDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(paymentElement, XML_CurrencyDate_TAG), "yyyyMMdd");
                if (currencyDate == CalendarUtility.DATETIME_DEFAULT)
                    currencyDate = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(paymentElement, XML_CurrencyDate_TAG), "yyyy-MM-dd");
                if (currencyDate == CalendarUtility.DATETIME_DEFAULT)
                    currencyDate = paymentDate;

                paymentIODTO.ChangeFullyPaid = true;
                paymentIODTO.Type = XmlUtil.GetElementIntValue(paymentElement, XML_Type_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(paymentElement, XML_Type_TAG)) : -1;
                paymentIODTO.SysPaymentTypeId = XmlUtil.GetElementIntValue(paymentElement, XML_SysPaymentTypeId_TAG) != 0 ? XmlUtil.GetElementIntValue(paymentElement, XML_SysPaymentTypeId_TAG) : -1;
                paymentIODTO.PayDate = paymentDate;
                paymentIODTO.InvoiceNr = XmlUtil.GetChildElementValue(paymentElement, XML_InvoiceNr_TAG);
                paymentIODTO.SupplierNr = XmlUtil.GetChildElementValue(paymentElement, XML_SupplierNr_TAG);
                paymentIODTO.InvoiceSeqNr = XmlUtil.GetElementNullableIntValue(paymentElement, XML_InvoiceSeqNr_TAG);
                paymentIODTO.PaymentNr = XmlUtil.GetChildElementValue(paymentElement, XML_PaymentNr_TAG);
                paymentIODTO.PaymentMethodCode = XmlUtil.GetChildElementValue(paymentElement, XML_PaymentMethodCode_TAG);
                paymentIODTO.VoucherDate = paymentVoucherDate;
                paymentIODTO.VoucherNr = XmlUtil.GetChildElementValue(paymentElement, XML_PaymentVoucherNr_TAG);
                paymentIODTO.VoucherSeriesNr = XmlUtil.GetChildElementValue(paymentElement, XML_PaymentVoucherSeriesNr_TAG);
                paymentIODTO.Amount = XmlUtil.GetElementDecimalValue(paymentElement, XML_Amount_TAG);
                paymentIODTO.AmountCurrency = XmlUtil.GetElementDecimalValue(paymentElement, XML_AmountCurrency_TAG);
                paymentIODTO.FullyPaid = XmlUtil.GetChildElementValue(paymentElement, XML_FullyPaid_TAG) == "1";
                paymentIODTO.CurrencyDate = currencyDate;
                paymentIODTO.CurrencyCode = XmlUtil.GetChildElementValue(paymentElement, XML_CurrencyCode_TAG);
                paymentIODTO.CurrencyRate = XmlUtil.GetElementDecimalValue(paymentElement, XML_CurrencyRate_TAG);
                paymentIODTO.AccountNr = XmlUtil.GetChildElementValue(paymentElement, XML_AccountNr_TAG);
                paymentIODTO.AccountDim2Nr = XmlUtil.GetChildElementValue(paymentElement, XML_AccountDim2Nr_TAG);
                paymentIODTO.AccountDim3Nr = XmlUtil.GetChildElementValue(paymentElement, XML_AccountDim3Nr_TAG);
                paymentIODTO.AccountDim4Nr = XmlUtil.GetChildElementValue(paymentElement, XML_AccountDim4Nr_TAG);
                paymentIODTO.AccountDim5Nr = XmlUtil.GetChildElementValue(paymentElement, XML_AccountDim5Nr_TAG);
                paymentIODTO.AccountDim6Nr = XmlUtil.GetChildElementValue(paymentElement, XML_AccountDim6Nr_TAG);
                paymentIODTO.Status = XmlUtil.GetElementIntValue(paymentElement, XML_Status_TAG);

                //Fix
                if (paymentIODTO.CurrencyRate <= 0)
                    paymentIODTO.CurrencyRate = 1;

                if (paymentIODTO.AmountCurrency != 0 && paymentIODTO.Amount == 0)
                    paymentIODTO.Amount = paymentIODTO.AmountCurrency;

                PaymentIOs.Add(paymentIODTO);
            }

        }
        #endregion
    }
}
