using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.time.employee.groups.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private AttestManager am;
        private AccountManager acm;
        private EmployeeManager em;
        private CalendarManager cm;
        private SettingManager sm;
        private TimeAccumulatorManager tam;
        private TimeDeviationCauseManager tdcm;
        private TimePeriodManager tpm;
        private TimeCodeManager tcm;
        private TimeStampManager tsm;

        protected EmployeeGroup employeeGroup;
        private TimeStampRounding timeStampRounding = null;

        //Accounts
        private IEnumerable<AccountDim> accountDims;
        private IEnumerable<EmployeeGroupAccountStd> employeeGroupAccounts;
        private EmployeeGroupAccountStd costAccount;
        private EmployeeGroupAccountStd incomeAccount;
        private Dictionary<int, int> costAccountDict;
        private Dictionary<int, int> incomeAccountDict;
        private string defaultCostAccountNr;
        private string defaultIncomeAccountNr;

        //Dictionarys
        public Dictionary<int, string> timePeriodTypesDict;
        public Dictionary<int, int> selectedDayTypesDict;
        public Dictionary<int, int> selectedHolidaySalaryDayTypesDict;
        public Dictionary<int, int> selectedTimeAccumulatorsDict;
        public Dictionary<int, int> selectedTimeDeviationCausesDict;
        public Dictionary<int, int> selectedTimeDeviationCauseRequestsDict;
        public Dictionary<int, int> selectedTimeDeviationCauseAbsenceAnnouncementsDict;
        public Dictionary<int, int> selectedTimeCodesDict;

        public int AccountDimId { get; set; }
        public string stdDimID; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Employee_Groups_Edit;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/cssjs/account.js");
            Scripts.Add("default.js");
            Scripts.Add("texts.js.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AttestManager(ParameterObject);
            acm = new AccountManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            em = new EmployeeManager(ParameterObject);
            cm = new CalendarManager(ParameterObject);
            tpm = new TimePeriodManager(ParameterObject);
            tdcm = new TimeDeviationCauseManager(ParameterObject);
            tcm = new TimeCodeManager(ParameterObject);
            tsm = new TimeStampManager(ParameterObject);
            tam = new TimeAccumulatorManager(ParameterObject);
            
            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            if (Int32.TryParse(QS["group"], out int employeeGroupId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    employeeGroup = em.GetPrevNextEmployeeGroup(employeeGroupId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (employeeGroup != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?group=" + employeeGroup.EmployeeGroupId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?group=" + employeeGroupId);
                }
                else
                {
                    employeeGroup = em.GetEmployeeGroup(employeeGroupId, false, true, true, false, true, true);
                    if (employeeGroup == null)
                    {
                        Form1.MessageWarning = GetText(5039, "Tidavtal hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(5037, "Redigera tidavtal");
            string registerModeTabHeaderText = GetText(5038, "Registrera tidavtal");
            PostOptionalParameterCheck(Form1, employeeGroup, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = employeeGroup != null ? employeeGroup.Name : "";

            MinutesBeforeLabel.Text = GetText(3525, "Minuter före");
            MinutesAfterLabel.Text = GetText(3526, "Minuter efter");

            #endregion

            #region UserControls

            AttestTransitions.InitControl(Form1);
            TimeCodeTimeDeviationCauses.InitControl(Form1);
            EmployeeGroupAccumulatorSettings.InitEmployeeGroup(Form1, SoeCompany.ActorCompanyId, (employeeGroup == null ? 0 : employeeGroup.EmployeeGroupId));

            #endregion

            #region Actions

            //Needed in Save
            GetTimePeriodTypes();
            GetAccounts();

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(SoeCompany.ActorCompanyId);
            if (accountDimStd == null)
                return;

            AccountDimId = accountDimStd.AccountDimId;
            stdDimID = AccountDimId.ToString();

            Dictionary<int, string> dayTypesDict = cm.GetDayTypesByCompanyDict(SoeCompany.ActorCompanyId, true);
            Dictionary<int, string> dayTypesHolidaySalaryDict = cm.GetDayTypesByCompanyDict(SoeCompany.ActorCompanyId, true, onlyHolidaySalary: true);
            Dictionary<int, string> timeAccumulatorsDict = tam.GetTimeAccumulatorsDict(SoeCompany.ActorCompanyId, true);
            Dictionary<int, string> timeCodesDict = tcm.GetTimeCodesDict(SoeCompany.ActorCompanyId, true, false, false);
            Dictionary<int, string> weekDaysDict = CalendarUtility.GetDayOfWeekNames();
            Dictionary<int, string> timeDeviationCausesDict = tdcm.GetTimeDeviationCausesDict(SoeCompany.ActorCompanyId, true);
            Dictionary<int, string> absenceTimeDeviationCausesDict = tdcm.GetTimeDeviationCausesAbsenceDict(SoeCompany.ActorCompanyId, true);
            Dictionary<int, string> defaultTimeDeviationCausesDict = tdcm.GetTimeDeviationCausesDict(SoeCompany.ActorCompanyId, true);
            Dictionary<int, string> timeReportType = GetGrpText(TermGroup.TimeReportType);
            Dictionary<int, string> qualifyingDayCalculationRuleDict = GetGrpText(TermGroup.QualifyingDayCalculationRule, sortByValue: false);
            Dictionary<int, string> timeWorkReductionCalculationRuleDict = GetGrpText(TermGroup.TimeWorkReductionCalculationRule, sortByValue: false);

            if (!SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsExtra, 0, SoeCompany.ActorCompanyId, 0))
            {
                qualifyingDayCalculationRuleDict.Remove((int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusExtraShifts);
                timeWorkReductionCalculationRuleDict.Remove((int)TermGroup_TimeWorkReductionCalculationRule.UseWorkTimeWeekPlusExtraShifts);
            }                

            selectedDayTypesDict = new Dictionary<int, int>();
            selectedHolidaySalaryDayTypesDict = new Dictionary<int, int>();
            selectedTimeAccumulatorsDict = new Dictionary<int, int>();
            selectedTimeDeviationCausesDict = new Dictionary<int, int>();
            selectedTimeDeviationCauseRequestsDict = new Dictionary<int, int>();
            selectedTimeDeviationCauseAbsenceAnnouncementsDict = new Dictionary<int, int>();
            selectedTimeCodesDict = new Dictionary<int, int>();

            DayTypes.DataSourceFrom = dayTypesDict;
            DayTypesHolidaySalary.DataSourceFrom = dayTypesHolidaySalaryDict;
            TimeAccumulators.DataSourceFrom = timeAccumulatorsDict;
            TimeDeviationCauses.DataSourceFrom = timeDeviationCausesDict;
            TimeDeviationCauseRequests.DataSourceFrom = absenceTimeDeviationCausesDict;
            TimeDeviationCauseAbsenceAnnouncements.DataSourceFrom = absenceTimeDeviationCausesDict;
            EmployeeGroupTimeCodes.DataSourceFrom = timeCodesDict;
            DefaultTimeDeviationCause.ConnectDataSource(defaultTimeDeviationCausesDict);
            DefaultTimeCode.ConnectDataSource(timeCodesDict);
            RestTimeWeekStartDaySelectEntry.ConnectDataSource(weekDaysDict);
            TimeReportType.ConnectDataSource(timeReportType);

            QualifyingDayCalculationRule.ConnectDataSource(qualifyingDayCalculationRuleDict);
            TimeWorkReductionCalculationRule.ConnectDataSource(timeWorkReductionCalculationRuleDict);

            KeepStampsTogetherWithinMinutes.InfoText = GetText(3873, "Vid instämpling innan midnatt räknas utstämpling till samma dag,") + " " + GetText(3874, "om tiden mellan in- och utstämpling är mindre än angivet antal minuter.");

            if (Repopulate && PreviousForm != null)
            {
                DayTypes.PreviousForm = PreviousForm;
                DayTypesHolidaySalary.PreviousForm = PreviousForm;
                TimeAccumulators.PreviousForm = PreviousForm;
                TimeDeviationCauses.PreviousForm = PreviousForm;
                TimeDeviationCauseRequests.PreviousForm = PreviousForm;
                TimeDeviationCauseAbsenceAnnouncements.PreviousForm = PreviousForm;
                EmployeeGroupTimeCodes.PreviousForm = PreviousForm;
            }
            else
            {
                if (employeeGroup != null)
                {
                    #region DayType

                    if (employeeGroup.DayType != null && employeeGroup.DayType.Count > 0)
                    {
                        int pos = 0;
                        foreach (DayType dayType in employeeGroup.DayType)
                        {
                            DayTypes.AddLabelValue(pos, dayType.DayTypeId.ToString());
                            DayTypes.AddValueFrom(pos, dayType.Name);
                            selectedDayTypesDict.Add(pos, dayType.DayTypeId);
                            DayTypes.Value = dayType.DayTypeId.ToString();

                            pos++;
                            if (pos == DayTypes.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion

                    #region DayType (Only HolidaySalary)
                    List<EmployeeGroupDayType> employeeGroupHolidaySalaryDayTypes = em.GetEmployeeGroupDayTypes(employeeGroup.EmployeeGroupId);
                    if (employeeGroupHolidaySalaryDayTypes != null && employeeGroupHolidaySalaryDayTypes.Any())
                    {
                        int pos = 0;
                        foreach (EmployeeGroupDayType employeeGroupDayType in employeeGroupHolidaySalaryDayTypes)
                        {
                            DayTypesHolidaySalary.AddLabelValue(pos, employeeGroupDayType.DayType.DayTypeId.ToString());
                            DayTypesHolidaySalary.AddValueFrom(pos, employeeGroupDayType.DayType.Name);
                            selectedHolidaySalaryDayTypesDict.Add(pos, employeeGroupDayType.DayType.DayTypeId);

                            pos++;
                            if (pos == DayTypesHolidaySalary.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion

                    #region TimeAccumulators

                    //When you connect a accumulator to a group, an entry in TimeAccumulatorEmployeeGroupRule is created
                    List<TimeAccumulatorEmployeeGroupRule> connectedAccumulatorRules = tam.GetTimeAccumulatorEmployeeGroupRules(employeeGroup.EmployeeGroupId);
                    if (connectedAccumulatorRules != null && connectedAccumulatorRules.Count > 0)
                    {
                        int pos = 0;
                        foreach (TimeAccumulatorEmployeeGroupRule rule in connectedAccumulatorRules)
                        {
                            TimeAccumulators.AddLabelValue(pos, rule.TimeAccumulator.TimeAccumulatorId.ToString());
                            TimeAccumulators.AddValueFrom(pos, rule.TimeAccumulator.Name);
                            selectedTimeAccumulatorsDict.Add(pos, rule.TimeAccumulator.TimeAccumulatorId);

                            pos++;
                            if (pos == DayTypes.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion

                    #region TimeDeviationCauses

                    List<EmployeeGroupTimeDeviationCause> connectedEmployeeGroupTimeDeviationCause = em.GetEmployeeGroupTimeDeviationCauses(employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId);
                    if (connectedEmployeeGroupTimeDeviationCause != null && connectedEmployeeGroupTimeDeviationCause.Count > 0)
                    {
                        int pos = 0;
                        foreach (EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause in connectedEmployeeGroupTimeDeviationCause)
                        {
                            TimeDeviationCauses.AddLabelValue(pos, employeeGroupTimeDeviationCause.TimeDeviationCauseId.ToString());
                            TimeDeviationCauses.AddValueFrom(pos, employeeGroupTimeDeviationCause.TimeDeviationCause.Name);
                            TimeDeviationCauses.AddValueCheck(pos, employeeGroupTimeDeviationCause.UseInTimeTerminal);
                            selectedTimeDeviationCausesDict.Add(pos, employeeGroupTimeDeviationCause.TimeDeviationCauseId);

                            pos++;
                            if (pos == TimeDeviationCauses.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion

                    #region TimeDeviationCauseRequests

                    List<TimeDeviationCause> connectedTimeDeviationCauseRequests = tdcm.GetTimeDeviationCausesEmployeeRequests(SoeCompany.ActorCompanyId, employeeGroup.EmployeeGroupId).ToList();
                    if (connectedTimeDeviationCauseRequests != null && connectedTimeDeviationCauseRequests.Count > 0)
                    {
                        int pos = 0;
                        foreach (TimeDeviationCause timeDeviationCause in connectedTimeDeviationCauseRequests)
                        {
                            TimeDeviationCauseRequests.AddLabelValue(pos, timeDeviationCause.TimeDeviationCauseId.ToString());
                            TimeDeviationCauseRequests.AddValueFrom(pos, timeDeviationCause.Name);
                            selectedTimeDeviationCauseRequestsDict.Add(pos, timeDeviationCause.TimeDeviationCauseId);

                            pos++;
                            if (pos == TimeDeviationCauseRequests.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion

                    #region TimeDeviationCauseAbsenceAnnouncement

                    List<TimeDeviationCause> connectedTimeDeviationCauseAbsenceAnnouncements = tdcm.GetTimeDeviationCausesAbsenceAnnouncements(SoeCompany.ActorCompanyId, employeeGroup.EmployeeGroupId).ToList();
                    if (connectedTimeDeviationCauseAbsenceAnnouncements != null && connectedTimeDeviationCauseAbsenceAnnouncements.Count > 0)
                    {
                        int pos = 0;
                        foreach (TimeDeviationCause timeDeviationCause in connectedTimeDeviationCauseAbsenceAnnouncements)
                        {
                            TimeDeviationCauseAbsenceAnnouncements.AddLabelValue(pos, timeDeviationCause.TimeDeviationCauseId.ToString());
                            TimeDeviationCauseAbsenceAnnouncements.AddValueFrom(pos, timeDeviationCause.Name);
                            selectedTimeDeviationCauseAbsenceAnnouncementsDict.Add(pos, timeDeviationCause.TimeDeviationCauseId);

                            pos++;
                            if (pos == TimeDeviationCauseAbsenceAnnouncements.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion

                    #region EmployeeGroupTimeCodes

                    List<TimeCode> timeCodes;
                    if (employeeGroup.TimeCodes != null && employeeGroup.TimeCodes.IsLoaded)
                        timeCodes = employeeGroup.TimeCodes.ToList();
                    else
                        timeCodes = tcm.GetTimeCodesForEmployeeGroup(SoeCompany.ActorCompanyId, employeeGroup.EmployeeGroupId).ToList();

                    if (timeCodes != null && timeCodes.Count > 0)
                    {
                        int pos = 0;
                        foreach (TimeCode timeCode in timeCodes)
                        {
                            EmployeeGroupTimeCodes.AddValueFrom(pos, timeCode.TimeCodeId.ToString());
                            selectedTimeCodesDict.Add(pos, timeCode.TimeCodeId);

                            pos++;
                            if (pos == EmployeeGroupTimeCodes.NoOfIntervals)
                                break;
                        }
                    }

                    #endregion
                }
            }

            TimeCodeTimeDeviationCauses.PopulateEmployeeGroupMapping(Repopulate, SoeCompany.ActorCompanyId, employeeGroup != null ? employeeGroup.EmployeeGroupId : 0);
            AttestTransitions.PopulateEmployeeGroupAttestTransitions(Repopulate, SoeCompany.ActorCompanyId, employeeGroup != null ? employeeGroup.EmployeeGroupId : 0, SoeModule.Time);

            #region Accounting prio

            // Accounting payroll
            Dictionary<int, string> payrollAccountingPrioDict = GetGrpText(TermGroup.EmployeeGroupPayrollProductAccountingPrio);

            // Change sort order on items
            Dictionary<int, string> payrollAccountingPrioDictSorted = new Dictionary<int, string>();
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.NotUsed))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.NotUsed, payrollAccountingPrioDict[(int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.NotUsed]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.PayrollProduct))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.PayrollProduct, payrollAccountingPrioDict[(int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.PayrollProduct]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmploymentAccount))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmploymentAccount, payrollAccountingPrioDict[(int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmploymentAccount]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.Project))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.Project, payrollAccountingPrioDict[(int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.Project]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.Customer))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.Customer, payrollAccountingPrioDict[(int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.Customer]);
            if (payrollAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmployeeGroup))
                payrollAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmployeeGroup, payrollAccountingPrioDict[(int)TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmployeeGroup]);
            PayrollProductAccountingPrio1.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio2.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio3.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio4.ConnectDataSource(payrollAccountingPrioDictSorted);
            PayrollProductAccountingPrio5.ConnectDataSource(payrollAccountingPrioDictSorted);

            // Accounting invoice
            Dictionary<int, string> invoiceAccountingPrioDict = GetGrpText(TermGroup.EmployeeGroupInvoiceProductAccountingPrio);

            // Change sort order on items
            Dictionary<int, string> invoiceAccountingPrioDictSorted = new Dictionary<int, string>();
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.NotUsed))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.NotUsed, invoiceAccountingPrioDict[(int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.NotUsed]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.InvoiceProduct))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.InvoiceProduct, invoiceAccountingPrioDict[(int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.InvoiceProduct]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.EmploymentAccount))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.EmploymentAccount, invoiceAccountingPrioDict[(int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.EmploymentAccount]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Project))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Project, invoiceAccountingPrioDict[(int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Project]);
            if (invoiceAccountingPrioDict.ContainsKey((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Customer))
                invoiceAccountingPrioDictSorted.Add((int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Customer, invoiceAccountingPrioDict[(int)TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Customer]);
            InvoiceProductAccountingPrio1.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio2.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio3.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio4.ConnectDataSource(invoiceAccountingPrioDictSorted);
            InvoiceProductAccountingPrio5.ConnectDataSource(invoiceAccountingPrioDictSorted);

            #endregion

            #region TimeStampRounding

            if (employeeGroup != null)
            {
                timeStampRounding = tsm.GetTimeStampRoundingByEmployeeGroup(employeeGroup.EmployeeGroupId);
            }

            if (employeeGroup == null || timeStampRounding == null)
            {
                timeStampRounding = new TimeStampRounding()
                {
                    RoundInNeg = 0,
                    RoundOutNeg = 0,
                    RoundInPos = 0,
                    RoundOutPos = 0
                };
            }

            //Value before
            SchemaIn.ValueFrom = timeStampRounding.RoundInNeg.ToString();
            SchemaIn.LabelFrom = GetText(7004, "före");
            //Value after
            SchemaIn.ValueTo = timeStampRounding.RoundInPos.ToString();
            SchemaIn.LabelTo = GetText(7005, "efter");
            //Value before
            SchemaUt.ValueFrom = timeStampRounding.RoundOutNeg.ToString();
            //Value after
            SchemaUt.ValueTo = timeStampRounding.RoundOutPos.ToString();

            #endregion

            #region Reminder settings

            Dictionary<int, string> attestStates = am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time, true, false);
            attestStates.AddRange(am.GetAttestStatesDict(SoeCompany.ActorCompanyId, TermGroup_AttestEntity.InvoiceTime, SoeModule.Time, false, false));
            AttestStateSelectEntry.ConnectDataSource(attestStates, "Value", "Key");

            Dictionary<int, string> periods = new Dictionary<int, string>();
            periods.Add((int)AttestPeriodType.Unknown, " ");
            periods.Add((int)AttestPeriodType.Day, GetText(7162, "dagen"));
            periods.Add((int)AttestPeriodType.Week, GetText(7163, "veckan"));
            periods.Add((int)AttestPeriodType.Month, GetText(7164, "månaden"));
            periods.Add((int)AttestPeriodType.Period, GetText(7165, "perioden"));
            PeriodSelectEntry.ConnectDataSource(periods, "Value", "Key");

            #endregion

            #endregion

            #region Set data

            BreakDayMinutesAfterMidnight.Value = "180";

            // Planning periods
            Dictionary<int, string> planningPeriods = new Dictionary<int, string>();
            planningPeriods.Add(0, " ");

            Dictionary<int, string> heads = tpm.GetTimePeriodHeadsDict(SoeCompany.ActorCompanyId, TermGroup_TimePeriodType.RuleWorkTime, false);
            foreach (KeyValuePair<int, string> head in heads)
            {
                Dictionary<int, string> pers = tpm.GetTimePeriodsDict(head.Key, false, SoeCompany.ActorCompanyId);
                foreach (KeyValuePair<int, string> per in pers)
                {
                    planningPeriods.Add(per.Key, String.Format("{0} - {1}", head.Value, per.Value));
                }
            }

            PlanningPeriod.Labels = planningPeriods;

            if (employeeGroup != null)
            {
                Name.Value = employeeGroup.Name;
                ExternalCodes.Value = employeeGroup.ExternalCodesString;
                DeviationTimeAxelHoursAfterSchema.Value = employeeGroup.DeviationAxelStopHours.ToString();
                DeviationTimeAxelHoursBeforeSchema.Value = employeeGroup.DeviationAxelStartHours.ToString();
                TimeReportType.Value = employeeGroup.TimeReportType.ToString();
                AlwaysDiscardBreakEvaluation.Value = employeeGroup.AlwaysDiscardBreakEvaluation.ToString();
                AutogenBreakOnStamping.Value = employeeGroup.AutogenBreakOnStamping.ToString();
                MergeScheduleBreaksOnDay.Value = employeeGroup.MergeScheduleBreaksOnDay.ToString();
                BreakRoundingUp.Value = employeeGroup.BreakRoundingUp.ToString();
                BreakRoundingDown.Value = employeeGroup.BreakRoundingDown.ToString();
                BreakDayMinutesAfterMidnight.Value = employeeGroup.BreakDayMinutesAfterMidnight.ToString();
                KeepStampsTogetherWithinMinutes.Value = employeeGroup.KeepStampsTogetherWithinMinutes.ToString();
                AutoGenTimeAndBreakForProject.Value = employeeGroup.AutoGenTimeAndBreakForProject.ToString();
                AlsoAttestAdditionsFromTime.Value = employeeGroup.AlsoAttestAdditionsFromTime ? Boolean.TrueString : Boolean.FalseString;
                ExtraShiftAsDefault.Value = employeeGroup.ExtraShiftAsDefault.ToString();
                if (employeeGroup.TimeReportType != (int)TermGroup_TimeReportType.Stamp)
                {
                    NotifyChangeOfDeviations.Value = employeeGroup.NotifyChangeOfDeviations.ToString();
                    NotifyChangeOfDeviations.Visible = true;
                }
                else
                {
                    NotifyChangeOfDeviations.Visible = false;
                }

                if (UseAccountHierarchy())
                {
                    DivAccountsHierarchy.Visible = true;
                    AllowShiftsWithoutAccount.Value = employeeGroup.AllowShiftsWithoutAccount.ToString();
                }
                else
                {
                    DivAccountsHierarchy.Visible = false;
                }
               
                //WorkRules
                RuleWorkTimeWeek.Value = CalendarUtility.FormatMinutes(employeeGroup.RuleWorkTimeWeek);
                RuleWorkTimeWeek.InfoText = GetText(5986, "Vid ändring kommer sysselsättningsgrad för alla anställningar med detta tidavtal att räknas om");
                RuleWorkTimeDayMinimum.Value = CalendarUtility.FormatMinutes(employeeGroup.RuleWorkTimeDayMinimum);
                RuleWorkTimeDayMaximumWorkDay.Value = CalendarUtility.FormatMinutes(employeeGroup.RuleWorkTimeDayMaximumWorkDay);
                RuleWorkTimeDayMaximumWeekend.Value = CalendarUtility.FormatMinutes(employeeGroup.RuleWorkTimeDayMaximumWeekend);
                MaxScheduleTimeFullTime.Value = CalendarUtility.FormatMinutes(employeeGroup.MaxScheduleTimeFullTime);
                MaxScheduleTimeFullTime.InfoText = GetText(11502, "Anges som +tid ( t ex 09:00) i förhållande till angiven Arbetstid (timmar på vecka)");
                MinScheduleTimeFullTime.Value = CalendarUtility.FormatMinutes(employeeGroup.MinScheduleTimeFullTime);
                MinScheduleTimeFullTime.InfoText = GetText(11501, "Anges som -tid ( t ex -09:00) i förhållande till angiven Arbetstid (timmar på vecka)");
                MaxScheduleTimePartTime.Value = CalendarUtility.FormatMinutes(employeeGroup.MaxScheduleTimePartTime);
                MaxScheduleTimePartTime.InfoText = GetText(11503, "Anges som +tid ( t ex 05:00) i förhållande till angiven Arbetstid (timmar på vecka)");
                MinScheduleTimePartTime.Value = CalendarUtility.FormatMinutes(employeeGroup.MinScheduleTimePartTime);
                MinScheduleTimePartTime.InfoText = GetText(11504, "Anges som -tid ( t ex -05:00) i förhållande till angiven Arbetstid (timmar på vecka)");
                MaxScheduleTimeWithoutBreaks.Value = CalendarUtility.FormatMinutes(employeeGroup.MaxScheduleTimeWithoutBreaks);
                RuleResttimeDay.Value = CalendarUtility.FormatMinutes(employeeGroup.RuleRestTimeDay);
                RuleRestTimeDayStartTime.Value = CalendarUtility.FormatTime(employeeGroup.RuleRestTimeDayStartTime).ToString();
                RuleRestTimeDayStartTime.InfoText = GetText(11564, "Om inget anges börjar dygnsvilan kl 12:00");
                RuleResttimeWeek.Value = CalendarUtility.FormatMinutes(employeeGroup.RuleRestTimeWeek);
                RuleRestTimeWeekStartTime.Value = CalendarUtility.FormatTime(employeeGroup.RuleRestTimeWeekStartTime).ToString();
                RuleRestTimeWeekStartTime.InfoText = GetText(11565, "Om inget anges börjar veckovilan Måndag 00:00");
                RestTimeWeekStartDaySelectEntry.Value = employeeGroup.RuleRestTimeWeekStartDayNumber.HasValue ? employeeGroup.RuleRestTimeWeekStartDayNumber.Value.ToString() : "1";
                RuleScheduleFreeWeekendsMinimumYear.Value = employeeGroup.RuleScheduleFreeWeekendsMinimumYear.ToString();
                RuleScheduleFreeWeekendsMinimumYear.InfoText = GetText(8939, "Gäller endast grundschema");
                RuleScheduledDaysMaximumWeek.Value = employeeGroup.RuleScheduledDaysMaximumWeek.ToString();
                RuleRestDayIncludePresence.Value = employeeGroup.RuleRestDayIncludePresence.ToString();
                RuleRestDayIncludePresence.InfoText = GetText(10136, "Gäller endast vid rapportering av närvaro");
                RuleRestWeekIncludePresence.Value = employeeGroup.RuleRestWeekIncludePresence.ToString();
                RuleRestWeekIncludePresence.InfoText = GetText(10136, "Gäller endast vid rapportering av närvaro");

                if (employeeGroup.EmployeeGroupRuleWorkTimePeriod != null && employeeGroup.EmployeeGroupRuleWorkTimePeriod.Any())
                {
                    int pos = 0;
                    foreach (var item in employeeGroup.EmployeeGroupRuleWorkTimePeriod)
                    {
                        PlanningPeriod.AddLabelValue(pos, item.TimePeriodId.ToString());
                        PlanningPeriod.AddValueFrom(pos, CalendarUtility.FormatMinutes(item.RuleWorkTime));
                        pos++;
                    }
                }

                if (employeeGroup.TimeCodeId.HasValue)
                    DefaultTimeCode.Value = employeeGroup.TimeCodeId.Value.ToString();

                if (employeeGroup != null && employeeGroup.TimeDeviationCause != null)
                    DefaultTimeDeviationCause.Value = employeeGroup.TimeDeviationCause.TimeDeviationCauseId.ToString();

                #region TimeAccumulators

                if (employeeGroup.TimeAccumulatorEmployeeGroupRule != null && employeeGroup.TimeAccumulatorEmployeeGroupRule.Count > 0)
                {
                    foreach (TimeAccumulatorEmployeeGroupRule rule in employeeGroup.TimeAccumulatorEmployeeGroupRule)
                    {
                        //TODO: render control    
                    }
                }

                #endregion

                #region AccountingPrio

                string[] payrollAccountingPrios = employeeGroup.PayrollProductAccountingPrio.Split(',');
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

                string[] invoiceAccountingPrios = employeeGroup.InvoiceProductAccountingPrio.Split(',');
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

                #endregion

                AttestStateSelectEntry.Value = employeeGroup.ReminderAttestStateId != null ? employeeGroup.ReminderAttestStateId.ToString() : "0";
                NoOfDaysTextEntry.Value = employeeGroup.ReminderNoOfDays != null ? employeeGroup.ReminderNoOfDays.ToString() : "";
                PeriodSelectEntry.Value = employeeGroup.ReminderPeriodType != null ? employeeGroup.ReminderPeriodType.ToString() : "0";

                TimeReportType.Value = employeeGroup.TimeReportType.ToString();

                QualifyingDayCalculationRule.Value = employeeGroup.QualifyingDayCalculationRule.ToString();
                TimeWorkReductionCalculationRule.Value = employeeGroup.TimeWorkReductionCalculationRule.ToString();
                QualifyingDayCalculationRuleLimitFirstDay.Value = employeeGroup.QualifyingDayCalculationRuleLimitFirstDay.ToString();
                CandidateForOvertimeOnZeroDayExcluded.Value = employeeGroup.CandidateForOvertimeOnZeroDayExcluded.ToString();
            }
            else
            {
                MaxScheduleTimeWithoutBreaks.Value = CalendarUtility.FormatMinutes(Convert.ToInt32(new TimeSpan(5, 0, 0).TotalMinutes));//default 5hr
                TimeReportType.Value = "0";
            }

            #region Render dynamic controls

            HtmlTableRow tRow;
            HtmlTableCell tCell;
            TextEntry text;
            SelectEntry select;
            Text label;

            #region EmployeeGroup accounts

            #region CostAccountTable AccountStd

            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 1130,
                DefaultTerm = "Standard",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "CostAccount";
            text.AutoComplete = false;
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            text.InfoText = defaultCostAccountNr;
            if (costAccount != null && costAccount.AccountStd != null && costAccount.AccountStd.Account != null)
            {
                text.Value = costAccount.AccountStd.Account.AccountNr;
                text.InfoText += costAccount.AccountStd.Account.Name;
            }
            text.OnChange = "accountSearch.searchField('CostAccount')";
            text.OnKeyUp = "accountSearch.keydown('CostAccount')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            // TODO: AccountStd
            // AccountStd on TimeScheduleTemplateBlock not present. 
            // So any AccountStd found in Accounting priority will not get passed to TimeBlock's and transactions.
            // AccountEmployeeGroupCost setting used instead
            // See also: TimeEngineManager ApplyAccountingPrioOnTimeScheduleTemplateBlock
            tRow.Visible = false;

            CostAccountTable.Rows.Add(tRow);

            #endregion

            #region IncomeAccountTable AccountStd

            tRow = new HtmlTableRow();

            tCell = new HtmlTableCell();
            label = new Text()
            {
                TermID = 1130,
                DefaultTerm = "Standard",
                FitInTable = true,
            };
            tCell.Controls.Add(label);
            tRow.Cells.Add(tCell);

            tCell = new HtmlTableCell();
            text = new TextEntry();
            text.ID = "IncomeAccount";
            text.AutoComplete = false;
            text.HideLabel = true;
            text.FitInTable = true;
            text.DisableSettings = true;
            text.Width = 40;
            text.InfoText = defaultIncomeAccountNr;
            if (incomeAccount != null && incomeAccount.AccountStd != null && incomeAccount.AccountStd.Account != null)
            {
                text.Value = incomeAccount.AccountStd.Account.AccountNr;
                text.InfoText += incomeAccount.AccountStd.Account.Name;
            }
            text.OnChange = "accountSearch.searchField('IncomeAccount')";
            text.OnKeyUp = "accountSearch.keydown('IncomeAccount')";
            tCell.Controls.Add(text);
            tRow.Cells.Add(tCell);

            // TODO: AccountStd
            // AccountStd on TimeScheduleTemplateBlock not present. 
            // So any AccountStd found in Accounting priority will not get passed to TimeBlock's and transactions.
            // AccountEmployeeGroupCost setting used instead
            // See also: TimeEngineManager ApplyAccountingPrioOnTimeScheduleTemplateBlock
            tRow.Visible = false;

            IncomeAccountTable.Rows.Add(tRow);

            #endregion

            foreach (AccountDim accountDim in accountDims)
            {
                // Only internal accounts
                if (accountDim.IsStandard)
                    continue;

                Dictionary<int, string> accountInternalDict = acm.GetAccountInternalsDict(accountDim.AccountDimId, SoeCompany.ActorCompanyId, true);

                #region CostAccountTable AccountInternal

                tRow = new HtmlTableRow();
                tRow.Attributes.Add("RowType", "AccountInternal");

                tCell = new HtmlTableCell();
                label = new Text()
                {
                    LabelSetting = accountDim.Name,
                    FitInTable = true,
                };
                tCell.Controls.Add(label);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                tCell.ColSpan = 2;
                select = new SelectEntry();
                select.ID = "CostAccount" + accountDim.AccountDimId;
                select.HideLabel = true;
                select.FitInTable = true;
                select.DisableSettings = true;
                select.ConnectDataSource(accountInternalDict);

                if (costAccountDict != null && costAccountDict.ContainsKey(accountDim.AccountDimId))
                    select.Value = costAccountDict[accountDim.AccountDimId].ToString();

                tCell.Controls.Add(select);
                tRow.Cells.Add(tCell);

                CostAccountTable.Rows.Add(tRow);

                #endregion

                #region IncomeAccountTable AccountInternal

                tRow = new HtmlTableRow();
                tRow.Attributes.Add("RowType", "InternalAccount");

                tCell = new HtmlTableCell();
                label = new Text()
                {
                    LabelSetting = accountDim.Name,
                    FitInTable = true,
                };
                tCell.Controls.Add(label);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                tCell.ColSpan = 2;
                select = new SelectEntry();
                select.ID = "IncomeAccount" + accountDim.AccountDimId;
                select.HideLabel = true;
                select.FitInTable = true;
                select.DisableSettings = true;
                select.ConnectDataSource(accountInternalDict);

                if (incomeAccountDict != null && incomeAccountDict.ContainsKey(accountDim.AccountDimId))
                    select.Value = incomeAccountDict[accountDim.AccountDimId].ToString();

                tCell.Controls.Add(select);
                tRow.Cells.Add(tCell);

                IncomeAccountTable.Rows.Add(tRow);

                #endregion
            }

            #endregion

            #endregion

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(5040, "Tidavtal sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(5041, "Tidavtal kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(5042, "Tidavtal uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(5954, "Tidavtal kunde inte uppdateras");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(5045, "Tidavtal borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                {
                    var validationResult = em.IsOkToDeleteEmployeeGroup(employeeGroup.EmployeeGroupId);
                    if (validationResult.Success)
                        Form1.MessageError = GetText(5046, "Tidavtal kunde inte tas bort");
                    else
                        Form1.MessageError = $"{GetText(5046, "Tidavtal kunde inte tas bort")}. {validationResult.ErrorMessage}";
                }
                    

                //Validations
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(5044, "Tidavtal finns redan");
                else if (MessageFromSelf == "TIMEDEVIATIONCAUSE_MANDATORY")
                    Form1.MessageError = GetText(5522, "Du måste ange standardorsak");
                else if (MessageFromSelf == "TIMEDEVIATIONCAUSE_NOTCHOOSEN")
                    Form1.MessageError = GetText(5524, "Standardorsak är inte kopplad till tidavtalet");
                else if (MessageFromSelf == "INVALID_BREAKSETTINGS")
                    Form1.MessageError = GetText(5756, "Ogiltig uppsättning av rasthantering för stämplingar");

                //Warning
                else if (MessageFromSelf == "SAVED_WITH_DAYTYPEERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(4407, "Koppling till dagtyper kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_DAYTYPEERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(4407, "Koppling till dagtyper kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_ACCUMULATORERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(8055, "Koppling till saldon kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_ACCUMULATORERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(8055, "Koppling till saldon kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_ACCUMULATORSETTINGERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(4517, "Saldoregler kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_ACCUMULATORSETTINGERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(4517, "Saldoregler kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_DEVIATIONCAUSEERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(8056, "Koppling till orsaker kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_DEVIATIONCAUSEERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(8056, "Koppling till orsaker kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_DEVIATIONCAUSEREQUESTSERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(8215, "Kopplingar till frånvaroorsaker kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_DEVIATIONCAUSEREQUESTSERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(8215, "Kopplingar till frånvaroorsaker kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_DEVIATIONCAUSEABSENCEANNOUNCEMENTSERRORS")
                    Form1.MessageError = GetText(5040, "Tidavtal sparad") + ". " + GetText(5955, "Tidavtal sparad, kopplingar till frånvaroorsaker/sjukanmälan med kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_DEVIATIONCAUSEABSENCEANNOUNCEMENTSERRORS")
                    Form1.MessageError = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(5955, "Tidavtal sparad, kopplingar till frånvaroorsaker/sjukanmälan med kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_TIMECODEMAPPINGERRORS")
                    Form1.MessageError = GetText(5040, "Tidavtal sparad") + ". " + GetText(5956, "Kopplingar mot tidkoder kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_RULEWORKTIMEPERIODERRORS")
                    Form1.MessageError = GetText(5040, "Tidavtal sparad") + ". " + GetText(11837, "Planeringsperioder kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_TIMECODEMAPPINGERRORS")
                    Form1.MessageError = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(5956, "Kopplingar mot tidkoder kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_TRANSITIONERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(3347, "Alla övergångar kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_TRANSITIONERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(3345, "Alla övergångar kunde inte uppdateras");
                else if (MessageFromSelf == "SAVED_WITH_TIMECODETIMEDEVIATIONCAUSEERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(4489, "Alla kopplingar mellan tidkod och orsak kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_TIMECODETIMEDEVIATIONCAUSEERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(4489, "Alla kopplingar mellan tidkod och orsak kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_ACCOUNTERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(3102, "Alla kontouppgifter kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_ACCOUNTERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(3102, "Alla kontouppgifter kunde inte sparas");
                else if (MessageFromSelf == "SAVED_WITH_TIMESTAMPERRORS")
                    Form1.MessageWarning = GetText(5040, "Tidavtal sparad") + ". " + GetText(0, "Stämplingsavrundningar kunde inte sparas");
                else if (MessageFromSelf == "UPDATED_WITH_TIMESTAMPERRORS")
                    Form1.MessageWarning = GetText(5042, "Tidavtal uppdaterad") + ". " + GetText(0, "Stämplingsavrundningar kunde inte sparas");
            }

            #endregion

            #region Navigation

            if (employeeGroup != null)
            {
                Form1.SetRegLink(GetText(5038, "Registrera tidavtal"), "",
                    Feature.Time_Employee_Groups_Edit, Permission.Modify);

                //Form1.AddLink(GetText(4296, "Visa avvikelsedagar"), "/modalforms/EmployeeGroupHolidays.aspx?empGroupId=" + employeeGroup.EmployeeGroupId,
                //    Feature.Time_Employee_Groups_Edit, Permission.Readonly, true);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            #region Init

            Collection<FormIntervalEntryItem> dayTypesItems = DayTypes.GetData(F);
            Collection<FormIntervalEntryItem> dayTypesHolidaySalaryItems = DayTypesHolidaySalary.GetData(F);
            Collection<FormIntervalEntryItem> timeAccumulatorsItems = TimeAccumulators.GetData(F);
            Collection<FormIntervalEntryItem> timeDeviationCausesItems = TimeDeviationCauses.GetData(F);
            Collection<FormIntervalEntryItem> timeDeviationCauseRequestsItems = TimeDeviationCauseRequests.GetData(F);
            Collection<FormIntervalEntryItem> timeDeviationCauseAbsenceAnnouncementItems = TimeDeviationCauseAbsenceAnnouncements.GetData(F);
            Collection<FormIntervalEntryItem> employeeGroupTimeCodesItems = EmployeeGroupTimeCodes.GetData(F);
            Collection<FormIntervalEntryItem> planningPeriodItems = PlanningPeriod.GetData(F);

            // TimeDeviationCause
            int.TryParse(F["DefaultTimeDeviationCause"], out int defaultTimeDeviationCauseId);
            if (defaultTimeDeviationCauseId == 0)
                RedirectToSelf("TIMEDEVIATIONCAUSE_MANDATORY", true);

            //Default TimeDeviationCause must be in TimeDeviationCauses list
            if (!timeDeviationCausesItems.Any(i => i.From == defaultTimeDeviationCauseId.ToString()))
                RedirectToSelf("TIMEDEVIATIONCAUSE_NOTCHOOSEN", true);

            // TimeCode
            int? timeCodeId = null;
            int.TryParse(F["DefaultTimeCode"], out int defaultTimeCodeId);
            if (defaultTimeCodeId > 0)
                timeCodeId = defaultTimeCodeId;

            string name = F["Name"];
            string externalCodes = F["ExternalCodes"];
            int breakDayMinutesAfterMidnight = StringUtility.GetInt(F["BreakDayMinutesAfterMidnight"], 0);
            int keepStampsTogetherWithinMinutes = StringUtility.GetInt(F["KeepStampsTogetherWithinMinutes"], 0);
            int deviationTimeAxelHoursAfterSchema = StringUtility.GetInt(F["DeviationTimeAxelHoursAfterSchema"], 0);
            int deviationTimeAxelHoursBeforeSchema = StringUtility.GetInt(F["DeviationTimeAxelHoursBeforeSchema"], 0);
            bool alwaysDiscardBreakEvaluation = StringUtility.GetBool(F["AlwaysDiscardBreakEvaluation"]);
            bool autogenBreakOnStamping = StringUtility.GetBool(F["AutogenBreakOnStamping"]);
            bool mergeScheduleBreaksOnDay = StringUtility.GetBool(F["MergeScheduleBreaksOnDay"]);
            int breakRoundingUp = StringUtility.GetInt(F["BreakRoundingUp"], 0);
            int breakRoundingDown = StringUtility.GetInt(F["BreakRoundingDown"], 0);
            bool autoGenTimeAndBreakForProject = StringUtility.GetBool(F["AutoGenTimeAndBreakForProject"]);
            bool notifyChangeOfDeviations = StringUtility.GetBool(F["NotifyChangeOfDeviations"]);
            bool allowShiftsWithoutAccount = StringUtility.GetBool(F["AllowShiftsWithoutAccount"]);
            bool alsoAttestAdditionsFromTime = StringUtility.GetBool(F["AlsoAttestAdditionsFromTime"]);
            bool extraShiftAsDefault = StringUtility.GetBool(F["ExtraShiftAsDefault"]);

            string payrollAccountingPrio = String.Format("{0},{1},{2},{3},{4}", F["PayrollProductAccountingPrio1"], F["PayrollProductAccountingPrio2"], F["PayrollProductAccountingPrio3"], F["PayrollProductAccountingPrio4"], F["PayrollProductAccountingPrio5"]);
            string invoiceAccountingPrio = String.Format("{0},{1},{2},{3},{4}", F["InvoiceProductAccountingPrio1"], F["InvoiceProductAccountingPrio2"], F["InvoiceProductAccountingPrio3"], F["InvoiceProductAccountingPrio4"], F["InvoiceProductAccountingPrio5"]);

            //WorkRules
            int ruleWorkTimeWeekMinutes = CalendarUtility.GetMinutes(F["RuleWorkTimeWeek"]);
            int ruleWorkTimeDayMinimumMinutes = CalendarUtility.GetMinutes(F["RuleWorkTimeDayMinimum"]);
            int ruleWorkTimeDayMaximumWorkDayMinutes = CalendarUtility.GetMinutes(F["RuleWorkTimeDayMaximumWorkDay"]);
            int ruleWorkTimeDayMaximumWeekendMinutes = CalendarUtility.GetMinutes(F["RuleWorkTimeDayMaximumWeekend"]);
            int maxScheduleTimeFullTimeMinutes = CalendarUtility.GetMinutes(F["MaxScheduleTimeFullTime"]);
            int minScheduleTimeFullTimeMinutes = CalendarUtility.GetMinutes(F["MinScheduleTimeFullTime"]);
            int maxScheduleTimePartTimeMinutes = CalendarUtility.GetMinutes(F["MaxScheduleTimePartTime"]);
            int minScheduleTimePartTimeMinutes = CalendarUtility.GetMinutes(F["MinScheduleTimePartTime"]);
            int maxScheduleTimeWithoutBreaks = CalendarUtility.GetMinutes(F["MaxScheduleTimeWithoutBreaks"]);
            int ruleRestTimeDaykMinutes = CalendarUtility.GetMinutes(F["RuleResttimeDay"]);
            int ruleRestTimeWeekMinutes = CalendarUtility.GetMinutes(F["RuleResttimeWeek"]);
            int ruleScheduleFreeWeekendsMinimumYear = StringUtility.GetInt(F["RuleScheduleFreeWeekendsMinimumYear"]);
            int ruleScheduledDaysMaximumWeek = StringUtility.GetInt(F["RuleScheduledDaysMaximumWeek"]);
            bool ruleRestDayIncludePresence = StringUtility.GetBool(F["RuleRestDayIncludePresence"]);
            bool ruleRestWeekIncludePresence = StringUtility.GetBool(F["RuleRestWeekIncludePresence"]);
            DateTime? ruleRestTimeDayStartTime = CalendarUtility.GetTime(F["RuleRestTimeDayStartTime"]);
            DateTime? ruleRestTimeWeekStartTime = CalendarUtility.GetTime(F["RuleRestTimeWeekStartTime"]);
            int.TryParse(F["RestTimeWeekStartDaySelectEntry"], out int ruleRestTimeWeekStartDayNumber);
            Int32.TryParse(F["AttestStateSelectEntry"], out int reminderAttestStateId);
            Int32.TryParse(F["NoOfDaysTextEntry"], out int reminderNoOfDays);
            Int32.TryParse(F["PeriodSelectEntry"], out int reminderPeriodType);
            bool isAttestReminderValid = reminderAttestStateId != 0 && reminderPeriodType != 0;
            Int32.TryParse(F["TimeReportType"], out int timeReportType);
            Int32.TryParse(F["QualifyingDayCalculationRule"], out int qualifyingDayCalculationRule);
            bool qualifyingDayCalculationRuleLimitFirstDay = StringUtility.GetBool(F["QualifyingDayCalculationRuleLimitFirstDay"]);
            Int32.TryParse(F["TimeWorkReductionCalculationRule"], out int timeWorkReductionCalculationRule);
            bool candidateForOvertimeOnZeroDayExcluded = StringUtility.GetBool(F["CandidateForOvertimeOnZeroDayExcluded"]);

            #endregion

            #region Validation

            bool valid = true;

            if (alwaysDiscardBreakEvaluation && (autogenBreakOnStamping || mergeScheduleBreaksOnDay))
                valid = false;

            if (!valid)
                RedirectToSelf("INVALID_BREAKSETTINGS");

            #endregion

            if (employeeGroup == null)
            {
                #region Add

                //Validation: Employee not already exist
                if (em.EmployeeGroupExists(name, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                employeeGroup = new EmployeeGroup()
                {
                    Name = name,
                    DeviationAxelStartHours = deviationTimeAxelHoursBeforeSchema,
                    DeviationAxelStopHours = deviationTimeAxelHoursAfterSchema,
                    PayrollProductAccountingPrio = payrollAccountingPrio,
                    InvoiceProductAccountingPrio = invoiceAccountingPrio,
                    AlwaysDiscardBreakEvaluation = alwaysDiscardBreakEvaluation,
                    AutogenBreakOnStamping = autogenBreakOnStamping,
                    AutoGenTimeAndBreakForProject = autoGenTimeAndBreakForProject,
                    MergeScheduleBreaksOnDay = mergeScheduleBreaksOnDay,
                    BreakRoundingUp = breakRoundingUp,
                    BreakRoundingDown = breakRoundingDown,
                    BreakDayMinutesAfterMidnight = breakDayMinutesAfterMidnight,
                    KeepStampsTogetherWithinMinutes = keepStampsTogetherWithinMinutes,
                    RuleWorkTimeWeek = ruleWorkTimeWeekMinutes,
                    RuleWorkTimeDayMinimum = ruleWorkTimeDayMinimumMinutes,
                    RuleWorkTimeDayMaximumWorkDay = ruleWorkTimeDayMaximumWorkDayMinutes,
                    RuleWorkTimeDayMaximumWeekend = ruleWorkTimeDayMaximumWeekendMinutes,
                    MaxScheduleTimeFullTime = maxScheduleTimeFullTimeMinutes,
                    MinScheduleTimeFullTime = minScheduleTimeFullTimeMinutes,
                    MaxScheduleTimePartTime = maxScheduleTimePartTimeMinutes,
                    MinScheduleTimePartTime = minScheduleTimePartTimeMinutes,
                    MaxScheduleTimeWithoutBreaks = maxScheduleTimeWithoutBreaks,
                    RuleRestTimeDay = ruleRestTimeDaykMinutes,
                    RuleRestTimeWeekStartTime = ruleRestTimeWeekStartTime,
                    RuleRestTimeDayStartTime = ruleRestTimeDayStartTime,
                    RuleRestTimeWeekStartDayNumber = ruleRestTimeWeekStartDayNumber,
                    RuleRestTimeWeek = ruleRestTimeWeekMinutes,
                    RuleScheduleFreeWeekendsMinimumYear = ruleScheduleFreeWeekendsMinimumYear,
                    RuleScheduledDaysMaximumWeek = ruleScheduledDaysMaximumWeek,
                    ReminderAttestStateId = isAttestReminderValid ? reminderAttestStateId : (int?)null,
                    ReminderNoOfDays = isAttestReminderValid ? reminderNoOfDays : (int?)null,
                    ReminderPeriodType = isAttestReminderValid ? reminderPeriodType : (int?)null,
                    TimeReportType = timeReportType,
                    AutogenTimeblocks = timeReportType == (int)TermGroup_TimeReportType.Deviation,
                    QualifyingDayCalculationRule = qualifyingDayCalculationRule,
                    QualifyingDayCalculationRuleLimitFirstDay = qualifyingDayCalculationRuleLimitFirstDay,
                    TimeWorkReductionCalculationRule = timeWorkReductionCalculationRule,
                    ExternalCodesString = externalCodes,
                    NotifyChangeOfDeviations = notifyChangeOfDeviations,
                    RuleRestDayIncludePresence = ruleRestDayIncludePresence,
                    RuleRestWeekIncludePresence = ruleRestWeekIncludePresence,
                    AllowShiftsWithoutAccount = allowShiftsWithoutAccount,
                    AlsoAttestAdditionsFromTime = alsoAttestAdditionsFromTime,
                    CandidateForOvertimeOnZeroDayExcluded = candidateForOvertimeOnZeroDayExcluded,
                    ExtraShiftAsDefault = extraShiftAsDefault,

                    //Set FK
                    ActorCompanyId = SoeCompany.ActorCompanyId,
                    TimeDeviationCauseId = defaultTimeDeviationCauseId,
                    TimeCodeId = timeCodeId,
                };

                if (em.AddEmployeeGroup(employeeGroup).Success)
                {
                    // DayTypes
                    if (!em.SaveEmployeeGroupDayTypesWorking(dayTypesItems, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("SAVED_WITH_DAYTYPEERRORS");
                    // DayTypes HolidaySalary
                    if (!em.SaveEmployeeGroupDayTypesHolidaySalary(dayTypesHolidaySalaryItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_DAYTYPEERRORS");
                    // TimeAccumulators
                    if (!em.SaveEmployeeGroupTimeAccumulators(timeAccumulatorsItems, employeeGroup.EmployeeGroupId, UserId).Success)
                        RedirectToSelf("SAVED_WITH_ACCUMULATORERRORS");
                    // TimeAccumulators settings
                    if (!EmployeeGroupAccumulatorSettings.SaveForEmployeeGroup(F, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_ACCUMULATORSETTINGERRORS");
                    // TimeDeviationCauses
                    if (!em.SaveEmployeeGroupTimeDeviationCauses(timeDeviationCausesItems, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("SAVED_WITH_DEVIATIONCAUSEERRORS");
                    // TimeDeviationCauseRequests
                    if (!em.SaveEmployeeGroupTimeDeviationCauseRequests(timeDeviationCauseRequestsItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_DEVIATIONCAUSEREQUESTSERRORS");
                    // TimeDeviationCauseAbsenceAnnouncements
                    if (!em.SaveEmployeeGroupTimeDeviationCauseAbsenceAnnouncements(timeDeviationCauseAbsenceAnnouncementItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_DEVIATIONCAUSEABSENCEANNOUNCEMENTSERRORS");
                    // EmployeeGroupTimeCodes
                    if (!em.SaveEmployeeGroupTimeCodes(employeeGroupTimeCodesItems, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("SAVED_WITH_TIMECODEMAPPINGERRORS");
                    // EmployeeGroupRuleWorkTimePeriod
                    if (!em.SaveEmployeeGroupRuleWorkTimePeriod(planningPeriodItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_RULEWORKTIMEPERIODERRORS");
                    // AttestTransitions
                    if (!AttestTransitions.SaveEmployeeGroupAttestTransitions(F, employeeGroup.EmployeeGroupId))
                        RedirectToSelf("SAVED_WITH_TRANSITIONERRORS");
                    // TimeCodeTimeDeviationCauses
                    if (!TimeCodeTimeDeviationCauses.SaveEmployeeGroupMappings(F, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId, UserId))
                        RedirectToSelf("SAVED_WITH_TIMECODETIMEDEVIATIONCAUSEERRORS");
                    // Standard accounts
                    if (!SaveAccountStd(EmployeeGroupAccountType.Cost) || !SaveAccountStd(EmployeeGroupAccountType.Income))
                        RedirectToSelf("SAVED_WITH_ACCOUNTERRORS");
                    // TimeStampRounding
                    if (!SaveTimeStampRoundingSetting())
                        RedirectToSelf("SAVED_WITH_TIMESTAMPERRORS");

                    RedirectToSelf("SAVED");
                }
                else
                {
                    RedirectToSelf("NOTSAVED", true);
                }

                #endregion
            }
            else
            {
                #region Update

                //Validation: Employee not already exist
                if (employeeGroup.Name != name && em.EmployeeGroupExists(name, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                bool updateWorkPercentageOnEmployees = employeeGroup.RuleWorkTimeWeek != ruleWorkTimeWeekMinutes;

                employeeGroup.Name = name;
                employeeGroup.DeviationAxelStopHours = deviationTimeAxelHoursAfterSchema;
                employeeGroup.DeviationAxelStartHours = deviationTimeAxelHoursBeforeSchema;
                employeeGroup.PayrollProductAccountingPrio = payrollAccountingPrio;
                employeeGroup.InvoiceProductAccountingPrio = invoiceAccountingPrio;
                employeeGroup.AlwaysDiscardBreakEvaluation = alwaysDiscardBreakEvaluation;
                employeeGroup.AutogenBreakOnStamping = autogenBreakOnStamping;
                employeeGroup.AutoGenTimeAndBreakForProject = autoGenTimeAndBreakForProject;
                employeeGroup.MergeScheduleBreaksOnDay = mergeScheduleBreaksOnDay;
                employeeGroup.BreakRoundingUp = breakRoundingUp;
                employeeGroup.BreakRoundingDown = breakRoundingDown;
                employeeGroup.BreakDayMinutesAfterMidnight = breakDayMinutesAfterMidnight;
                employeeGroup.KeepStampsTogetherWithinMinutes = keepStampsTogetherWithinMinutes;
                employeeGroup.TimeDeviationCauseId = defaultTimeDeviationCauseId;
                employeeGroup.TimeCodeId = timeCodeId;
                employeeGroup.RuleWorkTimeWeek = ruleWorkTimeWeekMinutes;
                employeeGroup.RuleWorkTimeDayMinimum = ruleWorkTimeDayMinimumMinutes;
                employeeGroup.RuleWorkTimeDayMaximumWorkDay = ruleWorkTimeDayMaximumWorkDayMinutes;
                employeeGroup.RuleWorkTimeDayMaximumWeekend = ruleWorkTimeDayMaximumWeekendMinutes;
                employeeGroup.MaxScheduleTimeFullTime = maxScheduleTimeFullTimeMinutes;
                employeeGroup.MinScheduleTimeFullTime = minScheduleTimeFullTimeMinutes;
                employeeGroup.MaxScheduleTimePartTime = maxScheduleTimePartTimeMinutes;
                employeeGroup.MinScheduleTimePartTime = minScheduleTimePartTimeMinutes;
                employeeGroup.MaxScheduleTimeWithoutBreaks = maxScheduleTimeWithoutBreaks;
                employeeGroup.RuleRestTimeDay = ruleRestTimeDaykMinutes;
                employeeGroup.RuleRestTimeDayStartTime = ruleRestTimeDayStartTime;
                employeeGroup.RuleRestTimeWeek = ruleRestTimeWeekMinutes;
                employeeGroup.RuleRestTimeWeekStartTime = ruleRestTimeWeekStartTime;
                employeeGroup.RuleRestTimeWeekStartDayNumber = ruleRestTimeWeekStartDayNumber;
                employeeGroup.RuleScheduleFreeWeekendsMinimumYear = ruleScheduleFreeWeekendsMinimumYear;
                employeeGroup.RuleScheduledDaysMaximumWeek = ruleScheduledDaysMaximumWeek;
                employeeGroup.ReminderAttestStateId = isAttestReminderValid ? reminderAttestStateId : (int?)null;
                employeeGroup.ReminderNoOfDays = isAttestReminderValid ? reminderNoOfDays : (int?)null;
                employeeGroup.ReminderPeriodType = isAttestReminderValid ? reminderPeriodType : (int?)null;
                employeeGroup.TimeReportType = timeReportType;
                employeeGroup.AutogenTimeblocks = employeeGroup.TimeReportType == (int)TermGroup_TimeReportType.Deviation;
                employeeGroup.QualifyingDayCalculationRule = qualifyingDayCalculationRule;
                employeeGroup.QualifyingDayCalculationRuleLimitFirstDay = qualifyingDayCalculationRuleLimitFirstDay;
                employeeGroup.TimeWorkReductionCalculationRule = timeWorkReductionCalculationRule;
                employeeGroup.ExternalCodesString = externalCodes;
                employeeGroup.NotifyChangeOfDeviations = notifyChangeOfDeviations;
                employeeGroup.RuleRestDayIncludePresence = ruleRestDayIncludePresence;
                employeeGroup.RuleRestWeekIncludePresence = ruleRestWeekIncludePresence;
                employeeGroup.AllowShiftsWithoutAccount = allowShiftsWithoutAccount;
                employeeGroup.AlsoAttestAdditionsFromTime = alsoAttestAdditionsFromTime;
                employeeGroup.CandidateForOvertimeOnZeroDayExcluded = candidateForOvertimeOnZeroDayExcluded;
                employeeGroup.ExtraShiftAsDefault = extraShiftAsDefault;

                if (em.UpdateEmployeeGroup(employeeGroup, updateWorkPercentageOnEmployees).Success)
                {
                    // DayTypes
                    if (!em.SaveEmployeeGroupDayTypesWorking(dayTypesItems, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("UPDATED_WITH_DAYTYPEERRORS", true);
                    // DayTypes HolidaySalary
                    if (!em.SaveEmployeeGroupDayTypesHolidaySalary(dayTypesHolidaySalaryItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_DAYTYPEERRORS");
                    // TimeAccumulators
                    if (!em.SaveEmployeeGroupTimeAccumulators(timeAccumulatorsItems, employeeGroup.EmployeeGroupId, UserId).Success)
                        RedirectToSelf("UPDATED_WITH_ACCUMULATORERRORS", true);
                    // TimeAccumulator settings
                    if (!EmployeeGroupAccumulatorSettings.SaveForEmployeeGroup(F, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("UPDATED_WITH_ACCUMULATORSETTINGERRORS", true);
                    // TimeDeviationCauses
                    if (!em.SaveEmployeeGroupTimeDeviationCauses(timeDeviationCausesItems, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("UPDATED_WITH_DEVIATIONCAUSEERRORS", true);
                    // TimeDeviationCauseRequests
                    if (!em.SaveEmployeeGroupTimeDeviationCauseRequests(timeDeviationCauseRequestsItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("UPDATED_WITH_DEVIATIONCAUSEREQUESTSERRORS", true);
                    // TimeDeviationCauseAbsenceAnnouncements
                    if (!em.SaveEmployeeGroupTimeDeviationCauseAbsenceAnnouncements(timeDeviationCauseAbsenceAnnouncementItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("UPDATED_WITH_DEVIATIONCAUSEABSENCEANNOUNCEMENTSERRORS", true);
                    // EmployeeGroupTimeCodes
                    if (!em.SaveEmployeeGroupTimeCodes(employeeGroupTimeCodesItems, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId).Success)
                        RedirectToSelf("UPDATED_WITH_TIMECODEMAPPINGERRORS", true);
                    // EmployeeGroupRuleWorkTimePeriod
                    if (!em.SaveEmployeeGroupRuleWorkTimePeriod(planningPeriodItems, employeeGroup.EmployeeGroupId).Success)
                        RedirectToSelf("SAVED_WITH_RULEWORKTIMEPERIODERRORS");
                    // AttestTransitions
                    if (!AttestTransitions.SaveEmployeeGroupAttestTransitions(F, employeeGroup.EmployeeGroupId))
                        RedirectToSelf("UPDATED_WITH_TRANSITIONERRORS", true);
                    // TimeCodeTimeDeviationCauses
                    if (!TimeCodeTimeDeviationCauses.SaveEmployeeGroupMappings(F, employeeGroup.EmployeeGroupId, SoeCompany.ActorCompanyId, UserId))
                        RedirectToSelf("UPDATED_WITH_TIMECODETIMEDEVIATIONCAUSEERRORS", true);
                    // Standard accounts
                    if (!SaveAccountStd(EmployeeGroupAccountType.Cost) || !SaveAccountStd(EmployeeGroupAccountType.Income))
                        RedirectToSelf("UPDATED_WITH_ACCOUNTERRORS", true);
                    // TimeStampRounding
                    if (!SaveTimeStampRoundingSetting())
                        RedirectToSelf("UPDATED_WITH_TIMESTAMPERRORS", true);

                    RedirectToSelf("UPDATED");
                }
                else
                {
                    RedirectToSelf("NOTUPDATED", true);
                }

                #endregion
            }
        }

        private bool SaveAccountStd(EmployeeGroupAccountType type)
        {
            #region Init

            string prefix = "";
            switch (type)
            {
                case EmployeeGroupAccountType.Cost:
                    prefix = "CostAccount";
                    break;
                case EmployeeGroupAccountType.Income:
                    prefix = "IncomeAccount";
                    break;
                default:
                    return false;
            }

            #endregion

            #region Prereq

            // Get entered AccountStd
            string accountNr = F[prefix];
            AccountStd accountStd = null;
            if (!String.IsNullOrEmpty(accountNr))
                accountStd = acm.GetAccountStdByNr(accountNr, SoeCompany.ActorCompanyId);

            // Get entered AccountInternals
            List<int> accountInternalIds = new List<int>();
            foreach (AccountDim accountDim in accountDims)
            {
                // Only AccountInternals
                if (accountDim.IsStandard)
                    continue;

                string id = prefix + accountDim.AccountDimId;

                if (Int32.TryParse(F[id], out int accountId) && accountId > 0)
                    accountInternalIds.Add(accountId);
            }

            #endregion

            // Check if there is an existing EmployeeGroupAccountStd
            EmployeeGroupAccountStd employeeGroupAccountStd = employeeGroupAccounts.GetEmployeeGroupAccount(type);

            if (accountStd != null || accountInternalIds.Count > 0)
            {
                if (employeeGroupAccountStd == null)
                {
                    #region Add

                    employeeGroupAccountStd = new EmployeeGroupAccountStd()
                    {
                        Type = (int)type,
                        Percent = 100,
                    };

                    // Set AccountStd
                    if (accountStd != null)
                        employeeGroupAccountStd.AccountId = accountStd.AccountId;

                    if (!em.AddEmployeeGroupAccountStd(employeeGroupAccountStd, employeeGroup.EmployeeGroupId).Success)
                        return false;

                    // Update EmployeeGroupAccountStd references
                    switch (type)
                    {
                        case EmployeeGroupAccountType.Cost:
                            costAccount = employeeGroupAccountStd;
                            break;
                        case EmployeeGroupAccountType.Income:
                            incomeAccount = employeeGroupAccountStd;
                            break;
                    }

                    #endregion
                }
                else
                {
                    #region Update

                    // Set AccountStd
                    if (accountStd != null)
                        employeeGroupAccountStd.AccountId = accountStd.AccountId;
                    else
                        employeeGroupAccountStd.AccountId = null;

                    // Update
                    if (!em.UpdateEmployeeGroupAccountStd(employeeGroupAccountStd).Success)
                        return false;

                    #endregion
                }

                #region AccountInternals

                if (!em.AddEmployeeGroupAccountInternals(employeeGroupAccountStd, accountInternalIds, SoeCompany.ActorCompanyId).Success)
                    return false;

                #endregion
            }
            else
            {
                #region Delete

                if (employeeGroupAccountStd != null && !em.DeleteEmployeeGroupAccountStd(employeeGroupAccountStd).Success)
                    return false;

                #endregion
            }

            return true;
        }

        private bool SaveTimeStampRoundingSetting()
        {
            int.TryParse(F["SchemaIn-from-1"], out int roundInNeg);
            int.TryParse(F["SchemaIn-to-1"], out int roundInPos);
            int.TryParse(F["SchemaUt-from-1"], out int roundOutNeg);
            int.TryParse(F["SchemaUt-to-1"], out int roundOutPos);

            TimeStampRounding rounding = tsm.GetTimeStampRoundingByEmployeeGroup(employeeGroup.EmployeeGroupId);
            if (rounding == null)
                rounding = new TimeStampRounding();

            //Value before
            rounding.RoundInNeg = roundInNeg;
            //Value after
            rounding.RoundInPos = roundInPos;
            //Value before
            rounding.RoundOutNeg = roundOutNeg;
            //Value after
            rounding.RoundOutPos = roundOutPos;

            if (rounding.TimeStampRoundingId > 0)
                return tsm.UpdateTimeStampRounding(rounding).Success;
            else
                return tsm.AddTimeStampRounding(rounding, employeeGroup.EmployeeGroupId).Success;
        }

        protected override void Delete()
        {
            var result = em.DeleteEmployeeGroup(employeeGroup);
            if (result.Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion

        #region Help-methods

        private void GetAccounts()
        {
            // Get AccountDims
            accountDims = acm.GetAccountDimsByCompany(SoeCompany.ActorCompanyId);

            // Get company default accounts
            defaultCostAccountNr = acm.GetAccountNr(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupCost, UserId, SoeCompany.ActorCompanyId, 0), true);
            defaultIncomeAccountNr = acm.GetAccountNr(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, UserId, SoeCompany.ActorCompanyId, 0), true);

            if (employeeGroup != null)
            {
                //Get CustomerAccounts once
                employeeGroupAccounts = em.GetEmployeeGroupAccounts(employeeGroup.EmployeeGroupId);

                // AccountStd Cost
                costAccount = employeeGroupAccounts.GetEmployeeGroupAccount(EmployeeGroupAccountType.Cost);

                // AccountStd Income
                incomeAccount = employeeGroupAccounts.GetEmployeeGroupAccount(EmployeeGroupAccountType.Income);

                // AccountInternals Cost
                costAccountDict = new Dictionary<int, int>();
                if (costAccount != null)
                    costAccountDict = em.GetEmployeeGroupInternalsDict(costAccount, accountDims);

                // AccountInternals Income
                incomeAccountDict = new Dictionary<int, int>();
                if (incomeAccount != null)
                    incomeAccountDict = em.GetEmployeeGroupInternalsDict(incomeAccount, accountDims);
            }
        }

        private void GetTimePeriodTypes()
        {
            timePeriodTypesDict = GetGrpText(TermGroup.TimePeriodHeadType);
        }

        #endregion
    }
}
