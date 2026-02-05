using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SendAttestReminderJob : ScheduledJobBase, IScheduledJob
    {
        #region Variables

        private AttestManager am = null;
        private CommunicationManager ccm = null;
        private EmployeeManager em = null;
        private SettingManager sm = null;
        private TimePeriodManager tpm = null;
        private UserManager um = null;

        private TimePeriod timePeriod = null;
        private int noOfSentMails;

        #endregion

        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            this.am = new AttestManager(parameterObject);
            this.ccm = new CommunicationManager(parameterObject);
            this.em = new EmployeeManager(parameterObject);
            this.um = new UserManager(parameterObject);
            this.sm = new SettingManager(parameterObject);
            this.tpm = new TimePeriodManager(parameterObject);

            int executeUserId = scheduledJob.ExecuteUserId;
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    var companies = base.GetCompanies(paramCompanyId, loadLicense: true);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb påminnelse om att attestera tider");

                    foreach (var company in companies)
                    {
                        #region Company

                        if (company.License == null || company.License.State != (int)SoeEntityState.Active)
                            continue;

                        SystemInfoSetting setting = sm.GetSystemInfoSetting((int)SystemInfoType.AttestReminder_Use, company.ActorCompanyId);
                        if (setting == null || !setting.BoolData.HasValue || !setting.BoolData.Value)
                            continue;

                        List<AttestState> companyAttestStates = am.GetAttestStates(company.ActorCompanyId);
                        if (companyAttestStates.IsNullOrEmpty())
                            continue;

                        List<int> reminderUserIds = new List<int>();
                        bool useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, company.ActorCompanyId, 0);
                        this.noOfSentMails = 0;

                        CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb för " + company.Name);

                        #endregion

                        #region AttestRoles

                        List<AttestRole> attestRoles = am.GetAttestRoles(company.ActorCompanyId, SoeModule.Time, loadAttestRoleUser: true);
                        foreach (AttestRole attestRole in attestRoles)
                        {
                            #region AttestRole

                            if (attestRole.ReminderAttestStateId.IsNullOrEmpty() || attestRole.ReminderPeriodType.IsNullOrEmpty())
                                continue;

                            if (!TryCalculateDates(company.ActorCompanyId, attestRole.ReminderPeriodType.Value, attestRole.ReminderNoOfDays.Value, out DateTime dateFrom, out DateTime dateTo))
                                continue;

                            AttestState attestStateReminder = companyAttestStates.FirstOrDefault(i => i.AttestStateId == attestRole.ReminderAttestStateId.Value);
                            if (attestStateReminder == null)
                                continue;

                            CreateLogEntry(ScheduledJobLogLevel.Information, "Startar påminnelse för attestroll " + attestRole.Name + " på " + company.Name);

                            #endregion

                            #region AttestRoleUsers

                            List<AttestRoleUser> attestRoleUsers = attestRole.AttestRoleUser.Where(w => w.State == (int)SoeEntityState.Active).ToList();
                            foreach (int userId in attestRoleUsers.Select(s => s.UserId).Distinct())
                            {
                                if (reminderUserIds.Contains(userId))
                                    continue;

                                User attestUser = um.GetUser(userId);
                                if (attestUser == null)
                                    continue;
                                if (!attestRoleUsers.Any(aru => aru.UserId == userId && !useAccountHierarchy || aru.IsExecutive))
                                    continue;

                                bool sendEmail = false;
                                bool addEmployeeNames = attestRole.AttestRoleUser.Count(aru => aru.UserId == userId) == 1;
                                List<string> employeeNames = new List<string>();

                                foreach (var attestRolesForUser in am.GetAttestRolesForUser(company.ActorCompanyId, attestUser.UserId, dateTo, module: SoeModule.Time))
                                {
                                    if (attestRole.AttestRoleId != attestRolesForUser.AttestRoleId)
                                        continue;

                                    List<Employee> employees = GetEmployees(company.ActorCompanyId, userId, dateFrom: dateFrom, dateTo: dateTo);
                                    if (!employees.IsNullOrEmpty())
                                    {
                                        List<int> employeeIds = employees.Select(e => e.EmployeeId).ToList();
                                        Dictionary<int, List<AttestState>> transactionAttestStatesByEmployee = am.GetTransactionAttestStatesByEmployee(company.ActorCompanyId, employeeIds, dateFrom, dateTo, attestStates: companyAttestStates);

                                        foreach (Employee employee in employees.Where(e => e.UserId.HasValue))
                                        {
                                            if (!transactionAttestStatesByEmployee.ContainsKey(employee.EmployeeId))
                                                continue;

                                            List<AttestState> attestStates = transactionAttestStatesByEmployee[employee.EmployeeId];
                                            if (attestStates.IsNullOrEmpty() || !attestStates.DoRemind(attestStateReminder))
                                                continue;

                                            sendEmail = true;
                                            if (addEmployeeNames)
                                                employeeNames.Add(employee.EmployeeNrAndName);
                                            else
                                                break;
                                        }
                                    }
                                }

                                if (sendEmail && !reminderUserIds.Contains(attestUser.UserId))
                                {
                                    if (addEmployeeNames && employeeNames.Any())
                                        employeeNames.Insert(0, attestRole.Name);

                                    if (employeeNames.Any())
                                        SendMailToAttestUser(company, attestUser, employeeNames.Distinct().ToList());
                                    reminderUserIds.Add(attestUser.UserId);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        #region EmployeeGroups

                        List<int> handledEmployeeIds = new List<int>();

                        List<EmployeeGroup> employeeGroups = em.GetEmployeeGroups(company.ActorCompanyId);
                        foreach (EmployeeGroup employeeGroup in employeeGroups)
                        {
                            #region EmployeeGroup

                            if (employeeGroup.ReminderAttestStateId.IsNullOrEmpty() || employeeGroup.ReminderPeriodType.IsNullOrEmpty())
                                continue;

                            if (!TryCalculateDates(company.ActorCompanyId, employeeGroup.ReminderPeriodType.Value, employeeGroup.ReminderNoOfDays ?? 0, out DateTime dateFrom, out DateTime dateTo))
                                continue;

                            AttestState attestStateReminder = companyAttestStates.FirstOrDefault(i => i.AttestStateId == employeeGroup.ReminderAttestStateId.Value);
                            if (attestStateReminder == null)
                                continue;

                            CreateLogEntry(ScheduledJobLogLevel.Information, "Startar påminnelse för tidavtal " + employeeGroup.Name + " på " + company.Name);

                            #endregion

                            #region Employees

                            List<Employee> employees = em.GetAllEmployeesByGroup(company.ActorCompanyId, employeeGroup.EmployeeGroupId).Where(e => e.UserId.HasValue).ToList();
                            if (!employees.IsNullOrEmpty())
                            {
                                List<int> employeeIds = employees.Select(e => e.EmployeeId).ToList();
                                var transactionAttestStatesByEmployee = am.GetTransactionAttestStatesByEmployee(company.ActorCompanyId, employeeIds, dateFrom, dateTo, attestStates: companyAttestStates);

                                foreach (Employee employee in employees)
                                {
                                    if (handledEmployeeIds.Contains(employee.EmployeeId) || !transactionAttestStatesByEmployee.ContainsKey(employee.EmployeeId))
                                        continue;

                                    List<AttestState> attestStates = transactionAttestStatesByEmployee[employee.EmployeeId];
                                    if (attestStates.IsNullOrEmpty())
                                        continue;

                                    if (attestStates.DoRemind(attestStateReminder))
                                        SendMailToEmployee(company, employee, employeeGroup, attestStateReminder, dateTo);

                                    handledEmployeeIds.Add(employee.EmployeeId);
                                }
                            }

                            #endregion
                        }

                        #endregion

                        jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, $"Skickade {this.noOfSentMails} påminnelsemail för {company.Name}");
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel vid exekvering av jobb: '{result.ErrorMessage}'");
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }

        private List<Employee> GetEmployees(int actorCompanyId, int userId, DateTime dateFrom, DateTime dateTo)
        {
            string key = $"AttestReminderEmployees{actorCompanyId}#{userId}#{dateFrom}#{dateTo}";

            List<Employee> employees = BusinessMemoryCache<List<Employee>>.Get(key);
            if (employees == null)
                employees = em.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, 0, dateFrom: dateFrom, dateTo: dateTo, getVacant: false);
            if (employees != null)
                BusinessMemoryCache<List<Employee>>.Set(key, employees, 60 * 10);

            return employees;
        }

        private bool TryCalculateDates(int actorCompanyId, int reminderPeriodType, int noOfDays, out DateTime dateFrom, out DateTime dateTo)
        {
            dateFrom = CalendarUtility.DATETIME_DEFAULT;
            dateTo = CalendarUtility.DATETIME_DEFAULT;
            DateTime today = DateTime.Today;

            switch (reminderPeriodType)
            {
                case (int)AttestPeriodType.Day:
                    #region Day

                    dateFrom = today.AddDays(-noOfDays);
                    dateTo = today.AddDays(-(noOfDays - 1));

                    #endregion
                    break;
                case (int)AttestPeriodType.Week:
                    #region Week

                    dateFrom = today.AddDays(-(int)today.DayOfWeek - 6); //Get monday of last week
                    dateTo = dateFrom.AddDays(7); //Sunday of last week

                    #endregion
                    break;
                case (int)AttestPeriodType.Month:
                    #region Month

                    DateTime month = new DateTime(today.Year, today.Month, 1);
                    dateFrom = month.AddMonths(-1);
                    dateTo = month.AddDays(-1);

                    #endregion
                    break;
                case (int)AttestPeriodType.Period:
                    #region Period

                    this.timePeriod = tpm.GetTimePeriod(actorCompanyId, today, noOfDays);
                    if (this.timePeriod != null)
                    {
                        dateFrom = timePeriod.StartDate;
                        dateTo = timePeriod.StopDate;
                    }

                    break;
                #endregion
                default:
                    break;
            }

            return dateFrom != CalendarUtility.DATETIME_DEFAULT && dateTo != CalendarUtility.DATETIME_DEFAULT && dateTo.Date.AddDays(noOfDays) == DateTime.Today;
        }

        private void SendMailToAttestUser(Company company, User user, List<string> employeeNames)
        {
            if (company == null || user == null)
                return;

            var (subject, body) = am.GetAttestReminderMailToExecutive(user, employeeNames);
            SendMail(company.ActorCompanyId, company.LicenseId, scheduledJob.ExecuteUserId, user.UserId, subject, body);
        }

        private void SendMailToEmployee(Company company, Employee employee, EmployeeGroup employeeGroup, AttestState reminderAttestState, DateTime dateTo)
        {
            if (company == null || employee == null || employeeGroup == null || employeeGroup.ReminderPeriodType.IsNullOrEmpty() || reminderAttestState == null)
                return;

            var (subject, body) = am.GetAttestReminderMailToEmployee(employee, employeeGroup.ReminderPeriodType.Value, reminderAttestState, dateTo, this.timePeriod);
            SendMail(company.ActorCompanyId, company.LicenseId, scheduledJob.ExecuteUserId, (int)employee.UserId, subject, body);
        }

        private void SendMail(int actorCompanyId, int licenseId, int senderUserId, int recipientUserId, string subject, string text)
        {
            MessageEditDTO dto = new MessageEditDTO()
            {
                ActorCompanyId = actorCompanyId,
                LicenseId = licenseId,
                SenderUserId = senderUserId,
                SenderName = "Autoreply SoftOne",
                Subject = subject,
                Text = StringUtility.ConvertNewLineToHtml(text),
                ForceSendToReceiver = true,
                Created = DateTime.Now,
                AnswerType = XEMailAnswerType.None,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageType = TermGroup_MessageType.AttestReminder,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.HTML,
                Recievers = new List<MessageRecipientDTO>()
                {
                    new MessageRecipientDTO()
                    {
                        UserId = recipientUserId,
                    }
                },
            };

            if (ccm.SendXEMail(dto, actorCompanyId, 0, senderUserId).Success)
                this.noOfSentMails += 1;
        }
    }
}
