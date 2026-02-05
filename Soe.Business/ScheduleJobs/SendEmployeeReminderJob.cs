using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SendEmployeeReminderJob : ScheduledJobBase, IScheduledJob
    {
        #region Variables

        private DateTime date;
        private DateTime logsFromDate;
        private DateTime logsToDate;
        private List<Employee> employees = null;
        private Dictionary<int, List<GenericType>> termsDict = null;
        private Company currentCompany;
        private int totalCompanies;
        private int companyCounter;
        private int noOfSentMails;
        private string companyDefaultSenderEmailAddress;

        private CommunicationManager ccm = null;
        private EmailManager emailm = null;
        private EmployeeManager em = null;
        private GeneralManager gm = null;
        private SettingManager sm = null;
        private TimeRuleManager trm = null;
        private TimeTransactionManager ttm = null;
        private TimeScheduleManager tsm = null;
        private UserManager um = null;

        #endregion

        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            ccm = new CommunicationManager(parameterObject);
            em = new EmployeeManager(parameterObject);
            emailm = new EmailManager(parameterObject);
            gm = new GeneralManager(parameterObject);
            sm = new SettingManager(parameterObject);
            trm = new TimeRuleManager(parameterObject);
            ttm = new TimeTransactionManager(parameterObject);
            tsm = new TimeScheduleManager(parameterObject);
            um = new UserManager(parameterObject);

            int userId = scheduledJob.ExecuteUserId;
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    var companies = base.GetCompanies(paramCompanyId);

                    this.termsDict = new Dictionary<int, List<GenericType>>();
                    this.totalCompanies = companies.Count;
                    this.date = DateTime.Now.Hour < 5 ? DateTime.Today.AddDays(-1) : DateTime.Today; //If job is execute after midnight, but before 5am, assume yesterday is the day to check
                    this.logsFromDate = CalendarUtility.GetBeginningOfDay(date.AddDays(-1));
                    this.logsToDate = CalendarUtility.GetEndOfDay(date);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb påminnelse för anställda och chefer (läkarintyg, försäkringskassan, anställningar, branchvana och ålder)");

                    using (CompEntities entities = new CompEntities())
                    {
                        foreach (Company company in companies.OrderBy(o => o.ActorCompanyId))
                        {
                            #region Company

                            StartNewCompany(company);

                            companyDefaultSenderEmailAddress = this.sm.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, 0, company.ActorCompanyId, 0);
                            if (companyDefaultSenderEmailAddress == null)
                                companyDefaultSenderEmailAddress = "";
                            else
                                companyDefaultSenderEmailAddress = companyDefaultSenderEmailAddress.Trim();

                            try
                            {
                                SendSickNoteAndSIAReminder(entities, company);
                            }
                            catch (Exception ex)
                            {
                                JobFailed(ex, company, "SendSickNoteAndSIAReminder");
                            }
                            try
                            {
                                SendEmploymentReminder(entities, company);
                            }
                            catch (Exception ex)
                            {
                                JobFailed(ex, company, "SendEmploymentReminder");
                            }
                            try
                            {
                                SendExperienceReminderMonths(entities, company);
                            }
                            catch (Exception ex)
                            {
                                JobFailed(ex, company, "SendExperienceReminderMonths");
                            }
                            try
                            {
                                SendUpdateExperienceMonthsReminder(entities, company);
                            }
                            catch (Exception ex)
                            {
                                JobFailed(ex, company, "SendUpdateExperienceMonthsReminder");
                            }
                            try
                            {
                                SendReminderDaysBeforeEmployeeAgeReached(entities, company);
                            }
                            catch (Exception ex)
                            {
                                JobFailed(ex, company, "SendReminderDaysBeforeEmployeeAgeReached");
                            }
                            try
                            {
                                SendReminderAfterLongAbsence(entities, company);
                            }
                            catch (Exception ex)
                            {
                                JobFailed(ex, company, "jobbSendReminderAfterLongAbsence");
                            }

                            #endregion

                            jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, "Skickade " + this.noOfSentMails.ToString() + " påminnelsemail för " + company.Name);
                            companyDefaultSenderEmailAddress = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);

                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }

            void JobFailed(Exception ex, Company company, string job)
            {
                string message = $"Fel vid exekvering av jobb {job}";
                result = new ActionResult(ex);
                CreateLogEntry(ScheduledJobLogLevel.Error, $"{company.Name} {message}: {result.ErrorMessage}");
                LogCollector.LogError($"{company.Name} {message}: {result.ErrorMessage}");
            }
        }

        private void SendSickNoteAndSIAReminder(CompEntities entities, Company company)
        {
            if (company == null)
                return;

            User adminUser = um.GetAdminUser(company.ActorCompanyId);
            if (adminUser == null)
                return;

            //Reminder sick note for user after x days
            SystemInfoSetting settingReminderIllness = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllness, company.ActorCompanyId);
            SystemInfoSetting settingReminderIllnessDays = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessDays, company.ActorCompanyId);
            bool hasSettingUserReminderIllness = settingReminderIllness != null && settingReminderIllness.BoolData == true;
            bool hasSettingUserReminderIllnessDays = settingReminderIllnessDays != null && settingReminderIllnessDays.IntData.HasValue && settingReminderIllnessDays.IntData.Value > 0;
            bool hasSettingUser = hasSettingUserReminderIllness && hasSettingUserReminderIllnessDays;

            //Reminder social insurancy agency for admin after x days
            SystemInfoSetting settingReminderIllnessSIA = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessSocialInsuranceAgency, company.ActorCompanyId);
            SystemInfoSetting settingReminderIllnessDaysSIA = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessDaysSocialInsuranceAgency, company.ActorCompanyId);
            SystemInfoSetting settingReminderIllnessEmailSIA = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderIllnessEmailSocialInsuranceAgency, company.ActorCompanyId);
            bool hasSettingAdminReminderIllnessSIA = settingReminderIllnessSIA != null && settingReminderIllnessSIA.BoolData == true;
            bool hasSettingAdminReminderIllnessDaysSIA = settingReminderIllnessDaysSIA != null && settingReminderIllnessDaysSIA.IntData.HasValue && settingReminderIllnessDaysSIA.IntData.Value > 0;
            bool hasSettingAdmin = hasSettingAdminReminderIllnessSIA && hasSettingAdminReminderIllnessDaysSIA;
            string emailSIA = settingReminderIllnessEmailSIA?.StrData ?? "";


            //Check that has either user or admin setting
            if (!hasSettingUser && !hasSettingAdmin)
                return;

            List<TimeAbsenceRuleHead> timeAbsenceRules = trm.GetTimeAbsenceRules(entities, new GetTimeAbsenceRulesInput(company.ActorCompanyId));
            TimeAbsenceRuleHead timeAbsenceRule = timeAbsenceRules?.FirstOrDefault(i => i.Type == (int)TermGroup_TimeAbsenceRuleType.Sick_PAID) ?? timeAbsenceRules?.FirstOrDefault(i => i.Type == (int)TermGroup_TimeAbsenceRuleType.Sick_UNPAID);
            if (timeAbsenceRule == null)
                return;

            if (!TryLoadEmployees())
                return;

            TermGroup_SysPayrollType sysPayrollTypeLevel1 = TermGroup_SysPayrollType.SE_GrossSalary;
            TermGroup_SysPayrollType sysPayrollTypeLevel2 = TermGroup_SysPayrollType.SE_GrossSalary_Absence;
            TermGroup_SysPayrollType sysPayrollTypeLevel3 = TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick;
            Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsForDayByEmployee = ttm.GetTimePayrollTransactionsForCompany(entities, company.ActorCompanyId, date, sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3).GroupBy(tpt => tpt.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
            if (timePayrollTransactionsForDayByEmployee.IsNullOrEmpty())
                return;

            List<SystemInfoLog> reminderIllnessLogs = gm.GetSystemInfoLogEntries(company.ActorCompanyId, SystemInfoType.ReminderIllness, activeOnly: false);
            List<SystemInfoLog> reminderIllnessSocialInsuranceAgencyLogs = gm.GetSystemInfoLogEntries(company.ActorCompanyId, SystemInfoType.ReminderIllnessSocialInsuranceAgency, activeOnly:false);
            TimeEngineManager tem = new TimeEngineManager(parameterObject, company.ActorCompanyId, adminUser.UserId);

            //Settings for day of illness            
            int maxDays = 0;
            if (hasSettingUser)
                maxDays = settingReminderIllnessDays.IntData.Value;
            if (hasSettingAdmin && settingReminderIllnessDaysSIA.IntData.Value > maxDays)
                maxDays = settingReminderIllnessDaysSIA.IntData.Value;

            foreach (Employee employee in this.employees.Where(w=> !w.ExcludeFromPayroll))
            {
                try
                {
                    List<TimePayrollTransaction> timePayrollTransactionsForEmployeeAndDay = timePayrollTransactionsForDayByEmployee.GetList(employee.EmployeeId, nullIfNotFound: true);
                    if (timePayrollTransactionsForEmployeeAndDay.IsNullOrEmpty())
                        continue;

                    var (illnessDayNumber, qualifyingDate) = tem.GetDayOfAbsenceNumber(employee.EmployeeId, date, sysPayrollTypeLevel3, maxDays + Constants.SICKNESS_RELAPSEDAYS, Constants.SICKNESS_RELAPSEDAYS);
                    if (illnessDayNumber <= 0)
                        continue;

                    var terms = GetTerms(employee);

                    #region Reminder sick note for user

                    int settingIllnessDays = employee.MedicalCertificateDays.HasValidValue() ? employee.MedicalCertificateDays.Value : settingReminderIllnessDays.IntData.Value;
                    if (hasSettingUser && employee.User != null && settingIllnessDays > 0 && (illnessDayNumber == settingIllnessDays || illnessDayNumber - 1 == settingIllnessDays))
                    {
                        //Check if reminder has already been sent for this day or day before
                        if (reminderIllnessLogs.HasSystemInfoLogs(employee.User.UserId, logsFromDate, logsToDate))
                            continue;

                        var subjectTerm = terms.FirstOrDefault(p => p.Id == 1);
                        var textTerm = terms.FirstOrDefault(p => p.Id == 2);
                        var logmessageTerm = terms.FirstOrDefault(p => p.Id == 3);
                        if (subjectTerm == null || textTerm == null || logmessageTerm == null)
                            continue;

                        if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, employee.User.UserId, adminUser.UserId, subjectTerm.Name, textTerm.Name).Success)
                        {
                            if (this.AddSystemInfoLogEntry(entities, company, employee, employee.User, SystemInfoType.ReminderIllness, subjectTerm.Name, logmessageTerm.Name).Success)
                                noOfSentMails += 1;
                        }
                    }

                    #endregion

                    #region Reminder social insurancy agency for admin

                    if (hasSettingAdmin && (illnessDayNumber == settingReminderIllnessDaysSIA.IntData.Value || illnessDayNumber - 1 == settingReminderIllnessDaysSIA.IntData.Value))
                    {
                        var subjectTerm = terms.FirstOrDefault(p => p.Id == 4);
                        var textTerm = terms.FirstOrDefault(p => p.Id == 5);
                        var logmessageTerm = terms.FirstOrDefault(p => p.Id == 3);
                        var scheduleTerm = terms.FirstOrDefault(p => p.Id == 12);
                        var sickTerm = terms.FirstOrDefault(p => p.Id == 13);
                        if (subjectTerm == null || textTerm == null || logmessageTerm == null)
                            continue;

                        string subject = subjectTerm.Name + " " + employee.EmployeeNrAndName;
                        string text = employee.EmployeeNrAndName + Environment.NewLine + textTerm.Name + Environment.NewLine;
                        string absenceInformation = "";
                        List<TimePayrollTransaction> transactions = ttm.GetTimePayrollTransactionsForEmployee(entities, employee.EmployeeId, qualifyingDate, date, loadTimeBlockDate: true).Where(x => x.SysPayrollTypeLevel1 == (int)sysPayrollTypeLevel1 && x.SysPayrollTypeLevel2 == (int)sysPayrollTypeLevel2 && x.SysPayrollTypeLevel3 == (int)sysPayrollTypeLevel3).ToList();
                        List<TimeScheduleTemplateBlock> scheduleBlocks = tsm.GetTimeScheduleTemplateBlocks(entities, employee.EmployeeId, qualifyingDate, date, includeBreaks: true, includePrel: false);
                        //DateTime loopDate = qualifyingDate.Date;
                        while (qualifyingDate <= date)
                        {
                            absenceInformation += qualifyingDate.ToShortDateString() + " ";

                            List<TimeScheduleTemplateBlock> currentDayScheduledShifts = scheduleBlocks.Where(b => b.Date.HasValue && b.Date.Value == qualifyingDate).ToList();
                            absenceInformation += CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(currentDayScheduledShifts.GetWorkMinutes()), false, false) + "h " + (scheduleTerm?.Name ?? "Schema") + ",";

                            List<TimePayrollTransaction> currentDayTransactions = transactions.Where(x => x.TimeBlockDate.Date == qualifyingDate).ToList();
                            absenceInformation += CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(currentDayTransactions.GetMinutes()), false, false) + "h " + (sickTerm?.Name ?? "Sjuk");

                            absenceInformation += Environment.NewLine;
                            qualifyingDate = qualifyingDate.AddDays(1);
                        }

                        text += Environment.NewLine + Environment.NewLine;
                        text += absenceInformation;

                        List<UserDTO> users = um.GetEmployeeNearestExecutives(employee, DateTime.Today.AddDays(-illnessDayNumber), DateTime.Today, company.ActorCompanyId);

                        bool hasExecutives = users.Any();

                        if (hasExecutives)
                        {
                            foreach (UserDTO user in users)
                            {
                                //Check if reminder has already been sent for this day or day before
                                if (reminderIllnessSocialInsuranceAgencyLogs.HasSystemInfoLogs(user.UserId, logsFromDate, logsToDate, employee.EmployeeId))
                                {
                                    continue;
                                }

                                if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, user.UserId, adminUser.UserId, subject, text).Success && this.AddSystemInfoLogEntry(entities, company, employee, user, SystemInfoType.ReminderIllnessSocialInsuranceAgency, subjectTerm.Name, logmessageTerm.Name).Success)
                                    noOfSentMails += 1;
                            }
                        }

                        // Control if email is already sent, Setting userid to Int.MinValue since no user is connected to this process
                        if (!string.IsNullOrEmpty(companyDefaultSenderEmailAddress) && !string.IsNullOrEmpty(emailSIA) && !reminderIllnessSocialInsuranceAgencyLogs.HasSystemInfoLogs(int.MinValue, logsFromDate, logsToDate, employee.EmployeeId))
                        {
                            this.AddSystemInfoLogEntry(entities, company, employee, new UserDTO() { UserId = int.MinValue }, SystemInfoType.ReminderIllnessSocialInsuranceAgency, subjectTerm.Name, logmessageTerm.Name);
                            this.SendEmail(companyDefaultSenderEmailAddress, new string[] { emailSIA }, subject, text);
                        }
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    LogCollector.LogError($"SendSickNoteAndSIAReminder employeeId: {employee.EmployeeId} " + ex.ToString());
                }
            }
        }

        private void SendEmploymentReminder(CompEntities entities, Company company)
        {
            if (company == null)
                return;

            User adminUser = um.GetAdminUser(company.ActorCompanyId);
            if (adminUser == null)
                return;

            //Reminder for user after x days
            SystemInfoSetting settingReminderEmployment = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderEmployment, company.ActorCompanyId);
            SystemInfoSetting settingReminderEmploymentDays = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderEmploymentDays, company.ActorCompanyId);
            bool hasSettingReminderEmployment = settingReminderEmployment != null && settingReminderEmployment.BoolData == true;
            bool hasSettingReminderEmploymentDays = settingReminderEmploymentDays != null && settingReminderEmploymentDays.IntData.HasValue && settingReminderEmploymentDays.IntData.Value > 0;
            bool hasSettingUser = hasSettingReminderEmployment && hasSettingReminderEmploymentDays;
            if (!hasSettingUser)
                return;

            if (!TryLoadEmployees())
                return;

            List<SystemInfoLog> reminderEmploymentLogs = gm.GetSystemInfoLogEntries(company.ActorCompanyId, SystemInfoType.ReminderEmployment);

            foreach (Employee employee in this.employees)
            {
                try
                {
                    #region Employee

                    var terms = GetTerms(employee);

                    #endregion

                    #region Reminder employment

                    DateTime? employmentEndDate = null;
                    foreach (Employment employment in employee.GetActiveEmployments(includeSecondary: true))
                    {
                        DateTime? endDate = employment.GetEndDate();
                        bool employmentIsEnding = endDate.HasValue && endDate.Value.AddDays(-settingReminderEmploymentDays.IntData.Value) == DateTime.Today;
                        if (employmentIsEnding)
                        {
                            employmentEndDate = endDate;
                            break;
                        }
                    }

                    if (!employmentEndDate.HasValue)
                        continue;

                    var subjectTerm = terms.FirstOrDefault(p => p.Id == 6);
                    var textTerm = terms.FirstOrDefault(p => p.Id == 7);
                    var logmessageTerm = terms.FirstOrDefault(p => p.Id == 3);
                    if (subjectTerm == null || textTerm == null || logmessageTerm == null)
                        continue;

                    var users = um.GetEmployeeNearestExecutives(employee, DateTime.Today, DateTime.Today, company.ActorCompanyId);
                    if (users != null)
                    {
                        foreach (var user in users)
                        {
                            //Check if reminder has already been sent for this day or day before
                            if (reminderEmploymentLogs.HasSystemInfoLogs(user.UserId, logsFromDate, logsToDate, employee.EmployeeId))
                                continue;

                            if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, user.UserId, adminUser.UserId, subjectTerm.Name + " " + employee.EmployeeNrAndName, String.Format(textTerm.Name, employee.EmployeeNrAndName, employmentEndDate.Value)).Success)
                            {
                                if (this.AddSystemInfoLogEntry(entities, company, employee, user, SystemInfoType.ReminderEmployment, subjectTerm.Name, logmessageTerm.Name).Success)
                                    noOfSentMails += 1;
                            }
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    LogCollector.LogError($"SendEmploymentReminder employeeId: {employee.EmployeeId} " + ex.ToString());
                }
            }
        }

        private void SendExperienceReminderMonths(CompEntities entities, Company company)
        {
            if (company == null)
                return;

            User adminUser = um.GetAdminUser(company.ActorCompanyId);
            if (adminUser == null)
                return;

            //Reminder for user after x days
            SystemInfoSetting useEmployeeExperienceReminderMonths = sm.GetSystemInfoSetting((int)SystemInfoType.UseEmployeeExperienceReminder, company.ActorCompanyId);
            if (useEmployeeExperienceReminderMonths == null || (useEmployeeExperienceReminderMonths.BoolData.HasValue && !useEmployeeExperienceReminderMonths.BoolData.Value))
                return;

            if (!sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, company.ActorCompanyId, 0))
                return;

            SystemInfoSetting settingExperienceReminderMonths = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeExperienceReminderMonths, company.ActorCompanyId);
            SystemInfoSetting settingReminderDaysBeforeEmployeeExperienceReached = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderDaysBeforeEmployeeExperienceReached, company.ActorCompanyId);
            bool hasSettingExperienceReminderMonths = settingExperienceReminderMonths != null && !string.IsNullOrEmpty(settingExperienceReminderMonths.StrData) && settingExperienceReminderMonths.StrData.Length > 1;
            bool hasSettingReminderDaysBeforeEmployeeExperienceReached = settingReminderDaysBeforeEmployeeExperienceReached != null && settingReminderDaysBeforeEmployeeExperienceReached.IntData.HasValue && settingReminderDaysBeforeEmployeeExperienceReached.IntData.Value > 0;
            bool hasAllNeededSettings = hasSettingExperienceReminderMonths && hasSettingReminderDaysBeforeEmployeeExperienceReached;
            if (!hasAllNeededSettings)
                return;

            if (!TryLoadEmployees())
                return;

            List<int> months = StringUtility.SplitNumericList(settingExperienceReminderMonths.StrData, false, true);
            if (!months.IsNullOrEmpty())
            {
                bool useExperienceMonthsOnEmploymentAsStartValue = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, company.ActorCompanyId, 0);
                List<SystemInfoLog> employeeExperienceReminderMonthsLogs = gm.GetSystemInfoLogEntries(company.ActorCompanyId, SystemInfoType.EmployeeExperienceReminderMonths, activeOnly: false);

                foreach (Employee employee in this.employees)
                {
                    int experienceMonths = 0;
                    DateTime dayOfExperience = DateTime.Today.AddDays(settingReminderDaysBeforeEmployeeExperienceReached.IntData.Value);

                    Employment employment = employee.GetEmployment(dayOfExperience);
                    if (employment != null)
                    {
                        int experience = em.GetExperienceMonths(entities, company.ActorCompanyId, employment, useExperienceMonthsOnEmploymentAsStartValue, dayOfExperience);
                        if (experience != 0 && months.Contains(experience))
                            experienceMonths = experience;
                    }
                    if (experienceMonths == 0)
                        continue;

                    var terms = GetTerms(employee);
                    var subjectTerm = terms.FirstOrDefault(p => p.Id == 8);
                    if (subjectTerm == null)
                        continue;
                    var subject = String.Format(subjectTerm.Name, employee.EmployeeNrAndName, experienceMonths.ToString());
                    var textTerm = subject;
                    var logmessageTerm = subject;

                    List<UserDTO> users = um.GetEmployeeNearestExecutives(employee, DateTime.Today, DateTime.Today, company.ActorCompanyId);
                    if (users != null)
                    {
                        foreach (UserDTO user in users)
                        {
                            //Check if reminder has already been sent for this day or day before
                            DateTime checkFrom = DateTime.Now.AddYears(-1);
                            if (employeeExperienceReminderMonthsLogs.HasSystemInfoLogs(user.UserId, checkFrom, logsToDate, employee.EmployeeId, subject))
                                continue;

                            if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, user.UserId, adminUser.UserId, subject, textTerm).Success)
                                noOfSentMails += 1;

                            this.AddSystemInfoLogEntry(entities, company, employee, user, SystemInfoType.EmployeeExperienceReminderMonths, subject, logmessageTerm, true);
                        }
                    }
                }
            }
        }

        private void SendUpdateExperienceMonthsReminder(CompEntities entities, Company company)
        {
            if (company == null)
                return;

            User adminUser = um.GetAdminUser(company.ActorCompanyId);
            if (adminUser == null)
                return;

            SystemInfoSetting useUpdateEmployeeExperienceReminder = sm.GetSystemInfoSetting((int)SystemInfoType.UseUpdateEmployeeExperienceReminder, company.ActorCompanyId);
            if (useUpdateEmployeeExperienceReminder == null || (useUpdateEmployeeExperienceReminder.BoolData.HasValue && !useUpdateEmployeeExperienceReminder.BoolData.Value))
                return;

            if (!sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, company.ActorCompanyId, 0))
                return;

            if (!TryLoadEmployees())
                return;

            foreach (Employee employee in this.employees)
            {
                bool needReminder = employee.GetActiveEmployments().Any(x => x.DateFrom.HasValue && x.DateFrom.Value.Date < DateTime.Today.Date && x.UpdateExperienceMonthsReminder && x.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.None);
                if (needReminder)
                {
                    var terms = GetTerms(employee);
                    var subjectTerm = terms.FirstOrDefault(p => p.Id == 11);
                    if (subjectTerm == null)
                        continue;
                    var subject = string.Format(subjectTerm.Name, employee.EmployeeNrAndName);
                    var textTerm = subject;
                    var logmessageTerm = subject;

                    List<UserDTO> users = um.GetEmployeeNearestExecutives(employee, DateTime.Today, DateTime.Today, company.ActorCompanyId);
                    if (users != null)
                    {
                        foreach (UserDTO user in users)
                        {
                            if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, user.UserId, adminUser.UserId, subject, textTerm).Success)
                                noOfSentMails += 1;

                            this.AddSystemInfoLogEntry(entities, company, employee, user, SystemInfoType.UseUpdateEmployeeExperienceReminder, subject, logmessageTerm, true);
                        }
                    }
                }
            }
        }

        private void SendReminderDaysBeforeEmployeeAgeReached(CompEntities entities, Company company)
        {
            if (company == null)
                return;

            User adminUser = um.GetAdminUser(company.ActorCompanyId);
            if (adminUser == null)
                return;

            //Reminder for user after x days
            SystemInfoSetting useUseEmployeeAgeReminder = sm.GetSystemInfoSetting((int)SystemInfoType.UseEmployeeAgeReminder, company.ActorCompanyId);
            if (useUseEmployeeAgeReminder == null || (useUseEmployeeAgeReminder.BoolData.HasValue && !useUseEmployeeAgeReminder.BoolData.Value))
                return;

            SystemInfoSetting settingUseEmployeeAgeReminder = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeAgeReminderAges, company.ActorCompanyId);
            SystemInfoSetting settingReminderDaysBeforeEmployeeAgeReached = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderDaysBeforeEmployeeAgeReached, company.ActorCompanyId);
            bool hasSettingUseEmployeeAgeReminder = settingUseEmployeeAgeReminder != null && !string.IsNullOrEmpty(settingUseEmployeeAgeReminder.StrData) && settingUseEmployeeAgeReminder.StrData.Length > 1;
            bool hasSettingReminderDaysBeforeEmployeeAgeReached = settingReminderDaysBeforeEmployeeAgeReached != null && settingReminderDaysBeforeEmployeeAgeReached.IntData.HasValue && settingReminderDaysBeforeEmployeeAgeReached.IntData.Value > 0;
            bool hasAllNeededSettings = hasSettingUseEmployeeAgeReminder && hasSettingReminderDaysBeforeEmployeeAgeReached;
            if (!hasAllNeededSettings)
                return;

            if (!TryLoadEmployees())
                return;

            List<int> years = StringUtility.SplitNumericList(settingUseEmployeeAgeReminder.StrData, false, true);
            if (!years.IsNullOrEmpty())
            {
                //Existing logs
                var useEmployeeAgeReminderLogs = gm.GetSystemInfoLogEntries(company.ActorCompanyId, SystemInfoType.UseEmployeeAgeReminder, activeOnly: false);

                foreach (Employee employee in this.employees)
                {
                    try
                    {
                        DateTime birthDayThisYear = DateTime.MinValue;

                        int age = 0;
                        var employment = employee.GetEmployment(DateTime.Today.AddDays(Math.Abs(settingReminderDaysBeforeEmployeeAgeReached.IntData.Value)));
                        if (employment != null)
                        {
                            var birth = CalendarUtility.GetBirthDateFromSecurityNumber(employee.SocialSec);
                            if (birth == null)
                                continue;

                            foreach (int year in years)
                            {
                                birthDayThisYear = birth.Value.AddYears(year);
                                if (birthDayThisYear.Date != DateTime.Today.AddDays(Math.Abs(settingReminderDaysBeforeEmployeeAgeReached.IntData.Value)).Date)
                                    continue;

                                age = year;
                                break;
                            }
                        }

                        if (age == 0)
                            continue;

                        var terms = GetTerms(employee);
                        var subjectTerm = terms.FirstOrDefault(p => p.Id == 9);
                        if (subjectTerm == null)
                            continue;
                        var subject = String.Format(subjectTerm.Name, employee.EmployeeNrAndName, age.ToString()).Trim() + " " + birthDayThisYear.ToShortDateString();
                        var textTerm = subject;
                        var logmessageTerm = subject;

                        List<UserDTO> users = um.GetEmployeeNearestExecutives(employee, DateTime.Today, DateTime.Today, company.ActorCompanyId);
                        if (users != null)
                        {
                            foreach (UserDTO user in users)
                            {
                                //Check if reminder has already been sent for this day or day before
                                DateTime checkFrom = DateTime.Today.AddDays(-100);
                                if (useEmployeeAgeReminderLogs.HasSystemInfoLogs(user.UserId, checkFrom, logsToDate, employee.EmployeeId))
                                    continue;

                                if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, user.UserId, adminUser.UserId, subject, textTerm).Success)
                                    noOfSentMails += 1;

                                this.AddSystemInfoLogEntry(entities, company, employee, user, SystemInfoType.UseEmployeeAgeReminder, subject, logmessageTerm, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCollector.LogError($"SendReminderDaysBeforeEmployeeAgeReached employeeId: {employee.EmployeeId} " + ex.ToString());
                    }
                }
            }
        }

        private void SendReminderAfterLongAbsence(CompEntities entities, Company company)
        {
            if (company == null)
                return;

            User adminUser = um.GetAdminUser(company.ActorCompanyId);
            if (adminUser == null)
                return;

            //Reminder for user after x days
            SystemInfoSetting useReminderAfterLongAbsence = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderAfterLongAbsence, company.ActorCompanyId);
            if (useReminderAfterLongAbsence == null || (useReminderAfterLongAbsence.BoolData.HasValue && !useReminderAfterLongAbsence.BoolData.Value))
                return;

            SystemInfoSetting settingReminderAfterLongAbsenceDaysInAdvance = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderAfterLongAbsenceDaysInAdvance, company.ActorCompanyId);
            SystemInfoSetting settingIsReminderAfterLongAbsenceAfterDays = sm.GetSystemInfoSetting((int)SystemInfoType.IsReminderAfterLongAbsenceAfterDays, company.ActorCompanyId);
            bool hasAllNeededSettings = settingReminderAfterLongAbsenceDaysInAdvance != null && settingIsReminderAfterLongAbsenceAfterDays != null;
            if (!hasAllNeededSettings)
                return;

            int reminderAfterLongAbsenceDaysInAdvance = settingReminderAfterLongAbsenceDaysInAdvance.IntData ?? 0;
            int isReminderAfterLongAbsenceAfterDays = settingIsReminderAfterLongAbsenceAfterDays.IntData ?? 0;
            DateTime dateOfReminder = DateTime.Today.AddDays(-reminderAfterLongAbsenceDaysInAdvance);
            DateTime lastStopdate = DateTime.Today.AddDays(reminderAfterLongAbsenceDaysInAdvance + 1);
            if (reminderAfterLongAbsenceDaysInAdvance == 0 || isReminderAfterLongAbsenceAfterDays == 0)
                return;

            //Must have planned Absence
            List<EmployeeRequestDTO> employeeRequests = tsm.GetEmployeeRequests(entities, company.ActorCompanyId, lastStopdate).ToDTOs().ToList();
            List<SystemInfoLog> reminderAfterLongAbsenceLogs = gm.GetSystemInfoLogEntries(company.ActorCompanyId, SystemInfoType.ReminderAfterLongAbsence, activeOnly: false);
            foreach (var employeeRequestsByEmployee in employeeRequests.GroupBy(g => g.EmployeeId))
            {
                try
                {
                    DateTime today = DateTime.Today;

                    if (HasMoreThanLimitDaysCoherentEmployeeRequests(employeeRequestsByEmployee.ToList(), isReminderAfterLongAbsenceAfterDays, lastStopdate))
                    {
                        EmployeeRequestDTO employeeRequest = employeeRequestsByEmployee.FirstOrDefault(f => f.Start < today && f.Stop < lastStopdate && f.Stop > today);
                        if (employeeRequest != null)
                        {
                            Employee employee = entities.Employee.Include("User").Include("ContactPerson").FirstOrDefault(f => f.EmployeeId == employeeRequestsByEmployee.Key);
                            if (employee == null)
                                continue;

                            var terms = GetTerms(employee);
                            var subjectTerm = terms.FirstOrDefault(p => p.Id == 10);
                            if (subjectTerm == null)
                                continue;
                            var subject = String.Format(subjectTerm.Name, employee.EmployeeNrAndName, employeeRequest.Stop.ToShortDateString());
                            var textTerm = subject;
                            var logmessageTerm = subject;

                            List<UserDTO> users = um.GetEmployeeNearestExecutives(employee, DateTime.Today, employeeRequest.Stop, company.ActorCompanyId);
                            if (users != null)
                            {
                                foreach (UserDTO user in users)
                                {
                                    //Check if reminder has already been sent for this day or day before
                                    DateTime checkFrom = dateOfReminder;
                                    if (reminderAfterLongAbsenceLogs.HasSystemInfoLogs(user.UserId, checkFrom, logsToDate, employee.EmployeeId))
                                        continue;

                                    if (this.SendXEMail(company.ActorCompanyId, company.LicenseId, user.UserId, adminUser.UserId, subject, textTerm).Success)
                                        noOfSentMails += 1;

                                    this.AddSystemInfoLogEntry(entities, company, employee, user, SystemInfoType.ReminderAfterLongAbsence, subject, logmessageTerm, true);
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogError($"SendReminderAfterLongAbsence employeeId: {employeeRequestsByEmployee.Key} " + ex.ToString());
                }
            }
        }

        private void StartNewCompany(Company company)
        {
            this.companyCounter++;
            this.currentCompany = company;
            this.noOfSentMails = 0;
            this.employees = null;
            CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar jobb för {company.ActorCompanyId} {company.Name} {this.companyCounter}/{this.totalCompanies}");
        }

        private bool TryLoadEmployees()
        {
            if (this.currentCompany == null)
                return false;
            if (this.employees == null)
                this.employees = em.GetAllEmployees(this.currentCompany.ActorCompanyId, active: true, loadUser: true, loadEmployment: true, getVacant: false);
          
            return !this.employees.IsNullOrEmpty();
        }

        private bool HasMoreThanLimitDaysCoherentEmployeeRequests(List<EmployeeRequestDTO> employeeRequests, int limit, DateTime lastStopDate)
        {
            if (employeeRequests.IsNullOrEmpty())
                return false;

            int days = 0;
            DateTime? prevDateFrom = null;
            foreach (EmployeeRequestDTO employeeRequest in employeeRequests.OrderByDescending(e => e.Stop))
            {
                try
                {
                    if (employeeRequest.Stop > lastStopDate)
                        continue;

                    DateTime empDateFrom = employeeRequest.Start.Date;
                    DateTime empDateTo = employeeRequest.Stop.Date;
                    if (prevDateFrom.HasValue && prevDateFrom.Value.Date.AddDays(-1) != empDateTo.Date)
                        days = 0;
                    else
                        days += (int)(empDateTo - empDateFrom).TotalDays + 1;

                    if (days >= limit)
                        return true;

                    prevDateFrom = empDateFrom.Date;

                    if (days == 0)
                        return false;
                }
                catch (Exception ex)
                {
                    LogCollector.LogError($"HasMoreThanLimitDaysCoherentEmployeeRequests employeeRequestId: {employeeRequest.EmployeeRequestId} " + ex.ToString());
                }
            }

            return false;
        }

        private List<GenericType> GetTerms(Employee employee)
        {
            int langId = employee?.User?.LangId ?? (int)TermGroup_Languages.Swedish;
            if (!termsDict.ContainsKey(langId))
                termsDict.Add(langId, TermCacheManager.Instance.GetTermGroupContent(TermGroup.SendEmployeeReminderJob, langId: langId));
            return termsDict[langId];
        }

        private ActionResult SendXEMail(int actorCompanyId, int licenseId, int recipientUserId, int senderUserId, string subject, string text)
        {
            var mail = new MessageEditDTO()
            {
                ActorCompanyId = actorCompanyId,
                LicenseId = licenseId,
                SenderUserId = senderUserId,
                SenderName = "SoftOne",
                Subject = subject,
                Text = StringUtility.ConvertNewLineToHtml(text),
                Created = DateTime.Now,
                AnswerType = XEMailAnswerType.None,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageType = TermGroup_MessageType.AttestReminder,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.HTML,
                ForceSendToReceiver = true,
                Recievers = new List<MessageRecipientDTO>()
                {
                    new MessageRecipientDTO()
                    {
                        UserId = recipientUserId,
                    }
                },
            };

            return ccm.SendXEMail(mail, actorCompanyId, 0, senderUserId);
        }


        private void SendEmail(string senderEmail, string[] recipients, string subject, string text)
        {
            if (!string.IsNullOrEmpty(senderEmail))
            {
                foreach (var recipient in recipients)
                {
                    if (string.IsNullOrEmpty(recipient))
                        continue;

                    emailm.SendEmailViaCommunicator(senderEmail, recipient, new List<string>(), subject, text, false, null);
                }
            }
        }


        private ActionResult AddSystemInfoLogEntry(CompEntities entities, Company company, Employee employee, User user, SystemInfoType systemInfoType, string subject, string message, bool addOnlyMessage = false)
        {
            return AddSystemInfoLogEntry(entities, company, employee, user?.ToDTO(), systemInfoType, subject, message, addOnlyMessage);
        }

        private ActionResult AddSystemInfoLogEntry(CompEntities entities, Company company, Employee employee, UserDTO user, SystemInfoType systemInfoType, string subject, string message, bool addOnlyMessage = false)
        {
            ActionResult result;

            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Employee");
            if (user == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

            string text = !addOnlyMessage ? String.Format("{0}: {1} {2} {3}, {4}", subject, message, employee.FirstName, employee.LastName, company.Name) : message;

            SystemInfoLog log = new SystemInfoLog()
            {
                Type = (int)systemInfoType,
                Entity = (int)SoeEntityType.XEMail,
                LogLevel = (int)SystemInfoLogLevel.Warning,
                RecordId = user?.UserId ?? int.MinValue,
                Text = text,
                Date = DateTime.Now,
                DeleteManually = false,
                EmployeeId = employee.EmployeeId,

                //Set FK
                ActorCompanyId = company.ActorCompanyId,
            };

            result = gm.AddSystemInfoLogEntry(entities, log);
            if (result.Success)
                jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, text);

            return result;
        }
    }
}
