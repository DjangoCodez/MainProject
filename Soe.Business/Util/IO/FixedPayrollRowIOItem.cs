using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class FixedPayrollRowIOItem
    {
        #region Collections

        public List<FixedPayrollRowIODTO> fixedPayrollRowIOs = new List<FixedPayrollRowIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "FixedPayrollRowIODTO";

        public string XML_ActorCompanyId_TAG = "ActorCompanyId";
        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_Created_TAG = "Created";
        public string XML_CreatedBy_TAG = "CreatedBy";
        public string XML_Modified_TAG = "Modified";
        public string XML_ModifiedBy_TAG = "ModifiedBy";
        public string XML_ProductNr_TAG = "ProductNr";
        public string XML_FromDate_TAG = "FromDate";
        public string XML_ToDate_TAG = "ToDate";
        public string XML_UnitPrice_TAG = "UnitPrice";
        public string XML_Quantity_TAG = "Quantity";
        public string XML_IsSpecifiedUnitPrice_TAG = "IsSpecifiedUnitPrice";
        public string XML_Amount_TAG = "Amount";
        public string XML_VatAmount_TAG = "VatAmount";

        #endregion

        #region Constructors

        public FixedPayrollRowIOItem()
        {
        }

        public FixedPayrollRowIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public FixedPayrollRowIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementFixedPayrollRowSEs = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementFixedPayrollRowSEs, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementFixedPayrollRowSEs, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementFixedPayrollRowSE in elementFixedPayrollRowSEs)
            {
                FixedPayrollRowIODTO fixedPayrollRowIODTO = new FixedPayrollRowIODTO();

                fixedPayrollRowIODTO.EmployeeNr = XmlUtil.GetChildElementValue(elementFixedPayrollRowSE, XML_EmployeeNr_TAG);
                fixedPayrollRowIODTO.Created = XmlUtil.GetElementNullableDateTimeValue(elementFixedPayrollRowSE, XML_Created_TAG);
                fixedPayrollRowIODTO.CreatedBy = XmlUtil.GetChildElementValue(elementFixedPayrollRowSE, XML_CreatedBy_TAG);
                fixedPayrollRowIODTO.Modified = XmlUtil.GetElementNullableDateTimeValue(elementFixedPayrollRowSE, XML_Modified_TAG);
                fixedPayrollRowIODTO.ModifiedBy = XmlUtil.GetChildElementValue(elementFixedPayrollRowSE, XML_ModifiedBy_TAG);
                fixedPayrollRowIODTO.ProductNr = XmlUtil.GetChildElementValue(elementFixedPayrollRowSE, XML_ProductNr_TAG);
                fixedPayrollRowIODTO.FromDate = XmlUtil.GetElementNullableDateTimeValue(elementFixedPayrollRowSE, XML_FromDate_TAG);
                fixedPayrollRowIODTO.ToDate = XmlUtil.GetElementNullableDateTimeValue(elementFixedPayrollRowSE, XML_ToDate_TAG);
                fixedPayrollRowIODTO.UnitPrice = XmlUtil.GetElementDecimalValue(elementFixedPayrollRowSE, XML_UnitPrice_TAG);
                fixedPayrollRowIODTO.Quantity = XmlUtil.GetElementDecimalValue(elementFixedPayrollRowSE, XML_Quantity_TAG);
                fixedPayrollRowIODTO.IsSpecifiedUnitPrice = XmlUtil.GetElementBoolValue(elementFixedPayrollRowSE, XML_IsSpecifiedUnitPrice_TAG);
                fixedPayrollRowIODTO.Amount = XmlUtil.GetElementDecimalValue(elementFixedPayrollRowSE, XML_Amount_TAG);
                fixedPayrollRowIODTO.VatAmount = XmlUtil.GetElementDecimalValue(elementFixedPayrollRowSE, XML_VatAmount_TAG);

                fixedPayrollRowIOs.Add(fixedPayrollRowIODTO);
            }

        }
        #endregion
    }
}


