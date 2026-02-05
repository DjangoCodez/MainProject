using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SoftOne.Soe.Web.soe.time.preferences.schedulesettings.halfdays.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private CalendarManager cm;
        private TimeCodeManager tcm;

        protected TimeHalfday timeHalfDay;
        protected Dictionary<int, int> selected;
        protected int clockValue1;
        protected int clockValue2;
        protected int clockValue3;
        protected int modalTimeHalfdayId = 0;
        protected int modalDaytypeId = 0;
        protected int modalType = 0;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_ScheduleSettings_Halfdays_Edit;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("script.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CalendarManager(ParameterObject);
            tcm = new TimeCodeManager(ParameterObject);

            //Mandatory parameters

            // Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            // Optional parameters
            int timeHalfDayId;
            if (Int32.TryParse(QS["halfday"], out timeHalfDayId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    timeHalfDay = cm.GetPrevNextTimeHalfday(timeHalfDayId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (timeHalfDay != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?halfday=" + timeHalfDay.TimeHalfdayId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?halfday=" + timeHalfDayId);
                }
                else
                {
                    timeHalfDay = cm.GetTimeHalfday(timeHalfDayId, SoeCompany.ActorCompanyId);
                    if (timeHalfDay == null)
                    {
                        Form1.MessageWarning = GetText(4419, "Halvdag hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(4420, "Redigera halvdag");
            string registerModeTabHeaderText = GetText(4412, "Registrera halvdag");
            PostOptionalParameterCheck(Form1, timeHalfDay, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = timeHalfDay != null ? timeHalfDay.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            DayType.ConnectDataSource(cm.GetDayTypesByCompanyDict(SoeCompany.ActorCompanyId, true));
            Type.ConnectDataSource(cm.GetTimeHalfdayTypesDict(true));

            clockValue1 = (int)SoeTimeHalfdayType.ClockInMinutes;
            clockValue2 = (int)SoeTimeHalfdayType.RelativeStartValue;
            clockValue3 = (int)SoeTimeHalfdayType.RelativeEndValue;

            selected = new Dictionary<int, int>();

            Breaks.DataSourceFrom = tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false, (int)SoeTimeCodeType.Break);
            if (Repopulate && PreviousForm != null)
            {
                Breaks.PreviousForm = PreviousForm;
            }
            else
            {
                #region TimeCodeBreak

                if (timeHalfDay != null && timeHalfDay.TimeCodeBreak != null)
                {
                    int pos = 0;
                    foreach (TimeCodeBreak timeCodeBreak in timeHalfDay.TimeCodeBreak)
                    {
                        Breaks.AddLabelValue(pos, timeCodeBreak.TimeCodeId.ToString());
                        Breaks.AddValueFrom(pos, timeCodeBreak.Name);
                        selected.Add(pos, timeCodeBreak.TimeCodeId);

                        pos++;
                        if (pos == Breaks.NoOfIntervals)
                            break;
                    }
                }

                #endregion
            }

            #endregion

            #region Set data

            if (timeHalfDay != null)
            {
                Name.Value = timeHalfDay.Name;
                Description.Value = timeHalfDay.Description;
                Type.Value = timeHalfDay.Type.ToString();

                if (timeHalfDay.Type == (int)SoeTimeHalfdayType.ClockInMinutes)
                {
                    StringBuilder sb = new StringBuilder();

                    int hours = (int)timeHalfDay.Value / 60;
                    int minutes = (int)timeHalfDay.Value - (hours * 60);

                    if (hours < 10)
                        sb.Append(" ");
                    sb.Append(hours);
                    sb.Append(":");
                    if (minutes < 10)
                        sb.Append("0");
                    sb.Append(minutes);

                    Value.Value = sb.ToString();
                }
                else
                {
                    Value.Value = timeHalfDay.Value.ToString();
                }

                DayType.Value = timeHalfDay.DayTypeId.ToString();
            }

            #endregion

            #region MessageFromSelf

            if (MessageFromSelf == "SAVED")
            {
                Form1.MessageSuccess = GetText(4428, "Halvdag sparad");
                TryDisplayModalWindow();
            }
            else if (MessageFromSelf == "NOTSAVED")
                Form1.MessageError = GetText(4429, "Halvdag kunde inte sparas");
            else if (MessageFromSelf == "UPDATED")
            {
                Form1.MessageSuccess = GetText(4430, "Halvdag uppdaterad");
                TryDisplayModalWindow();
            }
            else if (MessageFromSelf == "NOTUPDATED")
                Form1.MessageError = GetText(4431, "Halvdag kunde inte uppdateras");
            else if (MessageFromSelf == "NAME_MISSING")
                Form1.MessageWarning = GetText(1041, "Du måste ange namn");
            else if (MessageFromSelf == "EXIST")
                Form1.MessageInformation = GetText(4432, "En halvdag med samma namn eller dagtyp finns redan");
            else if (MessageFromSelf == "SAVED_WITH_ERRORS")
                Form1.MessageInformation = GetText(4439, "Halvdag sparad, raster sparades inte");
            else if (MessageFromSelf == "UPDATED_WITH_ERRORS")
                Form1.MessageInformation = GetText(4438, "Halvdag uppdaterad, raster uppdaterades inte");
            else if (MessageFromSelf == "VALUE_NOT_DECIMAL")
                Form1.MessageInformation = GetText(4445, "Värdet måste vara numeriskt");
            else if (MessageFromSelf == "VALUE_NOT_CLOCK")
                Form1.MessageInformation = GetText(4444, "Värdet måste vara ett klockslag");
            else if (MessageFromSelf == "DELETED")
            {
                Form1.MessageSuccess = GetText(4433, "Halvdag borttagen");
                TryDisplayModalWindow();
            }
            else if (MessageFromSelf == "NOTDELETED")
                Form1.MessageError = GetText(4434, "Halvdag kunde inte tas bort");

            #endregion

            #region Navigation

            if (timeHalfDay != null)
            {
                Form1.SetRegLink(GetText(4412, "Registrera halvdag"), "",
                   Feature.Time_Preferences_ScheduleSettings_Halfdays_Edit, Permission.Modify);
            }

            Form1.AddLink(GetText(4411, "Visa halvdagar"), "../",
                Feature.Time_Preferences_ScheduleSettings_Halfdays, Permission.Readonly);

            #endregion
        }

        private void TryDisplayModalWindow()
        {
            if (String.IsNullOrEmpty(QS["dialog"]))
                return;

            Int32.TryParse(QS["modaltype"], out modalType);
            Int32.TryParse(QS["halfday"], out modalTimeHalfdayId);

            switch ((SoeTimeUniqueDayEdit)modalType)
            {
                case SoeTimeUniqueDayEdit.UpdateHalfDay:
                    Int32.TryParse(QS["daytype"], out modalDaytypeId);
                    break;
            }
        }

        #region Actions

        protected override void Save()
        {
            #region Init

            string name = F["Name"];
            string description = F["Description"];

            int dayTypeId = 0;
            int.TryParse(F["DayType"], out dayTypeId);

            int type = 0;
            int.TryParse(F["Type"], out type);

            decimal value = 0M;

            if ((SoeTimeHalfdayType)type != SoeTimeHalfdayType.RelativeEndPercentage && (SoeTimeHalfdayType)type != SoeTimeHalfdayType.RelativeStartPercentage)
            {
                string clock = F["Value"].Replace(":", "");
                if (!Decimal.TryParse(clock, out value))
                    RedirectToSelf("VALUE_NOT_CLOCK", true);

                var clockParts = F["Value"].Split(":".ToCharArray());
                if (clockParts.Length == 2)
                {
                    value = Convert.ToInt32(clockParts[0]) * 60;
                    value += Convert.ToInt32(clockParts[1]);
                }
            }
            else
            {
                if (!decimal.TryParse(F["Value"], out value))
                    RedirectToSelf("VALUE_NOT_DECIMAL", true);
            }

            if (String.IsNullOrEmpty(name))
                RedirectToSelf("NAME_MISSING", true);

            Collection<FormIntervalEntryItem> breakItems = Breaks.GetData(F);

            #endregion

            if (timeHalfDay == null)
            {
                #region Add

                // Check if name already exists
                if (cm.TimeHalfdayExists(name, dayTypeId, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create new halfday
                timeHalfDay = new TimeHalfday()
                {
                    Name = name,
                    Description = description,
                    Type = type,
                    Value = value,
                };

                if (cm.AddTimeHalfday(timeHalfDay, dayTypeId, SoeCompany.ActorCompanyId).Success)
                {
                    if (!cm.SaveTimeHalfdayBreakReductions(breakItems, timeHalfDay.TimeHalfdayId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("SAVED_WITH_ERRORS");
                    RedirectToSelf("SAVED", "&modaltype=" + (int)SoeTimeUniqueDayEdit.AddHalfDay + "&dialog=true&halfday=" + timeHalfDay.TimeHalfdayId);
                }
                else
                    RedirectToSelf("NOTSAVED", true);

                #endregion
            }
            else
            {
                #region Update

                // If name has changed, check if the new name alredy exists
                if (timeHalfDay.Name != name || dayTypeId != timeHalfDay.DayTypeId && cm.TimeHalfdayExists(name, dayTypeId, timeHalfDay.TimeHalfdayId, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                timeHalfDay.Name = name;
                timeHalfDay.Description = description;
                timeHalfDay.Type = type;
                timeHalfDay.Value = value;

                if (cm.UpdateTimeHalfday(timeHalfDay, dayTypeId, SoeCompany.ActorCompanyId).Success)
                {
                    if (cm.SaveTimeHalfdayBreakReductions(breakItems, timeHalfDay.TimeHalfdayId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("UPDATED", "&modaltype=" + (int)SoeTimeUniqueDayEdit.UpdateHalfDay + "&dialog=true&halfday=" + timeHalfDay.TimeHalfdayId + "&daytype=" + dayTypeId, true);
                    RedirectToSelf("UPDATED_WITH_ERRORS");
                }
                else
                    RedirectToSelf("NOTUPDATED", true);

                #endregion
            }
        }

        protected override void Delete()
        {
            if (cm.DeleteTimeHalfday(timeHalfDay, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", "&modaltype=" + (int)SoeTimeUniqueDayEdit.DeleteHalfDay + "&dialog=true&halfday=" + timeHalfDay.TimeHalfdayId);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
