using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class TimeCodeTransactionIOItem
    {

        #region Collections

        public List<TimeCodeTransactionIODTO> timeCodeTransactionIOs = new List<TimeCodeTransactionIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "TimeCodeTransactions";
        public string XML_TimeCodeCode_TAG = "TimeCodeCode";
        public string XML_ProductNr_TAG = "ProductNr";
        public string XML_PayrollProductNr_TAG = "PayrollProductNr";
        public string XML_ProductName_TAG = "ProductName";
        public string XML_CustomerInvoiceNr_TAG = "CustomerInvoiceNr";
        public string XML_ProjectNr_TAG = "ProjectNr";
        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_SupplierNr_TAG = "SupplierNr";
        public string XML_SupplierInvoiceNr_TAG = "SupplierInvoiceNr";
        
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
        
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_AccountDim6Nr_TAG = "AccountDim6Nr";
        
        public string XML_TimeCodeName_TAG = "TimeCodeName";
        
        public string XML_TimeCodeTypeName_TAG = "TimeCodeTypeName";
        public string XML_QuantityText_TAG = "QuantityText";
        
        public string XML_PayrollAttestStateName_TAG = "PayrollAttestStateName";
        
        public string XML_IsRegistrationTypeQuantity_TAG = "IsRegistrationTypeQuantity";
        public string XML_IsRegistrationTypeTime_TAG = "IsRegistrationTypeTime";
        public string XML_IsReadOnly_TAG = "IsReadOnly";
        public string XML_IsAddition_TAG = "IsAddition";
	

        #endregion

        #region Constructors

        public TimeCodeTransactionIOItem()
        {
        }

        public TimeCodeTransactionIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public TimeCodeTransactionIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementTimeCodeTransactions = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            CreateObjects(elementTimeCodeTransactions, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementTimeCodeTransactions, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementTimeCodeTransaction in elementTimeCodeTransactions)
            {
                TimeCodeTransactionIODTO headDTO = new TimeCodeTransactionIODTO();
                List<TimeCodeTransactionIODTO> headDTOs = new List<TimeCodeTransactionIODTO>();

                headDTO.TimeCodeCode  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_TimeCodeCode_TAG);
                headDTO.ProductNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_ProductNr_TAG);
                headDTO.PayrollProductNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_PayrollProductNr_TAG);
                headDTO.ProductName  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_ProductName_TAG);
                headDTO.CustomerInvoiceNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_CustomerInvoiceNr_TAG);
                headDTO.ProjectNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_ProjectNr_TAG);
                headDTO.EmployeeNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_EmployeeNr_TAG);
                headDTO.SupplierNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_SupplierNr_TAG);
                headDTO.SupplierInvoiceNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_SupplierInvoiceNr_TAG);
                
                headDTO.Amount  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_Amount_TAG);
                headDTO.AmountCurrency  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_AmountCurrency_TAG);
                headDTO.AmountEntCurrency  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_AmountEntCurrency_TAG);
                headDTO.AmountLedgerCurrency  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_AmountLedgerCurrency_TAG);
                headDTO.Vat  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_Vat_TAG);
                headDTO.VatCurrency  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_VatCurrency_TAG);
                headDTO.VatEntCurrency  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_VatEntCurrency_TAG);
                headDTO.VatLedgerCurrency  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_VatLedgerCurrency_TAG);
                headDTO.Quantity  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_Quantity_TAG);
                headDTO.InvoiceQuantity  = XmlUtil.GetElementDecimalValue(elementTimeCodeTransaction, XML_InvoiceQuantity_TAG);
                
                headDTO.Start = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_Start_TAG));
                headDTO.Stop = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_Stop_TAG));
                headDTO.Comment  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_Comment_TAG);
                
                headDTO.AccountNr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_AccountNr_TAG);
                headDTO.AccountDim2Nr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_AccountDim2Nr_TAG);
                headDTO.AccountDim3Nr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_AccountDim3Nr_TAG);
                headDTO.AccountDim4Nr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_AccountDim4Nr_TAG);
                headDTO.AccountDim5Nr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_AccountDim5Nr_TAG);
                headDTO.AccountDim6Nr  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_AccountDim6Nr_TAG);
                
                headDTO.TimeCodeName  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_TimeCodeName_TAG);
                
                headDTO.TimeCodeTypeName  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_TimeCodeTypeName_TAG);
                headDTO.QuantityText  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_QuantityText_TAG);
                
                headDTO.PayrollAttestStateName  = XmlUtil.GetChildElementValue(elementTimeCodeTransaction, XML_PayrollAttestStateName_TAG);
                
                headDTO.IsRegistrationTypeQuantity  = XmlUtil.GetElementBoolValue(elementTimeCodeTransaction, XML_IsRegistrationTypeQuantity_TAG);
                headDTO.IsRegistrationTypeTime = XmlUtil.GetElementBoolValue(elementTimeCodeTransaction, XML_IsRegistrationTypeTime_TAG);
                headDTO.IsReadOnly = XmlUtil.GetElementBoolValue(elementTimeCodeTransaction, XML_IsReadOnly_TAG);
                headDTO.IsAddition = XmlUtil.GetElementBoolValue(elementTimeCodeTransaction, XML_IsAddition_TAG);

                //Fix
                if (headDTO.IsRegistrationTypeTime)
                {
                    headDTO.Quantity = headDTO.Quantity * 60;
                    headDTO.InvoiceQuantity = headDTO.InvoiceQuantity * 60;
                }

                if (headDTO.IsRegistrationTypeQuantity && headDTO.Quantity == 0 && headDTO.Amount != 0)
                    headDTO.Quantity = 1;

                if (headDTO.IsAddition && headDTO.Quantity == 0 && headDTO.Amount != 0)
                    headDTO.Quantity = 1;

                timeCodeTransactionIOs.Add(headDTO);
            }

        }
        #endregion
    }
}