using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class WebTimeStampManager : ManagerBase
    {
        #region Ctor

        public WebTimeStampManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public SoftOne.Soe.Common.Util.ActionResult SyncNewTimeStampEntries(int actorCompanyId, int terminalId, TimeStampEntryType timeStampEntryType, string employeeNr, int? causeId = null, int? accountId = null, bool isBreak = false)
        {
            DateTime now = DateTime.Now;
            using (var entities = new CompEntities())
            {

                var employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, actorCompanyId, onlyActive: true, loadContactPerson: true, loadEmployment: true);
                if (employee == null)
                    return new SoftOne.Soe.Common.Util.ActionResult((int)ActionResultSave.Unknown, "Kunde inte hitta anställd nr " + employeeNr);

                EmployeeGroup employeeGroup = employee.CurrentEmployeeGroup;
                int breakDayMinutesAfterMidninght = employeeGroup != null ? employeeGroup.BreakDayMinutesAfterMidnight : 0;

                if (timeStampEntryType == TimeStampEntryType.Unknown)
                {
                    var lastEntry = TimeStampManager.GetLastTimeStampEntryForEmployee(employee.EmployeeId, excludeFuture: true, actorCompanyId: (int?)actorCompanyId);
                    if (lastEntry == null || lastEntry.Type == (int)TimeStampEntryType.In)
                        timeStampEntryType = TimeStampEntryType.Out;
                    else
                        timeStampEntryType = TimeStampEntryType.In;

                    if (accountId > 0 && timeStampEntryType == TimeStampEntryType.Out)
                    {
                        // Special functionality for switching account, so switch this stamp to in and stamp out first instead 
                        timeStampEntryType = TimeStampEntryType.In;
                        // Stamp out (recursive)
                        var recResult = SyncNewTimeStampEntries(actorCompanyId, terminalId, TimeStampEntryType.Out, employeeNr, causeId);
                        if (recResult == null || !recResult.Success)
                            return recResult;
                        now = DateTime.Now;
                    }
                }

                now = AdjustForTimeZone(now, terminalId);


                //Adjust Time
                var adjustTime = TimeStampManager.GetTimeTerminalSetting(TimeTerminalSettingType.AdjustTime, terminalId);
                DateTime originalTime = now;

                if (adjustTime != null)
                {
                    int adjustHours = 0;
                    adjustHours = (adjustTime.IntData.HasValue && adjustTime.IntData.Value != 0) ? (int)adjustTime.IntData : 0;
                    now = now.AddHours(adjustHours);
                    originalTime = now;
                }

                now = now.RemoveSeconds();

                var entry = new TimeStampEntry()
                {
                    ActorCompanyId = actorCompanyId,
                    TimeTerminalId = terminalId,
                    Type = (int)timeStampEntryType,
                    OriginType = (int)TermGroup_TimeStampEntryOriginType.TerminalByEmployeeNumber,
                    EmployeeId = employee.EmployeeId,
                    OriginalTime = originalTime,
                    Time = now,
                    IsBreak = isBreak && timeStampEntryType == TimeStampEntryType.Out,
                    TimeBlockDate = TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, employee.EmployeeId, now.AddMinutes(-breakDayMinutesAfterMidninght).Date, true),
                };
                string groupName = null;

                if (causeId.HasValue && causeId > 0)
                {
                    entry.TimeDeviationCauseId = causeId;
                }

                if (isBreak && timeStampEntryType == TimeStampEntryType.Out)
                {
                    groupName = "Rast";
                }
                else if (accountId.HasValue && accountId > 0)
                {
                    entry.AccountId = accountId.Value;
                    groupName = AccountManager.GetAccount(actorCompanyId, accountId.Value).Name;
                }
                else
                {
                    // Fetch standard account
                    Account a = null;
                    var accountDimSetting = this.TimeStampManager.GetTimeTerminalSetting(SoftOne.Soe.Common.Util.TimeTerminalSettingType.AccountDim, terminalId);
                    TimeScheduleManager.IsEmployeeWithinSchedule(entities, employee, entry.Time, ref a, accountDimSetting.IntData);
                    if (a != null)
                    {
                        entry.AccountId = a.AccountId;
                        groupName = a.Name;
                    }
                }

                if (string.IsNullOrEmpty(groupName))
                {
                    groupName = this.GetDefaultGroupName(actorCompanyId, terminalId);
                }

                SetCreatedProperties(entry);
                entities.TimeStampEntry.AddObject(entry);

                var result = SaveChanges(entities);
                if (result.Success)
                {
                    result.IntegerValue = entry.EmployeeId;
                    result.BooleanValue = timeStampEntryType == TimeStampEntryType.Out;
                    string successMsg = employee.Name + " har stämplat ";
                    if (isBreak)
                        successMsg += (timeStampEntryType == TimeStampEntryType.Out ? "ut på " : "in från ") + "rast";
                    else
                        successMsg += (timeStampEntryType == TimeStampEntryType.Out ? "ut" : "in");
                    result.StringValue = successMsg;
                    result.Keys = EmployeeManager.GetEmployeeCategoryIds(employee.EmployeeId, actorCompanyId);
                    result.Value = new EmployeeModel()
                    {
                        Name = employee.Name,
                        EmployeeId = employee.EmployeeId,
                        EmployeeNr = employee.EmployeeNr,
                        Time = entry.Time,
                        Group = groupName,
                        ImageUrl = this.GetImageUrl(actorCompanyId, employee.EmployeeId),
                        OnBreak = isBreak && timeStampEntryType == TimeStampEntryType.Out,
                    };
                }

                if (timeStampEntryType == TimeStampEntryType.In && employee.UserId.HasValue)
                {
                    // Check if user has new messages
                    var msgCount = this.CommunicationManager.GetIncomingMessagesCount(null, employee.UserId.Value);
                    result.IntegerValue2 = msgCount;
                    result.Value2 = string.Format("OBS: Du har olästa xe-mail ({0}st)!", msgCount);
                }

                return result;
            }
        }

        public DateTime AdjustForTimeZone(DateTime now, int timeTerminalId)
        {
            //Check TimeZone Finnish support only
            var sysCountry = TimeStampManager.GetTimeTerminalSetting(TimeTerminalSettingType.SysCountryId, timeTerminalId);
            if (sysCountry != null && sysCountry.IntData.HasValue && sysCountry.IntData.Value == (int)TermGroup_Country.FI)
            {
                TimeZoneInfo helsinki = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
                TimeZoneInfo local = TimeZoneInfo.Local; // PDT for me
                now = DateTime.Now.Add(helsinki.BaseUtcOffset - local.BaseUtcOffset);
            }

            return now;
        }

        public IEnumerable<EmployeeModel> GetStampedInEmployees(int actorCompanyId, int timeTerminalId)
        {
            var employeesStampedIn = this.DashboardManager.GetTimeStampAttendance(actorCompanyId, 0, 0, SoftOne.Soe.Common.Util.TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours, true, onlyIncludeAttestRoleEmployees: false, includeEmployeeNrInString: false, timeTerminalId: timeTerminalId, includeBreaks: true);
            string defaultGroupName = this.GetDefaultGroupName(actorCompanyId, timeTerminalId);

            foreach (var e in employeesStampedIn)
            {
                string imageUrl = this.GetImageUrl(actorCompanyId, e.EmployeeId);
                yield return new EmployeeModel()
                {
                    Name = e.Name,
                    EmployeeId = e.EmployeeId,
                    EmployeeNr = e.EmployeeNr,
                    Time = e.Time,
                    Group = e.IsBreak ? "Rast" : e.AccountName ?? defaultGroupName,
                    ImageUrl = imageUrl,
                    OnBreak = e.IsBreak,
                };
            }
        }

        public string GetDefaultGroupName(int actorCompanyId, int timeTerminalId)
        {
            return "Saknar " + (this.GetAccountDimName(actorCompanyId, timeTerminalId) ?? string.Empty);
        }

        public string GetImageUrl(int actorCompanyId, int employeeId)
        {
            var hasImage = this.GraphicsManager.HasImage(actorCompanyId, SoftOne.Soe.Common.Util.SoeEntityImageType.EmployeePortrait, SoftOne.Soe.Common.Util.SoeEntityType.Employee, employeeId);
            var imageUrl = !hasImage ? "/Content/Images/no_user.png" : string.Format("/api/WtsApi/GetImage?actorCompanyId={0}&employeeId={1}", actorCompanyId, employeeId);

            return imageUrl;
        }

        public Dictionary<int, string> GetTimeDeviationCauseForEmployeeNow(int actorCompanyId, string employeeNr)
        {
            var em = new EmployeeManager(null);
            var employee = em.GetEmployeeByNr(employeeNr, actorCompanyId, onlyActive: true, loadContactPerson: true);
            if (employee == null)
                return null;

            var tdcm = new TimeDeviationCauseManager(null);
            var causes = tdcm.GetTimeDeviationCauseForEmployeeNow(actorCompanyId, employee.EmployeeId);

            return causes.ToDictionary(k => k.TimeDeviationCauseId, v => v.Name);
        }

        public Dictionary<int, string> GetAccounts(int actorCompanyId, int timeTerminalId)
        {
            var tm = new TimeStampManager(null);
            var accountDimSetting = tm.GetTimeTerminalSetting(SoftOne.Soe.Common.Util.TimeTerminalSettingType.AccountDim, timeTerminalId);
            if (accountDimSetting == null || !accountDimSetting.IntData.HasValue)
                return null;

            var accounts = tm.SyncAccount(actorCompanyId, accountDimSetting.IntData.Value, null);

            return accounts.Where(i => i.State == (int)SoeEntityState.Active).Take(500).ToDictionary(k => k.AccountId, v => v.Name);
        }

        public string GetCurrentTime(int timeTerminalId)
        {
            var tsm = new TimeStampManager(null);
            TimeTerminalSetting wttsetting = null;
            List<TimeTerminalSetting> wttsettings = tsm.GetTimeTerminalSettings(timeTerminalId);
            // SyncClockWithServer
            wttsetting = wttsettings.FirstOrDefault(s => s.Type == (int)SoftOne.Soe.Common.Util.TimeTerminalSettingType.SyncClockWithServer);
            if (wttsetting != null && wttsetting.BoolData.HasValue)
            {
                return DateTime.Now.ToUniversalTime().ToShortDateString() + "T" + DateTime.Now.ToUniversalTime().ToLongTimeString();
            }
            else
            {
                return null;
            }
        }

        public List<int> GetTerminalCategories(int actorCompanyId, int terminalId)
        {
            var setting = this.TimeStampManager.GetTimeTerminalSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, terminalId);
            if (setting == null || setting.BoolData == false)
                return new List<int>(0);

            var categories = this.TimeStampManager.GetCategoriesByTimeTerminal(actorCompanyId, terminalId).Select(c => c.CategoryId).ToList();
            return categories;
        }

        public string GetAccountDimName(int actorCompanyId, int terminalId)
        {
            var setting = this.TimeStampManager.GetTimeTerminalSetting(SoftOne.Soe.Common.Util.TimeTerminalSettingType.AccountDim, terminalId);
            return setting?.StrData;
        }

        public TimeTerminalSetting GetTerminalSetting(TimeTerminalSettingType settingType, int terminalId)
        {
            return this.TimeStampManager.GetTimeTerminalSetting(settingType, terminalId);
        }

        internal string GetTerminalName(int terminalId)
        {
            var terminal = TimeStampManager.GetTimeTerminalDiscardState(terminalId);
            string result = string.Empty;

            if (terminal != null && !string.IsNullOrEmpty(terminal.Name))
                result = terminal.Name;

            return result;
        }
    }

    public class EmployeeModel
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string EmployeeNr { get; set; }
        public DateTime Time { get; set; }
        public string Group { get; set; }
        public string ImageUrl { get; set; }
        public bool OnBreak { get; set; }
    }
}
