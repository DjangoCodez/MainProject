using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.time.preferences.compsettings
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am;
        private AttestManager atm;
        private ContactManager cm;
        private EmployeeManager em;
        private ProductManager pm;
        private PayrollManager prm;
        private ReportManager rptm;
        private SettingManager sm;
        private SequenceNumberManager snm;
        private TimeCodeManager tcm;
        private TimeDeviationCauseManager tdcm;
        private TimePeriodManager tpm;
        private TimeSalaryManager tsm;

        private readonly string planningPeriodDefaultColor1 = "DA1E28";  // --soe-color-semantic-error
        private readonly string planningPeriodDefaultColor2 = "24A148";  // --soe-color-semantic-success
        private readonly string planningPeriodDefaultColor3 = "0565C9";  // --soe-color-semantic-information

        public int AccountDimId { get; set; }
        public string stdDimID; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_CompSettings;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/cssjs/account.js");
            //Scripts.Add("texts1.js.aspx");
            Scripts.Add("default.js");
            Scripts.Add("/cssjs/jscolor/jscolor.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            //Managers
            am = new AccountManager(ParameterObject);
            atm = new AttestManager(ParameterObject);
            cm = new ContactManager(ParameterObject);
            em = new EmployeeManager(ParameterObject);
            pm = new ProductManager(ParameterObject);
            prm = new PayrollManager(ParameterObject);
            rptm = new ReportManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            snm = new SequenceNumberManager(ParameterObject);
            tcm = new TimeCodeManager(ParameterObject);
            tdcm = new TimeDeviationCauseManager(ParameterObject);
            tpm = new TimePeriodManager(ParameterObject);
            tsm = new TimeSalaryManager(ParameterObject);

            //Mandatory parameters

            //Mode
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            //Permissons
            DivPayroll.Visible = true;
            UsePayroll.Visible = IsSupportAdmin;
            //UsedPayrollSince.Visible = IsSupportAdmin;
            DontValidateSecurityNbr.Visible = IsSupportAdmin;

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Content

            #region Prereq

            AccountDim accountDimStd = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            AccountDimId = accountDimStd.AccountDimId;
            stdDimID = AccountDimId.ToString();

            Dictionary<int, string> accountsDict = new Dictionary<int, string>();
            accountsDict.Add(0, " ");
            accountsDict.AddRange(am.GetAccountStdsByCompany(SoeCompany.ActorCompanyId, true).OrderBy(o => o.Account.AccountNr).ToDictionary(k => k.AccountId, v => $"{v.Account.AccountNr} {v.Account.Name}"));

            Dictionary<int, string> attestStatusesPayroll = atm.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time, true, false);
            Dictionary<int, string> attestStatusesInvoice = atm.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.InvoiceTime, SoeModule.Time, true, false);
            Dictionary<int, string> paymentInformationRowsISO20022 = tsm.GetPaymentInformationViewsDictForISO20022(SoeCompany.ActorCompanyId, true);
            List<AccountDim> accountDims = am.GetAccountDimsByCompany(SoeCompany.ActorCompanyId);

            // Address types
            List<int> addressTypeIds = cm.GetSysContactAddressTypeIds((int)TermGroup_SysContactType.Employee);
            Dictionary<int, string> allAddressTypes = GetGrpText(TermGroup.SysContactAddressType);
            Dictionary<int, string> addressTypes = new Dictionary<int, string>();
            addressTypes.Add(0, string.Empty);
            // Filter address types based on contact type
            foreach (var addressType in allAddressTypes)
            {
                if (addressTypeIds.Contains(addressType.Key))
                    addressTypes.Add(addressType.Key, addressType.Value);
            }

            // ECom types
            List<int> ecomTypeIds = cm.GetSysContactEComsTypeIds((int)TermGroup_SysContactType.Employee);
            Dictionary<int, string> allEComTypes = GetGrpText(TermGroup.SysContactEComType);
            Dictionary<int, string> ecomTypes = new Dictionary<int, string>();
            ecomTypes.Add(0, string.Empty);
            // Filter ecom types based on contact type
            foreach (var ecomType in allEComTypes)
            {
                if (ecomTypeIds.Contains(ecomType.Key))
                    ecomTypes.Add(ecomType.Key, ecomType.Value);
            }

            // Load all settings for CompanySettingTypeGroup once!
            Dictionary<int, object> timeSettingsDict = new Dictionary<int, object>();
            timeSettingsDict.AddRange(sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Time, SoeCompany.ActorCompanyId));
            timeSettingsDict.AddRange(sm.GetCompanySettingsDict((int)CompanySettingTypeGroup.Payroll, SoeCompany.ActorCompanyId));
            bool useAccountHierarchy = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, SoeCompany.ActorCompanyId, 0);
            bool usePayroll = sm.GetBoolSettingFromDict(timeSettingsDict, (int)CompanySettingType.UsePayroll);
            bool useHibernatingEmployment = sm.GetBoolSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseHibernatingEmployment);
            DateTime usedPayrollSince = sm.GetDateTimeSettingFromDict(timeSettingsDict, (int)CompanySettingType.UsedPayrollSince);
            DateTime calculateExperienceFrom = sm.GetDateTimeSettingFromDict(timeSettingsDict, (int)CompanySettingType.CalculateExperienceFrom);

            if (usePayroll)
                UseHibernatingEmployment.ReadOnly = true;
            if (useHibernatingEmployment)
                UsePayroll.ReadOnly = true;

            #endregion

            #region Tab General

            //General
            DefaultTimeCode.ConnectDataSource(tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false));
            DefaultTimeCode.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultTimeCode, (int)SettingDataType.Integer);
            DefaultTimeDeviationCause.ConnectDataSource(tdcm.GetTimeDeviationCausesDict(SoeCompany.ActorCompanyId, true));
            DefaultTimeDeviationCause.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultTimeDeviationCause, (int)SettingDataType.Integer);
            DefaultEmployeeGroup.ConnectDataSource(em.GetEmployeeGroupsDict(SoeCompany.ActorCompanyId, true));
            DefaultEmployeeGroup.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeeGroup, (int)SettingDataType.Integer);
            DefaultPayrollGroup.ConnectDataSource(prm.GetPayrollGroupsDict(SoeCompany.ActorCompanyId, true));
            DefaultPayrollGroup.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultPayrollGroup, (int)SettingDataType.Integer);
            DefaultVacationGroup.ConnectDataSource(prm.GetVacationGroupsDict(SoeCompany.ActorCompanyId, true));
            DefaultVacationGroup.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultVacationGroup, (int)SettingDataType.Integer);
            DefaultTimePeriodHead.ConnectDataSource(tpm.GetTimePeriodHeadsDict(SoeCompany.ActorCompanyId, TermGroup_TimePeriodType.Payroll, true));
            DefaultTimePeriodHead.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultTimePeriodHead, (int)SettingDataType.Integer);
            DefaultPlanningPeriod.ConnectDataSource(tpm.GetTimePeriodHeadsDict(SoeCompany.ActorCompanyId, TermGroup_TimePeriodType.RuleWorkTime, true));
            DefaultPlanningPeriod.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultPlanningPeriod, (int)SettingDataType.Integer);
            DefaultPreviousTimePeriod.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultPreviousTimePeriod, (int)SettingDataType.Boolean);
            TimeDefaultTimeCodeEarnedHoliday.ConnectDataSource(tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false, (int)SoeTimeCodeType.AdditionDeduction));
            TimeDefaultTimeCodeEarnedHoliday.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultTimeCodeEarnedHoliday, (int)SettingDataType.Integer);
            TimeCodeBreakShowInvoiceProducts.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCodeBreakShowInvoiceProducts, (int)SettingDataType.Boolean);
            TimeCodeBreakShowPayrollProducts.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCodeBreakShowPayrollProducts, (int)SettingDataType.Boolean);
            UseSimplifiedEmployeeRegistration.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseSimplifiedEmployeeRegistration, (int)SettingDataType.Boolean);
            SuggestEmployeeNrAsUsername.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSuggestEmployeeNrAsUsername, (int)SettingDataType.Boolean);
            ForceSocialSecurityNbr.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeForceSocialSecNbr, (int)SettingDataType.Boolean);
            DontValidateSecurityNbr.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDontValidateSocialSecNbr, (int)SettingDataType.Boolean);
            SetEmploymentPercentManually.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSetEmploymentPercentManually, (int)SettingDataType.Boolean);
            SetNextFreePersonNumberAutomatically.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSetNextFreePersonNumberAutomatically, (int)SettingDataType.Boolean);
            EmployeeSeqNbrStart.Value = sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.EmployeeSeqNbrStart, 1).ToString();
            EmployeeSeqNbrStart.InfoText = String.Format("({0})", snm.GetLastUsedSequenceNumber(SoeCompany.ActorCompanyId, "Employee"));
            EmployeeKeepNbrOfYearsAfterEnd.Value = sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.EmployeeKeepNbrOfYearsAfterEnd, 7).ToString();
            EmployeeIncludeNbrOfMonthsAfterEnded.DataSource = SettingManager.NorOfMonthsToShowEndedEmployees;
            EmployeeIncludeNbrOfMonthsAfterEnded.DataBind();
            EmployeeIncludeNbrOfMonthsAfterEnded.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.EmployeeIncludeNbrOfMonthsAfterEnded, (int)SettingDataType.Integer, defaultValue: "3");
            UseEmploymentExperienceAsStartValue.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, (int)SettingDataType.Boolean);
            DoNotUseMessageGroupInAttest.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.DoNotUseMessageGroupInAttest, (int)SettingDataType.Boolean);
            if (useAccountHierarchy)
            {
                TimeAttestTreeIncludeAdditionalEmployees.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAttestTreeIncludeAdditionalEmployees, (int)SettingDataType.Boolean);
                TimeAttestTreeIncludeAdditionalEmployees.Visible = true;
                TimeSplitBreakOnAccount.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSplitBreakOnAccount, (int)SettingDataType.Boolean);
                TimeSplitBreakOnAccount.Visible = true;
            }
            else
            {
                TimeAttestTreeIncludeAdditionalEmployees.Visible = false;
                TimeSplitBreakOnAccount.Visible = false;
            }
            UseHibernatingEmployment.Value = useHibernatingEmployment.ToString();
            DontAllowIdenticalSSN.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.DontAllowIdenticalSSN, (int)SettingDataType.Boolean);
            UseIsNearestManagerOnAttestRoleUser.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseIsNearestManagerOnAttestRoleUser, (int)SettingDataType.Boolean);

            //Schedule
            MaxNoOfBrakes.DataSource = SettingManager.MaxNoOfBrakes;
            MaxNoOfBrakes.DataBind();
            MaxNoOfBrakes.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeMaxNoOfBrakes, (int)SettingDataType.Integer);
            StartOnFirstDayOfWeek.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultStartOnFirstDayOfWeek, (int)SettingDataType.Boolean);
            UseStopDateOnTemplate.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeUseStopDateOnTemplate, (int)SettingDataType.Boolean);
            CreateShiftsThatStartsAfterMidnigtInMobile.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCreateShiftsThatStartsAfterMidnigtInMobile, (int)SettingDataType.Boolean);

            //Placement
            PlacementDefaultPreliminary.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimePlacementDefaultPreliminary, (int)SettingDataType.Boolean);
            PlacementHideShiftTypes.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimePlacementHideShiftTypes, (int)SettingDataType.Boolean);
            PlacementHideAccountDims.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimePlacementHideAccountDims, (int)SettingDataType.Boolean);
            PlacementHidePreliminary.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimePlacementHidePreliminary, (int)SettingDataType.Boolean);

            //Reports
            DefaultTimeMonthlyReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeMonthlyReport, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeMonthlyReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultMonthlyReport, (int)SettingDataType.Integer);
            DefaultEmployeeScheduleDayReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeeScheduleDayReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeeScheduleDayReport, (int)SettingDataType.Integer);
            DefaultEmployeeScheduleWeekReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeeScheduleWeekReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeeScheduleWeekReport, (int)SettingDataType.Integer);
            DefaultEmployeeTemplateScheduleDayReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeTemplateSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeeTemplateScheduleDayReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeeTemplateScheduleDayReport, (int)SettingDataType.Integer);
            DefaultEmployeeTemplateScheduleWeekReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeTemplateSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeeTemplateScheduleWeekReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeeTemplateScheduleWeekReport, (int)SettingDataType.Integer);
            DefaultEmployeePostTemplateScheduleDayReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeTemplateSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeePostTemplateScheduleDayReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeePostTemplateScheduleDayReport, (int)SettingDataType.Integer);
            DefaultEmployeePostTemplateScheduleWeekReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeTemplateSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeePostTemplateScheduleWeekReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultEmployeePostTemplateScheduleWeekReport, (int)SettingDataType.Integer);
            DefaultScenarioScheduleDayReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultScenarioScheduleDayReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultScenarioScheduleDayReport, (int)SettingDataType.Integer);
            DefaultScenarioScheduleWeekReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmployeeSchedule, onlyOriginal: true, addEmptyRow: true));
            DefaultScenarioScheduleWeekReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultScenarioScheduleWeekReport, (int)SettingDataType.Integer);
            DefaultTimeScheduleTasksAndDeliverysDayReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeScheduleTasksAndDeliverysDayReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultScheduleTasksAndDeliverysDayReport, (int)SettingDataType.Integer);
            DefaultTimeScheduleTasksAndDeliverysWeekReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeScheduleTasksAndDeliverysWeekReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultScheduleTasksAndDeliverysWeekReport, (int)SettingDataType.Integer);
            DefaultTimeSalarySpecificationReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeSalarySpecificationReport, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeSalarySpecificationReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultTimeSalarySpecificationReport, (int)SettingDataType.Integer);
            DefaultTimeSalaryControlInfoReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeSalaryControlInfoReport, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeSalaryControlInfoReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultTimeSalaryControlInfoReport, (int)SettingDataType.Integer);
            DefaultTimeKU10Report.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.KU10Report, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeKU10Report.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultKU10Report, (int)SettingDataType.Integer);
            DefaultTimeSalarySettingReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.PayrollProductReport, onlyOriginal: true, addEmptyRow: true));
            DefaultTimeSalarySettingReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollSettingsDefaultReport, (int)SettingDataType.Integer);
            DefaultXEPayrollSlipReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.PayrollSlip, onlyOriginal: true, addEmptyRow: true));
            DefaultXEPayrollSlipReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.DefaultPayrollSlipReport, (int)SettingDataType.Integer);
            DefaultEmployeeVacationDebtReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.EmployeeVacationDebtReport, onlyOriginal: true, addEmptyRow: true));
            DefaultEmployeeVacationDebtReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.DefaultEmployeeVacationDebtReport, (int)SettingDataType.Integer);
            DefaultEmploymentContractShortSubstituteReport.ConnectDataSource(rptm.GetReportsByTemplateTypeDict(SoeCompany.ActorCompanyId, SoeReportTemplateType.TimeEmploymentContract, onlyOriginal: true, addEmptyRow: true));
            DefaultEmploymentContractShortSubstituteReport.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.DefaultEmploymentContractShortSubstituteReport, (int)SettingDataType.Integer);

            //AttestStatus
            ExportSalaryMinimumAttestStatus.ConnectDataSource(attestStatusesPayroll);
            ExportSalaryMinimumAttestStatus.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, (int)SettingDataType.Integer);
            ExportSalaryResultingAttestStatus.ConnectDataSource(attestStatusesPayroll);
            ExportSalaryResultingAttestStatus.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, (int)SettingDataType.Integer);
            MobileTimeAttestResultingAttestStatus.ConnectDataSource(attestStatusesPayroll);
            MobileTimeAttestResultingAttestStatus.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.MobileTimeAttestResultingAttestStatus, (int)SettingDataType.Integer);
            ExportInvoiceMinimumAttestStatus.ConnectDataSource(attestStatusesInvoice);
            ExportInvoiceMinimumAttestStatus.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, (int)SettingDataType.Integer);
            ExportInvoiceResultingAttestStatus.ConnectDataSource(attestStatusesInvoice);
            ExportInvoiceResultingAttestStatus.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportInvoiceResultingAttestStatus, (int)SettingDataType.Integer);

            //Stamping
            IgnoreOfflineTerminals.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeIgnoreOfflineTerminals, (int)SettingDataType.Boolean);
            TimeDoNotModifyTimeStampEntryType.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDoNotModifyTimeStampEntryType, (int)SettingDataType.Boolean);
            UseTimeScheduleTypeFromTime.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseTimeScheduleTypeFromTime, (int)SettingDataType.Boolean);
            LimitAttendanceViewToStampedTerminal.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.LimitAttendanceViewToStampedTerminal, (int)SettingDataType.Boolean);
            PossibilityToRegisterAdditionsInTerminal.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PossibilityToRegisterAdditionsInTerminal, (int)SettingDataType.Boolean);

            #endregion

            #region Tab Staffing

            //Staffing
            IncludeSecondaryEmploymentInWorkTimeWeek.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.IncludeSecondaryEmploymentInWorkTimeWeek, (int)SettingDataType.Boolean, "True");
            UseStaffing.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeUseStaffing, (int)SettingDataType.Boolean);
            UseVacant.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeUseVacant, (int)SettingDataType.Boolean);
            OrderPlanningIgnoreScheduledBreaksOnAssignment.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment, (int)SettingDataType.Boolean);
            StaffingShiftAccountDimId.ConnectDataSource(am.GetAccountDimsByCompanyDict(SoeCompany.ActorCompanyId, true));
            StaffingShiftAccountDimId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeStaffingShiftAccountDimId, (int)SettingDataType.Integer);
            TimeSchedulePlanningDayViewMinorTickLength.ConnectDataSource(GetGrpText(TermGroup.StaffingNeedsHeadInterval, addEmptyRow: true));
            TimeSchedulePlanningDayViewMinorTickLength.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength, (int)SettingDataType.Integer);

            if (useAccountHierarchy)
            {
                TimeSchedulePlanningInactivateLending.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningInactivateLending, (int)SettingDataType.Boolean);
                TimeSchedulePlanningInactivateLending.Visible = true;
                TimeSchedulePlanningInactivateLendingInstruction.DefaultIdentifier = " ";
                TimeSchedulePlanningInactivateLendingInstruction.DisableFieldset = true;
                TimeSchedulePlanningInactivateLendingInstruction.Instructions = new List<string>()
            {
                GetText(12529, "Denna inställning innebär att man frångår funktionalitet för in- och utlåning och kan därmed även redigera pass som har annan tillhörighet så länge man har tillhörighet till den anställde."),
                GetText(12530, "Använd endast om in- och utlåning ska inaktiveras och man vet vad det innebär!"),
            };
            }
            else
            {
                TimeSchedulePlanningInactivateLending.Visible = false;
                TimeSchedulePlanningInactivateLendingInstruction.Visible = false;
            }

            //SchedulePlanning
            TimeSchedulePlanningClockRounding.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningClockRounding, (int)SettingDataType.Integer);
            TimeDefaultDoNotKeepShiftsTogether.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeDefaultDoNotKeepShiftsTogether, (int)SettingDataType.Boolean);
            TimeSchedulePlanningSendXEMailOnChange.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningSendXEMailOnChange, (int)SettingDataType.Boolean);

            TimeSchedulePlanningSetShiftAsExtra.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsExtra, (int)SettingDataType.Boolean);
            TimeSchedulePlanningSetShiftAsSubstitute.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute, (int)SettingDataType.Boolean);

            HideRecipientsInShiftRequest.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.HideRecipientsInShiftRequest, (int)SettingDataType.Boolean);
            TimeSchedulePlanningSortQueueByLas.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningSortQueueByLas, (int)SettingDataType.Boolean);
            CreateEmployeeRequestWhenDeniedWantedShift.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.CreateEmployeeRequestWhenDeniedWantedShift, (int)SettingDataType.Boolean);
            ShowTemplateScheduleForEmployeesInApp.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.ShowTemplateScheduleForEmployeesInApp, (int)SettingDataType.Boolean);
            UseMultipleScheduleTypes.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseMultipleScheduleTypes, (int)SettingDataType.Boolean);
            UseMultipleScheduleTypes.InfoText = GetText(11972, "Kan påverka utfallet av befintliga tidsregler");
            SubstituteShiftIsAssignedDueToAbsenceOnlyIfSameBatch.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SubstituteShiftIsAssignedDueToAbsenceOnlyIfSameBatch, (int)SettingDataType.Boolean);
            SubstituteShiftDontIncludeCopiedOrMovedShifts.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SubstituteShiftDontIncludeCopiedOrMovedShifts, (int)SettingDataType.Boolean);
            ExtraShiftAsDefaultOnHidden.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.ExtraShiftAsDefaultOnHidden, (int)SettingDataType.Boolean);
            PrintAgreementOnAssignFromFreeShift.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PrintAgreementOnAssignFromFreeShift, (int)SettingDataType.Boolean);
            TimeSchedulePlanningDragDropMoveAsDefault.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningDragDropMoveAsDefault, (int)SettingDataType.Boolean);
            UseLeisureCodes.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseLeisureCodes, (int)SettingDataType.Boolean);
            UseAnnualLeave.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.UseAnnualLeave, (int)SettingDataType.Boolean);
            TimeSchedulePlanningSaveCopyOnPublish.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningSaveCopyOnPublish, (int)SettingDataType.Boolean);

            //Workrules
            TimeSchedulePlanningSkipWorkRules.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningSkipWorkRules, (int)SettingDataType.Boolean);
            TimeSchedulePlanningUseWorkRulesForMinors.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors, (int)SettingDataType.Boolean);
            TimeSchedulePlanningOverrideWorkRuleWarningsForMinors.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors, (int)SettingDataType.Boolean);
            TimeSchedulePlanningRuleRestTimeDayMandatory.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningRuleRestTimeDayMandatory, (int)SettingDataType.Boolean);
            TimeSchedulePlanningRuleRestTimeWeekMandatory.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningRuleRestTimeWeekMandatory, (int)SettingDataType.Boolean);
            TimeSchedulePlanningRuleRuleWorkTimeWeekDontEvaluateInSchedule.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningRuleWorkTimeWeekDontEvaluateInSchedule, (int)SettingDataType.Boolean);
            TimeSchedulePlanningUseRuleWorkTimeWeekForParttimeWorkersInSchedule.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningUseRuleWorkTimeWeekForParttimeWorkersInSchedule, (int)SettingDataType.Boolean);
            TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift, (int)SettingDataType.Integer, defaultValue: "0");
            TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift.InfoText = GetText(11560, "Ska vara större än noll för att träda i kraft.");

            TimeSchedulePlanningShiftRequestPreventTooEarly.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly, (int)SettingDataType.Boolean);
            TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore, (int)SettingDataType.Integer, defaultValue: "0");
            TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore.InfoText = GetText(11560, "Ska vara större än noll för att träda i kraft.");
            TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore, (int)SettingDataType.Integer, defaultValue: "0");
            TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore.InfoText = GetText(11560, "Ska vara större än noll för att träda i kraft.");
            TimeSchedulePlanningShiftRequestPreventTooEarlyInstruction.DefaultIdentifier = " ";
            TimeSchedulePlanningShiftRequestPreventTooEarlyInstruction.DisableFieldset = true;
            TimeSchedulePlanningShiftRequestPreventTooEarlyInstruction.Instructions = new List<string>()
            {
                GetText(12518, "Ett ledigt pass bör läggas ut som tillgängligt i god tid innan passets starttid för att anställda ska kunna önska och ställa sig i kö till det."),
                GetText(12519, "Endast då det är ont om tid bör passförfrågan istället användas för att riktas mot specifika anställda."),
                GetText(12520, "Använd ovanstående inställningar för att se till att detta flöde följs."),
                GetText(12521, "Det går att välja om det ska bara ska komma upp en varning eller vara helt stoppande om man skickar för tidigt."),
                GetText(12522, "Det går även att använda sig av både varning och stopp. Varning ska då ha ett lägre värde än stopp."),
            };

            //SchedulePlanning - SchedulePlanningCalendarView
            TimeSchedulePlanningCalendarViewShowDaySummary.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningCalendarViewShowDaySummary, (int)SettingDataType.Boolean);

            //SchedulePlanning - TimeSchedulePlanningDayView
            TimeSchedulePlanningDayViewStartTime.Value = CalendarUtility.FormatMinutes(sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, 0));
            TimeSchedulePlanningDayViewEndTime.Value = CalendarUtility.FormatMinutes(sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningDayViewEndTime, 0));
            DayViewEndTimeInstruction.DefaultIdentifier = " ";
            DayViewEndTimeInstruction.DisableFieldset = true;
            DayViewEndTimeInstruction.Instructions = new List<string>()
            {
                GetText(3404, "Det får max vara 24 timmar mellan start och sluttid på dagen."),
                GetText(3405, "Det går dock bra att ange start till exempelvis 04:00 och slut till 28:00 för att förskjuta dagen över midnatt."),
            };
            TimeSchedulePlanningBreakVisibility.ConnectDataSource(GetGrpText(TermGroup.TimeSchedulePlanningBreakVisibility));
            TimeSchedulePlanningBreakVisibility.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningBreakVisibility, (int)SettingDataType.Integer);

            //SchedulePlanning - EditShift
            TimeEditShiftShowEmployeeInGridView.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeEditShiftShowEmployeeInGridView, (int)SettingDataType.Boolean);
            TimeEditShiftShowDateInGridView.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeEditShiftShowDateInGridView, (int)SettingDataType.Boolean);
            TimeShiftTypeMandatory.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeShiftTypeMandatory, (int)SettingDataType.Boolean);
            TimeEditShiftAllowHoles.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeEditShiftAllowHoles, (int)SettingDataType.Boolean);

            //SchedulePlanning - Costs
            StaffingUseTemplateCost.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.StaffingUseTemplateCost, (int)SettingDataType.Boolean);
            StaffingTemplateCost.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.StaffingTemplateCost, decimals: 2).ToString();

            //SchedulePlanning - Availability
            TimeAvailabilityLockDaysBefore.Value = sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAvailabilityLockDaysBefore).ToString();
            TimeAvailabilityLockDaysBeforeInstruction.DefaultIdentifier = " ";
            TimeAvailabilityLockDaysBeforeInstruction.DisableFieldset = true;
            TimeAvailabilityLockDaysBeforeInstruction.Instructions = new List<string>()
            {
                GetText(3038, "Anställd kan ej redigera sin tillgänglighet ett antal dagar innan den infaller"),
                GetText(3039, "0 = Ingen låsning"),
            };

            // SchedulePlanning - Contact information
            PlanningContactInformationInstruction.DefaultIdentifier = " ";
            PlanningContactInformationInstruction.DisableFieldset = true;
            PlanningContactInformationInstruction.Instructions = new List<string>()
            {
                GetText(12033, "Vid högerklick på anställd inne i schemaplaneringen kan olika kontaktuppgifter visas."),
                GetText(12034, "Här anges vilka typer av kontaktuppgifter som kan visas där."),
            };

            PlanningContactInformationAddressTypes.DataSourceFrom = addressTypes;
            string addressTypesString = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningContactAddressTypes, (int)SettingDataType.String);
            string[] addressTypesList = addressTypesString.Split(',');
            int pos = 0;
            foreach (string item in addressTypesList)
            {
                PlanningContactInformationAddressTypes.AddValueFrom(pos, item);
                pos++;
                if (pos == PlanningContactInformationAddressTypes.NoOfIntervals)
                    break;
            }

            PlanningContactInformationEComTypes.DataSourceFrom = ecomTypes;
            string ecomTypesString = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningContactEComTypes, (int)SettingDataType.String);
            string[] ecomTypesList = ecomTypesString.Split(',');
            pos = 0;
            foreach (string item in ecomTypesList)
            {
                PlanningContactInformationEComTypes.AddValueFrom(pos, item);
                pos++;
                if (pos == PlanningContactInformationEComTypes.NoOfIntervals)
                    break;
            }

            // Minors
            MinorsSchoolDayStartMinutes.Value = CalendarUtility.FormatMinutes(sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.MinorsSchoolDayStartMinutes, 0));
            MinorsSchoolDayStopMinutes.Value = CalendarUtility.FormatMinutes(sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.MinorsSchoolDayStopMinutes, 0));
            InstructionsMinors.DefaultIdentifier = " ";
            InstructionsMinors.DisableFieldset = true;
            InstructionsMinors.Instructions = new List<string>()
            {
                GetText(8733, "Om inget annat anges så anses skoldag vara kl 8-16"),
            };

            // Planning periods
            CalculatePlanningPeriodScheduledTime.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTime, (int)SettingDataType.Boolean);
            CalculatePlanningPeriodScheduledTimeIncludeExtraShift.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeIncludeExtraShift, (int)SettingDataType.Boolean, "false");
            CalculatePlanningPeriodScheduledTimeInstruction.Visible = !base.IsSupportAdmin;
            CalculatePlanningPeriodScheduledTimeUseAveragingPeriod.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod, (int)SettingDataType.Boolean);

            string planningPeriodColorString = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeColors, (int)SettingDataType.String);
            string[] planningPeriodColorList = !string.IsNullOrEmpty(planningPeriodColorString) && planningPeriodColorString.Contains(';') ? planningPeriodColorString.Split(';') : new string[] { };
            PlanningPeriodColorOver.Value = planningPeriodColorList.Length > 0 && !string.IsNullOrEmpty(planningPeriodColorList[0]) ? planningPeriodColorList[0] : this.planningPeriodDefaultColor1;
            PlanningPeriodColorEqual.Value = planningPeriodColorList.Length > 1 && !string.IsNullOrEmpty(planningPeriodColorList[1]) ? planningPeriodColorList[1] : this.planningPeriodDefaultColor2;
            PlanningPeriodColorUnder.Value = planningPeriodColorList.Length > 2 && !string.IsNullOrEmpty(planningPeriodColorList[2]) ? planningPeriodColorList[2] : this.planningPeriodDefaultColor3;

            PlanningPeriodColorInstructions.DefaultIdentifier = " ";
            PlanningPeriodColorInstructions.DisableFieldset = true;
            PlanningPeriodColorInstructions.Instructions = new List<string>()
            {
                GetText(12178, "Här finns möjlighet att ändra färgerna för markering av hur väl en anställd är planerad inom aktuell planeringsperiod."),
                GetText(12179, "Färgerna påverkar den vertikala stapeln som visas på den anställde i schemaplaneringen, samt bakgrundsfärgen på summeringen i detaljdialogen."),
            };

            // SchedulePlanning - Gauge thresholds
            GaugeSalesThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold1, 50).ToString("N1");
            GaugeSalesThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold2, 80).ToString("N1");
            GaugeHoursThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold1, 50).ToString("N1");
            GaugeHoursThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold2, 80).ToString("N1");
            GaugeSalaryCostThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold1, 50).ToString("N1");
            GaugeSalaryCostThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold2, 80).ToString("N1");
            GaugeSalaryPercentThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold1, 50).ToString("N1");
            GaugeSalaryPercentThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold2, 80).ToString("N1");
            GaugeLPATThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold1, 50).ToString("N1");
            GaugeLPATThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold2, 80).ToString("N1");
            GaugeFPATThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold1, 50).ToString("N1");
            GaugeFPATThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold2, 80).ToString("N1");
            GaugeBPATThreshold1.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold1, 50).ToString("N1");
            GaugeBPATThreshold2.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold2, 80).ToString("N1");
            InstructionsGaugeThresholds.DefaultIdentifier = " ";
            InstructionsGaugeThresholds.DisableFieldset = true;
            InstructionsGaugeThresholds.Instructions = new List<string>()
            {
                GetText(3658, "Gränsvärden för färger i indikatorer"),
                GetText(3659, "Ange ett tal mellan 0 och 100 för var respektive brytpunkt mellan rött, gult och grönt ska vara"),
            };

            //Skills (5)
            Dictionary<int, int> nbrOfSkillLevels = new Dictionary<int, int>();
            for (int i = 0; i <= 5; i++)
            {
                nbrOfSkillLevels.Add(i, i);
            }
            TimeNbrOfSkillLevels.ConnectDataSource(nbrOfSkillLevels);
            TimeNbrOfSkillLevels.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeNbrOfSkillLevels, (int)SettingDataType.Integer);
            TimeSkillLevelHalfPrecision.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSkillLevelHalfPrecision, (int)SettingDataType.Boolean);
            TimeSkillCantBeOverridden.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSkillCantBeOverridden, (int)SettingDataType.Boolean);
            TimeSkillCantBeOverriddenInstruction.DefaultIdentifier = " ";
            TimeSkillCantBeOverriddenInstruction.DisableFieldset = true;
            TimeSkillCantBeOverriddenInstruction.Instructions = new List<string>()
            {
                GetText(3975, "Ja = Går ej att spara pass om inte passtypens alla kompetenser är uppfyllda"),
                GetText(3976, "Nej = Visar endast en varning"),
            };

            //StaffingNeeds
            StaffingNeedsChartType.ConnectDataSource(GetGrpText(TermGroup.StaffingNeedsAnalysisChartType));
            StaffingNeedsChartType.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeStaffingNeedsAnalysisChartType, (int)SettingDataType.Integer);
            var accountDimDict = new Dictionary<int, string>();
            accountDimDict.Add(0, GetText(12173, "Automatiskt"));
            accountDimDict.Add(int.MaxValue, GetText(12172, "Sätt inte kontering"));
            accountDims.Where(w => w.IsInternal).ToList().ForEach(f => accountDimDict.Add(f.AccountDimId, f.Name));

            StaffingNeedsFrequencyAccountDim.ConnectDataSource(accountDimDict);
            StaffingNeedsFrequencyAccountDim.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.StaffingNeedsFrequencyAccountDim, (int)SettingDataType.Integer);
            StaffingNeedsFrequencyParentAccountDim.ConnectDataSource(accountDimDict);
            StaffingNeedsFrequencyParentAccountDim.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.StaffingNeedsFrequencyParentAccountDim, (int)SettingDataType.Integer);
            StaffingNeedsRatioSalesPerScheduledHour.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeStaffingNeedsAnalysisRatioSalesPerScheduledHour, (int)SettingDataType.Boolean);
            StaffingNeedsRatioSalesPerWorkHour.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeStaffingNeedsAnalysisRatioSalesPerWorkHour, (int)SettingDataType.Boolean);
            StaffingNeedsRatioFrequencyAverage.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeStaffingNeedsAnalysisRatioFrequencyAverage, (int)SettingDataType.Boolean);
            StaffingNeedsWorkingPeriodMaxLength.Value = CalendarUtility.FormatMinutes(sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeStaffingNeedsWorkingPeriodMaxLength, 0));
            StaffingNeedRoundUp.Value = sm.GetDecimalSettingFromDict(timeSettingsDict, (int)CompanySettingType.StaffingNeedRoundUp, decimals: 2).ToString();
            EmployeePostPrefix.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeEmployeePostPrefix, (int)SettingDataType.String);

            //Approve absence
            SetApprovedYesAsDefault.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeSetApprovedYesAsDefault, (int)SettingDataType.Boolean);
            OnlyNoReplacementIsElectable.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeOnlyNoReplacementIsSelectable, (int)SettingDataType.Boolean);
            IncludeNoteInMessages.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages, (int)SettingDataType.Boolean);
            ValidateVacationWholeDay.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.ValidateVacationWholeDayWhenSaving, (int)SettingDataType.Boolean);
            ValidateVacationWholeDayInstructionList.DefaultIdentifier = " ";
            ValidateVacationWholeDayInstructionList.DisableFieldset = true;
            ValidateVacationWholeDayInstructionList.Instructions = new List<string>()
            {
                GetText(10933, "Förutsätter att orsaken semester har inställningarna Endast heldag och Hanteras som semester påslagna")
            };

            // Absence
            RemoveScheduleTypeOnAbsence.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.RemoveScheduleTypeOnAbsence, (int)SettingDataType.Boolean);

            #endregion

            #region Tab Accounting

            #region Payroll

            Dictionary<int, string> payrollAccountingPrioDict = GetGrpText(TermGroup.CompanyPayrollProductAccountingPrio);

            //Accounting payroll - Change sort order on items
            Dictionary<int, string> payrollAccountingPrioDictSorted = new Dictionary<int, string>();
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.NotUsed))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.NotUsed, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.NotUsed]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.PayrollProduct))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.PayrollProduct, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.PayrollProduct]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.EmploymentAccount))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.EmploymentAccount, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.EmploymentAccount]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.EmployeeAccount))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.EmployeeAccount, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.EmployeeAccount]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.Project))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.Project, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.Project]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.Customer))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.Customer, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.Customer]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_CompanyPayrollProductAccountingPrio.EmployeeGroup))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_CompanyPayrollProductAccountingPrio.EmployeeGroup, payrollAccountingPrioDict[(int)TermGroup_CompanyPayrollProductAccountingPrio.EmployeeGroup]);
            PayrollProductAccountingPrio1.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio2.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio3.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio4.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio5.ConnectDataSource(payrollAccountingPrioDictSorted);

            string payrollAccountingPrio = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCompanyPayrollProductAccountingPrio, (int)SettingDataType.String);
            string[] payrollAccountingPrios = payrollAccountingPrio.Split(',');
            if (payrollAccountingPrios != null)
            {
                if (payrollAccountingPrios.Length > 0)
                    PayrollProductAccountingPrio1.Value = payrollAccountingPrios[0];
                if (payrollAccountingPrios.Length > 1)
                    PayrollProductAccountingPrio2.Value = payrollAccountingPrios[1];
                if (payrollAccountingPrios.Length > 2)
                    PayrollProductAccountingPrio3.Value = payrollAccountingPrios[2];
                if (payrollAccountingPrios.Length > 3)
                    PayrollProductAccountingPrio4.Value = payrollAccountingPrios[3];
                if (payrollAccountingPrios.Length > 4)
                    PayrollProductAccountingPrio5.Value = payrollAccountingPrios[4];
            }

            #endregion

            #region Invoice

            Dictionary<int, string> invoiceAccountingPrioDict = GetGrpText(TermGroup.CompanyInvoiceProductAccountingPrio);

            //Accounting payroll - Change sort order on items
            Dictionary<int, string> invoiceAccountingPrioDictSorted = new Dictionary<int, string>();
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_CompanyInvoiceProductAccountingPrio.NotUsed))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_CompanyInvoiceProductAccountingPrio.NotUsed, invoiceAccountingPrioDict[(int)TermGroup_CompanyInvoiceProductAccountingPrio.NotUsed]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_CompanyInvoiceProductAccountingPrio.InvoiceProduct))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_CompanyInvoiceProductAccountingPrio.InvoiceProduct, invoiceAccountingPrioDict[(int)TermGroup_CompanyInvoiceProductAccountingPrio.InvoiceProduct]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_CompanyInvoiceProductAccountingPrio.EmploymentAccount))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_CompanyInvoiceProductAccountingPrio.EmploymentAccount, invoiceAccountingPrioDict[(int)TermGroup_CompanyInvoiceProductAccountingPrio.EmploymentAccount]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_CompanyInvoiceProductAccountingPrio.EmployeeAccount))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_CompanyInvoiceProductAccountingPrio.EmployeeAccount, invoiceAccountingPrioDict[(int)TermGroup_CompanyInvoiceProductAccountingPrio.EmployeeAccount]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_CompanyInvoiceProductAccountingPrio.Project))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_CompanyInvoiceProductAccountingPrio.Project, invoiceAccountingPrioDict[(int)TermGroup_CompanyInvoiceProductAccountingPrio.Project]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_CompanyInvoiceProductAccountingPrio.Customer))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_CompanyInvoiceProductAccountingPrio.Customer, invoiceAccountingPrioDict[(int)TermGroup_CompanyInvoiceProductAccountingPrio.Customer]);
            InvoiceProductAccountingPrio1.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio2.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio3.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio4.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio5.ConnectDataSource(invoiceAccountingPrioDictSorted);

            string invoiceAccountingPrio = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeCompanyInvoiceProductAccountingPrio, (int)SettingDataType.String);
            if (invoiceAccountingPrio != null)
            {
                string[] invoiceAccountingPrios = invoiceAccountingPrio.Split(',');
                if (invoiceAccountingPrios.Length > 0)
                    InvoiceProductAccountingPrio1.Value = invoiceAccountingPrios[0];
                if (invoiceAccountingPrios.Length > 1)
                    InvoiceProductAccountingPrio2.Value = invoiceAccountingPrios[1];
                if (invoiceAccountingPrios.Length > 2)
                    InvoiceProductAccountingPrio3.Value = invoiceAccountingPrios[2];
                if (invoiceAccountingPrios.Length > 3)
                    InvoiceProductAccountingPrio4.Value = invoiceAccountingPrios[3];
                if (invoiceAccountingPrios.Length > 4)
                    InvoiceProductAccountingPrio5.Value = invoiceAccountingPrios[4];
            }

            #endregion

            #region Vacation

            VacationValueDaysCreditAccountId.ConnectDataSource(accountsDict);
            VacationValueDaysCreditAccountId.ConnectDataSource(accountsDict);
            VacationValueDaysDebitAccountId.ConnectDataSource(accountsDict);
            VacationValueDaysCreditAccountId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.VacationValueDaysCreditAccountId, (int)SettingDataType.Integer);
            VacationValueDaysDebitAccountId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.VacationValueDaysDebitAccountId, (int)SettingDataType.Integer);

            #endregion

            #region Calculations

            RecalculateFutureAccountingWhenChangingMainAllocation.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.RecalculateFutureAccountingWhenChangingMainAllocation, (int)SettingDataType.Boolean);

            #endregion

            #endregion

            #region Tab AutoAttest

            //AutoAttest
            TimeAutoAttestRunService.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAutoAttestRunService, (int)SettingDataType.Boolean);
            TimeAutoAttestSourceAttestStateId.ConnectDataSource(attestStatusesPayroll);
            TimeAutoAttestSourceAttestStateId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAutoAttestSourceAttestStateId, (int)SettingDataType.Integer);
            TimeAutoAttestSourceAttestStateId2.ConnectDataSource(attestStatusesPayroll);
            TimeAutoAttestSourceAttestStateId2.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAutoAttestSourceAttestStateId2, (int)SettingDataType.Integer);
            TimeAutoAttestTargetAttestStateId.ConnectDataSource(attestStatusesPayroll);
            TimeAutoAttestTargetAttestStateId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAutoAttestTargetAttestStateId, (int)SettingDataType.Integer);
            TimeAutoAttestEmployeeManuallyAdjustedTimeStamps.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.TimeAutoAttestEmployeeManuallyAdjustedTimeStamps, (int)SettingDataType.Boolean);
            EmployeeManuallyAdjustedTimeStampsInstructionList.DefaultIdentifier = " ";
            EmployeeManuallyAdjustedTimeStampsInstructionList.DisableFieldset = true;
            EmployeeManuallyAdjustedTimeStampsInstructionList.Instructions = new List<string>()
            {
                GetText(8596, "Gäller stämplingar som den anställde själv har lagt till eller ändrat från 'Min tid'.")
            };

            #endregion

            #region Tab Payroll

            //General
            UsePayroll.Value = usePayroll.ToString();

            if (usedPayrollSince > CalendarUtility.DATETIME_DEFAULT)
            {
                if (base.IsLanguageSwedish())
                    UsedPayrollSince.Value = String.Format("{0:yyyy-MM-dd}", usedPayrollSince);
                else if (base.IsLanguageEnglish())
                    UsedPayrollSince.Value = String.Format("{0:MM/dd/yyyy}", usedPayrollSince);
                else if (base.IsLanguageFinnish() || base.IsLangugeNorwegian())
                    UsedPayrollSince.Value = String.Format("{0:dd.MM.yyyy}", usedPayrollSince);
            }
            if (calculateExperienceFrom > CalendarUtility.DATETIME_DEFAULT)
            {
                if (base.IsLanguageSwedish())
                    CalculateExperienceFrom.Value = String.Format("{0:yyyy-MM-dd}", calculateExperienceFrom);
                else if (base.IsLanguageEnglish())
                    CalculateExperienceFrom.Value = String.Format("{0:MM/dd/yyyy}", calculateExperienceFrom);
                else if (base.IsLanguageFinnish() || base.IsLangugeNorwegian())
                    CalculateExperienceFrom.Value = String.Format("{0:dd.MM.yyyy}", calculateExperienceFrom);
            }
            //PayrollAgreement
            PayrollGroupMandatory.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollGroupMandatory, (int)SettingDataType.Boolean);
            PayrollAgreementUseExeption2to6.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUseException2to6InWorkingAgreement, (int)SettingDataType.Boolean);
            PayrollAgreementUseOvertimeCompensation.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUseOverTimeCompensation, (int)SettingDataType.Boolean);
            PayrollAgreementUseTravelCompensation.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUseTravelCompansation, (int)SettingDataType.Boolean);
            PayrollAgreementUseVacationRightsDays.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUseVacationRights, (int)SettingDataType.Boolean);
            PayrollAgreementUseWorkTimeShiftCompensation.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUseWorkTimeShiftCompensation, (int)SettingDataType.Boolean);
            PayrollAgreementUseGrossNetTimeInStaffing.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing, (int)SettingDataType.Boolean);
            PayrollAgreementUsePayrollTax.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAgreementUsePayrollTax, (int)SettingDataType.Boolean);

            //EmploymentType
            PayrollEmploymentTypeUse_SE_Probationary.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_Probationary, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_Substitute.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_Substitute, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_SubstituteVacation.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_SubstituteVacation, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_Permanent.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_Permanent, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_FixedTerm.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_FixedTerm, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_Seasonal.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_Seasonal, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_SpecificWork.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_SpecificWork, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_Trainee.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_Trainee, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_NormalRetirementAge.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_NormalRetirementAge, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_CallContract.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_CallContract, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_FixedTerm14days.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_FixedTerm14days, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_Apprentice.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_Apprentice, (int)SettingDataType.Boolean);
            PayrollEmploymentTypeUse_SE_SpecialFixedTerm.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollEmploymentTypeUse_SE_SpecialFixedTerm, (int)SettingDataType.Boolean);


            //AttestStates
            PayrollCalculationLockedStatus.ConnectDataSource(attestStatusesPayroll);
            PayrollCalculationLockedStatus.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentLockedAttestStateId, (int)SettingDataType.Integer);
            PayrollCalculationApproved1Status.ConnectDataSource(attestStatusesPayroll);
            PayrollCalculationApproved1Status.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentApproved1AttestStateId, (int)SettingDataType.Integer);
            PayrollCalculationApproved2Status.ConnectDataSource(attestStatusesPayroll);
            PayrollCalculationApproved2Status.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentApproved2AttestStateId, (int)SettingDataType.Integer);
            PayrollCalculationPaymentFileCreated.ConnectDataSource(attestStatusesPayroll);
            PayrollCalculationPaymentFileCreated.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, (int)SettingDataType.Integer);

            //SalaryExport
            ExportTarget.ConnectDataSource(tsm.GetExportTargetsDict(true));
            ExportTarget.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportTarget, (int)SettingDataType.Integer);
            ExternalExportID.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportExternalExportID, (int)SettingDataType.String);
            ExternalExportSubId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportExternalExportSubId, (int)SettingDataType.String);
            SalaryExportEmail.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportEmail, (int)SettingDataType.String);
            SalaryExportNoComments.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportNoComments, (int)SettingDataType.Boolean);
            SalaryExportEmailCopy.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportEmailCopy, (int)SettingDataType.String);
            ExportVatProductId.ConnectDataSource(pm.GetPayrollProductsDict(SoeCompany.ActorCompanyId, true));
            ExportVatProductId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportVatProductId, (int)SettingDataType.Integer);
            SalaryExportUseSocSecFormat.ConnectDataSource(GetGrpText(TermGroup.SalaryExportUseSocSecFormat, addEmptyRow: false));
            SalaryExportUseSocSecFormat.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportUseSocSecFormat, (int)SettingDataType.Integer);
            SalaryExportLockPeriod.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportLockPeriod, (int)SettingDataType.Boolean);
            SalaryExportAllowPreliminary.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryExportAllowPreliminary, (int)SettingDataType.Boolean);

            //SalaryPaymentExport
            SalaryPaymentExportType.ConnectDataSource(GetGrpText(TermGroup.TimeSalaryPaymentExportType, addEmptyRow: true));
            SalaryPaymentExportType.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportType, (int)SettingDataType.Integer);
            SalaryPaymentExportSenderIdentification.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportSenderIdentification, (int)SettingDataType.String);
            SalaryPaymentExportSenderBankGiro.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportSenderBankGiro, (int)SettingDataType.String);
            SalaryPaymentExportCompanyIsRegisterHolder.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportCompanyIsRegisterHolder, (int)SettingDataType.Boolean);
            //SalaryPaymentExportBank.ConnectDataSource(GetGrpText((int)TermGroup.TimeSalaryPaymentExportBank, true, false));
            //SalaryPaymentExportBank.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportBank, (int)SettingDataType.Integer);
            SalaryPaymentExportPaymentAccount.ConnectDataSource(paymentInformationRowsISO20022);
            SalaryPaymentExportPaymentAccount.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportPaymentAccount, (int)SettingDataType.Integer);
            //SalaryPaymentExportUseAccountNrAsBBAN.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportUseAccountNrAsBBAN, (int)SettingDataType.Boolean);
            SalaryPaymentExportUsePaymentDateAsExecutionDate.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportUsePaymentDateAsExecutionDate, (int)SettingDataType.Boolean);
            SalaryPaymentExportUseIBANOnEmployee.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportUseIBANOnEmployee, (int)SettingDataType.Boolean);
            SalaryPaymentExportAgreementNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportAgreementNumber, (int)SettingDataType.String);
            SalaryPaymentExportAgreementNumber.InfoText = GetText(12161, "Gäller endast Nordea");
            SalaryPaymentExportDivisionName.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportDivisionName, (int)SettingDataType.String);
            SalaryPaymentExportDivisionName.InfoText = GetText(8872, "Gäller endast DNB");
            SalaryPaymentExportUseExtendedCurrencyNOK.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK, (int)SettingDataType.Boolean);
            SalaryPaymentExportExtendedAgreementNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportExtendedAgreementNumber, (int)SettingDataType.String);
            SalaryPaymentExportExtendedSenderIdentification.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportExtendedSenderIdentification, (int)SettingDataType.String);
            SalaryPaymentExportExtendedPaymentAccount.ConnectDataSource(paymentInformationRowsISO20022);
            SalaryPaymentExportExtendedPaymentAccount.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SalaryPaymentExportExtendedPaymentAccount, (int)SettingDataType.Integer);

            //Pension
            ForaAgreementNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportForaAgreementNumber, (int)SettingDataType.String);
            ITP1Number.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportITP1Number, (int)SettingDataType.String);
            ITP2Number.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportITP2Number, (int)SettingDataType.String);
            KPAAgreementNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportKPAAgreementNumber, (int)SettingDataType.String);
            KPAManagementNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportKPAManagementNumber, (int)SettingDataType.String);
            SkandiaSortingConcept.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportSkandiaSortingConcept, (int)SettingDataType.String);

            //Pension - SN/KFO
            SNKFOMemberNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportSNKFOMemberNumber, (int)SettingDataType.String);
            SNKFOWorkPlaceNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber, (int)SettingDataType.String);
            SNKFOAffiliateNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportSNKFOAffiliateNumber, (int)SettingDataType.String);
            SNKFOAgreementNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportSNKFOAgreementNumber, (int)SettingDataType.String);

            //Pension - Community
            CommunityCode.ConnectDataSource(sm.GetSysParameters(1, true, true));  // Select correct parameters
            CommunityCode.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportCommunityCode, (int)SettingDataType.String);

            //Pension - SCB
            SCBWorkSite.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportSCBWorkSite, (int)SettingDataType.String);
            CFARNumber.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportCFARNumber, (int)SettingDataType.String);

            //Pension - Arbetsgivarintyg
            Apinyckel.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollArbetsgivarintygnuApiNyckel, (int)SettingDataType.String);
            ArbetsgivarId.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollArbetsgivarintygnuArbetsgivarId, (int)SettingDataType.String);

            // Pension - Folksam
            FolksamCustomerNumber.Value = sm.GetIntSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportFolksamCustomerNumber, 0).ToString();

            //Skatteverket
            PlaceOfEmploymentAddress.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportPlaceOfEmploymentAddress, (int)SettingDataType.String);
            PlaceOfEmploymentCity.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollExportPlaceOfEmploymentCity, (int)SettingDataType.String);

            //Support area
            PayrollMaxRegionalSupportAmount.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollMaxRegionalSupportAmount, (int)SettingDataType.Decimal);
            PayrollMaxRegionalSupportPercent.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollMaxRegionalSupportPercent, (int)SettingDataType.Decimal);
            PayrollMaxResearchSupportAmount.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollMaxResearchSupportAmount, (int)SettingDataType.Decimal);

            #region Support area regional accounts

            #region Regional support accounts

            int accountPayrollMaxRegionalSupportDim1Id = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountPayrollMaxRegionalSupportDim1, UserId, SoeCompany.ActorCompanyId, 0);
            Account accountPayrollMaxRegionalSupportDim1 = am.GetAccount(SoeCompany.ActorCompanyId, accountPayrollMaxRegionalSupportDim1Id);
            int accountPayrollMaxRegionalSupportDim2Id = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountPayrollMaxRegionalSupportDim2, UserId, SoeCompany.ActorCompanyId, 0);
            Account accountPayrollMaxRegionalSupportDim2 = am.GetAccount(SoeCompany.ActorCompanyId, accountPayrollMaxRegionalSupportDim2Id);
            int accountPayrollMaxRegionalSupportDim3Id = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountPayrollMaxRegionalSupportDim3, UserId, SoeCompany.ActorCompanyId, 0);
            Account accountPayrollMaxRegionalSupportDim3 = am.GetAccount(SoeCompany.ActorCompanyId, accountPayrollMaxRegionalSupportDim3Id);
            int accountPayrollMaxRegionalSupportDim4Id = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountPayrollMaxRegionalSupportDim4, UserId, SoeCompany.ActorCompanyId, 0);
            Account accountPayrollMaxRegionalSupportDim4 = am.GetAccount(SoeCompany.ActorCompanyId, accountPayrollMaxRegionalSupportDim4Id);
            int accountPayrollMaxRegionalSupportDim5Id = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountPayrollMaxRegionalSupportDim5, UserId, SoeCompany.ActorCompanyId, 0);
            Account accountPayrollMaxRegionalSupportDim5 = am.GetAccount(SoeCompany.ActorCompanyId, accountPayrollMaxRegionalSupportDim5Id);
            int accountPayrollMaxRegionalSupportDim6Id = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountPayrollMaxRegionalSupportDim6, UserId, SoeCompany.ActorCompanyId, 0);
            Account accountPayrollMaxRegionalSupportDim6 = am.GetAccount(SoeCompany.ActorCompanyId, accountPayrollMaxRegionalSupportDim6Id);

            PayrollMaxRegionalSupportAccountDim1.LabelSetting = accountDimStd != null ? accountDimStd.Name : GetText(1130, "Standard");
            if (accountPayrollMaxRegionalSupportDim1 != null)
            {
                PayrollMaxRegionalSupportAccountDim1.Value = accountPayrollMaxRegionalSupportDim1.AccountNr;
                PayrollMaxRegionalSupportAccountDim1.InfoText += accountPayrollMaxRegionalSupportDim1.Name;
            }

            foreach (AccountDim accountDim in accountDims)
            {
                // Only internal accounts
                if (accountDim.IsStandard)
                    continue;

                Dictionary<int, string> accountInternalDict = am.GetAccountInternalsDict(accountDim.AccountDimId, SoeCompany.ActorCompanyId, true);

                HtmlTableRow row = new HtmlTableRow();
                row.Attributes.Add("RowType", "AccountInternal");

                HtmlTableCell column1 = new HtmlTableCell()
                {
                    Width = "200"
                };
                Text label =  new Text()
                {
                    LabelSetting = accountDim.Name,
                    FitInTable = true,
                };
                column1.Controls.Add(label);
                row.Cells.Add(column1);

                HtmlTableCell column2 = new HtmlTableCell
                {
                    ColSpan = 2,
                };
                SelectEntry select = new SelectEntry
                {
                    ID = "PayrollMaxRegionalSupportAccountDim" + accountDim.AccountDimNr,
                    HideLabel = true,
                    FitInTable = true,
                    DisableSettings = true,
                    Width = 150,
                };
                select.ConnectDataSource(accountInternalDict);

                if (accountDim.AccountDimNr == 2 && accountPayrollMaxRegionalSupportDim2 != null)
                    select.Value = accountPayrollMaxRegionalSupportDim2Id.ToString();
                else if (accountDim.AccountDimNr == 3 && accountPayrollMaxRegionalSupportDim3 != null)
                    select.Value = accountPayrollMaxRegionalSupportDim3Id.ToString();
                else if (accountDim.AccountDimNr == 4 && accountPayrollMaxRegionalSupportDim4 != null)
                    select.Value = accountPayrollMaxRegionalSupportDim4Id.ToString();
                else if (accountDim.AccountDimNr == 5 && accountPayrollMaxRegionalSupportDim5 != null)
                    select.Value = accountPayrollMaxRegionalSupportDim5Id.ToString();
                else if (accountDim.AccountDimNr == 6 && accountPayrollMaxRegionalSupportDim6 != null)
                    select.Value = accountPayrollMaxRegionalSupportDim6Id.ToString();

                column2.Controls.Add(select);
                row.Cells.Add(column2);

                PayrollMaxRegionalSupportAccountTable.Rows.Add(row);
            }

            #endregion

            #endregion

            #region Provision

            AccountProvisionTimeCode.ConnectDataSource(tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false));
            AccountProvisionTimeCode.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAccountProvisionTimeCode, (int)SettingDataType.Integer);
            AccountProvisionAccountDim.ConnectDataSource(am.GetAccountDimsByCompanyDict(SoeCompany.ActorCompanyId, true, onlyInternal: true));
            AccountProvisionAccountDim.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAccountProvisionAccountDim, (int)SettingDataType.Integer);

            #endregion

            #region Misc Settings

            AccountingDistributionPayrollProduct.ConnectDataSource(pm.GetPayrollProductsDict(SoeCompany.ActorCompanyId, true));
            AccountingDistributionPayrollProduct.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollAccountingDistributionPayrollProduct, (int)SettingDataType.Integer);
            PublishPayrollSlipWhenLockingPeriod.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PublishPayrollSlipWhenLockingPeriod, (int)SettingDataType.Boolean);
            SendNoticeWhenPayrollSlipPublished.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.SendNoticeWhenPayrollSlipPublished, (int)SettingDataType.Boolean);
            GrossalaryRoundingPayrollProduct.ConnectDataSource(pm.GetPayrollProductsDict(SoeCompany.ActorCompanyId, true));
            GrossalaryRoundingPayrollProduct.Value = sm.GetSettingFromDict(timeSettingsDict, (int)CompanySettingType.PayrollGrossalaryRoundingPayrollProduct, (int)SettingDataType.Integer);
            GrossalaryRoundingPayrollProductInstructionList.DefaultIdentifier = " ";
            GrossalaryRoundingPayrollProductInstructionList.DisableFieldset = true;
            GrossalaryRoundingPayrollProductInstructionList.Instructions = new List<string>()
            {
                GetText(8942, "Om den totala bruttolönen i perioden är mellan -0,99 och 0,99 läggs en justeringspost på vald löneart så att bruttolönen blir 0.")
            };
            #endregion

            #endregion

            #endregion

            #region MessageFromSelf

            if (MessageFromSelf == "UPDATED")
                Form1.MessageSuccess = GetText(3013, "Inställningar uppdaterade");
            else if (MessageFromSelf == "NOTUPDATED")
                Form1.MessageError = GetText(3014, "Inställningar kunde inte uppdateras");
            else if (MessageFromSelf == "NOTUPDATED_AUTOATTEST_STATES")
                Form1.MessageError = GetText(3451, "Om automatattest ska köras måste attestnivåer för från och till vara inställda");

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            bool success = true;

            // Validate auto attest settings
            // If run auto attest is checked, attest states must also be selected
            if (StringUtility.GetBool(F["TimeAutoAttestRunService"]) && (Int32.Parse(F["TimeAutoAttestSourceAttestStateId"]) == 0 || Int32.Parse(F["TimeAutoAttestTargetAttestStateId"]) == 0))
            {
                success = false;
                RedirectToSelf("NOTUPDATED_AUTOATTEST_STATES", true);
            }

            var boolValues = new Dictionary<int, bool>();
            var intValues = new Dictionary<int, int>();
            var decimalValues = new Dictionary<int, decimal>();
            var stringValues = new Dictionary<int, string>();
            var dateTimeValues = new Dictionary<int, DateTime>();

            #region Tab General

            // General
            intValues.Add((int)CompanySettingType.TimeDefaultTimeCode, F["DefaultTimeCode"] != null ? Int32.Parse(F["DefaultTimeCode"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultTimeDeviationCause, F["DefaultTimeDeviationCause"] != null ? Int32.Parse(F["DefaultTimeDeviationCause"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeeGroup, F["DefaultEmployeeGroup"] != null ? Int32.Parse(F["DefaultEmployeeGroup"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultPayrollGroup, F["DefaultPayrollGroup"] != null ? Int32.Parse(F["DefaultPayrollGroup"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultVacationGroup, F["DefaultVacationGroup"] != null ? Int32.Parse(F["DefaultVacationGroup"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultTimePeriodHead, F["DefaultTimePeriodHead"] != null ? Int32.Parse(F["DefaultTimePeriodHead"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultPlanningPeriod, F["DefaultPlanningPeriod"] != null ? Int32.Parse(F["DefaultPlanningPeriod"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultTimeCodeEarnedHoliday, F["TimeDefaultTimeCodeEarnedHoliday"] != null ? Int32.Parse(F["TimeDefaultTimeCodeEarnedHoliday"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeDefaultPreviousTimePeriod, StringUtility.GetBool(F["DefaultPreviousTimePeriod"]));
            boolValues.Add((int)CompanySettingType.TimeCodeBreakShowInvoiceProducts, StringUtility.GetBool(F["TimeCodeBreakShowInvoiceProducts"]));
            boolValues.Add((int)CompanySettingType.TimeCodeBreakShowPayrollProducts, StringUtility.GetBool(F["TimeCodeBreakShowPayrollProducts"]));
            boolValues.Add((int)CompanySettingType.UseSimplifiedEmployeeRegistration, StringUtility.GetBool(F["UseSimplifiedEmployeeRegistration"]));
            boolValues.Add((int)CompanySettingType.TimeSuggestEmployeeNrAsUsername, StringUtility.GetBool(F["SuggestEmployeeNrAsUsername"]));
            boolValues.Add((int)CompanySettingType.TimeForceSocialSecNbr, StringUtility.GetBool(F["ForceSocialSecurityNbr"]));
            boolValues.Add((int)CompanySettingType.TimeDontValidateSocialSecNbr, StringUtility.GetBool(F["DontValidateSecurityNbr"]));
            boolValues.Add((int)CompanySettingType.TimeSetEmploymentPercentManually, StringUtility.GetBool(F["SetEmploymentPercentManually"]));
            boolValues.Add((int)CompanySettingType.TimeSetNextFreePersonNumberAutomatically, StringUtility.GetBool(F["SetNextFreePersonNumberAutomatically"]));
            intValues.Add((int)CompanySettingType.EmployeeSeqNbrStart, F["EmployeeSeqNbrStart"] != null ? Int32.Parse(F["EmployeeSeqNbrStart"]) : 0);
            intValues.Add((int)CompanySettingType.EmployeeKeepNbrOfYearsAfterEnd, F["EmployeeKeepNbrOfYearsAfterEnd"] != null ? Int32.Parse(F["EmployeeKeepNbrOfYearsAfterEnd"]) : 7);
            boolValues.Add((int)CompanySettingType.UseEmploymentExperienceAsStartValue, StringUtility.GetBool(F["UseEmploymentExperienceAsStartValue"]));
            intValues.Add((int)CompanySettingType.EmployeeIncludeNbrOfMonthsAfterEnded, F["EmployeeIncludeNbrOfMonthsAfterEnded"] != null ? Int32.Parse(F["EmployeeIncludeNbrOfMonthsAfterEnded"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeAttestTreeIncludeAdditionalEmployees, StringUtility.GetBool(F["TimeAttestTreeIncludeAdditionalEmployees"]));
            boolValues.Add((int)CompanySettingType.TimeSplitBreakOnAccount, StringUtility.GetBool(F["TimeSplitBreakOnAccount"]));
            boolValues.Add((int)CompanySettingType.DoNotUseMessageGroupInAttest, StringUtility.GetBool(F["DoNotUseMessageGroupInAttest"]));
            boolValues.Add((int)CompanySettingType.UseHibernatingEmployment, !StringUtility.GetBool(F["UsePayroll"]) && StringUtility.GetBool(F["UseHibernatingEmployment"]));
            boolValues.Add((int)CompanySettingType.DontAllowIdenticalSSN, StringUtility.GetBool(F["DontAllowIdenticalSSN"]));
            boolValues.Add((int)CompanySettingType.UseIsNearestManagerOnAttestRoleUser, StringUtility.GetBool(F["UseIsNearestManagerOnAttestRoleUser"]));

            // Schedule
            intValues.Add((int)CompanySettingType.TimeMaxNoOfBrakes, F["MaxNoOfBrakes"] != null ? Int32.Parse(F["MaxNoOfBrakes"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeDefaultStartOnFirstDayOfWeek, StringUtility.GetBool(F["StartOnFirstDayOfWeek"]));
            boolValues.Add((int)CompanySettingType.TimeUseStopDateOnTemplate, StringUtility.GetBool(F["UseStopDateOnTemplate"]));
            boolValues.Add((int)CompanySettingType.TimeCreateShiftsThatStartsAfterMidnigtInMobile, StringUtility.GetBool(F["CreateShiftsThatStartsAfterMidnigtInMobile"]));

            // Placement
            boolValues.Add((int)CompanySettingType.TimePlacementDefaultPreliminary, StringUtility.GetBool(F["PlacementDefaultPreliminary"]));
            boolValues.Add((int)CompanySettingType.TimePlacementHideShiftTypes, StringUtility.GetBool(F["PlacementHideShiftTypes"]));
            boolValues.Add((int)CompanySettingType.TimePlacementHideAccountDims, StringUtility.GetBool(F["PlacementHideAccountDims"]));
            boolValues.Add((int)CompanySettingType.TimePlacementHidePreliminary, StringUtility.GetBool(F["PlacementHidePreliminary"]));

            // Reports
            intValues.Add((int)CompanySettingType.TimeDefaultMonthlyReport, F["DefaultTimeMonthlyReport"] != null ? Int32.Parse(F["DefaultTimeMonthlyReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeeScheduleDayReport, F["DefaultEmployeeScheduleDayReport"] != null ? Int32.Parse(F["DefaultEmployeeScheduleDayReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeeScheduleWeekReport, F["DefaultEmployeeScheduleWeekReport"] != null ? Int32.Parse(F["DefaultEmployeeScheduleWeekReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeeTemplateScheduleDayReport, F["DefaultEmployeeTemplateScheduleDayReport"] != null ? Int32.Parse(F["DefaultEmployeeTemplateScheduleDayReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeeTemplateScheduleWeekReport, F["DefaultEmployeeTemplateScheduleWeekReport"] != null ? Int32.Parse(F["DefaultEmployeeTemplateScheduleWeekReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeePostTemplateScheduleDayReport, F["DefaultEmployeePostTemplateScheduleDayReport"] != null ? Int32.Parse(F["DefaultEmployeePostTemplateScheduleDayReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultEmployeePostTemplateScheduleWeekReport, F["DefaultEmployeePostTemplateScheduleWeekReport"] != null ? Int32.Parse(F["DefaultEmployeePostTemplateScheduleWeekReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultScenarioScheduleDayReport, F["DefaultScenarioScheduleDayReport"] != null ? Int32.Parse(F["DefaultScenarioScheduleDayReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultScenarioScheduleWeekReport, F["DefaultScenarioScheduleWeekReport"] != null ? Int32.Parse(F["DefaultScenarioScheduleWeekReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultScheduleTasksAndDeliverysDayReport, F["DefaultTimeScheduleTasksAndDeliverysDayReport"] != null ? Int32.Parse(F["DefaultTimeScheduleTasksAndDeliverysDayReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultScheduleTasksAndDeliverysWeekReport, F["DefaultTimeScheduleTasksAndDeliverysWeekReport"] != null ? Int32.Parse(F["DefaultTimeScheduleTasksAndDeliverysWeekReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultTimeSalarySpecificationReport, F["DefaultTimeSalarySpecificationReport"] != null ? Int32.Parse(F["DefaultTimeSalarySpecificationReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultTimeSalaryControlInfoReport, F["DefaultTimeSalaryControlInfoReport"] != null ? Int32.Parse(F["DefaultTimeSalaryControlInfoReport"]) : 0);
            intValues.Add((int)CompanySettingType.TimeDefaultKU10Report, F["DefaultTimeKU10Report"] != null ? Int32.Parse(F["DefaultTimeKU10Report"]) : 0);
            intValues.Add((int)CompanySettingType.PayrollSettingsDefaultReport, F["DefaultTimeSalarySettingReport"] != null ? Int32.Parse(F["DefaultTimeSalarySettingReport"]) : 0);
            intValues.Add((int)CompanySettingType.DefaultPayrollSlipReport, F["DefaultXEPayrollSlipReport"] != null ? Int32.Parse(F["DefaultXEPayrollSlipReport"]) : 0);
            intValues.Add((int)CompanySettingType.DefaultEmployeeVacationDebtReport, F["DefaultEmployeeVacationDebtReport"] != null ? Int32.Parse(F["DefaultEmployeeVacationDebtReport"]) : 0);
            intValues.Add((int)CompanySettingType.DefaultEmploymentContractShortSubstituteReport, F["DefaultEmploymentContractShortSubstituteReport"] != null ? Int32.Parse(F["DefaultEmploymentContractShortSubstituteReport"]) : 0);

            //AttestStatus
            intValues.Add((int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, F["ExportSalaryMinimumAttestStatus"] != null ? Int32.Parse(F["ExportSalaryMinimumAttestStatus"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, F["ExportSalaryResultingAttestStatus"] != null ? Int32.Parse(F["ExportSalaryResultingAttestStatus"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, F["ExportInvoiceMinimumAttestStatus"] != null ? Int32.Parse(F["ExportInvoiceMinimumAttestStatus"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryExportInvoiceResultingAttestStatus, F["ExportInvoiceResultingAttestStatus"] != null ? Int32.Parse(F["ExportInvoiceResultingAttestStatus"]) : 0);
            intValues.Add((int)CompanySettingType.MobileTimeAttestResultingAttestStatus, F["MobileTimeAttestResultingAttestStatus"] != null ? Int32.Parse(F["MobileTimeAttestResultingAttestStatus"]) : 0);

            // Stamping
            boolValues.Add((int)CompanySettingType.TimeIgnoreOfflineTerminals, StringUtility.GetBool(F["IgnoreOfflineTerminals"]));
            boolValues.Add((int)CompanySettingType.TimeDoNotModifyTimeStampEntryType, StringUtility.GetBool(F["TimeDoNotModifyTimeStampEntryType"]));
            boolValues.Add((int)CompanySettingType.UseTimeScheduleTypeFromTime, StringUtility.GetBool(F["UseTimeScheduleTypeFromTime"]));
            boolValues.Add((int)CompanySettingType.LimitAttendanceViewToStampedTerminal, StringUtility.GetBool(F["LimitAttendanceViewToStampedTerminal"]));
            boolValues.Add((int)CompanySettingType.PossibilityToRegisterAdditionsInTerminal, StringUtility.GetBool(F["PossibilityToRegisterAdditionsInTerminal"]));

            #endregion

            #region Tab Staffing

            // Staffing
            if (StringUtility.GetBool(F["UseStaffing"]))
            {
                //Verify that Staffing hidden Employe exists before setting is saved
                if (em.AddHiddenEmployee(SoeCompany.ActorCompanyId).Success)
                    boolValues.Add((int)CompanySettingType.TimeUseStaffing, true);
                else
                    boolValues.Add((int)CompanySettingType.TimeUseStaffing, false);
            }
            else
                boolValues.Add((int)CompanySettingType.TimeUseStaffing, false);
            boolValues.Add((int)CompanySettingType.TimeUseVacant, StringUtility.GetBool(F["UseVacant"]));
            boolValues.Add((int)CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment, StringUtility.GetBool(F["OrderPlanningIgnoreScheduledBreaksOnAssignment"]));
            intValues.Add((int)CompanySettingType.TimeStaffingShiftAccountDimId, F["StaffingShiftAccountDimId"] != null ? Int32.Parse(F["StaffingShiftAccountDimId"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningInactivateLending, StringUtility.GetBool(F["TimeSchedulePlanningInactivateLending"]));
            boolValues.Add((int)CompanySettingType.IncludeSecondaryEmploymentInWorkTimeWeek, StringUtility.GetBool(F["IncludeSecondaryEmploymentInWorkTimeWeek"]));

            // SchedulePlanning
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningClockRounding, StringUtility.GetInt(F["TimeSchedulePlanningClockRounding"], 0));
            boolValues.Add((int)CompanySettingType.TimeDefaultDoNotKeepShiftsTogether, StringUtility.GetBool(F["TimeDefaultDoNotKeepShiftsTogether"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningSendXEMailOnChange, StringUtility.GetBool(F["TimeSchedulePlanningSendXEMailOnChange"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningSetShiftAsExtra, StringUtility.GetBool(F["TimeSchedulePlanningSetShiftAsExtra"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute, StringUtility.GetBool(F["TimeSchedulePlanningSetShiftAsSubstitute"]));
            boolValues.Add((int)CompanySettingType.HideRecipientsInShiftRequest, StringUtility.GetBool(F["HideRecipientsInShiftRequest"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningSortQueueByLas, StringUtility.GetBool(F["TimeSchedulePlanningSortQueueByLas"]));
            boolValues.Add((int)CompanySettingType.CreateEmployeeRequestWhenDeniedWantedShift, StringUtility.GetBool(F["CreateEmployeeRequestWhenDeniedWantedShift"]));
            boolValues.Add((int)CompanySettingType.ShowTemplateScheduleForEmployeesInApp, StringUtility.GetBool(F["ShowTemplateScheduleForEmployeesInApp"]));
            boolValues.Add((int)CompanySettingType.UseMultipleScheduleTypes, StringUtility.GetBool(F["UseMultipleScheduleTypes"]));
            boolValues.Add((int)CompanySettingType.SubstituteShiftIsAssignedDueToAbsenceOnlyIfSameBatch, StringUtility.GetBool(F["SubstituteShiftIsAssignedDueToAbsenceOnlyIfSameBatch"]));
            boolValues.Add((int)CompanySettingType.SubstituteShiftDontIncludeCopiedOrMovedShifts, StringUtility.GetBool(F["SubstituteShiftDontIncludeCopiedOrMovedShifts"]));
            boolValues.Add((int)CompanySettingType.ExtraShiftAsDefaultOnHidden, StringUtility.GetBool(F["ExtraShiftAsDefaultOnHidden"]));
            boolValues.Add((int)CompanySettingType.PrintAgreementOnAssignFromFreeShift, StringUtility.GetBool(F["PrintAgreementOnAssignFromFreeShift"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningDragDropMoveAsDefault, StringUtility.GetBool(F["TimeSchedulePlanningDragDropMoveAsDefault"]));
            boolValues.Add((int)CompanySettingType.UseLeisureCodes, StringUtility.GetBool(F["UseLeisureCodes"]));
            boolValues.Add((int)CompanySettingType.UseAnnualLeave, StringUtility.GetBool(F["UseAnnualLeave"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningSaveCopyOnPublish, StringUtility.GetBool(F["TimeSchedulePlanningSaveCopyOnPublish"]));

            //Scheduleplanning - Workrules
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningSkipWorkRules, StringUtility.GetBool(F["TimeSchedulePlanningSkipWorkRules"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningUseWorkRulesForMinors, StringUtility.GetBool(F["TimeSchedulePlanningUseWorkRulesForMinors"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningOverrideWorkRuleWarningsForMinors, StringUtility.GetBool(F["TimeSchedulePlanningOverrideWorkRuleWarningsForMinors"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningRuleRestTimeDayMandatory, StringUtility.GetBool(F["TimeSchedulePlanningRuleRestTimeDayMandatory"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningRuleRestTimeWeekMandatory, StringUtility.GetBool(F["TimeSchedulePlanningRuleRestTimeWeekMandatory"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningRuleWorkTimeWeekDontEvaluateInSchedule, StringUtility.GetBool(F["TimeSchedulePlanningRuleRuleWorkTimeWeekDontEvaluateInSchedule"]));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningUseRuleWorkTimeWeekForParttimeWorkersInSchedule, StringUtility.GetBool(F["TimeSchedulePlanningUseRuleWorkTimeWeekForParttimeWorkersInSchedule"]));
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift, StringUtility.GetInt(F["TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift"], 0));
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly, StringUtility.GetBool(F["TimeSchedulePlanningShiftRequestPreventTooEarly"]));
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore, StringUtility.GetInt(F["TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore"], 0));
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore, StringUtility.GetInt(F["TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore"], 0));

            //SchedulePlanning - SchedulePlanningCalendarView
            boolValues.Add((int)CompanySettingType.TimeSchedulePlanningCalendarViewShowDaySummary, StringUtility.GetBool(F["TimeSchedulePlanningCalendarViewShowDaySummary"]));

            //SchedulePlanning - TimeSchedulePlanningDayView
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningDayViewStartTime, CalendarUtility.GetMinutes(F["TimeSchedulePlanningDayViewStartTime"]));
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningDayViewEndTime, CalendarUtility.GetMinutes(F["TimeSchedulePlanningDayViewEndTime"]));
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength, StringUtility.GetInt(F["TimeSchedulePlanningDayViewMinorTickLength"], 0));
            intValues.Add((int)CompanySettingType.TimeSchedulePlanningBreakVisibility, F["TimeSchedulePlanningBreakVisibility"] != null ? Int32.Parse(F["TimeSchedulePlanningBreakVisibility"]) : 0);

            //SchedulePlanning - EditShift
            boolValues.Add((int)CompanySettingType.TimeEditShiftShowEmployeeInGridView, StringUtility.GetBool(F["TimeEditShiftShowEmployeeInGridView"]));
            boolValues.Add((int)CompanySettingType.TimeEditShiftShowDateInGridView, StringUtility.GetBool(F["TimeEditShiftShowDateInGridView"]));
            boolValues.Add((int)CompanySettingType.TimeShiftTypeMandatory, StringUtility.GetBool(F["TimeShiftTypeMandatory"]));
            boolValues.Add((int)CompanySettingType.TimeEditShiftAllowHoles, StringUtility.GetBool(F["TimeEditShiftAllowHoles"]));

            //SchedulePlanning - Costs
            boolValues.Add((int)CompanySettingType.StaffingUseTemplateCost, StringUtility.GetBool(F["StaffingUseTemplateCost"]));
            decimalValues.Add((int)CompanySettingType.StaffingTemplateCost, StringUtility.GetDecimal(F["StaffingTemplateCost"]));

            //SchedulePlanning - Availability
            intValues.Add((int)CompanySettingType.TimeAvailabilityLockDaysBefore, F["TimeAvailabilityLockDaysBefore"] != null ? Int32.Parse(F["TimeAvailabilityLockDaysBefore"]) : 0);

            //SchedulePlanning - Contact information
            Collection<FormIntervalEntryItem> addressTypeItems = PlanningContactInformationAddressTypes.GetData(F);
            string addressTypesString = string.Empty;
            foreach (var item in addressTypeItems)
            {
                if (item.From != "0")
                {
                    if (!addressTypesString.IsNullOrEmpty())
                        addressTypesString += ",";
                    addressTypesString += item.From;
                }
            }
            stringValues.Add((int)CompanySettingType.TimeSchedulePlanningContactAddressTypes, addressTypesString);

            Collection<FormIntervalEntryItem> ecomTypeItems = PlanningContactInformationEComTypes.GetData(F);
            string ecomTypesString = string.Empty;
            foreach (var item in ecomTypeItems)
            {
                if (item.From != "0")
                {
                    if (!ecomTypesString.IsNullOrEmpty())
                        ecomTypesString += ",";
                    ecomTypesString += item.From;
                }
            }
            stringValues.Add((int)CompanySettingType.TimeSchedulePlanningContactEComTypes, ecomTypesString);

            //Minors
            intValues.Add((int)CompanySettingType.MinorsSchoolDayStartMinutes, CalendarUtility.GetMinutes(F["MinorsSchoolDayStartMinutes"]));
            intValues.Add((int)CompanySettingType.MinorsSchoolDayStopMinutes, CalendarUtility.GetMinutes(F["MinorsSchoolDayStopMinutes"]));

            // Planning periods
            if (base.IsSupportAdmin)
                boolValues.Add((int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTime, StringUtility.GetBool(F["CalculatePlanningPeriodScheduledTime"]));
            boolValues.Add((int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeIncludeExtraShift, StringUtility.GetBool(F["CalculatePlanningPeriodScheduledTimeIncludeExtraShift"]));
            boolValues.Add((int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod, StringUtility.GetBool(F["CalculatePlanningPeriodScheduledTimeUseAveragingPeriod"]));

            string planningPeriodScheduledTimeColors = string.Empty;
            planningPeriodScheduledTimeColors += F["PlanningPeriodColorOver"].Trim() == this.planningPeriodDefaultColor1 ? string.Empty : F["PlanningPeriodColorOver"].Trim();
            planningPeriodScheduledTimeColors += ";";
            planningPeriodScheduledTimeColors += F["PlanningPeriodColorEqual"].Trim() == this.planningPeriodDefaultColor2 ? string.Empty : F["PlanningPeriodColorEqual"].Trim();
            planningPeriodScheduledTimeColors += ";";
            planningPeriodScheduledTimeColors += F["PlanningPeriodColorUnder"].Trim() == this.planningPeriodDefaultColor3 ? string.Empty : F["PlanningPeriodColorUnder"].Trim();
            stringValues.Add((int)CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeColors, planningPeriodScheduledTimeColors == ";;" ? string.Empty : planningPeriodScheduledTimeColors);

            // Dashboard
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold1, StringUtility.GetDecimal(F["GaugeSalesThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold2, StringUtility.GetDecimal(F["GaugeSalesThreshold2"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold1, StringUtility.GetDecimal(F["GaugeHoursThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold2, StringUtility.GetDecimal(F["GaugeHoursThreshold2"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold1, StringUtility.GetDecimal(F["GaugeSalaryCostThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold2, StringUtility.GetDecimal(F["GaugeSalaryCostThreshold2"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold1, StringUtility.GetDecimal(F["GaugeSalaryPercentThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold2, StringUtility.GetDecimal(F["GaugeSalaryPercentThreshold2"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold1, StringUtility.GetDecimal(F["GaugeLPATThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold2, StringUtility.GetDecimal(F["GaugeLPATThreshold2"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold1, StringUtility.GetDecimal(F["GaugeFPATThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold2, StringUtility.GetDecimal(F["GaugeFPATThreshold2"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold1, StringUtility.GetDecimal(F["GaugeBPATThreshold1"]));
            decimalValues.Add((int)CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold2, StringUtility.GetDecimal(F["GaugeBPATThreshold2"]));

            // Skills
            intValues.Add((int)CompanySettingType.TimeNbrOfSkillLevels, F["TimeNbrOfSkillLevels"] != null ? Int32.Parse(F["TimeNbrOfSkillLevels"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeSkillLevelHalfPrecision, StringUtility.GetBool(F["TimeSkillLevelHalfPrecision"]));
            boolValues.Add((int)CompanySettingType.TimeSkillCantBeOverridden, StringUtility.GetBool(F["TimeSkillCantBeOverridden"]));

            // Staffing needs
            intValues.Add((int)CompanySettingType.TimeStaffingNeedsAnalysisChartType, F["StaffingNeedsChartType"] != null ? Int32.Parse(F["StaffingNeedsChartType"]) : 1);
            intValues.Add((int)CompanySettingType.StaffingNeedsFrequencyAccountDim, F["StaffingNeedsFrequencyAccountDim"] != null ? Int32.Parse(F["StaffingNeedsFrequencyAccountDim"]) : 0);
            intValues.Add((int)CompanySettingType.StaffingNeedsFrequencyParentAccountDim, F["StaffingNeedsFrequencyParentAccountDim"] != null ? Int32.Parse(F["StaffingNeedsFrequencyParentAccountDim"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeStaffingNeedsAnalysisRatioSalesPerScheduledHour, StringUtility.GetBool(F["StaffingNeedsRatioSalesPerScheduledHour"]));
            boolValues.Add((int)CompanySettingType.TimeStaffingNeedsAnalysisRatioSalesPerWorkHour, StringUtility.GetBool(F["StaffingNeedsRatioSalesPerWorkHour"]));
            boolValues.Add((int)CompanySettingType.TimeStaffingNeedsAnalysisRatioFrequencyAverage, StringUtility.GetBool(F["StaffingNeedsRatioFrequencyAverage"]));
            intValues.Add((int)CompanySettingType.TimeStaffingNeedsWorkingPeriodMaxLength, CalendarUtility.GetMinutes(F["StaffingNeedsWorkingPeriodMaxLength"]));
            decimalValues.Add((int)CompanySettingType.StaffingNeedRoundUp, StringUtility.GetDecimal(F["StaffingNeedRoundUp"]));
            stringValues.Add((int)CompanySettingType.TimeEmployeePostPrefix, F["EmployeePostPrefix"] != null ? F["EmployeePostPrefix"] : String.Empty);

            // Absence
            boolValues.Add((int)CompanySettingType.RemoveScheduleTypeOnAbsence, StringUtility.GetBool(F["RemoveScheduleTypeOnAbsence"]));
            
            #endregion

            #region Accounting

            // Accounting payroll
            stringValues.Add((int)CompanySettingType.TimeCompanyPayrollProductAccountingPrio, String.Format("{0},{1},{2},{3},{4}", F["PayrollProductAccountingPrio1"], F["PayrollProductAccountingPrio2"], F["PayrollProductAccountingPrio3"], F["PayrollProductAccountingPrio4"], F["PayrollProductAccountingPrio5"]));

            // Accounting invoice
            stringValues.Add((int)CompanySettingType.TimeCompanyInvoiceProductAccountingPrio, String.Format("{0},{1},{2},{3},{4}", F["InvoiceProductAccountingPrio1"], F["InvoiceProductAccountingPrio2"], F["InvoiceProductAccountingPrio3"], F["InvoiceProductAccountingPrio4"], F["InvoiceProductAccountingPrio5"]));

            #region Vacation

            intValues.Add((int)CompanySettingType.VacationValueDaysCreditAccountId, F["VacationValueDaysCreditAccountId"] != null ? Int32.Parse(F["VacationValueDaysCreditAccountId"]) : 0);
            intValues.Add((int)CompanySettingType.VacationValueDaysDebitAccountId, F["VacationValueDaysDebitAccountId"] != null ? Int32.Parse(F["VacationValueDaysDebitAccountId"]) : 0);

            #endregion

            #region Calculations

            boolValues.Add((int)CompanySettingType.RecalculateFutureAccountingWhenChangingMainAllocation, StringUtility.GetBool(F["RecalculateFutureAccountingWhenChangingMainAllocation"]));
            
            #endregion

            #endregion

            #region Tab AutoAttest

            // Autoattest
            boolValues.Add((int)CompanySettingType.TimeAutoAttestRunService, StringUtility.GetBool(F["TimeAutoAttestRunService"]));
            intValues.Add((int)CompanySettingType.TimeAutoAttestSourceAttestStateId, F["TimeAutoAttestSourceAttestStateId"] != null ? Int32.Parse(F["TimeAutoAttestSourceAttestStateId"]) : 0);
            intValues.Add((int)CompanySettingType.TimeAutoAttestSourceAttestStateId2, F["TimeAutoAttestSourceAttestStateId2"] != null ? Int32.Parse(F["TimeAutoAttestSourceAttestStateId2"]) : 0);
            intValues.Add((int)CompanySettingType.TimeAutoAttestTargetAttestStateId, F["TimeAutoAttestTargetAttestStateId"] != null ? Int32.Parse(F["TimeAutoAttestTargetAttestStateId"]) : 0);
            boolValues.Add((int)CompanySettingType.TimeAutoAttestEmployeeManuallyAdjustedTimeStamps, StringUtility.GetBool(F["TimeAutoAttestEmployeeManuallyAdjustedTimeStamps"]));

            #endregion

            #region Tab Payroll

            // Payroll
            if (base.IsSupportAdmin)
            {
                var usePayroll = StringUtility.GetBool(F["UsePayroll"]);
                boolValues.Add((int)CompanySettingType.UsePayroll, usePayroll);
                dateTimeValues.Add((int)CompanySettingType.UsedPayrollSince, CalendarUtility.GetDateTime(F["UsedPayrollSince"]));
                dateTimeValues.Add((int)CompanySettingType.CalculateExperienceFrom, CalendarUtility.GetDateTime(F["CalculateExperienceFrom"]));
            }
            else
            {
                dateTimeValues.Add((int)CompanySettingType.CalculateExperienceFrom, CalendarUtility.GetDateTime(F["CalculateExperienceFrom"]));
            }

            // Approve absence
            boolValues.Add((int)CompanySettingType.TimeSetApprovedYesAsDefault, StringUtility.GetBool(F["SetApprovedYesAsDefault"]));
            boolValues.Add((int)CompanySettingType.TimeOnlyNoReplacementIsSelectable, StringUtility.GetBool(F["OnlyNoReplacementIsElectable"]));
            boolValues.Add((int)CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages, StringUtility.GetBool(F["IncludeNoteInMessages"]));
            boolValues.Add((int)CompanySettingType.ValidateVacationWholeDayWhenSaving, StringUtility.GetBool(F["ValidateVacationWholeDay"]));

            // PayrollAgreement
            boolValues.Add((int)CompanySettingType.PayrollGroupMandatory, StringUtility.GetBool(F["PayrollGroupMandatory"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUseOverTimeCompensation, StringUtility.GetBool(F["PayrollAgreementUseOvertimeCompensation"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUseException2to6InWorkingAgreement, StringUtility.GetBool(F["PayrollAgreementUseExeption2to6"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUseTravelCompansation, StringUtility.GetBool(F["PayrollAgreementUseTravelCompensation"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUseWorkTimeShiftCompensation, StringUtility.GetBool(F["PayrollAgreementUseWorkTimeShiftCompensation"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUseVacationRights, StringUtility.GetBool(F["PayrollAgreementUseVacationRightsDays"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing, StringUtility.GetBool(F["PayrollAgreementUseGrossNetTimeInStaffing"]));
            boolValues.Add((int)CompanySettingType.PayrollAgreementUsePayrollTax, StringUtility.GetBool(F["PayrollAgreementUsePayrollTax"]));

            // EmploymentType
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_Probationary, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_Probationary"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_Substitute, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_Substitute"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_SubstituteVacation, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_SubstituteVacation"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_Permanent, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_Permanent"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_FixedTerm, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_FixedTerm"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_SpecialFixedTerm, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_SpecialFixedTerm"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_Seasonal, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_Seasonal"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_SpecificWork, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_SpecificWork"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_Trainee, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_Trainee"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_NormalRetirementAge, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_NormalRetirementAge"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_CallContract, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_CallContract"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_FixedTerm14days, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_FixedTerm14days"]));
            boolValues.Add((int)CompanySettingType.PayrollEmploymentTypeUse_SE_Apprentice, StringUtility.GetBool(F["PayrollEmploymentTypeUse_SE_Apprentice"]));

            //AttestState
            intValues.Add((int)CompanySettingType.SalaryPaymentLockedAttestStateId, F["PayrollCalculationLockedStatus"] != null ? Int32.Parse(F["PayrollCalculationLockedStatus"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryPaymentApproved1AttestStateId, F["PayrollCalculationApproved1Status"] != null ? Int32.Parse(F["PayrollCalculationApproved1Status"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryPaymentApproved2AttestStateId, F["PayrollCalculationApproved2Status"] != null ? Int32.Parse(F["PayrollCalculationApproved2Status"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, F["PayrollCalculationPaymentFileCreated"] != null ? Int32.Parse(F["PayrollCalculationPaymentFileCreated"]) : 0);

            //SalaryExport
            intValues.Add((int)CompanySettingType.SalaryExportTarget, F["ExportTarget"] != null ? Int32.Parse(F["ExportTarget"]) : 0);
            stringValues.Add((int)CompanySettingType.SalaryExportExternalExportID, F["ExternalExportID"] != null ? F["ExternalExportID"] : String.Empty);
            stringValues.Add((int)CompanySettingType.SalaryExportExternalExportSubId, F["ExternalExportSubId"] != null ? F["ExternalExportSubId"] : String.Empty);
            stringValues.Add((int)CompanySettingType.SalaryExportEmail, F["SalaryExportEmail"] != null ? F["SalaryExportEmail"] : String.Empty);
            stringValues.Add((int)CompanySettingType.SalaryExportEmailCopy, F["SalaryExportEmailCopy"] != null ? F["SalaryExportEmailCopy"] : String.Empty);
            boolValues.Add((int)CompanySettingType.SalaryExportNoComments, StringUtility.GetBool(F["SalaryExportNoComments"]));
            intValues.Add((int)CompanySettingType.SalaryExportVatProductId, F["ExportVatProductId"] != null ? Int32.Parse(F["ExportVatProductId"]) : 0);
            boolValues.Add((int)CompanySettingType.SalaryExportLockPeriod, StringUtility.GetBool(F["SalaryExportLockPeriod"]));
            boolValues.Add((int)CompanySettingType.SalaryExportAllowPreliminary, StringUtility.GetBool(F["SalaryExportAllowPreliminary"]));

            //SalaryPaymentExport
            intValues.Add((int)CompanySettingType.SalaryPaymentExportType, F["SalaryPaymentExportType"] != null ? Int32.Parse(F["SalaryPaymentExportType"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryExportUseSocSecFormat, F["SalaryExportUseSocSecFormat"] != null ? Int32.Parse(F["SalaryExportUseSocSecFormat"]) : 0);
            stringValues.Add((int)CompanySettingType.SalaryPaymentExportSenderIdentification, F["SalaryPaymentExportSenderIdentification"] != null ? F["SalaryPaymentExportSenderIdentification"] : String.Empty);
            stringValues.Add((int)CompanySettingType.SalaryPaymentExportSenderBankGiro, F["SalaryPaymentExportSenderBankGiro"] != null ? F["SalaryPaymentExportSenderBankGiro"] : String.Empty);
            boolValues.Add((int)CompanySettingType.SalaryPaymentExportCompanyIsRegisterHolder, StringUtility.GetBool(F["SalaryPaymentExportCompanyIsRegisterHolder"]));
            //intValues.Add((int)CompanySettingType.SalaryPaymentExportBank, F["SalaryPaymentExportBank"] != null ? Int32.Parse(F["SalaryPaymentExportBank"]) : 0);
            intValues.Add((int)CompanySettingType.SalaryPaymentExportPaymentAccount, F["SalaryPaymentExportPaymentAccount"] != null ? Int32.Parse(F["SalaryPaymentExportPaymentAccount"]) : 0);
            //boolValues.Add((int)CompanySettingType.SalaryPaymentExportUseAccountNrAsBBAN, StringUtility.GetBool(F["SalaryPaymentExportUseAccountNrAsBBAN"]));
            boolValues.Add((int)CompanySettingType.SalaryPaymentExportUsePaymentDateAsExecutionDate, StringUtility.GetBool(F["SalaryPaymentExportUsePaymentDateAsExecutionDate"]));
            boolValues.Add((int)CompanySettingType.SalaryPaymentExportUseIBANOnEmployee, StringUtility.GetBool(F["SalaryPaymentExportUseIBANOnEmployee"]));
            stringValues.Add((int)CompanySettingType.SalaryPaymentExportAgreementNumber, F["SalaryPaymentExportAgreementNumber"] != null ? F["SalaryPaymentExportAgreementNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.SalaryPaymentExportDivisionName, F["SalaryPaymentExportDivisionName"] != null ? F["SalaryPaymentExportDivisionName"] : String.Empty);
            boolValues.Add((int)CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK, StringUtility.GetBool(F["SalaryPaymentExportUseExtendedCurrencyNOK"]));
            stringValues.Add((int)CompanySettingType.SalaryPaymentExportExtendedSenderIdentification, F["SalaryPaymentExportExtendedSenderIdentification"] != null ? F["SalaryPaymentExportExtendedSenderIdentification"] : String.Empty);
            stringValues.Add((int)CompanySettingType.SalaryPaymentExportExtendedAgreementNumber, F["SalaryPaymentExportExtendedAgreementNumber"] != null ? F["SalaryPaymentExportExtendedAgreementNumber"] : String.Empty);
            intValues.Add((int)CompanySettingType.SalaryPaymentExportExtendedPaymentAccount, F["SalaryPaymentExportExtendedPaymentAccount"] != null ? Int32.Parse(F["SalaryPaymentExportExtendedPaymentAccount"]) : 0);

            //Support area
            decimalValues.Add((int)CompanySettingType.PayrollMaxRegionalSupportAmount, StringUtility.GetDecimal(F["PayrollMaxRegionalSupportAmount"]));
            decimalValues.Add((int)CompanySettingType.PayrollMaxRegionalSupportPercent, StringUtility.GetDecimal(F["PayrollMaxRegionalSupportPercent"]));
            intValues.Add((int)CompanySettingType.AccountPayrollMaxRegionalSupportDim1, GetAccountId(F["PayrollMaxRegionalSupportAccountDim1"]));
            intValues.Add((int)CompanySettingType.AccountPayrollMaxRegionalSupportDim2, StringUtility.GetInt(F["PayrollMaxRegionalSupportAccountDim2"]));
            intValues.Add((int)CompanySettingType.AccountPayrollMaxRegionalSupportDim3, StringUtility.GetInt(F["PayrollMaxRegionalSupportAccountDim3"]));
            intValues.Add((int)CompanySettingType.AccountPayrollMaxRegionalSupportDim4, StringUtility.GetInt(F["PayrollMaxRegionalSupportAccountDim4"]));
            intValues.Add((int)CompanySettingType.AccountPayrollMaxRegionalSupportDim5, StringUtility.GetInt(F["PayrollMaxRegionalSupportAccountDim5"]));
            intValues.Add((int)CompanySettingType.AccountPayrollMaxRegionalSupportDim6, StringUtility.GetInt(F["PayrollMaxRegionalSupportAccountDim6"]));
            decimalValues.Add((int)CompanySettingType.PayrollMaxResearchSupportAmount, StringUtility.GetDecimal(F["PayrollMaxResearchSupportAmount"]));

            //Pension
            stringValues.Add((int)CompanySettingType.PayrollExportForaAgreementNumber, F["ForaAgreementNumber"] != null ? F["ForaAgreementNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportITP1Number, F["ITP1Number"] != null ? F["ITP1Number"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportITP2Number, F["ITP2Number"] != null ? F["ITP2Number"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportKPAAgreementNumber, F["KPAAgreementNumber"] != null ? F["KPAAgreementNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportKPAManagementNumber, F["KPAManagementNumber"] != null ? F["KPAManagementNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportSkandiaSortingConcept, F["SkandiaSortingConcept"] != null ? F["SkandiaSortingConcept"] : String.Empty);

            //Pension - SN/KFO
            stringValues.Add((int)CompanySettingType.PayrollExportSNKFOMemberNumber, F["SNKFOMemberNumber"] != null ? F["SNKFOMemberNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber, F["SNKFOWorkPlaceNumber"] != null ? F["SNKFOWorkPlaceNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportSNKFOAffiliateNumber, F["SNKFOAffiliateNumber"] != null ? F["SNKFOAffiliateNumber"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportSNKFOAgreementNumber, F["SNKFOAgreementNumber"] != null ? F["SNKFOAgreementNumber"] : String.Empty);

            //Pension - Community
            stringValues.Add((int)CompanySettingType.PayrollExportCommunityCode, F["CommunityCode"] != null ? F["CommunityCode"] : String.Empty);

            //Pension - SCB
            stringValues.Add((int)CompanySettingType.PayrollExportSCBWorkSite, F["SCBWorkSite"] != null ? F["SCBWorkSite"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportCFARNumber, F["CFARNumber"] != null ? F["CFARNumber"] : String.Empty);

            //Arbetsgivarintyg
            stringValues.Add((int)CompanySettingType.PayrollArbetsgivarintygnuApiNyckel, F["Apinyckel"] != null ? F["Apinyckel"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollArbetsgivarintygnuArbetsgivarId, F["ArbetsgivarId"] != null ? F["ArbetsgivarId"] : String.Empty);

            // Folksam
            intValues.Add((int)CompanySettingType.PayrollExportFolksamCustomerNumber, F["FolksamCustomerNumber"] != null ? Int32.Parse(F["FolksamCustomerNumber"]) : 0);

            //Skatteverket
            stringValues.Add((int)CompanySettingType.PayrollExportPlaceOfEmploymentAddress, F["PlaceOfEmploymentAddress"] != null ? F["PlaceOfEmploymentAddress"] : String.Empty);
            stringValues.Add((int)CompanySettingType.PayrollExportPlaceOfEmploymentCity, F["PlaceOfEmploymentCity"] != null ? F["PlaceOfEmploymentCity"] : String.Empty);

            //Provision
            intValues.Add((int)CompanySettingType.PayrollAccountProvisionTimeCode, F["AccountProvisionTimeCode"] != null ? Int32.Parse(F["AccountProvisionTimeCode"]) : 0);
            intValues.Add((int)CompanySettingType.PayrollAccountProvisionAccountDim, F["AccountProvisionAccountDim"] != null ? Int32.Parse(F["AccountProvisionAccountDim"]) : 0);

            //Misc settings
            intValues.Add((int)CompanySettingType.PayrollAccountingDistributionPayrollProduct, F["AccountingDistributionPayrollProduct"] != null ? Int32.Parse(F["AccountingDistributionPayrollProduct"]) : 0);
            boolValues.Add((int)CompanySettingType.PublishPayrollSlipWhenLockingPeriod, StringUtility.GetBool(F["PublishPayrollSlipWhenLockingPeriod"]));
            boolValues.Add((int)CompanySettingType.SendNoticeWhenPayrollSlipPublished, StringUtility.GetBool(F["SendNoticeWhenPayrollSlipPublished"]));
            intValues.Add((int)CompanySettingType.PayrollGrossalaryRoundingPayrollProduct, F["GrossalaryRoundingPayrollProduct"] != null ? Int32.Parse(F["GrossalaryRoundingPayrollProduct"]) : 0);

            #endregion

            if (!sm.UpdateInsertBoolSettings(SettingMainType.Company, boolValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;
            if (!sm.UpdateInsertIntSettings(SettingMainType.Company, intValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;
            if (!sm.UpdateInsertStringSettings(SettingMainType.Company, stringValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;
            if (!sm.UpdateInsertDecimalSettings(SettingMainType.Company, decimalValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;
            if (!sm.UpdateInsertDateSettings(SettingMainType.Company, dateTimeValues, UserId, SoeCompany.ActorCompanyId, 0).Success)
                success = false;
            if (success)
                RedirectToSelf("UPDATED");
            RedirectToSelf("NOTUPDATED", true);
        }

        #endregion

        #region Help-methods

        private int GetAccountId(string accountNr)
        {
            // No account entered
            if (String.IsNullOrEmpty(accountNr))
                return 0;

            // Get account by specified number
            AccountStd acc = am.GetAccountStdByNr(accountNr, SoeCompany.ActorCompanyId);

            // Invalid account number
            if (acc == null)
                return 0;

            return acc.AccountId;
        }

        #endregion
    }
}
