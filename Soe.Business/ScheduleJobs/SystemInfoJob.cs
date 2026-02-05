using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SystemInfoJob : ScheduledJobBase, IScheduledJob
    {
        #region Variables

        private EmployeeManager em = null;
        private GeneralManager gm = null;
        private InvoiceManager im = null;
        private SettingManager sm = null;
        private TimeScheduleManager tsm = null;

        #endregion

        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            em = new EmployeeManager(parameterObject);
            gm = new GeneralManager(parameterObject);
            im = new InvoiceManager(parameterObject);
            sm = new SettingManager(parameterObject);
            tsm = new TimeScheduleManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    SystemInfoSetting setting;

                    #region CheckSettings

                    foreach (int companyId in cm.GetActiveCompanyIds())
                    {
                        //Deactivate old items
                        gm.DisableSystemInfoLogEntries(companyId);

                        #region Skills

                        setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSkill_Use, companyId);

                        if (setting != null && setting.BoolData != null && setting.BoolData == true)
                        {
                            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSkill_Ends, companyId);
                            if (setting != null && setting.IntData.HasValue)
                            {
                                if (!CheckEndingSkills(companyId, setting.IntData.Value).Success)
                                {
                                    jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, String.Format("SystemInfoLog skapades ej. EmployeeSkill_Ends. Value {0} Företag {1}", setting.IntData.Value, companyId));
                                }
                            }
                        }

                        #endregion

                        #region Schedule

                        setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSchedule_Use, companyId);

                        if (setting != null && setting.BoolData != null && setting.BoolData == true)
                        {
                            setting = sm.GetSystemInfoSetting((int)SystemInfoType.EmployeeSchedule_Ends, companyId);
                            if (setting != null && setting.IntData.HasValue)
                            {
                                if (!CheckEndingEmployeeSchedules(companyId, setting.IntData.Value).Success)
                                {
                                    jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, String.Format("SystemInfoLog skapades ej. EmployeeSchedule_Ends. Value {0} Företag {1}", setting.IntData.Value, companyId));
                                }
                            }
                        }

                        setting = sm.GetSystemInfoSetting((int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks_Use, companyId);

                        if (setting != null && setting.BoolData != null && setting.BoolData == true)
                        {
                            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks, companyId);
                            if (setting != null && setting.IntData.HasValue)
                            {
                                if (!CheckClosePreliminarySchedules(companyId, setting.IntData.Value).Success)
                                {
                                    jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, String.Format("SystemInfoLog skapades ej. ClosePreliminaryTimeScheduleTemplateBlocks. Value {0} Företag {1}", setting.IntData.Value, companyId));
                                }
                            }
                        }

                        #endregion

                        #region Order

                        setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderOrderSchedule_Use, companyId);

                        if (setting != null && setting.BoolData != null && setting.BoolData == true)
                        {
                            setting = sm.GetSystemInfoSetting((int)SystemInfoType.ReminderOrderSchedule, companyId);
                            if (setting != null && setting.IntData.HasValue)
                            {
                                if (!CheckOrderWithRemaningTime(companyId, setting.IntData.Value).Success)
                                {
                                    jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, String.Format("SystemInfoLog skapades ej. ReminderOrderSchedule. Value {0} Företag {1}", setting.IntData.Value, companyId));
                                }
                            }
                        }

                        #endregion
                    }

                    #endregion
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

        #region Time

        protected ActionResult CheckEndingSkills(int actorCompanyId, int days)
        {
            ActionResult result = new ActionResult(true);

            List<EmployeeSkill> employeeSkills = em.GetEndingEmployeeSkills(actorCompanyId, DateTime.Now.AddDays(days));

            if (employeeSkills.Count > 0)
            {
                List<SystemInfoLog> logList = new List<SystemInfoLog>();

                foreach (EmployeeSkill employeeSkill in employeeSkills)
                {
                    TimeSpan diff = (DateTime)employeeSkill.DateTo - DateTime.Now;

                    SystemInfoLog log = new SystemInfoLog()
                    {
                        Type = (int)SystemInfoType.EmployeeSkill_Ends,
                        Entity = (int)SoeEntityType.EmployeeSkill,
                        LogLevel = (int)SystemInfoLogLevel.Warning,
                        RecordId = employeeSkill.EmployeeSkillId,
                        Text = employeeSkill.Employee.ContactPerson.FirstName + " " + employeeSkill.Employee.ContactPerson.LastName + "{0} " + employeeSkill.Skill.Name + " {1} " + diff.Days + " {2}",
                        Date = DateTime.Now,
                        DeleteManually = false,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employeeSkill.EmployeeId,
                    };

                    logList.Add(log);
                }

                result = gm.AddSystemInfoLogEntries(logList);
            }

            return result;
        }

        protected ActionResult CheckEndingEmployeeSchedules(int actorCompanyId, int days)
        {
            ActionResult result = new ActionResult(true);

            List<EmployeeSchedule> employeeSchedules = tsm.GetEndingEmployeeSchedules(actorCompanyId, DateTime.Now.AddDays(days));

            if (employeeSchedules.Count > 0)
            {
                List<SystemInfoLog> logList = new List<SystemInfoLog>();

                foreach (EmployeeSchedule employeeSchedule in employeeSchedules)
                {
                    TimeSpan diff = employeeSchedule.StopDate - DateTime.Now;

                    SystemInfoLog log = new SystemInfoLog()
                    {
                        Type = (int)SystemInfoType.EmployeeSchedule_Ends,
                        Entity = (int)SoeEntityType.EmployeeSchedule,
                        LogLevel = (int)SystemInfoLogLevel.Warning,
                        RecordId = employeeSchedule.EmployeeScheduleId,
                        Text = employeeSchedule.Employee.ContactPerson.FirstName + " " + employeeSchedule.Employee.ContactPerson.LastName + "{0} " + diff.Days + " {1}",
                        Date = DateTime.Now,
                        DeleteManually = false,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = employeeSchedule.EmployeeId,
                    };

                    logList.Add(log);
                }

                result = gm.AddSystemInfoLogEntries(logList);
            }


            return result;
        }

        protected ActionResult CheckClosePreliminarySchedules(int actorCompanyId, int days)
        {
            ActionResult result = new ActionResult(true);

            List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocks = tsm.GetClosePreliminarySchedules(actorCompanyId, DateTime.Now.AddDays(days));

            if (timeScheduleTemplateBlocks.Count > 0)
            {
                List<SystemInfoLog> logList = new List<SystemInfoLog>();
                string prevEmployeeId = "";

                foreach (TimeScheduleTemplateBlock timeScheduleTemplateBlock in timeScheduleTemplateBlocks)
                {
                    string employeeId = timeScheduleTemplateBlock.EmployeeId.ToString();
                    if (employeeId == prevEmployeeId)
                        continue;

                    if ((DateTime)timeScheduleTemplateBlock.Date != DateTime.Now.Date)
                    {
                        TimeSpan diff = (DateTime)timeScheduleTemplateBlock.Date - DateTime.Now.Date;

                        TimeSpan zeroDays = new TimeSpan();

                        if (diff != zeroDays)
                        {

                            SystemInfoLog log = new SystemInfoLog()
                            {
                                Type = (int)SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks,
                                Entity = (int)SoeEntityType.TimeScheduleTemplateBlock,
                                LogLevel = (int)SystemInfoLogLevel.Warning,
                                RecordId = timeScheduleTemplateBlock.TimeScheduleTemplateBlockId,
                                Text = timeScheduleTemplateBlock.Employee.ContactPerson.FirstName + " " + timeScheduleTemplateBlock.Employee.ContactPerson.LastName + "{0} " + diff.Days + " {1}",
                                Date = DateTime.Now,
                                DeleteManually = false,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                                EmployeeId = timeScheduleTemplateBlock.EmployeeId,
                            };
                            prevEmployeeId = timeScheduleTemplateBlock.EmployeeId.ToString();

                            logList.Add(log);
                        }
                    }
                }

                result = gm.AddSystemInfoLogEntries(logList);
            }

            return result;
        }

        protected ActionResult CheckOrderWithRemaningTime(int actorCompanyId, int days)
        {
            ActionResult result = new ActionResult(true);

            List<CustomerInvoice> customerInvoices = im.GetCustomerInvoicesWithRemainingTime(actorCompanyId, DateTime.Now.AddDays(days));

            using (SqlConnection connection = new SqlConnection(FrownedUponSQLClient.GetADOConnectionString()))
            {
                DateTime to = DateTime.Today;
                var sql = $"Delete from SystemInfoLog where  actorcompanyId = {actorCompanyId} and Date < {CalendarUtility.ToSqlFriendlyDateTime(to)} and type = {(int)SystemInfoType.ReminderOrderSchedule}";
                FrownedUponSQLClient.ExcuteQuery(connection, sql);
            }

            if (customerInvoices.Count > 0)
            {
                List<SystemInfoLog> logList = new List<SystemInfoLog>();
                    DateTime from = DateTime.Now.AddYears(-1);
                    foreach (CustomerInvoice inv in customerInvoices.Where(w => (w.Modified.HasValue && w.Modified.Value > from) || (w.Created.HasValue && w.Created.Value > from)))
                    {
                        SystemInfoLog log = new SystemInfoLog()
                        {
                            Type = (int)SystemInfoType.ReminderOrderSchedule,
                            Entity = (int)SoeEntityType.Order,
                            LogLevel = (int)SystemInfoLogLevel.Warning,
                            RecordId = inv.InvoiceId,
                            Text = "{0} " + inv.InvoiceNr + " {1}",
                            Date = DateTime.Now,
                            DeleteManually = false,

                            //Set FK
                            ActorCompanyId = actorCompanyId,
                        };
                        logList.Add(log);
                    }

                    result = gm.AddSystemInfoLogEntries(logList);
                
            }


            return result;
        }
        #endregion
    }
}
