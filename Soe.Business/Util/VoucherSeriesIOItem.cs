using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class VoucherSeriesIOItem
    {

        #region Collections

        public List<VoucherSeriesIODTO> voucherSeriesIOs = new List<VoucherSeriesIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "VoucherSeries";
        public string XML_Name_TAG = "Name";
        public string XML_Nr_TAG = "Nr";
        public string XML_Description_TAG = "Description";
        public string XML_StartNumber_TAG = "StartNumber";
        public string XML_VoucherNrLatest_TAG = "VoucherNrLatest";

        public string XML_VoucherDateLatest_TAG = "VoucherDateLatest";
        public string XML_Status_TAG = "Status";

        #endregion

        #region Constructors

        public VoucherSeriesIOItem()
        {
        }

        public VoucherSeriesIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public VoucherSeriesIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> accountDistributionRowElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(accountDistributionRowElements, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> voucherSeriesElements, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var voucherSeriesElement in voucherSeriesElements)
            {
                VoucherSeriesIODTO voucherSeriesIODTO = new VoucherSeriesIODTO();

                DateTime voucherDateLatest = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(voucherSeriesElement, XML_VoucherDateLatest_TAG), "yyyyMMdd");
                if (voucherDateLatest == CalendarUtility.DATETIME_DEFAULT)
                    voucherDateLatest = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(voucherSeriesElement, XML_VoucherDateLatest_TAG), "yyyy-MM-dd");

                voucherSeriesIODTO.Name = XmlUtil.GetChildElementValue(voucherSeriesElement, XML_Name_TAG);
                voucherSeriesIODTO.Description = XmlUtil.GetChildElementValue(voucherSeriesElement, XML_Description_TAG);
                voucherSeriesIODTO.Nr = XmlUtil.GetChildElementValue(voucherSeriesElement, XML_Nr_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(voucherSeriesElement, XML_Nr_TAG)) : -1;
                voucherSeriesIODTO.StartNumber = XmlUtil.GetChildElementValue(voucherSeriesElement, XML_StartNumber_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(voucherSeriesElement, XML_StartNumber_TAG)) : 0;
                voucherSeriesIODTO.VoucherNrLatest = XmlUtil.GetChildElementValue(voucherSeriesElement, XML_VoucherNrLatest_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(voucherSeriesElement, XML_VoucherNrLatest_TAG)) : -1;
                voucherSeriesIODTO.Status = XmlUtil.GetChildElementValue(voucherSeriesElement, XML_Status_TAG) != "" ? Convert.ToInt32(XmlUtil.GetChildElementValue(voucherSeriesElement, XML_Status_TAG)) : 0;
                voucherSeriesIODTO.VoucherDateLatest = voucherDateLatest;

                voucherSeriesIOs.Add(voucherSeriesIODTO);
            }
        }
        #endregion
    }
}
