using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class SupplierInvoiceIOItem
    {
        #region Variables

        private string specialFunctionality;

        #endregion
        #region Collections

        public List<SupplierInvoiceIORawData> supplierInvoices = new List<SupplierInvoiceIORawData>();
        
        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "SupplierInvoiceHeadIO";
        public const string XML_CHILD_TAG = "SupplierInvoiceAccountingRowIO";

        #region Parent

        public string XML_SupplierNr_TAG = "SupplierNr";
        public string XML_SupplierInvoiceNr_TAG = "SupplierInvoiceNumber";
	    public string XML_SeqNr_TAG = "SeqNr";
	    public string XML_InvoiceDate_TAG = "InvoiceDate";
	    public string XML_DueDate_TAG = "DueDate";
	    public string XML_VoucherDate_TAG = "VoucherDate";
	    public string XML_Reference_Our_TAG = "ReferenceOur";
	    public string XML_Reference_Your_TAG = "ReferenceYour";
	    public string XML_OCR_TAG = "OCR";
	    public string XML_CurrencyRate_TAG = "CurrencyRate";
	    public string XML_CurrencyDate_TAG = "CurrencyDate";
	    public string XML_TotalAmount_TAG = "TotalAmount";
	    public string XML_TotalAmount_Currency_TAG = "TotalAmountCurrency";
	    public string XML_VATAmount_TAG = "VATAmount";
	    public string XML_VATAmount_Currency_TAG = "VATAmountCurrency";
	    public string XML_PaidAmount_TAG = "PaidAmount";
	    public string XML_PaidAmount_Currency_TAG = "PaidAmountCurrency";
	    public string XML_RemainingAmount_TAG = "RemainingAmount";
	    public string XML_FullyPayed_TAG = "FullyPayed";
	    public string XML_PaymentNr_TAG = "PaymentNr";
	    public string XML_VoucherNr_TAG = "VoucherNr";
	    public string XML_CreateAccountingInXE_TAG = "CreateAccountingInXE";
        public string XML_Note_TAG = "Note";
        public string XML_Currency_TAG = "Currency";
        public string XML_BillingType_TAG = "BillingType";

        public string XML_InvoiceNrPart1_TAG = "InvoiceNrPart1";
        public string XML_InvoiceNrPart2_TAG = "InvoiceNrPart2";
        public string XML_InvoiceNrPart3_TAG = "InvoiceNrPart3";

        public string XML_SupplierNrPart1_TAG = "SupplierNrPart1";
        public string XML_SupplierNrPart2_TAG = "SupplierNrPart2";
        public string XML_SupplierNrPart3_TAG = "SupplierNrPart3";
        public string XML_SupplierNrPart4_TAG = "SupplierNrPart4";

        public string XML_OriginStatus_TAG = "OriginStatus";

        #endregion

        #region Child Nodes
        
        public string XML_CHILD_SupplierInvoiceHeadIOId_TAG = "SupplierInvoiceHeadIOId";
        public string XML_CHILD_SupplierInvoiceId_TAG = "SupplierInvoiceId";
        public string XML_CHILD_SupplierId_TAG = "SupplierId";
        public string XML_CHILD_SupplierNr_TAG = "SupplierNr";
        public string XML_CHILD_SupplierInvoiceNr_TAG = "SupplierInvoiceNr";
        public string XML_CHILD_Amount_TAG = "Amount";
        public string XML_CHILD_AmountCurrency_TAG = "AmountCurrency";
        public string XML_CHILD_Quantity_TAG = "Quantity";
        public string XML_CHILD_AccountNr_TAG = "AccountNr";        
        public string XML_CHILD_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_CHILD_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_CHILD_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_CHILD_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_CHILD_AccountDim6Nr_TAG = "AccountDim6Nr";
        public string XML_CHILD_AccountSieDim1_TAG = "AccountSieDim1";
        public string XML_CHILD_AccountSieDim6_TAG = "AccountSieDim6";
        public string XML_CHILD_Text_TAG = "Text";
        public string XML_CHILD_InterimRow_TAG = "InterimRow";
        public string XML_CHILD_VatRow_TAG = "VatRow";
        public string XML_CHILD_ContractorVatRow_TAG = "ContractorVatRow";
        public string XML_CHILD_CreditRow_TAG = "CreditRow";
        public string XML_CHILD_DebitRow_TAG = "DebitRow";

        #endregion

        #endregion

        #region Constructors

        public SupplierInvoiceIOItem()
        {
        }

        public SupplierInvoiceIOItem(List<string> contents, TermGroup_IOImportHeadType headType, string specialFunctionality)
        {
            this.specialFunctionality = specialFunctionality;
            CreateObjects(contents, headType);
        }

        public SupplierInvoiceIOItem(string content, TermGroup_IOImportHeadType headType)
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

            List<XElement> supplierInvoiceIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            foreach (var supplierInvoiceIOElement in supplierInvoiceIOElements)
            {
                SupplierInvoiceIORawData supplierInvoiceIOData = new SupplierInvoiceIORawData();

                #region Extract SupplierInvoice Data

                supplierInvoiceIOData.SupplierNr = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_SupplierNr_TAG);
	            supplierInvoiceIOData.SupplierInvoiceNr = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_SupplierInvoiceNr_TAG);
	            supplierInvoiceIOData.SeqNr = XmlUtil.GetElementNullableIntValue(supplierInvoiceIOElement, XML_SeqNr_TAG);
	            supplierInvoiceIOData.InvoiceDate =  CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_InvoiceDate_TAG));
                supplierInvoiceIOData.DueDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_DueDate_TAG));
	            supplierInvoiceIOData.VoucherDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_VoucherDate_TAG));
	            supplierInvoiceIOData.ReferenceOur = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_Reference_Our_TAG);
	            supplierInvoiceIOData.ReferenceYour = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_Reference_Your_TAG);
	            supplierInvoiceIOData.OCR = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_OCR_TAG);
	            supplierInvoiceIOData.CurrencyRate = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_CurrencyRate_TAG);
                supplierInvoiceIOData.CurrencyDate = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_CurrencyDate_TAG));
                supplierInvoiceIOData.TotalAmount = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_TotalAmount_TAG);
                supplierInvoiceIOData.TotalAmountCurrency = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_TotalAmount_Currency_TAG);
                supplierInvoiceIOData.VATAmount = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_VATAmount_TAG);
                supplierInvoiceIOData.VATAmountCurrency = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_VATAmount_Currency_TAG);
                supplierInvoiceIOData.PaidAmount = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_PaidAmount_TAG);
                supplierInvoiceIOData.PaidAmountCurrency = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_PaidAmount_Currency_TAG);
                supplierInvoiceIOData.RemainingAmount = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceIOElement, XML_RemainingAmount_TAG);
	            supplierInvoiceIOData.FullyPayed = XmlUtil.GetElementNullableBoolValue(supplierInvoiceIOElement, XML_FullyPayed_TAG);
	            supplierInvoiceIOData.PaymentNr = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_PaymentNr_TAG);
	            supplierInvoiceIOData.VoucherNr = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_VoucherNr_TAG);
                supplierInvoiceIOData.CreateAccountingInXE = XmlUtil.GetElementNullableBoolValue(supplierInvoiceIOElement, XML_CreateAccountingInXE_TAG);
                supplierInvoiceIOData.Note = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_Note_TAG);
                supplierInvoiceIOData.Currency = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_Currency_TAG);
                supplierInvoiceIOData.BillingType = XmlUtil.GetElementNullableIntValue(supplierInvoiceIOElement, XML_BillingType_TAG);
                supplierInvoiceIOData.OriginStatus = XmlUtil.GetElementNullableIntValue(supplierInvoiceIOElement, XML_OriginStatus_TAG);

                if (!supplierInvoiceIOData.InvoiceDate.HasValue && !supplierInvoiceIOData.DueDate.HasValue && !supplierInvoiceIOData.VoucherDate.HasValue)
                    supplierInvoiceIOData.InvoiceDate = supplierInvoiceIOData.DueDate = supplierInvoiceIOData.VoucherDate = DateTime.Now.Date;

                #endregion

                if (specialFunctionality == "ansjö")
                {
                    #region InvoiceNr field parts

                    string InvoiceNrPart1 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_InvoiceNrPart1_TAG);
                    string InvoiceNrPart2 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_InvoiceNrPart2_TAG);
                    string InvoiceNrPart3 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_InvoiceNrPart3_TAG);

                    if (string.IsNullOrEmpty(supplierInvoiceIOData.SupplierInvoiceNr))
                    {
                        supplierInvoiceIOData.SupplierInvoiceNr = string.Empty;

                        if (!string.IsNullOrEmpty(InvoiceNrPart1))
                            supplierInvoiceIOData.SupplierInvoiceNr += InvoiceNrPart1;

                        if (!string.IsNullOrEmpty(InvoiceNrPart2))
                            supplierInvoiceIOData.SupplierInvoiceNr += InvoiceNrPart2;

                        if (!string.IsNullOrEmpty(InvoiceNrPart3))
                            supplierInvoiceIOData.SupplierInvoiceNr += InvoiceNrPart3;
                    }

                    #endregion

                    #region SupplierNr field parts

                    string SupplierNrPart1 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_SupplierNrPart1_TAG);
                    string SupplierNrPart2 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_SupplierNrPart2_TAG);
                    string SupplierNrPart3 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_SupplierNrPart3_TAG);
                    string SupplierNrPart4 = XmlUtil.GetChildElementValue(supplierInvoiceIOElement, XML_SupplierNrPart4_TAG);

                    if (string.IsNullOrEmpty(supplierInvoiceIOData.SupplierNr))
                    {
                        supplierInvoiceIOData.SupplierNr = string.Empty;

                        if (!string.IsNullOrEmpty(SupplierNrPart1))
                            supplierInvoiceIOData.SupplierNr += SupplierNrPart1;

                        if (!string.IsNullOrEmpty(SupplierNrPart2))
                            supplierInvoiceIOData.SupplierNr += SupplierNrPart2;

                        if (!string.IsNullOrEmpty(SupplierNrPart3))
                            supplierInvoiceIOData.SupplierNr += SupplierNrPart3;

                        if (!string.IsNullOrEmpty(SupplierNrPart4))
                            supplierInvoiceIOData.SupplierNr += SupplierNrPart4;
                    }

                    #endregion
                }

                List<XElement> SupplierInvoiceAccountingRowIODataElements = XmlUtil.GetChildElements(supplierInvoiceIOElement, XML_CHILD_TAG);

                foreach (var supplierInvoiceAccountingRowIODataElement in SupplierInvoiceAccountingRowIODataElements)
                {
                    SupplierInvoiceAccountingRowIORawData supplierInvoiceAccountingRowIOData = new SupplierInvoiceAccountingRowIORawData();

                    #region Extract SupplierInvoiceAccountingRowIO Data

                    supplierInvoiceAccountingRowIOData.SupplierNr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_SupplierNr_TAG);
                    supplierInvoiceAccountingRowIOData.SupplierInvoiceNr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_SupplierInvoiceNr_TAG);
                    supplierInvoiceAccountingRowIOData.Amount = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_Amount_TAG);
                    supplierInvoiceAccountingRowIOData.AmountCurrency = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AmountCurrency_TAG);
                    supplierInvoiceAccountingRowIOData.Quantity = XmlUtil.GetElementNullableDecimalValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_Quantity_TAG);
                    supplierInvoiceAccountingRowIOData.AccountNr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountNr_TAG);                    
                    supplierInvoiceAccountingRowIOData.AccountDim2Nr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountDim2Nr_TAG);
                    supplierInvoiceAccountingRowIOData.AccountDim3Nr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountDim3Nr_TAG);
                    supplierInvoiceAccountingRowIOData.AccountDim4Nr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountDim4Nr_TAG);
                    supplierInvoiceAccountingRowIOData.AccountDim5Nr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountDim5Nr_TAG);
                    supplierInvoiceAccountingRowIOData.AccountDim6Nr = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountDim6Nr_TAG);
                    supplierInvoiceAccountingRowIOData.AccountSieDim1 = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountSieDim1_TAG);
                    supplierInvoiceAccountingRowIOData.AccountSieDim6 = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_AccountSieDim6_TAG);
                    supplierInvoiceAccountingRowIOData.Text = XmlUtil.GetChildElementValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_Text_TAG);
                    supplierInvoiceAccountingRowIOData.InterimRow = XmlUtil.GetElementBoolValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_InterimRow_TAG);
                    supplierInvoiceAccountingRowIOData.VatRow = XmlUtil.GetElementBoolValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_VatRow_TAG);
                    supplierInvoiceAccountingRowIOData.ContractorVatRow = XmlUtil.GetElementBoolValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_ContractorVatRow_TAG);
                    supplierInvoiceAccountingRowIOData.CreditRow = XmlUtil.GetElementNullableBoolValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_CreditRow_TAG);
                    supplierInvoiceAccountingRowIOData.DebitRow = XmlUtil.GetElementNullableBoolValue(supplierInvoiceAccountingRowIODataElement, XML_CHILD_DebitRow_TAG);

                    #endregion

                    supplierInvoiceIOData.Accountingrows.Add(supplierInvoiceAccountingRowIOData);
                }

                supplierInvoices.Add(supplierInvoiceIOData);

            }
        }

        #endregion
    }

    public class SupplierInvoiceIORawData
    {
        public List<SupplierInvoiceAccountingRowIORawData> Accountingrows = new List<SupplierInvoiceAccountingRowIORawData>();
        public List<SupplierInvoiceProductRowIORawData> ProductRows;
        public Dictionary<string,byte[]> Attachements;
        public Dictionary<int, string> PaymentNumbers;

        #region Variables

        public string SupplierNr;
        public string SupplierInvoiceNr;
        public string SupplierOrgNr;
        public string SupplierVatNr;
        public string SupplierExternalNr;
        public int? SeqNr;
	    public DateTime? InvoiceDate;
	    public DateTime? DueDate;
	    public DateTime? VoucherDate;
	    public string ReferenceOur;
	    public string ReferenceYour;
	    public string OCR;
	    public decimal? CurrencyRate;
	    public DateTime? CurrencyDate;
	    public decimal? TotalAmount;
	    public decimal? TotalAmountCurrency;
	    public decimal? VATAmount;
	    public decimal? VATAmountCurrency;
	    public decimal? PaidAmount;
	    public decimal? PaidAmountCurrency;
	    public decimal? RemainingAmount;
	    public bool? FullyPayed;
	    public string PaymentNr;
	    public string VoucherNr;
	    public bool? CreateAccountingInXE;
        public string Note;
        public string Currency;
        public int? BillingType;
        public int? OriginStatus;
        public decimal? VatPercent;
        public string ExternalId;
        public string OrderNr;
        public TermGroup_SysPaymentType SysPaymentType;

        #endregion
    }

    public class SupplierInvoiceAccountingRowIORawData
    {
        #region Variables

        public int SupplierInvoiceHeadIOId;
	    public int? SupplierInvoiceId;
	    public int? SupplierId;
	    public string SupplierNr;
	    public string SupplierInvoiceNr;
	    public decimal? Amount;
	    public decimal? AmountCurrency;
	    public decimal? Quantity;
	    public string AccountNr;	    
	    public string AccountDim2Nr;
	    public string AccountDim3Nr;
	    public string AccountDim4Nr;
        public string AccountDim5Nr;
	    public string AccountDim6Nr;
        public string AccountSieDim1;
        public string AccountSieDim6;
        public string Text;
	    public bool InterimRow;
	    public bool VatRow;
	    public bool ContractorVatRow;
	    public bool? CreditRow;
        public bool? DebitRow;


        #endregion
    }

    public class SupplierInvoiceProductRowIORawData
    {
        #region Variables
        public int? SupplierInvoiceId;
        public string SellerProductNumber;
        public string Text;
        public string UnitCode;
        public decimal Quantity;
        public decimal PriceCurrency;
        public decimal AmountCurrency;
        public decimal VatAmountCurrency;
        public decimal VatRate;
        public SupplierInvoiceRowType SupplierInvoiceRowType;
        #endregion

        public void SetCreditProperties()
        {
            if (this.PriceCurrency < 0)
            {
                //Price always postive
                this.PriceCurrency *= -1;
            }
            //Quantity & sums should be negative
            if (this.Quantity > 0)
            {
                this.Quantity *= -1;
            } 
            if (this.AmountCurrency > 0)
            {
                this.AmountCurrency *= -1;
            }
            if (this.VatAmountCurrency > 0)
            {
                this.VatAmountCurrency *= -1;
            }
        }
        public void SetDebitProperties()
        {
            if (this.PriceCurrency < 0)
            {
                //Price always postive
                this.PriceCurrency *= -1;
            }
            //Quantity & sums should be positive
            if (this.Quantity < 0)
            {
                this.Quantity *= -1;
            }
            if (this.AmountCurrency < 0)
            {
                this.AmountCurrency *= -1;
            }
            if (this.VatAmountCurrency < 0)
            {
                this.VatAmountCurrency *= -1;
            }
        }
    }
}
