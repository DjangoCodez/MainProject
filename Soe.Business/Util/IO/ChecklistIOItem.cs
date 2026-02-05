using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class ChecklistIOItem
    {

        #region Collections

        public List<ChecklistHeadIODTO> checklistIOs = new List<ChecklistHeadIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "ChecklistHeadIO";
        public const string XML_CHILD_TAG = "ChecklistRowIO";

        #region Parent

        public string XML_ReportNr_TAG = "ReportNr";

        public string XML_Type_TAG = "Type";
        public string XML_Name_TAG = "Name";
        public string XML_Description_TAG = "Description";

        #endregion

        #region Child Nodes

        public string XML_RowType_TAG = "RowType";
        public string XML_RowNr_TAG = "RowNr";
        public string XML_Mandatory_TAG = "Mandatory";
        public string XML_Text_TAG = "Text";

        #endregion

        #endregion

        #region Constructors

        public ChecklistIOItem()
        {
        }

        public ChecklistIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public ChecklistIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementChecklists = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            CreateObjects(elementChecklists, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementChecklists, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementChecklist in elementChecklists)
            {
                ChecklistHeadIODTO headDTO = new ChecklistHeadIODTO();
                List<ChecklistRowIODTO> rowDTOs = new List<ChecklistRowIODTO>();

                headDTO.Type = XmlUtil.GetElementIntValue(elementChecklist, XML_Type_TAG);
                headDTO.Name = XmlUtil.GetChildElementValue(elementChecklist, XML_Name_TAG);
                headDTO.Description = XmlUtil.GetChildElementValue(elementChecklist, XML_Description_TAG);
                headDTO.ReportNr = XmlUtil.GetElementIntValue(elementChecklist, XML_ReportNr_TAG);
                headDTO.ChecklistRows = new List<ChecklistRowIODTO>();

                List<XElement> rowElements = XmlUtil.GetChildElements(elementChecklist, XML_CHILD_TAG);

                foreach (var rowElement in rowElements)
                {
                    ChecklistRowIODTO rowDTO = new ChecklistRowIODTO();

                    rowDTO.Type = XmlUtil.GetElementIntValue(rowElement, XML_RowType_TAG);
                    rowDTO.RowNr = XmlUtil.GetElementIntValue(rowElement, XML_RowNr_TAG);
                    rowDTO.Mandatory = XmlUtil.GetElementBoolValue(rowElement, XML_Mandatory_TAG);
                    rowDTO.Text = XmlUtil.GetChildElementValue(rowElement, XML_Text_TAG);

                    #region fix

                    if (rowDTO.Type == 0)
                        rowDTO.Type = (int)TermGroup_ChecklistRowType.String;

                    #endregion

                    headDTO.ChecklistRows.Add(rowDTO);
                }

                checklistIOs.Add(headDTO);
            }
        }

        #endregion

    }

}