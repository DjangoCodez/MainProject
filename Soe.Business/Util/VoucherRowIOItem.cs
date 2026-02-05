using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class VoucherRowIOItem
    {

        #region Collections

        public List<VoucherRowIODTO> VoucherRows = new List<VoucherRowIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "VoucherRowIO";

        public string XML_Text_TAG = "Text";
        public string XML_Amount_TAG = "Amount";
        public string XML_DebitAmount_TAG = "DebetAmount";
        public string XML_CreditAmount_TAG = "CreditAmount";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_AccountDim6Nr_TAG = "AccountDim6Nr";
        public string XML_AccountSieDim1_TAG = "AccountSieDim1";
        public string XML_AccountSieDim3_TAG = "AccountSieDim3";
        public string XML_AccountSieDim6_TAG = "AccountSieDim6";


        #endregion

        #region Constructors

        public VoucherRowIOItem()
        {
        }

        public VoucherRowIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public VoucherRowIOItem(string content, TermGroup_IOImportHeadType headType)
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

            List<XElement> voucherRowIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(voucherRowIOElements, headType);

        }

        public void CreateObjects(List<XElement> voucherRowIOElements, TermGroup_IOImportHeadType headType)
        {
            foreach (var voucherRowIOElement in voucherRowIOElements)
            {
                VoucherRowIODTO voucherRowIODTO = new VoucherRowIODTO();

                //if debet and credti is missing use amount
                decimal? debet = XmlUtil.GetElementNullableDecimalValue(voucherRowIOElement, XML_DebitAmount_TAG);
                decimal? credit = XmlUtil.GetElementNullableDecimalValue(voucherRowIOElement, XML_CreditAmount_TAG);
                decimal? amount = XmlUtil.GetElementNullableDecimalValue(voucherRowIOElement, XML_Amount_TAG);

                if (credit == null && debet == null && amount != null && amount != 0)
                {
                    if (amount < 0)
                        credit = decimal.Negate((decimal)amount);
                    else
                        debet = amount;
                }

                if (credit == null && debet == null && amount == null)
                {
                    amount = 0;
                    debet = 0;
                    amount = 0;
                }

                voucherRowIODTO.Text = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_Text_TAG);
                voucherRowIODTO.Amount = amount;
                voucherRowIODTO.DebetAmount = debet;
                voucherRowIODTO.CreditAmount = credit;
                voucherRowIODTO.Quantity = XmlUtil.GetElementNullableDecimalValue(voucherRowIOElement, XML_Quantity_TAG);

                voucherRowIODTO.AccountNr = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountNr_TAG);
                voucherRowIODTO.AccountDim2Nr = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountDim2Nr_TAG);
                voucherRowIODTO.AccountDim3Nr = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountDim3Nr_TAG);
                voucherRowIODTO.AccountDim4Nr = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountDim4Nr_TAG);
                voucherRowIODTO.AccountDim5Nr = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountDim5Nr_TAG);
                voucherRowIODTO.AccountDim6Nr = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountDim6Nr_TAG);
                voucherRowIODTO.AccountSieDim1 = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountSieDim1_TAG);
                voucherRowIODTO.AccountSieDim3 = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountSieDim3_TAG);
                voucherRowIODTO.AccountSieDim6 = XmlUtil.GetChildElementValue(voucherRowIOElement, XML_AccountSieDim6_TAG);

                if (string.IsNullOrEmpty(voucherRowIODTO.AccountNr))
                    continue;

                VoucherRows.Add(voucherRowIODTO);
            }

        }
        #endregion
    }
}
