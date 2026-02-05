using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class TimeScheduleBlockIOItem
    {

        #region Collections

        public List<TimeScheduleBlockIODTO> TimeScheduleBlockIODTOs = new List<TimeScheduleBlockIODTO>();

        #endregion

        #region XML Nodes

        public string XML_PARENT_TAG = "TimeScheduleBlockIO";
        public string XML_TimeScheduleTemplatePeriodId_TAG = "TimeScheduleTemplatePeriodId";
        public string XML_TimeScheduleEmployeePeriodId_TAG = "TimeScheduleEmployeePeriodId";
        public string XML_EmployeeNr_TAG = "EmployeeNr";
        public string XML_TimeScheduleTypeId_TAG = "TimeScheduleTypeId";
        public string XML_DayNumber_TAG = "DayNumber";
        public string XML_Description_TAG = "Description";

        public string XML_StartTime_TAG = "privatStartTime;";
        public string XML_StopTime_TAG = "privatStopTime;";
        public string XML_LengthMinutes_TAG = "LengthMinutes;";
        public string XML_Date_TAG = "Date";

        public string XML_Break1Id_TAG = "Break1Id";
        public string XML_Break1StartTime_TAG = "Break1StartTime";
        public string XML_Break1Minutes_TAG = "Break1Minutes";
        public string XML_Break1Link_TAG = "Break1Link";
        public string XML_Break2Id_TAG = "Break2Id";
        public string XML_Break2StartTime_TAG = "Break2StartTime";
        public string XML_Break2Minutes_TAG = "Break2Minutes";
        public string XML_Break2Link_TAG = "Break2Link";
        public string XML_Break3Id_TAG = "Break3Id";
        public string XML_Break3StartTime_TAG = "Break3StartTime";
        public string XML_Break3Minutes_TAG = "Break3Minutes";
        public string XML_Break3Link_TAG = "Break3Link";
        public string XML_Break4Id_TAG = "Break4Id";
        public string XML_Break4StartTime_TAG = "Break4StartTime";
        public string XML_Break4Minutes_TAG = "Break4Minutes";
        public string XML_Break4Link_TAG = "Break4Link";
        public string XML_HasBreakTimes_TAG = "HasBreakTimes";
        public string XML_IsBreak_TAG = "IsBreak";


        public string XML_ShiftTypeId_TAG = "ShiftTypeId";
        public string XML_ShiftTypeName_TAG = "ShiftTypeName";
        public string XML_ShiftTypeDescription_TAG = "ShiftTypeDescription";
        public string XML_ShiftTypeTimeScheduleTypeId_TAG = "ShiftTypeTimeScheduleTypeId";
        public string XML_Link_TAG = "Link";

        public string XML_AccountNr_TAG = "AccountNr";
        public string XML_AccountDim2Nr_TAG = "AccountDim2Nr";
        public string XML_AccountDim3Nr_TAG = "AccountDim3Nr";
        public string XML_AccountDim4Nr_TAG = "AccountDim4Nr";
        public string XML_AccountDim5Nr_TAG = "AccountDim5Nr";
        public string XML_AccountDim6Nr_TAG = "AccountDim6Nr";


        #endregion

        #region Constructors

        public TimeScheduleBlockIOItem()
        {
        }

        public TimeScheduleBlockIOItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId, List<dynamic> objects = null)
        {
            if (objects == null)
            {
                CreateObjects(contents, headType, actorCompanyId);
            }
            else
            {
                foreach (var item in objects)
                {
                    this.TimeScheduleBlockIODTOs.Add(item as TimeScheduleBlockIODTO);
                }
            }
        }

        public TimeScheduleBlockIOItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
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

            List<XElement> timeScheduleBlockYearElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(timeScheduleBlockYearElements, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> timeScheduleBlockElements, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var timeScheduleBlockElement in timeScheduleBlockElements)
            {
                TimeScheduleBlockIODTO timeScheduleBlockIODTO = new TimeScheduleBlockIODTO();

                DateTime startTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime stopTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime date = CalendarUtility.DATETIME_DEFAULT;

                DateTime.TryParse(XML_StartTime_TAG, out startTime);
                DateTime.TryParse(XML_StopTime_TAG, out stopTime);
                DateTime.TryParse(XML_Date_TAG, out date);

                timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleTemplatePeriodId_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleTemplatePeriodId_TAG)) : -1;
                timeScheduleBlockIODTO.TimeScheduleTemplatePeriodId = XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleTemplatePeriodId_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleTemplatePeriodId_TAG)) : -1;
                timeScheduleBlockIODTO.TimeScheduleEmployeePeriodId = XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleEmployeePeriodId_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleEmployeePeriodId_TAG)) : -1;
                timeScheduleBlockIODTO.EmployeeNr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_EmployeeNr_TAG);
                timeScheduleBlockIODTO.TimeScheduleTypeId = XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleTypeId_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_TimeScheduleTypeId_TAG)) : -1;
                timeScheduleBlockIODTO.DayNumber = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_DayNumber_TAG);
                timeScheduleBlockIODTO.Description = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_Description_TAG);

                timeScheduleBlockIODTO.StartTime = startTime;
                timeScheduleBlockIODTO.StopTime = stopTime;
                timeScheduleBlockIODTO.LengthMinutes = XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_LengthMinutes_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementNullableIntValue(timeScheduleBlockElement, XML_LengthMinutes_TAG)) : -1;
                timeScheduleBlockIODTO.Date = date;

                DateTime break1StartTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime break2StartTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime break3StartTime = CalendarUtility.DATETIME_DEFAULT;
                DateTime break4StartTime = CalendarUtility.DATETIME_DEFAULT;

                DateTime.TryParse(XML_Break1StartTime_TAG, out break1StartTime);
                DateTime.TryParse(XML_Break2StartTime_TAG, out break2StartTime);
                DateTime.TryParse(XML_Break3StartTime_TAG, out break3StartTime);
                DateTime.TryParse(XML_Break4StartTime_TAG, out break4StartTime);


                timeScheduleBlockIODTO.Break1Id = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break1Id_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break1Id_TAG)) : -1;
                timeScheduleBlockIODTO.Break1StartTime = break1StartTime;
                timeScheduleBlockIODTO.Break1Minutes = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break1Minutes_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break1Minutes_TAG)) : -1;
                timeScheduleBlockIODTO.Break1Link = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_Break1Link_TAG);
                timeScheduleBlockIODTO.Break2Id = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break2Id_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break2Id_TAG)) : -1;
                timeScheduleBlockIODTO.Break2StartTime = break2StartTime;
                timeScheduleBlockIODTO.Break2Minutes = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break2Minutes_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break2Minutes_TAG)) : -1;
                timeScheduleBlockIODTO.Break2Link = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_Break2Link_TAG);
                timeScheduleBlockIODTO.Break3Id = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break3Id_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break3Id_TAG)) : -1;
                timeScheduleBlockIODTO.Break3StartTime = break3StartTime;
                timeScheduleBlockIODTO.Break3Minutes = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break3Minutes_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break3Minutes_TAG)) : -1;
                timeScheduleBlockIODTO.Break3Link = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_Break3Link_TAG);
                timeScheduleBlockIODTO.Break4Id = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break4Id_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break4Id_TAG)) : -1;
                timeScheduleBlockIODTO.Break4StartTime = break4StartTime;
                timeScheduleBlockIODTO.Break4Minutes = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break4Minutes_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_Break4Minutes_TAG)) : -1;
                timeScheduleBlockIODTO.Break4Link = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_Break4Link_TAG);
                timeScheduleBlockIODTO.HasBreakTimes = XmlUtil.GetElementBoolValue(timeScheduleBlockElement, XML_HasBreakTimes_TAG);
                timeScheduleBlockIODTO.IsBreak = XmlUtil.GetElementBoolValue(timeScheduleBlockElement, XML_IsBreak_TAG);

                timeScheduleBlockIODTO.ShiftTypeId = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_ShiftTypeId_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_ShiftTypeId_TAG)) : -1;
                timeScheduleBlockIODTO.ShiftTypeName = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_ShiftTypeName_TAG);
                timeScheduleBlockIODTO.ShiftTypeDescription = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_ShiftTypeDescription_TAG);
                timeScheduleBlockIODTO.ShiftTypeTimeScheduleTypeId = XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_ShiftTypeTimeScheduleTypeId_TAG) != 0 ? Convert.ToInt32(XmlUtil.GetElementIntValue(timeScheduleBlockElement, XML_ShiftTypeTimeScheduleTypeId_TAG)) : -1;
                timeScheduleBlockIODTO.Link = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_Link_TAG);

                timeScheduleBlockIODTO.AccountNr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_AccountNr_TAG);
                timeScheduleBlockIODTO.AccountDim2Nr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_AccountDim2Nr_TAG);
                timeScheduleBlockIODTO.AccountDim3Nr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_AccountDim3Nr_TAG);
                timeScheduleBlockIODTO.AccountDim4Nr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_AccountDim4Nr_TAG);
                timeScheduleBlockIODTO.AccountDim5Nr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_AccountDim5Nr_TAG);
                timeScheduleBlockIODTO.AccountDim6Nr = XmlUtil.GetChildElementValue(timeScheduleBlockElement, XML_AccountDim6Nr_TAG);

                TimeScheduleBlockIODTOs.Add(timeScheduleBlockIODTO);
            }

        }
        #endregion
    }
}
