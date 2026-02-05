using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.holidays.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected CalendarManager calm;
        protected Holiday holiday;

        public int modalHolidayId = 0;
        public int modalDaytypeId = 0;
        public int modalType = 0;
        public string deleteDate = "";

        //Module specifics
        public bool EnableManage { get; set; }
        public bool EnableTime { get; set; }
        private Feature FeatureList = Feature.None;
        private Feature FeatureEdit = Feature.None;
        private Feature FeatureImport = Feature.None;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Manage_Preferences_Registry_Holidays_Edit:
                        EnableManage = true;
                        FeatureList = Feature.Manage_Preferences_Registry_Holidays;
                        FeatureEdit = Feature.Manage_Preferences_Registry_Holidays_Edit;
                        FeatureImport = Feature.Manage_Preferences_Registry_Holidays_Edit;
                        break;
                    case Feature.Time_Preferences_ScheduleSettings_Holidays_Edit:
                        EnableTime = true;
                        FeatureList = Feature.Time_Preferences_ScheduleSettings_Holidays;
                        FeatureEdit = Feature.Time_Preferences_ScheduleSettings_Holidays_Edit;
                        FeatureImport = Feature.Time_Preferences_ScheduleSettings_Holidays_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            calm = new CalendarManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            int holidayId;
            if (Int32.TryParse(QS["holiday"], out holidayId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    holiday = calm.GetPrevNextHoliday(holidayId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (holiday != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?holiday=" + holiday.HolidayId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?holiday=" + holidayId);
                }
                else
                {
                    holiday = calm.GetHoliday(holidayId, SoeCompany.ActorCompanyId);
                    if (holiday == null)
                    {
                        Form1.MessageWarning = GetText(4281, "Avvikelsedag hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(4283, "Redigera avvikelsedag");
            string registerModeTabHeaderText = GetText(4282, "Registrera avvikelsedag");
            PostOptionalParameterCheck(Form1, holiday, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = holiday != null ? holiday.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            DayType.ConnectDataSource(calm.GetDayTypesByCompanyDict(SoeCompany.ActorCompanyId, false));

            #endregion

            #region Set data

            if (holiday != null)
            {
                Name.Value = holiday.Name;
                Date.Value = holiday.Date.ToShortDateString();
                IsRedDay.Value = holiday.IsRedDay ? Boolean.TrueString : Boolean.FalseString;
                Description.Value = holiday.Description;
                DayType.Value = holiday.DayTypeId.ToString();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                {
                    Form1.MessageSuccess = GetText(4284, "Avvikelsedag sparad");
                    TryDisplayModalWindow();
                }
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(4285, "Avvikelsedag kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                {
                    Form1.MessageSuccess = GetText(4286, "Avvikelsedag uppdaterad");
                    TryDisplayModalWindow();
                }
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(4287, "Avvikelsedag kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(4288, "Avvikelsedag finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(4289, "Avvikelsedag kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                {
                    Form1.MessageSuccess = GetText(4290, "Avvikelsedag borttagen");
                    TryDisplayModalWindow();
                }
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(4291, "Avvikelsedag kunde inte tas bort");
            }

            #endregion

            #region Navigation

            if (holiday != null)
            {
                Form1.SetRegLink(GetText(4282, "Registrera avvikelsedag"), "",
                    FeatureEdit, Permission.Modify);
            }

            Form1.AddLink(GetText(4296, "Visa avvikelsedagar"), "../",
                FeatureList, Permission.Readonly);

            Form1.AddLink(GetText(4293, "Importera avvikelsedagar"), "../import/",
                FeatureImport, Permission.Modify);

            #endregion
        }

        private void TryDisplayModalWindow()
        {
            if (String.IsNullOrEmpty(QS["dialog"]))
                return;

            Int32.TryParse(QS["modaltype"], out modalType);
            Int32.TryParse(QS["holiday"], out modalHolidayId);
            Int32.TryParse(QS["dayType"], out modalDaytypeId);

            if (!String.IsNullOrEmpty(QS["oldDate"]))
                deleteDate = QS["oldDate"];
            else
                deleteDate = CalendarUtility.URL_FRIENDLY_DATETIME_DEFAULT;
        }

        #region Action-methods

        protected override void Save()
        {
            #region Init

            string name = F["Name"];
            string description = F["Description"];
            DateTime date = Convert.ToDateTime(F["Date"]);
            bool isRedDay = StringUtility.GetBool(F["IsRedDay"]);
            int dayTypeId = Convert.ToInt32(F["DayType"]);

            #endregion

            if (holiday == null)
            {
                #region Add

                // Validation: Holiday not already exist
                if (calm.HolidayExists(date, dayTypeId, SoeCompany.ActorCompanyId, null))
                    RedirectToSelf("EXIST");

                holiday = new Holiday()
                {
                    Name = name,
                    Description = description,
                    Date = date,
                    IsRedDay = isRedDay,
                    DayTypeId = dayTypeId,
                };

                if (calm.AddHoliday(holiday, SoeCompany.ActorCompanyId, dayTypeId).Success)
                    RedirectToSelf("SAVED", "&modaltype=" + (int)SoeTimeUniqueDayEdit.AddHoliday + "&dialog=true&holiday=" + holiday.HolidayId + "&daytype=" + dayTypeId);
                else
                    RedirectToSelf("NOTSAVED", true);

                #endregion
            }
            else
            {
                #region Update

                if (holiday.Date != date && holiday.DayTypeId != dayTypeId)
                {
                    // Validation: Holiday not already exist
                    if (calm.HolidayExists(date, dayTypeId, SoeCompany.ActorCompanyId, null))
                        RedirectToSelf("EXIST");
                }

                DateTime? oldDate = null;
                if (holiday.Date != date)
                    oldDate = holiday.Date;

                String deleteDateValue = CalendarUtility.DATETIME_DEFAULT.ToShortDateString();
                if (oldDate.HasValue)
                    deleteDateValue = oldDate.Value.ToShortDateString();

                deleteDateValue = deleteDateValue.Replace("-", "");

                holiday.Name = name;
                holiday.Date = date;
                holiday.IsRedDay = isRedDay;
                holiday.Description = description;
                holiday.DayTypeId = dayTypeId;

                if (calm.UpdateHoliday(holiday, dayTypeId, SoeCompany.ActorCompanyId).Success)
                {
                    string url = "&modaltype=" + (int)SoeTimeUniqueDayEdit.UpdateHoliday + "&dialog=true&holiday=" + holiday.HolidayId + "&daytype=" + dayTypeId + "&oldDate=" + deleteDateValue;
                    RedirectToSelf("UPDATED", url, true);
                }
                else
                    RedirectToSelf("NOTUPDATED", true);

                #endregion
            }

            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (calm.DeleteHoliday(holiday, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", "&modaltype=" + (int)SoeTimeUniqueDayEdit.DeleteHoliday + "&dialog=true&holiday=" + holiday.HolidayId + "&daytype=" + holiday.DayType.DayTypeId);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
