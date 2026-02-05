using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class UserCompanySettingIOItem
    {

        #region Collections

        public List<UserCompanySettingIODTO> userCompanySettingIOs = new List<UserCompanySettingIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "UserCompanySettingIO";
        public string XML_UserId_TAG = "UserId";
        public string XML_SettingTypeId_TAG = "SettingTypeId";
        public string XML_DataTypeId_TAG = "DataTypeId";
        public string XML_SettingName_TAG = "SettingName";
        public string XML_UserName_TAG = "UserName";
        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_StringData_TAG = "StringData";
        public string XML_IntData_TAG = "IntData";
        public string XML_BoolData_TAG = "BoolData";
        public string XML_DecimalData_TAG = "DecimalData";
        public string XML_DateData_TAG = "DateData";    

        #endregion

        #region Constructors

        public UserCompanySettingIOItem()
        {
        }

        public UserCompanySettingIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public UserCompanySettingIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementUserCompanySettings = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);

            CreateObjects(elementUserCompanySettings, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementUserCompanySettings, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementUserCompanySetting in elementUserCompanySettings)
            {
                UserCompanySettingIODTO headDTO = new UserCompanySettingIODTO();
                List<UserCompanySettingIODTO> headDTOs = new List<UserCompanySettingIODTO>();

                headDTO.UserId  = XmlUtil.GetElementNullableIntValue(elementUserCompanySetting, XML_UserId_TAG);
                headDTO.SettingTypeId  = XmlUtil.GetElementIntValue(elementUserCompanySetting, XML_SettingTypeId_TAG);
                headDTO.DataTypeId  = XmlUtil.GetElementIntValue(elementUserCompanySetting, XML_DataTypeId_TAG);
                headDTO.SettingName  = XmlUtil.GetChildElementValue(elementUserCompanySetting, XML_SettingName_TAG);
                headDTO.UserName  = XmlUtil.GetChildElementValue(elementUserCompanySetting, XML_UserName_TAG);
                headDTO.EmployeeNr  = XmlUtil.GetChildElementValue(elementUserCompanySetting, XML_EmployeeNr_TAG);
                headDTO.StringData  = XmlUtil.GetChildElementValue(elementUserCompanySetting, XML_StringData_TAG);
                headDTO.IntData  = XmlUtil.GetElementNullableIntValue(elementUserCompanySetting, XML_IntData_TAG);
                headDTO.BoolData  = XmlUtil.GetElementNullableBoolValue(elementUserCompanySetting, XML_BoolData_TAG);
                headDTO.DecimalData  = XmlUtil.GetElementDecimalValue(elementUserCompanySetting, XML_DecimalData_TAG);
                headDTO.DateData = CalendarUtility.GetNullableDateTime(XmlUtil.GetElementNullableValue(elementUserCompanySetting, XML_DateData_TAG));

                headDTOs.Add(headDTO);
            }

        }
        #endregion
    }
}