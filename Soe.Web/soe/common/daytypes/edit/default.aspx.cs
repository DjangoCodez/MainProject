using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.common.daytypes.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected CalendarManager cm;
        protected DayType dayType;

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
                    case Feature.Manage_Preferences_Registry_DayTypes_Edit:
                        EnableManage = true;
                        FeatureList = Feature.Manage_Preferences_Registry_DayTypes;
                        FeatureEdit = Feature.Manage_Preferences_Registry_DayTypes_Edit;
                        FeatureImport = Feature.Manage_Preferences_Registry_DayTypes_Edit;
                        break;
                    case Feature.Time_Preferences_ScheduleSettings_DayTypes_Edit:
                        EnableTime = true;
                        FeatureList = Feature.Time_Preferences_ScheduleSettings_DayTypes;
                        FeatureEdit = Feature.Time_Preferences_ScheduleSettings_DayTypes_Edit;
                        FeatureImport = Feature.Time_Preferences_ScheduleSettings_DayTypes_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CalendarManager(ParameterObject);

            //Mandatory parameters

            // Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            // Optional parameters
            int dayTypeId;
            string name = QS["name"];
            if (Int32.TryParse(QS["daytype"], out dayTypeId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    dayType = cm.GetPrevNextDayType(dayTypeId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (dayType != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?daytype=" + dayType.DayTypeId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?daytype=" + dayTypeId);
                }
                else
                {
                    dayType = cm.GetDayType(dayTypeId, SoeCompany.ActorCompanyId);
                    if (dayType == null)
                    {
                        Form1.MessageWarning = GetText(3067, "Dagtyp hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3068, "Redigera dagtyp");
            string registerModeTabHeaderText = GetText(3069, "Registrera dagtyp");
            PostOptionalParameterCheck(Form1, dayType, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = dayType != null ? dayType.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            var daysOfWeekTypes = cm.GetDaysOfWeekDict(true);
            FromDayType.ConnectDataSource(daysOfWeekTypes);
            ToDayType.ConnectDataSource(daysOfWeekTypes);

            #endregion

            #region Set data

            if (dayType != null)
            {
                Name.Value = dayType.Name;
                Description.Value = dayType.Description;

                if (dayType.StandardWeekdayFrom.HasValue)
                    FromDayType.Value = (dayType.StandardWeekdayFrom.Value + 1).ToString();

                if (dayType.StandardWeekdayTo.HasValue)
                    ToDayType.Value = (dayType.StandardWeekdayTo.Value + 1).ToString();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3061, "Dagtyp sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3062, "Dagtyp kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3063, "Dagtyp uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3064, "Dagtyp kunde inte uppdateras");
                else if (MessageFromSelf == "NAME_MISSING")
                    Form1.MessageWarning = GetText(1041, "Du måste ange namn");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3065, "En dagtyp med det namnet finns redan");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1987, "Dagtyp borttagen");
                else if (MessageFromSelf == "HAS_HOLIDAYS")
                    Form1.MessageError = GetText(3845, "Dagtyp kunde inte tas bort, det finns helgdagar kopplade till den");
                else if (MessageFromSelf == "HAS_HALFDAYS")
                    Form1.MessageError = GetText(3846, "Dagtyp kunde inte tas bort, det finns halvdagar kopplade till den");
                else if (MessageFromSelf == "HAS_TIMERULES")
                    Form1.MessageError = GetText(3847, "Dagtyp kunde inte tas bort, det finns tidsregler kopplade till den");
                else if (MessageFromSelf == "HAS_ATTESTRULEHEADS")
                    Form1.MessageError = GetText(3848, "Dagtyp kunde inte tas bort, det finns attestregler kopplade till den");
                else if (MessageFromSelf == "HAS_EMPLOYEEGROUPS")
                    Form1.MessageError = GetText(3849, "Dagtyp kunde inte tas bort, det finns tidavtal kopplade till den");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3066, "Dagtyp kunde inte tas bort, kontrollera att den inte används");
                else if (MessageFromSelf == "DAYTYPEWEEKDAY_INVALID")
                    Form1.MessageError = GetText(5523, "Ogilltiga veckodagar. Vecka får inte sluta efter Söndag");
            }

            #endregion

            #region Navigation

            if (dayType != null)
            {
                Form1.SetRegLink(GetText(3069, "Registrera dagtyp"), "",
                   FeatureEdit, Permission.Modify);
            }

            Form1.AddLink(GetText(4295, "Visa dagtyper"), "../",
                FeatureList, Permission.Readonly);

            Form1.AddLink(GetText(4294, "Importera dagtyper"), "../import",
                FeatureImport, Permission.Modify);

            #endregion
        }

        #region Actions

        protected override void Save()
        {
            #region Init

            string name = F["Name"];
            string description = F["Description"];
            int standardWeekDayFrom = 0;
            int standardWeekDayTo = 0;

            int.TryParse(F["FromDayType"], out standardWeekDayFrom);
            int.TryParse(F["ToDayType"], out standardWeekDayTo);

            //Subtract to align with Weekday enum
            standardWeekDayFrom = standardWeekDayFrom > 0 ? --standardWeekDayFrom : standardWeekDayFrom;
            standardWeekDayTo = standardWeekDayTo > 0 ? --standardWeekDayTo : standardWeekDayTo;

            //Week must end on sunday, ex: Saturday-Monday is not valid
            if (!CalendarUtility.IsWeekdayRangeValid(standardWeekDayFrom, standardWeekDayTo))
                RedirectToSelf("DAYTYPEWEEKDAY_INVALID", true);

            if (String.IsNullOrEmpty(name))
                RedirectToSelf("NAME_MISSING", true);

            #endregion

            if (dayType == null)
            {
                #region Add

                // Check if name already exists
                if (cm.GetDayType(name, SoeCompany.ActorCompanyId) != null)
                    RedirectToSelf("EXIST", true);

                // Create new day type
                dayType = new DayType()
                {
                    Name = name,
                    Description = description,
                    StandardWeekdayFrom = standardWeekDayFrom,
                    StandardWeekdayTo = standardWeekDayTo,
                };

                if (dayType.StandardWeekdayFrom == -1)
                    dayType.StandardWeekdayFrom = null;

                if (dayType.StandardWeekdayTo == -1)
                    dayType.StandardWeekdayTo = null;

                if (cm.AddDayType(dayType, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("SAVED");
                RedirectToSelf("NOTSAVED", true);

                #endregion
            }
            else
            {
                #region Update

                // If name has changed, check if the new name alredy exists
                if (dayType.Name != name)
                {
                    if (cm.GetDayType(name, SoeCompany.ActorCompanyId) != null)
                        RedirectToSelf("EXIST", true);
                }

                dayType.Name = name;
                dayType.Description = description;
                dayType.StandardWeekdayFrom = standardWeekDayFrom;
                dayType.StandardWeekdayTo = standardWeekDayTo;

                if (dayType.StandardWeekdayFrom == -1)
                    dayType.StandardWeekdayFrom = null;

                if (dayType.StandardWeekdayTo == -1)
                    dayType.StandardWeekdayTo = null;

                if (cm.UpdateDayType(dayType, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("UPDATED");
                RedirectToSelf("NOTUPDATED", true);

                #endregion
            }
        }

        protected override void Delete()
        {
            CalendarManager cm = new CalendarManager(ParameterObject);

            ActionResult result = cm.DeleteDayType(dayType, SoeCompany.ActorCompanyId);
            if (result.Success)
                RedirectToSelf("DELETED", false, true);
            else
            {
                switch (result.ErrorNumber)
                {
                    case (int)ActionResultDelete.DayTypeHasHolidays:
                        RedirectToSelf("HAS_HOLIDAYS", true);
                        break;
                    case (int)ActionResultDelete.DayTypeHasTimeHalfdays:
                        RedirectToSelf("HAS_HALFDAYS", true);
                        break;
                    case (int)ActionResultDelete.DayTypeHasTimeRules:
                        RedirectToSelf("HAS_TIMERULES", true);
                        break;
                    case (int)ActionResultDelete.DayTypeHasAttestRuleHeads:
                        RedirectToSelf("HAS_ATTESTRULEHEADS", true);
                        break;
                    case (int)ActionResultDelete.DayTypeHasEmployeeGroups:
                        RedirectToSelf("HAS_EMPLOYEEGROUPS", true);
                        break;
                    default:
                        RedirectToSelf("NOTDELETED", true);
                        break;
                }
            }
        }

        #endregion
    }
}
