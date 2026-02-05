using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class VoucherHeadIOItem
    {

        #region Collections

        public List<VoucherHeadIODTO> Vouchers = new List<VoucherHeadIODTO>();

        #endregion

        #region XML Nodes
        public const string XML_PARENT_TAG = "VoucherHeadIO";

        public const string XML_Date_TAG = "Date";
        public const string XML_VoucherNr_TAG = "VoucherNr";
        public const string XML_Text_TAG = "Text";
        public const string XML_IsVatVoucher_TAG = "IsVatVoucher";
        public const string XML_Note_TAG = "Note";
        public const string XML_VoucherSeriesTypeNr_TAG = "VoucherSeriesTypeNr";

        #endregion

        #region Constructors

        public VoucherHeadIOItem()
        {
        }

        public VoucherHeadIOItem(List<string> contents, TermGroup_IOImportHeadType headType)
        {
            CreateObjects(contents, headType);
        }

        public VoucherHeadIOItem(string content, TermGroup_IOImportHeadType headType)
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

            List<XElement> voucherHeadIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            foreach (var voucherHeadIOElement in voucherHeadIOElements)
            {
                VoucherHeadIODTO voucherHeadIODTO = new VoucherHeadIODTO();

                //Try different dateformats
                DateTime date = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_Date_TAG), "yyyyMMdd");
                if (date == CalendarUtility.DATETIME_DEFAULT)
                   date = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_Date_TAG), "yyyy-MM-dd");


                voucherHeadIODTO.Date = date;
                voucherHeadIODTO.VoucherNr = XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_VoucherNr_TAG);
                voucherHeadIODTO.Text = XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_Text_TAG);
                //voucherHeadIODTO.IsVatVoucher = XmlUtil.GetElementNullableValue(voucherHeadIOElement, XML_IsVatVoucher_TAG);
                voucherHeadIODTO.Note = XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_Note_TAG);
                voucherHeadIODTO.VoucherSeriesTypeNr = XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_VoucherSeriesTypeNr_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(voucherHeadIOElement, XML_VoucherSeriesTypeNr_TAG)) : -1;

                VoucherRowIOItem rowIOItem = new VoucherRowIOItem();

                List<XElement> voucherRowIOElements = XmlUtil.GetChildElements(voucherHeadIOElement, rowIOItem.XML_PARENT_TAG);
                rowIOItem.CreateObjects(voucherRowIOElements, headType);

                if (voucherHeadIODTO.Rows == null)
                    voucherHeadIODTO.Rows = new List<VoucherRowIODTO>();

                voucherHeadIODTO.Rows.AddRange(rowIOItem.VoucherRows);

                Vouchers.Add(voucherHeadIODTO);
            }

        }
        #endregion
    }
}
