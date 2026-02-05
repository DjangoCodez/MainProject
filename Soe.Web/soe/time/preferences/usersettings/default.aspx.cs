using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.time.preferences.usersettings
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SettingManager sm;

        protected bool attestPermission;
        protected bool schedulePlanningPermission;
        protected bool schedulePlanningUserPermission;
        protected bool orderPlanningPermission;
        protected bool orderPlanningUserPermission;
        protected bool calendarViewPermission;
        protected bool dayViewPermission;
        protected bool scheduleViewPermission;
        protected bool templateDayViewPermission;
        protected bool templateScheduleViewPermission;
        protected bool scenarioDayViewPermission;
        protected bool scenarioScheduleViewPermission;
        protected bool staffingNeedsPermission;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            // Permissions
            schedulePlanningPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning, Permission.Modify);
            schedulePlanningUserPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser, Permission.Modify);
            orderPlanningPermission = HasRolePermission(Feature.Billing_Order_Planning, Permission.Modify);
            orderPlanningUserPermission = HasRolePermission(Feature.Billing_Order_PlanningUser, Permission.Modify);

            calendarViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_CalendarView, Permission.Modify) ||
                                     HasRolePermission(Feature.Billing_Order_Planning_CalendarView, Permission.Modify) ||
                                     HasRolePermission(Feature.Billing_Order_PlanningUser_CalendarView, Permission.Modify);
            dayViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_DayView, Permission.Modify) ||
                                     HasRolePermission(Feature.Billing_Order_Planning_DayView, Permission.Modify) ||
                                     HasRolePermission(Feature.Billing_Order_PlanningUser_DayView, Permission.Modify);
            scheduleViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScheduleView, Permission.Modify) ||
                                     HasRolePermission(Feature.Billing_Order_Planning_ScheduleView, Permission.Modify) ||
                                     HasRolePermission(Feature.Billing_Order_PlanningUser_ScheduleView, Permission.Modify);
            templateDayViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_TemplateDayView, Permission.Modify);
            templateScheduleViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_TemplateScheduleView, Permission.Modify);
            scenarioDayViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScenarioDayView, Permission.Modify);
            scenarioScheduleViewPermission = HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScenarioScheduleView, Permission.Modify);
            staffingNeedsPermission = HasRolePermission(Feature.Time_Schedule_StaffingNeeds, Permission.Modify);

            DivTimeSchedulePlanning.Visible = schedulePlanningPermission || schedulePlanningUserPermission || orderPlanningPermission || orderPlanningUserPermission;
            DivCalendarView.Visible = (schedulePlanningPermission || schedulePlanningUserPermission || orderPlanningPermission || orderPlanningUserPermission) && calendarViewPermission;
            DivDayView.Visible = (schedulePlanningPermission || schedulePlanningUserPermission) && dayViewPermission;
            DivScheduleView.Visible = (schedulePlanningPermission || schedulePlanningUserPermission) && scheduleViewPermission;
            DivEditShift.Visible = (schedulePlanningPermission || schedulePlanningUserPermission) && (dayViewPermission || scheduleViewPermission);
            DivStaffingNeeds.Visible = staffingNeedsPermission;

            attestPermission = HasRolePermission(Feature.Time_Time_Attest, Permission.Readonly);

            // Company settings
            bool useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, UserId, SoeCompany.ActorCompanyId, 0);

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            // TimeSchedulePlanning views
            Dictionary<int, string> views = new Dictionary<int, string>();
            if (calendarViewPermission)
                views.Add((int)TermGroup_TimeSchedulePlanningViews.Calendar, GetText(6011, 1004));
            if (dayViewPermission)
                views.Add((int)TermGroup_TimeSchedulePlanningViews.Day, GetText(6012, 1004));
            if (scheduleViewPermission)
                views.Add((int)TermGroup_TimeSchedulePlanningViews.Schedule, GetText(6013, 1004));
            TimeSchedulePlanningDefaultView.ConnectDataSource(views);

            // TimeSchedulePlanning intervals
            SortedDictionary<int, string> intervals = GetGrpTextSorted(TermGroup.TimeSchedulePlanningVisibleDays, false, false);
            intervals.Remove((int)TermGroup_TimeSchedulePlanningVisibleDays.Custom);
            intervals.Remove((int)TermGroup_TimeSchedulePlanningVisibleDays.Year);
            TimeSchedulePlanningDefaultInterval.ConnectDataSource(intervals);

            // TimeSchedulePlanning shift styles
            Dictionary<int, string> styles = GetGrpText(TermGroup.TimeSchedulePlanningShiftStyle).Sort(true, true);
            TimeSchedulePlanningDefaultShiftStyle.ConnectDataSource(styles);

            if (!templateDayViewPermission && !templateScheduleViewPermission)
                TimeSchedulePlanningDisableTemplateScheduleWarning.Visible = false;

            // Default group by/sort by
            Dictionary<int, string> dayViewGroupBy = GetGrpText(TermGroup.TimeSchedulePlanningDayViewGroupBy).Sort(true, false);
            dayViewGroupBy.Remove((int)TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftTypeFirstOnDay);
            if (useAccountHierarchy)
                dayViewGroupBy.Remove((int)TermGroup_TimeSchedulePlanningDayViewGroupBy.Category);
            TimeSchedulePlanningDayViewGroupBy.ConnectDataSource(dayViewGroupBy);

            Dictionary<int, string> dayViewSortBy = GetGrpText(TermGroup.TimeSchedulePlanningDayViewSortBy).Sort(true, false);
            TimeSchedulePlanningDayViewSortBy.ConnectDataSource(dayViewSortBy);

            Dictionary<int, string> scheduleViewGroupBy = GetGrpText(TermGroup.TimeSchedulePlanningScheduleViewGroupBy).Sort(true, false);
            if (useAccountHierarchy)
                scheduleViewGroupBy.Remove((int)TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Category);
            TimeSchedulePlanningScheduleViewGroupBy.ConnectDataSource(scheduleViewGroupBy);

            Dictionary<int, string> scheduleViewSortBy = GetGrpText(TermGroup.TimeSchedulePlanningScheduleViewSortBy).Sort(true, false);
            TimeSchedulePlanningScheduleViewSortBy.ConnectDataSource(scheduleViewSortBy);

            #endregion

            #region Set data

            TimeSchedulePlanningDefaultView.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDefaultView, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            int defaultInterval = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDefaultInterval, UserId, SoeCompany.ActorCompanyId, 0);
            if (defaultInterval == 0)
                defaultInterval = 7;
            TimeSchedulePlanningDefaultInterval.Value = defaultInterval.ToString();
            TimeSchedulePlanningStartWeek.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningStartWeek, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningCalendarViewCountType.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningCalendarViewCountType, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningCalendarViewShowToolTipInfo.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningCalendarViewShowToolTipInfo, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningShowEmployeeList.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningShowEmployeeList, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDisableCheckBreakTimesWarning.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDisableCheckBreakTimesWarning, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDisableAutoLoad.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDisableAutoLoad, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDisableTemplateScheduleWarning.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDisableTemplateScheduleWarning, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDisableSaveOnNavigateWarning.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDisableSaveOnNavigateWarning, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDisableBreaksWithinHolesWarning.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDefaultShiftStyle.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDefaultShiftStyle, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDefaultShiftStyleInstruction.DefaultIdentifier = " ";
            TimeSchedulePlanningDefaultShiftStyleInstruction.DisableFieldset = true;
            TimeSchedulePlanningDefaultShiftStyleInstruction.Instructions = new List<string>()
            {
                GetText(3033, "Detaljerad = Varje pass visas utdraget över hela dagen på en egen rad."),
                GetText(3034, "Verklig tid = Passen är utlagda enligt sina respektive klockslag på en och samma rad."),
                GetText(3035, "Komprimerad = Som 'Verklig tid' men hälften så höga."),
            };
            TimeSchedulePlanningDayViewGroupBy.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDayViewDefaultGroupBy, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningDayViewSortBy.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningScheduleViewGroupBy.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningScheduleViewDefaultGroupBy, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            TimeSchedulePlanningScheduleViewSortBy.Value = sm.GetIntSetting(SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            StaffingNeedsDayViewShowDiagram.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.StaffingNeedsDayViewShowDiagram, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            StaffingNeedsDayViewShowDetailedSummary.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.StaffingNeedsDayViewShowDetailedSummary, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            StaffingNeedsScheduleViewShowDetailedSummary.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.StaffingNeedsScheduleViewShowDetailedSummary, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            EmployeeGridDisableAutoLoad.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.EmployeeGridDisableAutoLoad, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            AttestTreeDisableAutoLoad.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeAttestTreeDisableAutoLoad, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            AttestDisableSaveAttestWarning.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeDisableApplySaveAttestWarning, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            AttestDisableApplyRestoreWarning.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeDisableApplyRestoreWarning, UserId, SoeCompany.ActorCompanyId, 0).ToString();
            PayrollCalculationTreeDisableAutoLoad.Value = sm.GetBoolSetting(SettingMainType.User, (int)UserSettingType.PayrollCalculationTreeDisableAutoLoad, UserId, SoeCompany.ActorCompanyId, 0).ToString();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
            }

            #endregion
        }

        protected override void Save()
        {
            bool success = true;

            #region Bool

            var boolValues = new Dictionary<int, bool>();

            boolValues.Add((int)UserSettingType.TimeSchedulePlanningCalendarViewCountType, StringUtility.GetBool(F["TimeSchedulePlanningCalendarViewCountType"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningCalendarViewShowToolTipInfo, StringUtility.GetBool(F["TimeSchedulePlanningCalendarViewShowToolTipInfo"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningShowEmployeeList, StringUtility.GetBool(F["TimeSchedulePlanningShowEmployeeList"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningDisableCheckBreakTimesWarning, StringUtility.GetBool(F["TimeSchedulePlanningDisableCheckBreakTimesWarning"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningDisableAutoLoad, StringUtility.GetBool(F["TimeSchedulePlanningDisableAutoLoad"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningDisableTemplateScheduleWarning, StringUtility.GetBool(F["TimeSchedulePlanningDisableTemplateScheduleWarning"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningDisableSaveOnNavigateWarning, StringUtility.GetBool(F["TimeSchedulePlanningDisableSaveOnNavigateWarning"]));
            boolValues.Add((int)UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning, StringUtility.GetBool(F["TimeSchedulePlanningDisableBreaksWithinHolesWarning"]));

            boolValues.Add((int)UserSettingType.StaffingNeedsDayViewShowDiagram, StringUtility.GetBool(F["StaffingNeedsDayViewShowDiagram"]));
            boolValues.Add((int)UserSettingType.StaffingNeedsDayViewShowDetailedSummary, StringUtility.GetBool(F["StaffingNeedsDayViewShowDetailedSummary"]));
            boolValues.Add((int)UserSettingType.StaffingNeedsScheduleViewShowDetailedSummary, StringUtility.GetBool(F["StaffingNeedsScheduleViewShowDetailedSummary"]));

            boolValues.Add((int)UserSettingType.EmployeeGridDisableAutoLoad, StringUtility.GetBool(F["EmployeeGridDisableAutoLoad"]));
            boolValues.Add((int)UserSettingType.TimeAttestTreeDisableAutoLoad, StringUtility.GetBool(F["AttestTreeDisableAutoLoad"]));
            boolValues.Add((int)UserSettingType.TimeDisableApplySaveAttestWarning, StringUtility.GetBool(F["AttestDisableSaveAttestWarning"]));
            boolValues.Add((int)UserSettingType.TimeDisableApplyRestoreWarning, StringUtility.GetBool(F["AttestDisableApplyRestoreWarning"]));
            boolValues.Add((int)UserSettingType.PayrollCalculationTreeDisableAutoLoad, StringUtility.GetBool(F["PayrollCalculationTreeDisableAutoLoad"]));

            if (!sm.UpdateInsertBoolSettings(SettingMainType.User, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            #region Integer

            var intValues = new Dictionary<int, int>();

            intValues.Add((int)UserSettingType.TimeSchedulePlanningDefaultView, StringUtility.GetInt(F["TimeSchedulePlanningDefaultView"], 0));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningDefaultInterval, StringUtility.GetInt(F["TimeSchedulePlanningDefaultInterval"], 7));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningStartWeek, StringUtility.GetInt(F["TimeSchedulePlanningStartWeek"], 0));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningDefaultShiftStyle, StringUtility.GetInt(F["TimeSchedulePlanningDefaultShiftStyle"], 0));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningDayViewDefaultGroupBy, StringUtility.GetInt(F["TimeSchedulePlanningDayViewGroupBy"], 0));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy, StringUtility.GetInt(F["TimeSchedulePlanningDayViewSortBy"], 0));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningScheduleViewDefaultGroupBy, StringUtility.GetInt(F["TimeSchedulePlanningScheduleViewGroupBy"], 0));
            intValues.Add((int)UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy, StringUtility.GetInt(F["TimeSchedulePlanningScheduleViewSortBy"], 0));

            if (!sm.UpdateInsertIntSettings(SettingMainType.User, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;

            #endregion

            RedirectToSelf(success ? "UPDATED" : "NOTUPDATED");
        }
    }
}
