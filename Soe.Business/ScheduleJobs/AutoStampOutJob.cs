using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class AutoStampOutJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            TimeStampManager tsm = new TimeStampManager(parameterObject);
            CompanyManager cm = new CompanyManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Skapar automatiska utstämplingar");

                    List<TimeTerminal> terminals = tsm.GetAutoStampOutTerminals();
                    int prevCompanyId = 0;
                    int stampCounter = 0;
                    foreach (TimeTerminal terminal in terminals.OrderBy(t => t.ActorCompanyId))
                    {
                        // Log each time a new company is processed
                        if (terminal.ActorCompanyId != prevCompanyId)
                        {
                            if (prevCompanyId != 0)
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("{0} utstämplingar skapade", stampCounter));
                            stampCounter = 0;
                            prevCompanyId = terminal.ActorCompanyId;
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Skapar automatiska utstämplingar för {0} ({1})", terminal.Company.Name, terminal.ActorCompanyId));
                        }
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Terminal: {0}", terminal.Name));

                        // Get time setting for current terminal
                        TimeSpan timeSetting = new TimeSpan();
                        TimeTerminalSetting setting = terminal.TimeTerminalSetting.FirstOrDefault(s => s.Type == (int)TimeTerminalSettingType.UseAutoStampOutTime && s.IntData.HasValue);
                        if (setting != null)
                            timeSetting = TimeSpan.FromMinutes(setting.IntData.Value);

                        // Adjust time based on time zone setting on terminal
                        DateTime now = CalendarUtility.ClearSeconds(tsm.GetLocalTimeForTerminal(DateTime.UtcNow, terminal.TimeTerminalId));

                        // Get all last entries for each employee and loop over type IN
                        List<TimeStampEntry> pubSubEntries = new List<TimeStampEntry>();
                        List<TimeStampEntry> entries = tsm.GetLastTimeStampEntryForEachEmployee(terminal.TimeTerminalId, TimeStampEntryType.In);
                        foreach (TimeStampEntry entry in entries)
                        {
                            #region Validate

                            // Check if auto stamp was already created
                            if (entry.CreatedBy == Constants.CREATED_BY_AUTO_STAMP_OUT_JOB)
                                continue;

                            // Check if time of the setting has passed
                            DateTime time = CalendarUtility.MergeDateAndTime(entry.Time, timeSetting);
                            if (time < entry.Time)
                                time = time.AddDays(1);
                            if (time >= now)
                                continue;

                            // Check that employee has not stamped out on any other terminal
                            TimeStampEntry lastEntry = tsm.GetLastTimeStampEntryForEmployee(entry.EmployeeId);
                            if (entry.TimeStampEntryId != lastEntry.TimeStampEntryId)
                                continue;

                            // Check that TimeBlockDate exists
                            if (!entry.TimeBlockDateId.HasValue)
                                continue;

                            // Check that total number of stamps for current employee and date are odd
                            int nbrOfEntries = tsm.GetNumberOfTimeStampEntries(entry.TimeBlockDateId.Value);
                            if (nbrOfEntries % 2 == 0)
                                continue;

                            #endregion

                            // Time has passed, create a new time stamp entry
                            int langId = entry.Employee.User != null && entry.Employee.User.LangId != null ? (int)entry.Employee.User.LangId : (int)TermGroup_Languages.Swedish;
                            string note = TermCacheManager.Instance.GetText(3139, 1, "Automatisk utstämpling", langId);

                            using (CompEntities entities = new CompEntities())
                            {
                                TimeStampEntry newEntry = new TimeStampEntry()
                                {
                                    TimeTerminalId = entry.TimeTerminalId,
                                    Type = (int)TimeStampEntryType.Out,
                                    Note = note,
                                    Time = time,
                                    OriginalTime = time,
                                    ManuallyAdjusted = true,
                                    EmployeeManuallyAdjusted = false,
                                    AutoStampOut = true,
                                    Status = (int)TermGroup_TimeStampEntryStatus.New,
                                    OriginType = (int)TermGroup_TimeStampEntryOriginType.AutoStampOutJob,
                                    Created = DateTime.Now,
                                    CreatedBy = Constants.CREATED_BY_AUTO_STAMP_OUT_JOB,

                                    //Set FK
                                    ActorCompanyId = entry.ActorCompanyId,
                                    EmployeeId = entry.EmployeeId,
                                    TimeDeviationCauseId = entry.TimeDeviationCauseId,
                                    AccountId = entry.AccountId,
                                    TimeScheduleTemplatePeriodId = entry.TimeScheduleTemplatePeriodId,
                                };

                                if (entry.Time.Date == time.Date)
                                    newEntry.TimeBlockDateId = entry.TimeBlockDateId;

                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Skapar utstämpling för ({0}) {1} med tiden {2}", entry.Employee.EmployeeNr, entry.Employee.Name, time.ToShortDateShortTimeString()));

                                entities.TimeStampEntry.AddObject(newEntry);
                                if (entities.SaveChanges() > 0)
                                {
                                    stampCounter++;
                                    pubSubEntries.Add(entry);
                                }
                                else
                                    CreateLogEntry(ScheduledJobLogLevel.Error, "Fel vid skapande av stämpling");
                            }
                        }

                        if (pubSubEntries.Any())
                        {
                            using (CompEntities entities = new CompEntities())
                            {
                                List<int> terminalIds = tsm.GetTimeTerminalIdsForPubSub(entities, terminal.ActorCompanyId);
                                foreach (TimeStampEntry entry in pubSubEntries)
                                {
                                    tsm.SendWebPubSubMessage(entities, entry, WebPubSubMessageAction.Insert, terminalIds);
                                }
                            }
                        }
                    }
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("{0} utstämplingar skapade", stampCounter));
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
        }
    }
}
