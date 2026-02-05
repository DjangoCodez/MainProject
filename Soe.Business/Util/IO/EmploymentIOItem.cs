using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class EmploymentIOItem
    {
        #region Collections

        public List<EmploymentIODTO> employmentIOs = new List<EmploymentIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "EmploymentIO";

        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_EmploymentType_TAG = "EmploymentType";
        public string XML_EmploymentTypeName_TAG = "EmploymentTypeName";
        public string XML_DateFrom_TAG = "DateFrom";
        public string XML_DateTo_TAG = "DateTo";
        public string XML_WorkTimeWeek_TAG = "WorkTimeWeek";
        public string XML_Percent_TAG = "Percent";
        public string XML_ExperienceMonths_TAG = "ExperienceMonths";
        public string XML_ExperienceAgreedOrEstablished_TAG = "ExperienceAgreedOrEstablished";
        public string XML_WorkPlace_TAG = "WorkPlace";
        public string XML_SpecialConditions_TAG = "SpecialConditions";
        public string XML_Comment_TAG = "Comment";
        public string XML_EmploymentEndReason_TAG = "EmploymentEndReason";
        public string XML_EmploymentEndReasonName_TAG = "EmploymentEndReasonName";
        public string XML_BaseWorkTimeWeek_TAG = "BaseWorkTimeWeek";
        public string XML_SubstituteFor_TAG = "SubstituteFor";
        public string XML_EmployeeGroupName_TAG = "EmployeeGroupName";
        public string XML_PayrollGroupName_TAG = "PayrollGroupName";
        public string XML_VacationGroupName_TAG = "VacationGroupName";
        public string XML_WorkTasks_TAG = "WorkTasks";
        public string XML_ExternalCode_TAG = "ExternalCode";
        public string XML_IsSecondaryEmployment_TAG = "IsSecondaryEmployment";
        public string XML_SubstituteForDueTo_TAG = "SubstituteForDueTo";
        public string XML_UpdateExperienceMonthsReminder_TAG = "UpdateExperienceMonthsReminder";


        #endregion

        #region Constructors

        public EmploymentIOItem()
        {
        }

        public EmploymentIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public EmploymentIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> elementEmployments = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementEmployments, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementEmployments, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementEmployment in elementEmployments)
            {
                EmploymentIODTO employmentIODTO = new EmploymentIODTO();

                employmentIODTO.EmployeeNr = XmlUtil.GetChildElementValue(elementEmployment, XML_EmployeeNr_TAG);
                employmentIODTO.EmploymentType = XmlUtil.GetElementIntValue(elementEmployment, XML_EmploymentType_TAG);
                employmentIODTO.EmploymentTypeName = XmlUtil.GetChildElementValue(elementEmployment, XML_EmploymentTypeName_TAG);
                employmentIODTO.DateFrom = CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(elementEmployment, XML_DateFrom_TAG));
                employmentIODTO.DateTo = CalendarUtility.GetNullableDateTime(XmlUtil.GetChildElementValue(elementEmployment, XML_DateTo_TAG));
                employmentIODTO.WorkTimeWeek = XmlUtil.GetElementIntValue(elementEmployment, XML_WorkTimeWeek_TAG);
                employmentIODTO.Percent = XmlUtil.GetElementDecimalValue(elementEmployment, XML_Percent_TAG);
                employmentIODTO.ExperienceMonths = XmlUtil.GetElementIntValue(elementEmployment, XML_ExperienceMonths_TAG);
                employmentIODTO.ExperienceAgreedOrEstablished = XmlUtil.GetElementBoolValue(elementEmployment, XML_ExperienceAgreedOrEstablished_TAG);
                employmentIODTO.WorkPlace = XmlUtil.GetChildElementValue(elementEmployment, XML_WorkPlace_TAG);
                employmentIODTO.SpecialConditions = XmlUtil.GetChildElementValue(elementEmployment, XML_SpecialConditions_TAG);
                employmentIODTO.Comment = XmlUtil.GetChildElementValue(elementEmployment, XML_Comment_TAG);
                employmentIODTO.EmploymentEndReason = XmlUtil.GetElementIntValue(elementEmployment, XML_EmploymentEndReason_TAG);
                employmentIODTO.EmploymentEndReasonName = XmlUtil.GetChildElementValue(elementEmployment, XML_EmploymentEndReasonName_TAG);
                employmentIODTO.BaseWorkTimeWeek = XmlUtil.GetElementIntValue(elementEmployment, XML_BaseWorkTimeWeek_TAG);
                employmentIODTO.SubstituteFor = XmlUtil.GetChildElementValue(elementEmployment, XML_SubstituteFor_TAG);
                employmentIODTO.EmployeeGroupName = XmlUtil.GetChildElementValue(elementEmployment, XML_EmployeeGroupName_TAG);
                employmentIODTO.PayrollGroupName = XmlUtil.GetChildElementValue(elementEmployment, XML_PayrollGroupName_TAG);
                employmentIODTO.VacationGroupName = XmlUtil.GetChildElementValue(elementEmployment, XML_VacationGroupName_TAG);
                employmentIODTO.WorkTasks = XmlUtil.GetChildElementValue(elementEmployment, XML_WorkTasks_TAG);
                employmentIODTO.ExternalCode = XmlUtil.GetChildElementValue(elementEmployment, XML_ExternalCode_TAG);
                employmentIODTO.IsSecondaryEmployment = XmlUtil.GetElementBoolValue(elementEmployment, XML_IsSecondaryEmployment_TAG);
                employmentIODTO.SubstituteForDueTo = XmlUtil.GetChildElementValue(elementEmployment, XML_SubstituteForDueTo_TAG);
                employmentIODTO.UpdateExperienceMonthsReminder = XmlUtil.GetElementBoolValue(elementEmployment, XML_UpdateExperienceMonthsReminder_TAG);
                employmentIOs.Add(employmentIODTO);
            }

        }
        #endregion
    }
}