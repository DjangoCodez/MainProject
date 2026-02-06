using Bridge.Shared.Models;
using Bridge.Shared.Util;
using Microsoft.Owin.Security.Provider;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using SoftOne.Soe.Business.ArbetsgivarintygNu;
using SoftOne.Soe.Business.Core.Bridge.Visma;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http.Results;

namespace SoftOne.Soe.Business.Core
{
    public class ScheduledJobManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ScheduledJobManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ScheduledJobHead

        public List<ScheduledJobHead> GetScheduledJobHeads(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScheduledJobHead.NoTracking();
            return GetScheduledJobHeads(entities, actorCompanyId);
        }

        public List<ScheduledJobHead> GetScheduledJobHeads(CompEntities entities, int actorCompanyId)
        {
            return (from s in entities.ScheduledJobHead
                    where s.ActorCompanyId == actorCompanyId &&
                    s.State == (int)SoeEntityState.Active
                    orderby s.Name
                    select s).ToList();
        }

        public Dictionary<int, string> GetScheduledJobHeadsDict(int actorCompanyId, bool addEmptyRow, bool includeSharedOnLicense)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.ScheduledJobHead.NoTracking();
            var heads = (from s in entitiesReadOnly.ScheduledJobHead
                         where s.ActorCompanyId == actorCompanyId &&
                         s.State == (int)SoeEntityState.Active
                         select new { s.ScheduledJobHeadId, s.Name }).ToList();

            if (includeSharedOnLicense)
            {
                var company = CompanyManager.GetCompany(actorCompanyId);
                heads.AddRange((from s in entitiesReadOnly.ScheduledJobHead
                                where s.ActorCompanyId != actorCompanyId &&
                                s.Company.LicenseId == company.LicenseId &&
                                s.SharedOnLicense &&
                                s.State == (int)SoeEntityState.Active
                                select new { s.ScheduledJobHeadId, s.Name }).ToList());
            }

            foreach (var head in heads.OrderBy(h => h.Name))
            {
                dict.Add(head.ScheduledJobHeadId, head.Name);
            }

            return dict;
        }

        public ScheduledJobHead GetScheduledJobHead(int scheduledJobHeadId, int actorCompanyId, bool loadRows, bool loadLogs, bool loadSettings, bool loadSettingOptions, bool setRecurrenceIntervalText, bool setTimeIntervalText)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScheduledJobHead.NoTracking();
            return GetScheduledJobHead(entities, scheduledJobHeadId, actorCompanyId, loadRows, loadLogs, loadSettings, loadSettingOptions, setRecurrenceIntervalText, setTimeIntervalText);
        }

        public ScheduledJobHead GetScheduledJobHead(CompEntities entities, int scheduledJobHeadId, int actorCompanyId, bool loadRows, bool loadLogs, bool loadSettings, bool loadSettingOptions, bool setRecurrenceIntervalText, bool setTimeIntervalText)
        {
            IQueryable<ScheduledJobHead> oQuery = entities.ScheduledJobHead;
            if (loadRows)
                oQuery = oQuery.Include("ScheduledJobRow");
            if (loadLogs)
                oQuery = oQuery.Include("ScheduledJobLog");
            if (loadSettings)
                oQuery = oQuery.Include("ScheduledJobSetting");

            ScheduledJobHead head = (from s in oQuery
                                     where s.ScheduledJobHeadId == scheduledJobHeadId &&
                                     s.ActorCompanyId == actorCompanyId
                                     select s).FirstOrDefault();

            if (head != null)
            {
                if (loadSettings && loadSettingOptions && !head.ScheduledJobSetting.IsNullOrEmpty())
                {
                    foreach (ScheduledJobSetting setting in head.ScheduledJobSetting)
                    {
                        List<SmallGenericType> options = GetScheduledJobSettingOptions(setting.Type, actorCompanyId);
                        if (options != null)
                            setting.Options = options;
                    }
                }

                if ((setRecurrenceIntervalText || setTimeIntervalText) && !head.ScheduledJobRow.IsNullOrEmpty())
                {
                    Dictionary<int, string> sysTimeIntervals = null;
                    if (setTimeIntervalText)
                        sysTimeIntervals = CalendarManager.GetSysTimeIntervalsDict();

                    foreach (ScheduledJobRow row in head.ScheduledJobRow)
                    {
                        if (setRecurrenceIntervalText)
                            row.RecurrenceIntervalText = CalendarManager.GetRecurrenceIntervalText(row.RecurrenceInterval);

                        if (setTimeIntervalText && row.SysTimeIntervalId.HasValue)
                            row.TimeIntervalText = sysTimeIntervals.ContainsKey(row.SysTimeIntervalId.Value) ? sysTimeIntervals[row.SysTimeIntervalId.Value] : string.Empty;
                    }
                }
            }

            return head;
        }

        public List<SmallGenericType> GetScheduledJobSettingOptions(int settingType, int actorCompanyId)
        {
            switch ((TermGroup_ScheduledJobSettingType)settingType)
            {
                case TermGroup_ScheduledJobSettingType.BridgeJobType:
                    return TermCacheManager.Instance.GetTermGroupContent(TermGroup.BridgeJobType).ToSmallGenericTypes();
                case TermGroup_ScheduledJobSettingType.ExportId:
                    return ImportExportManager.GetExportsDict(actorCompanyId).ToSmallGenericTypes();
                case TermGroup_ScheduledJobSettingType.BridgeJobFileType:
                    return TermCacheManager.Instance.GetTermGroupContent(TermGroup.BridgeJobFileType).ToSmallGenericTypes();
                case TermGroup_ScheduledJobSettingType.BridgeFileInformationDefinitionId:
                    return ImportExportManager.GetSysImportDefinitionsDict().ToSmallGenericTypes();
                case TermGroup_ScheduledJobSettingType.BridgeFileInformationImportHeadId:
                    return ImportExportManager.GetSysImportHeadsDict().ToSmallGenericTypes();
                case TermGroup_ScheduledJobSettingType.SpecifiedType:
                    return TermCacheManager.Instance.GetTermGroupContent(TermGroup.ScheduledJobSpecifiedType).ToSmallGenericTypes();
            }

            return null;
        }

        public List<ScheduledJobLog> GetScheduledJobLogs(int scheduledJobHeadId, int actorCompanyId, bool setLogLevelName, bool setStatusName)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ScheduledJobLog.NoTracking();
            List<ScheduledJobLog> scheduledJobLogs = (from s in entities.ScheduledJobLog
                                                      where s.ScheduledJobHeadId == scheduledJobHeadId &&
                                                      s.ScheduledJobHead.ActorCompanyId == actorCompanyId
                                                      orderby s.BatchNr, s.Time
                                                      select s).ToList();

            if (setLogLevelName || setStatusName)
            {
                List<GenericType> levels = null;
                if (setLogLevelName)
                    levels = GetTermGroupContent(TermGroup.ScheduledJobLogLevel);

                List<GenericType> statuses = null;
                if (setStatusName)
                    statuses = GetTermGroupContent(TermGroup.ScheduledJobLogStatus);

                foreach (ScheduledJobLog jobLog in scheduledJobLogs)
                {
                    if (setLogLevelName)
                    {
                        GenericType level = levels.FirstOrDefault(l => l.Id == jobLog.LogLevel);
                        if (level != null)
                            jobLog.LogLevelName = level.Name;
                    }

                    if (setStatusName)
                    {
                        GenericType status = statuses.FirstOrDefault(s => s.Id == jobLog.Status);
                        if (status != null)
                            jobLog.StatusName = status.Name;
                    }
                }
            }

            return scheduledJobLogs.OrderByDescending(o => o.Time).ToList();
        }

        public ActionResult SaveScheduledJobHead(ScheduledJobHeadDTO headInput, int actorCompanyId)
        {
            if (headInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ScheduledJobHead");

            ActionResult result = new ActionResult();

            int scheduledJobHeadId = headInput.ScheduledJobHeadId;
            if (headInput.Description == "ExecuteJobNow")
            {
                RunJobsOnHead(headInput.ScheduledJobHeadId, actorCompanyId, GetBatchNumer(headInput.ScheduledJobHeadId), forceExecuteNow: true);
                headInput.Description = "JobRequestExecuted";
            }

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region ScheduledJobHead

                        ScheduledJobHead head = GetScheduledJobHead(entities, scheduledJobHeadId, actorCompanyId, true, false, true, false, false, false);
                        if (head == null)
                        {
                            #region Add

                            head = new ScheduledJobHead()
                            {
                                ActorCompanyId = actorCompanyId,
                            };
                            SetCreatedProperties(head);
                            entities.AddObject("ScheduledJobHead", head);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            SetModifiedProperties(head);

                            #endregion
                        }

                        #region Common

                        head.ParentId = (headInput.ParentId.HasValidValue() ? headInput.ParentId.Value : (int?)null);
                        head.Name = headInput.Name;
                        head.Description = headInput.Description;
                        head.Sort = headInput.Sort;
                        head.SharedOnLicense = headInput.SharedOnLicense;
                        head.State = (int)headInput.State;

                        #endregion

                        #endregion

                        #region ScheduledJobRow

                        List<ScheduledJobRow> existingRows = head.ScheduledJobHeadId > 0 ? head.ScheduledJobRow.Where(r => r.State == (int)SoeEntityState.Active).ToList() : new List<ScheduledJobRow>();

                        #region Delete rows that exists in db but not in input.

                        if (existingRows != null)
                        {
                            foreach (ScheduledJobRow existingRow in existingRows)
                            {
                                if (!headInput.Rows.Any(r => r.ScheduledJobRowId == existingRow.ScheduledJobRowId))
                                    ChangeEntityState(existingRow, SoeEntityState.Deleted);
                            }
                        }

                        #endregion

                        #region Add/update rows

                        foreach (ScheduledJobRowDTO rowInput in headInput.Rows ?? new List<ScheduledJobRowDTO>())
                        {
                            ScheduledJobRow existingRow = existingRows != null ? existingRows.FirstOrDefault(r => r.ScheduledJobRowId == rowInput.ScheduledJobRowId) : null;
                            if (existingRow == null)
                            {
                                existingRow = new ScheduledJobRow()
                                {
                                    ScheduledJobHead = head,
                                    NextExecutionTime = SchedulerUtility.GetNextExecutionTime(rowInput.RecurrenceInterval),
                                    State = (int)SoeEntityState.Active,
                                };
                                SetCreatedProperties(existingRow);
                                entities.AddObject("ScheduledJobRow", existingRow);
                            }
                            else
                            {
                                if (existingRow.RecurrenceInterval != rowInput.RecurrenceInterval || existingRow.NextExecutionTime < DateTime.Now)
                                    existingRow.NextExecutionTime = SchedulerUtility.GetNextExecutionTime(rowInput.RecurrenceInterval);

                                SetModifiedProperties(existingRow);
                            }

                            #region Common

                            existingRow.RecurrenceInterval = rowInput.RecurrenceInterval;
                            existingRow.SysTimeIntervalId = rowInput.SysTimeIntervalId;

                            #endregion
                        }

                        #endregion

                        #endregion

                        #region ScheduledJobSetting

                        List<ScheduledJobSetting> existingSettings = head.ScheduledJobHeadId > 0 ? head.ScheduledJobSetting.Where(r => r.State == (int)SoeEntityState.Active).ToList() : new List<ScheduledJobSetting>();

                        #region Delete settings that exists in db but not in input.

                        if (existingSettings != null)
                        {
                            foreach (ScheduledJobSetting existingSetting in existingSettings)
                            {
                                if (!headInput.Settings.Any(r => r.ScheduledJobSettingId == existingSetting.ScheduledJobSettingId))
                                    ChangeEntityState(existingSetting, SoeEntityState.Deleted);
                            }
                        }

                        #endregion

                        #region Add/update settings

                        foreach (ScheduledJobSettingDTO settingInput in headInput.Settings ?? new List<ScheduledJobSettingDTO>())
                        {
                            ScheduledJobSetting existingSetting = existingSettings != null ? existingSettings.FirstOrDefault(r => r.ScheduledJobSettingId == settingInput.ScheduledJobSettingId) : null;
                            if (existingSetting == null)
                            {
                                existingSetting = new ScheduledJobSetting()
                                {
                                    ScheduledJobHead = head,
                                    State = (int)SoeEntityState.Active,
                                };
                                SetCreatedProperties(existingSetting);
                                entities.AddObject("ScheduledJobSetting", existingSetting);
                                existingSettings.Add(existingSetting);
                            }
                            else
                            {
                                SetModifiedProperties(existingSetting);
                            }

                            #region Common

                            existingSetting.Type = (int)settingInput.Type;
                            existingSetting.DataType = (int)settingInput.DataType;
                            // name has 50 char max
                            existingSetting.Name = string.IsNullOrEmpty(settingInput.Name) ? string.Empty : settingInput.Name.Length > 49 ? settingInput.Name.Substring(0, 49) : settingInput.Name;
                            if (settingInput.StrData != "********")
                                existingSetting.StrData = settingInput.StrData;
                            existingSetting.IntData = settingInput.IntData;
                            existingSetting.DecimalData = settingInput.DecimalData;
                            existingSetting.BoolData = settingInput.BoolData;
                            existingSetting.DateData = settingInput.DateData;
                            existingSetting.TimeData = settingInput.TimeData;

                            #endregion
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);

                        BridgeManager.EncryptionUpdate(entities, existingSettings);

                        if (result.Success)
                        {
                            transaction.Complete();

                            scheduledJobHeadId = head.ScheduledJobHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                        result.IntegerValue = scheduledJobHeadId;
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteScheduledJobHead(int scheduledJobHeadId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Check relations
                if (ScheduledJobHeadHasAttestRuleHeads(scheduledJobHeadId))
                    return new ActionResult((int)ActionResultDelete.ScheduledJobHeadHasAttestRuleHeads, GetText(12017, "Jobbet är kopplat till en automatattestregel och kan därför inte tas bort"));
                if (ScheduledJobHeadHasTimeAccumulatorEmployeeGroupRules(scheduledJobHeadId))
                    return new ActionResult((int)ActionResultDelete.ScheduledJobHeadHasAttestRuleHeads, GetText(12018, "Jobbet är kopplat till ett saldo och kan därför inte tas bort"));

                #endregion

                ScheduledJobHead head = GetScheduledJobHead(entities, scheduledJobHeadId, actorCompanyId, false, false, false, false, false, false);
                if (head == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ScheduledJobHead");

                return ChangeEntityState(entities, head, SoeEntityState.Deleted, true);
            }
        }

        private bool ScheduledJobHeadHasAttestRuleHeads(int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ScheduledJobHeadHasAttestRuleHeads(entities, scheduledJobHeadId);
        }
        private bool ScheduledJobHeadHasAttestRuleHeads(CompEntities entities, int scheduledJobHeadId)
        {
            return entities.AttestRuleHead.Any(s => s.State == (int)SoeEntityState.Active && s.ScheduledJobHeadId == scheduledJobHeadId);
        }

        private bool ScheduledJobHeadHasBridgeSetting(int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ScheduledJobHeadHasBridgeSetting(entities, scheduledJobHeadId);
        }

        private bool ScheduledJobHeadHasBridgeSetting(CompEntities entities, int scheduledJobHeadId)
        {
            return entities.ScheduledJobSetting.Any(w => w.ScheduledJobHeadId == scheduledJobHeadId && w.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJob && w.State == (int)SoeEntityState.Active && w.BoolData == true);
        }

        private bool ScheduledJobHeadIsRunning(CompEntities entities, int scheduledJobHeadId, string scheduledJobHeadIsRunningString)
        {
            DateTime after = DateTime.Now.AddHours(-2);
            var lastLogs = entities.ScheduledJobLog.Where(s => s.Time > after && s.ScheduledJobHeadId == scheduledJobHeadId).OrderByDescending(o => o.Time).Take(20).ToList();
            var lastLog = lastLogs.FirstOrDefault(s => !s.Message.Equals(scheduledJobHeadIsRunningString));
            return lastLog != null && lastLog.Status == (int)ScheduledJobState.Running;
        }

        private bool ScheduledJobHeadHasCustomExportSetting(int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ScheduledJobHeadHasCustomExportSetting(entities, scheduledJobHeadId);
        }

        private bool ScheduledJobHeadHasCustomExportSetting(CompEntities entities, int scheduledJobHeadId)
        {
            return entities.ScheduledJobSetting.Any(w => w.ScheduledJobHeadId == scheduledJobHeadId && w.Type == (int)TermGroup_ScheduledJobSettingType.ExportCustomJob && w.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(w.StrData));
        }

        private bool ScheduledJobHeadHasSpecifiedTypeSetting(int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ScheduledJobHeadHasSpecifiedTypeSetting(entities, scheduledJobHeadId);
        }

        private bool ScheduledJobHeadHasSpecifiedTypeSetting(CompEntities entities, int scheduledJobHeadId)
        {
            return entities.ScheduledJobSetting.Any(w => w.ScheduledJobHeadId == scheduledJobHeadId && w.Type == (int)TermGroup_ScheduledJobSettingType.SpecifiedType && w.State == (int)SoeEntityState.Active && w.IntData.HasValue);
        }

        private bool ScheduledJobHeadHasTimeAccumulatorEmployeeGroupRules(int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ScheduledJobHeadHasTimeAccumulatorEmployeeGroupRules(entities, scheduledJobHeadId);
        }
        private bool ScheduledJobHeadHasTimeAccumulatorEmployeeGroupRules(CompEntities entities, int scheduledJobHeadId)
        {
            return entities.TimeAccumulatorEmployeeGroupRule.Any(t => t.State == (int)SoeEntityState.Active && t.ScheduledJobHeadId == scheduledJobHeadId);
        }

        private bool ScheduledJobHeadHasReportUserSelections(int scheduledJobHeadId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ScheduledJobHeadHasReportUserSelections(entities, scheduledJobHeadId);
        }
        private bool ScheduledJobHeadHasReportUserSelections(CompEntities entities, int scheduledJobHeadId)
        {
            return entities.ReportUserSelection.Any(t => t.State == (int)SoeEntityState.Active && t.ScheduledJobHeadId == scheduledJobHeadId);
        }

        public List<ScheduledJobHead> GetScheduleJobHeadIdsReadyToExcute(CompEntities entities)
        {
            DateTime now = DateTime.Now;
            return entities.ScheduledJobRow.Where(w => w.NextExecutionTime < now && w.State == (int)SoeEntityState.Active && w.ScheduledJobHead.State == (int)SoeEntityState.Active).Select(s => s.ScheduledJobHead).ToList();
        }

        #endregion

        #region ScheduledJobRow


        #endregion

        #region ScheduledJobLog

        public ScheduledJobLog StartLog(int scheduledJobHeadId, int scheduledJobRowId, int batchNr, int? adjustMinutesNextExecution = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                var scheduledJobLog = new ScheduledJobLog()
                {
                    Status = (int)TermGroup_ScheduledJobLogStatus.Running,
                    Time = DateTime.Now,
                    ScheduledJobHeadId = scheduledJobHeadId,
                    BatchNr = batchNr,
                    LogLevel = (int)SystemInfoLogLevel.Information,
                    ScheduledJobRowId = scheduledJobRowId
                };

                entities.ScheduledJobLog.AddObject(scheduledJobLog);
                SaveChanges(entities);

                if (adjustMinutesNextExecution.HasValue)
                {
                    var row = entities.ScheduledJobRow.FirstOrDefault(f => f.ScheduledJobRowId == scheduledJobRowId);
                    if (row != null && row.NextExecutionTime < DateTime.Now && row.NextExecutionTime > DateTime.Now.AddHours(-1))
                    {
                        row.NextExecutionTime = row.NextExecutionTime.AddMinutes(adjustMinutesNextExecution.Value);
                        SaveChanges(entities);
                    }
                }

                return scheduledJobLog;
            }
        }

        public void ErrorLog(int? scheduledJobHeadId, int? scheduledJobRowId, int batchNr, string error)
        {
            using (CompEntities entities = new CompEntities())
            {
                if (!scheduledJobHeadId.HasValue || !scheduledJobRowId.HasValue)
                {
                    var headName = string.Empty;
                    var rowId = string.Empty;
                    if (scheduledJobHeadId.HasValue)
                        headName = "Head " + entities.ScheduledJobHead.FirstOrDefault(f => f.ScheduledJobHeadId == scheduledJobHeadId)?.Name ?? "is null";

                    if (scheduledJobRowId.HasValue)
                        rowId = "RowId " + entities.ScheduledJobRow.FirstOrDefault(f => f.ScheduledJobRowId == scheduledJobRowId)?.ScheduledJobRowId ?? "is null";

                    LogError(new Exception($"InfoLog scheduledJobHeadId or scheduledJobRowId is null. {headName} {rowId} {batchNr}{error}"), log);
                }

                var scheduledJobLog = new ScheduledJobLog()
                {
                    Status = (int)TermGroup_ScheduledJobLogStatus.Aborted,
                    Time = DateTime.Now,
                    ScheduledJobHeadId = scheduledJobHeadId.Value,
                    ScheduledJobRowId = scheduledJobRowId.Value,
                    BatchNr = batchNr,
                    LogLevel = (int)SystemInfoLogLevel.Error,
                    Message = error
                };

                entities.ScheduledJobLog.AddObject(scheduledJobLog);
                SaveChanges(entities);
            }
        }

        public void InfoLog(CompEntities entities, int? scheduledJobHeadId, int? scheduledJobRowId, int batchNr, string message)
        {
            if (!scheduledJobHeadId.HasValue || !scheduledJobRowId.HasValue)
            {
                var headName = string.Empty;
                var rowId = string.Empty;
                if (scheduledJobHeadId.HasValue)
                    headName = "Head " + entities.ScheduledJobHead.FirstOrDefault(f => f.ScheduledJobHeadId == scheduledJobHeadId)?.Name ?? "is null";

                if (scheduledJobRowId.HasValue)
                    rowId = "RowId " + entities.ScheduledJobRow.FirstOrDefault(f => f.ScheduledJobRowId == scheduledJobRowId)?.ScheduledJobRowId ?? "is null";

                LogError(new Exception($"InfoLog scheduledJobHeadId or scheduledJobRowId is null. {headName} {rowId} {batchNr}{message}"), log);
            }

            var scheduledJobLog = new ScheduledJobLog()
            {
                Status = (int)TermGroup_ScheduledJobLogStatus.Running,
                Time = DateTime.Now,
                ScheduledJobHeadId = scheduledJobHeadId.Value,
                ScheduledJobRowId = scheduledJobRowId.Value,
                BatchNr = batchNr,
                LogLevel = (int)SystemInfoLogLevel.Information,
                Message = message
            };

            entities.ScheduledJobLog.AddObject(scheduledJobLog);
            SaveChanges(entities);
        }

        public ActionResult StopLog(int scheduledJobHeadId, int scheduledJobRowId, int batchNr, ActionResult result)
        {
            using (CompEntities entities = new CompEntities())
            {
                string message = result.ErrorMessage;

                if (!result.Success)
                {
                    entities.ScheduledJobLog.AddObject(new ScheduledJobLog()
                    {
                        Status = (int)ScheduledJobState.Interrupted,
                        Time = DateTime.Now,
                        ScheduledJobHeadId = scheduledJobHeadId,
                        ScheduledJobRowId = scheduledJobRowId,
                        BatchNr = batchNr,
                        LogLevel = (int)SystemInfoLogLevel.Error,
                        Message = message
                    });

                    message = string.Empty;
                }

                SaveChanges(entities);

                entities.ScheduledJobLog.AddObject(new ScheduledJobLog()
                {
                    Status = (int)TermGroup_ScheduledJobLogStatus.Finished,
                    Time = DateTime.Now,
                    ScheduledJobHeadId = scheduledJobHeadId,
                    ScheduledJobRowId = scheduledJobRowId,
                    BatchNr = batchNr,
                    LogLevel = (int)SystemInfoLogLevel.Information,
                    Message = message
                });

                return SaveChanges(entities);
            }
        }

        public ActionResult SetNextExecution(int scheduledJobRowId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var row = entities.ScheduledJobRow.FirstOrDefault(f => f.ScheduledJobRowId == scheduledJobRowId);

                if (row != null)
                    row.NextExecutionTime = SchedulerUtility.GetNextExecutionTime(row.RecurrenceInterval);

                return SaveChanges(entities);
            }
        }

        public int GetBatchNumer(int scheduledJobHeadId)
        {
            try
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                return entitiesReadOnly.ScheduledJobLog.Max(s => s.ScheduledJobLogId) + scheduledJobHeadId;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Running job

        public ActionResult RunJobsOnHead(int scheduledJobHeadId, int actorCompanyId, int batchNr, int recursive = 0, ScheduledJobRow parentRow = null, bool forceExecuteNow = false)
        {
            ActionResult result = new ActionResult();
            result.ErrorMessage = string.Empty;
            recursive++;
            var allScheduleJobHeads = GetScheduledJobHeads(actorCompanyId);

            if (recursive > 10)
                return new ActionResult($"RunJobsOnHead recursive > 10 actorCompanyId: {actorCompanyId}");

            DateTime now = DateTime.Now;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var head = entitiesReadOnly.ScheduledJobHead.Include("ScheduledJobRow").Include("ScheduledJobSetting").FirstOrDefault(f => f.State == (int)SoeEntityState.Active && f.ScheduledJobHeadId == scheduledJobHeadId);

            if (head != null && (forceExecuteNow || !head.ScheduledJobRow.IsNullOrEmpty() && head.ScheduledJobRow.Any(a => a.State == (int)SoeEntityState.Active && (parentRow != null || a.NextExecutionTime < now))))
            {
                var row = head.ScheduledJobRow.FirstOrDefault(a => a.State == (int)SoeEntityState.Active && (parentRow != null || a.NextExecutionTime < now));

                if (forceExecuteNow && row == null)
                    row = head.ScheduledJobRow.FirstOrDefault(a => a.State == (int)SoeEntityState.Active);

                if (row == null)
                    return new ActionResult();
                else if (parentRow == null)
                    parentRow = row;

                head.Children.Load();
                string scheduledJobHeadIsRunning = "ScheduledJobHeadIsRunning";
                if (!forceExecuteNow && ScheduledJobHeadIsRunning(entitiesReadOnly, scheduledJobHeadId, scheduledJobHeadIsRunning))
                {
                    using (CompEntities entities = new CompEntities())
                        InfoLog(entities, scheduledJobHeadId, row.ScheduledJobRowId, batchNr, scheduledJobHeadIsRunning);
                }
                else
                {
                    try
                    {
                        if (ScheduledJobHeadHasReportUserSelections(scheduledJobHeadId))
                        {
                            StartLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr);
                            RunReportUserSelection(actorCompanyId, batchNr, now, head, parentRow);
                            StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, result);
                        }

                        if (ScheduledJobHeadHasTimeAccumulatorEmployeeGroupRules(scheduledJobHeadId))
                        {
                            StartLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr);
                            RunTimeAccumulatorEmployeeGroupRules(actorCompanyId, batchNr, now, head, parentRow, forceExecuteNow);
                            StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, result);
                        }

                        if (ScheduledJobHeadHasAttestRuleHeads(scheduledJobHeadId))
                        {
                            StartLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr);
                            result = RunAutoAttest(actorCompanyId, batchNr, now, head, allScheduleJobHeads, parentRow);
                            StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, result);
                        }

                        if (ScheduledJobHeadHasCustomExportSetting(scheduledJobHeadId))
                        {
                            StartLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, 60);
                            result = RunCustomJob(actorCompanyId, batchNr, now, head, parentRow);
                            StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, result);
                        }

                        if (ScheduledJobHeadHasSpecifiedTypeSetting(scheduledJobHeadId))
                        {
                            StartLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr);
                            result = RunSpecifiedTypeJob(actorCompanyId, batchNr, now, head, parentRow);
                            StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, result);
                        }

                        if (ScheduledJobHeadHasBridgeSetting(scheduledJobHeadId))
                        {
                            var bridgeJobRunChildJobs = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJobRunChildJobs && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.State == (int)SoeEntityState.Active)?.BoolData ?? false;

                            base.parameterObject = parameterObject.Clone(actorCompanyId: actorCompanyId,
                                            activeRoleId: UserManager.GetDefaultRoleId(actorCompanyId, base.UserId),
                                            thread: THREAD);

                            if (bridgeJobRunChildJobs && !head.Children.IsNullOrEmpty())
                            {
                                var childResult = new ActionResult();
                                foreach (var child in head.Children.Where(w => w.State == (int)SoeEntityState.Active).OrderBy(o => o.Sort))
                                {
                                    child.ScheduledJobSetting.Load();

                                    base.parameterObject = parameterObject.Clone(
                                        actorCompanyId: child.ActorCompanyId,
                                        activeRoleId: UserManager.GetDefaultRoleId(child.ActorCompanyId, base.UserId),
                                        thread: THREAD);

                                    if (base.parameterObject.RoleId == 0)
                                        continue;

                                    childResult = RunBridgeJob(entitiesReadOnly, child.ActorCompanyId, batchNr, now, child, row, childResult, isChild: true);

                                    if (!string.IsNullOrEmpty(childResult.ErrorMessage))
                                    {
                                        result.ErrorMessage += childResult.ErrorMessage + Environment.NewLine;
                                    }
                                }

                                base.parameterObject = parameterObject.Clone(actorCompanyId: actorCompanyId,
                                                activeRoleId: UserManager.GetDefaultRoleId(actorCompanyId, base.UserId),
                                                thread: THREAD);

                                var lastResultbridgeResult = RunBridgeJob(entitiesReadOnly, actorCompanyId, batchNr, now, head, row, childResult);
                                StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, lastResultbridgeResult);

                                result = lastResultbridgeResult;
                            }
                            else
                            {
                                StartLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr);
                                var bridgeResult = RunBridgeJob(entitiesReadOnly, actorCompanyId, batchNr, now, head, row);
                                StopLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, result);
                            }
                        }
                        else
                        {
                            if (!head.Children.IsNullOrEmpty())
                            {
                                foreach (var child in head.Children.OrderBy(o => o.Sort).Where(w => w.ActorCompanyId == actorCompanyId))
                                    RunJobsOnHead(child.ScheduledJobHeadId, actorCompanyId, batchNr, recursive, parentRow);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog(scheduledJobHeadId, row.ScheduledJobRowId, batchNr, "Job failed");
                        LogError(ex, log);
                    }

                    if (recursive == 1 && row.ScheduledJobRowId == parentRow.ScheduledJobRowId)
                        result = SetNextExecution(row.ScheduledJobRowId);
                }
            }
            else
            {
                var headinfo = head == null ? "Head is null" : head.Name;
                var nextExecutionInfo = "Next execution: " + (head.ScheduledJobRow?.OrderByDescending(f => f.NextExecutionTime).FirstOrDefault()?.NextExecutionTime.ToString() ?? "Unknown");
                result.ErrorMessage = "No jobs to run {headinfo}  {nextExecutionInfo} ";
            }

            return result;
        }

        public void RunTimeAccumulatorEmployeeGroupRules(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow parentRow = null, bool forceExecuteNow = false)
        {
            #region Prereq

            var sendToUserSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.TimeAccumulator_SendToUser && a.DataType == (int)SettingDataType.Boolean);

            bool sendToUser = true;
            if (sendToUserSetting != null)
            {
                if (!sendToUserSetting.BoolData.HasValue)
                    sendToUser = false;
                else
                    sendToUser = sendToUserSetting.BoolData.Value;
            }

            var sendToExecutiveSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.TimeAccumulator_SendToExecutive && a.DataType == (int)SettingDataType.Boolean);

            bool sendToExecutive = true;
            if (sendToExecutiveSetting != null)
            {
                if (!sendToExecutiveSetting.BoolData.HasValue)
                    sendToExecutive = false;
                else
                    sendToExecutive = sendToExecutiveSetting.BoolData.Value;
            }

            var includeFutureMonthSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.TimeAccumulator_IncludeFutureMonth && a.DataType == (int)SettingDataType.Boolean);

            var includeFutureMonth = true;
            if (includeFutureMonthSetting != null)
            {
                if (!includeFutureMonthSetting.BoolData.HasValue)
                    includeFutureMonth = false;
                else
                    includeFutureMonth = includeFutureMonthSetting.BoolData.Value;
            }

            bool adjustCurrentBalance = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.TimeAccumulator_AdjustCurrentBalance && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.BoolData.Value)?.BoolData ?? false;
            string cronValue = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.TimeAccumulator_AdjustCurrentBalanceDate && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData))?.StrData ?? "";

            DateTime adjustCurrentBalanceDate = DateTime.Today;
            bool cronValueValid = false;
            bool cronHadLChar = false;
            if (!string.IsNullOrEmpty(cronValue))
            {
                cronHadLChar = cronValue.ToLower().Contains("l");
                // If day is L in CRON it it the last day
                if (cronHadLChar)
                {
                    // Replace cronvalue with the last day in previous month
                    var lastDay = CalendarUtility.GetLastDateOfMonth(DateTime.Now.AddMonths(-1)).Day;
                    var originalCronValue = cronValue;
                    cronValue = cronValue.Replace("L", lastDay.ToString());

                    using (CompEntities entities = new CompEntities())
                        InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Cron value {originalCronValue} contains L for last day of month, replacing L with {lastDay} to {cronValue}");
                }

                var lastDate = SchedulerUtility.GetLastPreviousValidTime(cronValue, now);

                using (CompEntities entities = new CompEntities())
                {
                    if (now <= lastDate)
                        InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Cron value {cronValue} is not valid for this date, is the same as the current time {lastDate}");
                    else
                    {
                        InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Cron value {cronValue} is valid for this date, last valid date is {lastDate}");
                        adjustCurrentBalanceDate = lastDate;
                        cronValueValid = true;
                    }
                }
            }

            List<TimeBalanceIODTO> timeBalances = new List<TimeBalanceIODTO>();

            List<TimeAccumulatorEmployeeGroupRule> timeAccumulatorEmployeeGroupRules = TimeAccumulatorManager.GetTimeAccumulatorEmployeeGroupRulesForCompany(actorCompanyId, head.ScheduledJobHeadId).ToList();
            if (timeAccumulatorEmployeeGroupRules.IsNullOrEmpty())
                return;

            List<TimeAccumulator> timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(actorCompanyId, loadEmployeeGroupRule: true);
            if (timeAccumulators.IsNullOrEmpty())
                return;

            ScheduledJobRow row = head.ScheduledJobRow.FirstOrDefault(f => f.NextExecutionTime < now && f.State == (int)SoeEntityState.Active);
            if (row?.SysTimeIntervalId == null && !forceExecuteNow)
                return;

            if (forceExecuteNow && row == null)
                row = head.ScheduledJobRow.FirstOrDefault(f => f.SysTimeIntervalId.HasValue && f.State == (int)SoeEntityState.Active);

            if (row?.SysTimeIntervalId == null)
                return;

            SysTimeInterval sysInterval = CalendarManager.GetSysTimeInterval(row.SysTimeIntervalId.Value, false);
            DateRangeDTO interval = parentRow == null ? CalendarManager.GetSysTimeIntervalDateRange(row.SysTimeIntervalId.Value, row.NextExecutionTime) : CalendarManager.GetSysTimeIntervalDateRange(parentRow.SysTimeIntervalId.Value, parentRow.NextExecutionTime);
            if (interval == null)
                return;

            if (!includeFutureMonth && sysInterval.Period == (int)TermGroup_TimeIntervalPeriod.Year && interval.Start.Month == 1 && interval.Start.Year == DateTime.Today.Year && DateTime.Today.Month == 1 && DateTime.Today.Day < 15)
            {
                interval.Start = interval.Start.AddYears(-1);
                interval.Stop = interval.Stop.AddYears(-1);

                using (CompEntities entities = new CompEntities())
                    InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Adjusting interval startDate to {interval.Start.ToShortDateString()} since interval is Year and time remains in last year");
            }

            if (!cronValueValid && adjustCurrentBalance)
            {
                // Default to day after interval stop if no valid cron value is set
                using (CompEntities entities = new CompEntities())
                    InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"No valid cron value set, defaulting to day after interval stop {interval.Stop.AddDays(1)}");

                adjustCurrentBalanceDate = interval.Stop.AddDays(1);
            }

            if (!includeFutureMonth && interval.Stop > adjustCurrentBalanceDate && sysInterval.Period == (int)TermGroup_TimeIntervalPeriod.Year)
            {
                if (!cronHadLChar)
                    interval.Stop = adjustCurrentBalanceDate.AddDays(-1);
                else
                    interval.Stop = adjustCurrentBalanceDate;

                using (CompEntities entities = new CompEntities())
                    InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Adjusting interval stopDate to {interval.Stop.ToShortDateString()} since interval is Year and time remains in year");
            }

            if (interval.Start > interval.Stop)
            {
                string additionalInfo = row.NextExecutionTime.Year > DateTime.Now.Year ? " Next execution is in future year. " : string.Empty;
                using (CompEntities entities = new CompEntities())
                    InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Interval start date {interval.Start.ToShortDateString()} is after stop date {interval.Stop.ToShortDateString()}, {additionalInfo}exiting");

                return;
            }

            List<Employee> allEmployees = EmployeeManager.GetAllEmployees(actorCompanyId, true, loadEmployment: true);
            if (allEmployees.IsNullOrEmpty())
                return;

            using (CompEntities entities = new CompEntities())
                InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Found {allEmployees.Count} employees");

            parameterObject.SetSoeCompany(CompanyManager.GetSoeCompany(actorCompanyId));
            parameterObject.SetSoeUser(UserManager.GetSoeUserAdmin(actorCompanyId));
            if (parameterObject.SoeCompany == null || parameterObject.SoeUser == null)
                return;

            var validEmployeeItems = ValidateEmployees();
            if (validEmployeeItems.IsNullOrEmpty())
                return;

            using (CompEntities entities = new CompEntities())
                InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, $"Found {validEmployeeItems.Count} employees with valid employee group");

            List<Employee> validEmployees = validEmployeeItems.Select(i => i.Employee).ToList();
            List<int> employeeGroupTimeAccumulatorIds = timeAccumulatorEmployeeGroupRules.Select(s => s.TimeAccumulatorId).Distinct().ToList();

            GetTimeAccumulatorItemsInput input;
            if (includeFutureMonth)
            {
                input = GetTimeAccumulatorItemsInput.CreateInput(actorCompanyId, parameterObject.UserId, 0, interval.Stop, interval.Stop.AddMonths(1), addSourceIds: true, calculateDay: true, calculatePeriod: true, calculatePlanningPeriod: true, calculateYear: true, calculateAccToday: true, employees: validEmployees, timeAccumulatorIds: employeeGroupTimeAccumulatorIds);
            }
            else
            {
                input = GetTimeAccumulatorItemsInput.CreateInput(actorCompanyId, parameterObject.UserId, 0, interval.Start, interval.Stop, addSourceIds: true, calculateDay: true, calculatePeriod: true, calculatePlanningPeriod: false, calculateYear: true, calculateAccToday: true, employees: validEmployees, timeAccumulatorIds: employeeGroupTimeAccumulatorIds, overrideDateOnYear: cronValueValid, includeBalanceYear: true);
            }

            Dictionary<int, List<TimeAccumulatorItem>> itemsByEmployee = TimeAccumulatorManager.GetTimeAccumulatorItemsByEmployee(input);
            var userMessages = new List<Tuple<int, string>>();
            var executiveMessages = new List<Tuple<int, bool, string, string, string, string, string>>();

            #endregion

            #region Perform

            foreach (var employeeItem in validEmployeeItems.Where(i => i.Employee.UserId.HasValue))
            {
                AnalyzeEmployeeAccumulators(employeeItem.Employee, employeeItem.EmployeeGroup);
            }

            if (!userMessages.Any() && !executiveMessages.Any())
                return;

            List<SystemInfoLog> infoEntries = GeneralManager.GetSystemInfoLogEntries(actorCompanyId, SystemInfoType.TimeAccumulatorWarning, activeOnly: false).Where(w => w.Date > DateTime.Today.AddMonths(-1)).ToList();

            foreach (var message in userMessages.GroupBy(g => g.Item1))
            {
                SendMessage(message);
            }

            #endregion

            List<(Employee Employee, EmployeeGroup EmployeeGroup)> ValidateEmployees()
            {
                var tupleList = new List<(Employee Employee, EmployeeGroup EmployeeGroup)>();

                List<int> employeeGroupIds = timeAccumulatorEmployeeGroupRules.Select(i => i.EmployeeGroupId).Distinct().ToList();
                if (employeeGroupIds.IsNullOrEmpty())
                    return tupleList;

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                List<EmployeeGroup> employeeGroups = base.GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
                List<PayrollPriceType> payrollPriceTypes = base.GetPayrollPriceTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
                List<PayrollGroup> payrollGroups = base.GetPayrollGroupsForPersonalDataRepoFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
                foreach (Employee employee in allEmployees.Where(w => w.UserId.HasValue))
                {
                    if (tupleList.Any(i => i.Employee.EmployeeId == employee.EmployeeId))
                        continue;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(interval.Start, interval.Stop, employeeGroups, forward: false);
                    if (employeeGroup == null || !employeeGroupIds.Contains(employeeGroup.EmployeeGroupId))
                        continue;

                    if (!employee.UserReference.IsLoaded)
                        employee.UserReference.Load();

                    tupleList.Add((employee, employeeGroup));
                }

                return tupleList;
            }

            void AnalyzeEmployeeAccumulators(Employee employee, EmployeeGroup employeeGroup)
            {
                if (!itemsByEmployee.ContainsKey(employee.EmployeeId))
                    return;
                List<TimeAccumulatorItem> itemsForEmployee = itemsByEmployee[employee.EmployeeId];
                if (employee == null || employeeGroup == null || itemsForEmployee.IsNullOrEmpty())
                    return;

                int langId = employee.User?.LangId ?? (int)TermGroup_Languages.Swedish;

                foreach (TimeAccumulatorItem item in itemsForEmployee)
                {
                    if (!employeeGroupTimeAccumulatorIds.Contains(item.TimeAccumulatorId))
                        continue;

                    TimeAccumulator timeAccumulator = timeAccumulators.FirstOrDefault(f => f.TimeAccumulatorId == item.TimeAccumulatorId);
                    if (timeAccumulator == null)
                        continue;

                    foreach (TimeAccumulatorRuleItem ruleItem in item.EmployeeGroupRules.Where(i => i.ShowWarning || i.ShowError))
                    {
                        string value = CalendarUtility.GetHoursAndMinutesString(Convert.ToInt32(ruleItem.ValueMinutes));
                        string warning = CalendarUtility.GetHoursAndMinutesString(ruleItem.WarningMinutes);
                        string error = CalendarUtility.GetHoursAndMinutesString(ruleItem.WarningMinutes);
                        decimal limit = decimal.Round(decimal.Divide(ruleItem.WarningMinutes, 60), 2);
                        string adjustingInfo = string.Empty;
                        if (adjustCurrentBalance)
                            adjustingInfo = string.Format(GetText(12028, (int)TermGroup.General, langId, "Justerar automatiskt till gräns på datum {0}"), adjustCurrentBalanceDate.ToShortDateString());

                        var logPost = "Automatically adjusting time balance for employee " + employee.EmployeeNr + " from " + value + " to " + CalendarUtility.GetHoursAndMinutesString(Convert.ToInt32(limit * 60)) + " on acc " + timeAccumulator.Name + " @ date " + adjustCurrentBalanceDate.ToShortDateString();

                        var logMessage = string.Empty;

                        switch (ruleItem.Comparison)
                        {
                            case SoeTimeAccumulatorComparison.LessThanMinWarning:
                                userMessages.Add(Tuple.Create(employee.UserId.Value, string.Format(GetText(12027, (int)TermGroup.General, langId, "Du understiger nedre varninggräns på {0} saldo {1} gräns {2}"), timeAccumulator.Name, value, warning)));
                                executiveMessages.Add(Tuple.Create(employee.UserId.Value, false, employee.EmployeeNrAndName, timeAccumulator.Name, value, warning, adjustingInfo));
                                break;
                            case SoeTimeAccumulatorComparison.MoreThanMaxWarning:
                                userMessages.Add(Tuple.Create(employee.UserId.Value, string.Format(GetText(12020, (int)TermGroup.General, langId, "Du överstiger varningsgräns på {0} saldo {1} gräns {2}"), timeAccumulator.Name, value, warning)));
                                executiveMessages.Add(Tuple.Create(employee.UserId.Value, true, employee.EmployeeNrAndName, timeAccumulator.Name, value, warning, ""));
                                break;
                            case SoeTimeAccumulatorComparison.LessThanMin:
                                userMessages.Add(Tuple.Create(employee.UserId.Value, string.Format(GetText(12106, (int)TermGroup.General, langId, "Du understiger nedre gräns på {0} saldo {1} gräns {2}"), timeAccumulator.Name, value, warning)));
                                executiveMessages.Add(Tuple.Create(employee.UserId.Value, false, employee.EmployeeNrAndName, timeAccumulator.Name, value, error, ""));
                                logMessage = logPost;
                                timeBalances.Add(new TimeBalanceIODTO()
                                {
                                    Code = timeAccumulator.Name,
                                    TimeBalanceIOType = TimeBalanceIOType.TimeAccumulator,
                                    AdjustmentDate = adjustCurrentBalanceDate,
                                    Date = adjustCurrentBalanceDate,
                                    EmployeeNr = employee.EmployeeNr,
                                    Quantity = limit
                                });
                                break;
                            case SoeTimeAccumulatorComparison.MoreThanMax:
                                userMessages.Add(Tuple.Create(employee.UserId.Value, string.Format(GetText(12107, (int)TermGroup.General, langId, "Du överstiger gräns på {0} saldo {1} gräns {2}"), timeAccumulator.Name, value, warning)));
                                executiveMessages.Add(Tuple.Create(employee.UserId.Value, true, employee.EmployeeNrAndName, timeAccumulator.Name, value, error, adjustingInfo));
                                logMessage = logPost;
                                timeBalances.Add(new TimeBalanceIODTO()
                                {
                                    Code = timeAccumulator.Name,
                                    TimeBalanceIOType = TimeBalanceIOType.TimeAccumulator,
                                    AdjustmentDate = adjustCurrentBalanceDate,
                                    Date = adjustCurrentBalanceDate,
                                    EmployeeNr = employee.EmployeeNr,
                                    Quantity = limit,
                                });
                                break;
                        }

                        if (!string.IsNullOrEmpty(logMessage))
                            using (CompEntities entities = new CompEntities())
                                InfoLog(entities, head.ScheduledJobHeadId, parentRow.ScheduledJobRowId, batchNr, logMessage);

                    }
                }
            }

            void SendMessage(IGrouping<int, Tuple<int, string>> message)
            {
                var affectedUser = UserManager.GetUser(message.Key);
                using (CompEntities entities = new CompEntities())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in message)
                    {
                        sb.Append(item.Item2).Append(Environment.NewLine);
                    }

                    bool sentToExec = false;
                    Employee employee = validEmployees.FirstOrDefault(f => f.UserId.HasValue && f.UserId.Value == message.Key);
                    List<UserDTO> execUsers = UserManager.GetEmployeeNearestExecutives(employee, DateTime.Today, DateTime.Today.AddDays(10), actorCompanyId);
                    if (execUsers != null)
                    {
                        foreach (var execUser in execUsers)
                        {
                            //Check if reminder has already been sent for this day or last week
                            var logsForUser = infoEntries.FilterSystemInfoLogs(execUser.UserId, DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1)).Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                            sentToExec = false;
                            int langId = execUser.LangId.HasValue ? execUser.LangId.Value : (int)TermGroup_Languages.Swedish;
                            var text = string.Join(Environment.NewLine, executiveMessages
                                .Where(w => w.Item1 == message.Key)
                                .Select(s => string.Format(GetText(s.Item2 ? 12021 : 12027, (int)TermGroup.General, langId, s.Item2 ? "{0} överstiger varningsgräns på {1} saldo {2} gräns {3}" : "{0} understiger nedre varningsgräns på {1} saldo {2} gräns {3}. {4}"), s.Item3, s.Item4, s.Item5, s.Item6, s.Item7)).Distinct().ToList());

                            if (logsForUser.Any() && logsForUser.OrderByDescending(o => o.Created).FirstOrDefault().Text == text)
                                continue;

                            SystemInfoLog systemInfoLog = new SystemInfoLog()
                            {
                                Type = (int)SystemInfoType.TimeAccumulatorWarning,
                                Entity = (int)SoeEntityType.XEMail,
                                LogLevel = (int)SystemInfoLogLevel.Warning,
                                RecordId = execUser.UserId,
                                Text = text,
                                Date = DateTime.Now,
                                DeleteManually = true,
                                EmployeeId = employee.EmployeeId,

                                //Set FK
                                ActorCompanyId = actorCompanyId,
                            };

                            infoEntries.Add(systemInfoLog);
                            var result = GeneralManager.AddSystemInfoLogEntry(entities, systemInfoLog);

                            MessageEditDTO messageEdit = new MessageEditDTO()
                            {
                                Entity = SoeEntityType.User,
                                RecordId = 0,
                                ActorCompanyId = null,
                                LicenseId = parameterObject.SoeUser.LicenseId,
                                SenderUserId = parameterObject.UserId,
                                Subject = GetText(12029, 1, langId, "Saldovarning"),
                                Text = Common.Util.StringUtility.ConvertNewLineToHtml(text),
                                ShortText = text,
                                SenderName = execUser.Name,
                                SenderEmail = "",
                                Created = DateTime.Now,
                                SentDate = DateTime.Now,
                                MessagePriority = TermGroup_MessagePriority.None,
                                MessageType = TermGroup_MessageType.AutomaticInformation,
                                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                                MessageTextType = TermGroup_MessageTextType.Text,
                            };

                            MessageRecipientDTO recipient = new MessageRecipientDTO()
                            {
                                UserId = execUser.UserId,
                                UserName = execUser.LoginName,
                                Name = execUser.Name,
                                EmailAddress = execUser.Email,
                                Type = XEMailRecipientType.User,
                            };

                            messageEdit.Recievers.Add(recipient);

                            if (result.Success && sendToExecutive)
                            {
                                sentToExec = true;
                                CommunicationManager.SendXEMail(messageEdit, actorCompanyId, 0, parameterObject.UserId);
                            }
                        }
                    }

                    if ((sendToUser && sendToExecutive && sentToExec) || sendToUser)
                    {
                        var textTerm = sb.ToString();
                        int langId = affectedUser.LangId.HasValue ? affectedUser.LangId.Value : (int)TermGroup_Languages.Swedish;

                        MessageEditDTO messageEditForUser = new MessageEditDTO()
                        {
                            Entity = SoeEntityType.User,
                            RecordId = 0,
                            ActorCompanyId = null,
                            LicenseId = parameterObject.SoeUser.LicenseId,
                            SenderUserId = parameterObject.UserId,
                            Subject = GetText(12029, 1, langId, "Saldovarning"),
                            Text = Common.Util.StringUtility.ConvertNewLineToHtml(textTerm),
                            ShortText = textTerm,
                            SenderName = parameterObject.SoeUser.Name,
                            SenderEmail = "",
                            Created = DateTime.Now,
                            SentDate = DateTime.Now,
                            MessagePriority = TermGroup_MessagePriority.None,
                            MessageType = TermGroup_MessageType.AutomaticInformation,
                            MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                            MessageTextType = TermGroup_MessageTextType.Text,
                        };

                        messageEditForUser.Recievers.Add(new MessageRecipientDTO()
                        {
                            UserId = affectedUser.UserId,
                            UserName = affectedUser.LoginName,
                            Name = affectedUser.Name,
                            EmailAddress = affectedUser.Email,
                            Type = XEMailRecipientType.User,
                        });

                        CommunicationManager.SendXEMail(messageEditForUser, actorCompanyId, 0, parameterObject.UserId);
                    }
                }
            }

            if (adjustCurrentBalance)
            {
                ImportExportManager.ImportFromTimeBalanceIO(timeBalances, actorCompanyId);
            }
        }

        public ActionResult RunReportUserSelection(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow parentRow = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var reportUserSelections = entitiesReadOnly.ReportUserSelection.Where(w => w.UserId.HasValue && w.ScheduledJobHeadId == head.ScheduledJobHeadId && w.State == (int)SoeEntityState.Active).ToList();

            if (!reportUserSelections.IsNullOrEmpty())
            {
                foreach (var reportUserSelection in reportUserSelections)
                {
                    var row = head.ScheduledJobRow.FirstOrDefault(f => f.NextExecutionTime < now);
                    if (row == null)
                        return new ActionResult(false);

                    parameterObject.SetSoeUser(UserManager.GetSoeUser(actorCompanyId, reportUserSelection.UserId.Value));
                    parameterObject.SetSoeCompany(CompanyManager.GetSoeCompany(actorCompanyId));

                    ReportDataManager reportDataManager = new ReportDataManager(parameterObject);
                    reportDataManager.RePrintMigratedReportUserSelectionDTO(reportUserSelection.ReportUserSelectionId, reportUserSelection.UserId.Value, parentRow != null ? parentRow.SysTimeIntervalId : row.SysTimeIntervalId);
                }
            }

            return new ActionResult();
        }

        public ActionResult RunAutoAttest(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, List<ScheduledJobHead> allScheduleJobHeads, ScheduledJobRow parentRow = null)
        {
            var row = head.ScheduledJobRow.FirstOrDefault(f => f.NextExecutionTime < now);
            if (row?.SysTimeIntervalId == null)
                return new ActionResult(false);

            parameterObject.SetSoeUser(UserManager.GetSoeUserAdmin(actorCompanyId));
            parameterObject.SetSoeCompany(CompanyManager.GetSoeCompany(actorCompanyId));

            var interval = parentRow == null ? CalendarManager.GetSysTimeIntervalDateRange(row.SysTimeIntervalId.Value, row.NextExecutionTime) : CalendarManager.GetSysTimeIntervalDateRange(parentRow.SysTimeIntervalId.Value, parentRow.NextExecutionTime);
            TimeEngineManager tem = new TimeEngineManager(parameterObject, actorCompanyId, parameterObject.SoeUser?.UserId ?? 0);
            ActionResult result = tem.RunAutoAttest(EmployeeManager.GetAllEmployeeIds(actorCompanyId), interval.Start, interval.Stop, RunTogetherHeads(head, allScheduleJobHeads));

            return result;
        }

        public ActionResult RunCustomJob(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow parentRow = null)
        {
            var row = head.ScheduledJobRow.FirstOrDefault(f => f.NextExecutionTime < now);

            if (row == null)
                row = head.ScheduledJobRow.FirstOrDefault(f => f.State == (int)SoeEntityState.Active);

            if (row == null)
                return new ActionResult(false);

            parameterObject.SetSoeUser(UserManager.GetSoeUserAdmin(actorCompanyId));
            parameterObject.SetSoeCompany(CompanyManager.GetSoeCompany(actorCompanyId));


            var interval = row?.SysTimeIntervalId != null ? CalendarManager.GetSysTimeIntervalDateRange(row.SysTimeIntervalId.Value, row.NextExecutionTime) : parentRow?.SysTimeIntervalId != null ? CalendarManager.GetSysTimeIntervalDateRange(parentRow.SysTimeIntervalId.Value, parentRow.NextExecutionTime) : null;

            using (CompEntities entities = new CompEntities())
            {
                var setting = entities.ScheduledJobSetting.FirstOrDefault(w => w.ScheduledJobHeadId == head.ScheduledJobHeadId && w.Type == (int)TermGroup_ScheduledJobSettingType.ExportCustomJob && w.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(w.StrData));

                if (base.IsAxfood() && setting.StrData == "Axfood")
                {
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Axfood database confirmed");
                    var company = CompanyManager.GetCompany(entities, actorCompanyId);
                    var jobHead = entities.ScheduledJobHead.Include(i => i.ScheduledJobSetting).FirstOrDefault(f => f.ScheduledJobHeadId == head.ScheduledJobHeadId);

                    if (jobHead == null)
                    {
                        LogInfo($"{actorCompanyId} JobHead is null");
                        return new ActionResult((int)ActionResultSelect.EntityNotFound, "ScheduledJobHead");
                    }

                    var scheduledJobSettings = head.ScheduledJobSetting.Where(w => w.State == (int)SoeEntityState.Active).ToList();

                    var customJobSetting = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportCustomJob && a.State == (int)SoeEntityState.Active);

                    if (customJobSetting == null)
                    {
                        LogInfo($"{actorCompanyId} {jobHead.Name} is not a CustomJob");
                        return new ActionResult(false);
                    }

                    var split800Setting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.State == (int)SoeEntityState.Active && a.StrData == "Split800");

                    var split800 = false;

                    if (split800Setting == null)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Split800 setting is null (ExportKey with value Split800 does not exist)");
                    }
                    else
                    {
                        split800 = true;
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Split800 setting is not null and active");
                    }

                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Split800: {split800}");

                    var numberOfDaysSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(a.StrData) && a.StrData.StartsWith("NumberOfDays:"));

                    int numberOfDays = 35;
                    DateTime? specifiedDateFrom = null;
                    DateTime? specifiedDateTo = null;

                    if (numberOfDaysSetting != null)
                    {
                        var parts = numberOfDaysSetting.StrData.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int parsedDays))
                        {
                            numberOfDays = parsedDays;
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"numberOfDays setting found with value {numberOfDays}");
                        }
                        else
                        {
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"numberOfDays setting found but could not parse value from {numberOfDaysSetting.StrData}, using default {numberOfDays}");
                        }
                    }
                    else
                    {
                        var specifiedDateFromSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(a.StrData) && a.StrData.StartsWith("DateFrom:"))?.StrData;

                        if (specifiedDateFromSetting != null)
                        {
                            var parts = specifiedDateFromSetting.Split(':');
                            if (parts.Length == 2 && DateTime.TryParse(parts[1], out DateTime parsedDateFrom))
                            {
                                specifiedDateFrom = parsedDateFrom;
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"specifiedDateFrom setting found with value {specifiedDateFrom.Value.ToShortDateString()}");
                            }
                            else
                            {
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"specifiedDateFrom setting found but could not parse value from {specifiedDateFrom}, using default numberOfDays {numberOfDays}");
                            }
                        }

                        var specifiedDateToSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(a.StrData) && a.StrData.StartsWith("DateTo:"))?.StrData;
                        if (specifiedDateToSetting != null)
                        {
                            var parts = specifiedDateToSetting.Split(':');
                            if (parts.Length == 2 && DateTime.TryParse(parts[1], out DateTime parsedDateTo))
                            {
                                specifiedDateTo = parsedDateTo;
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"specifiedDateTo setting found with value {specifiedDateTo.Value.ToShortDateString()}");
                            }
                            else
                            {
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"specifiedDateTo setting found but could not parse value from {specifiedDateTo}, using default numberOfDays {numberOfDays}");
                            }
                        }

                        if (specifiedDateFrom.HasValue && specifiedDateTo.HasValue && specifiedDateTo < specifiedDateFrom)
                        {
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"specifiedDateTo {specifiedDateTo.Value.ToShortDateString()} is before specifiedDateFrom {specifiedDateFrom.Value.ToShortDateString()}, ignoring both and using default numberOfDays {numberOfDays}");
                            specifiedDateFrom = null;
                            specifiedDateTo = null;
                        }
                    }

                    DateTime fromDate = specifiedDateFrom ?? interval?.Start ?? DateTime.Now.AddDays(-numberOfDays).Date;
                    DateTime toDate = specifiedDateTo ?? interval?.Stop ?? DateTime.Now.Date;

                    if (fromDate > toDate)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"From date {fromDate.ToShortDateString()} is after to date {toDate.ToShortDateString()}, cannot proceed");
                        return new ActionResult($"From date {fromDate.ToShortDateString()} is after to date {toDate.ToShortDateString()}, cannot proceed");
                    }

                    if (DateTime.Now.Hour >= 23)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Added one day on start date and stop date because it is almost midnight");
                        fromDate = fromDate.AddDays(1);
                        toDate = toDate.AddDays(1);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Adjusted date interval {fromDate.ToShortDateString()}-{toDate.ToShortDateString()}");
                    }
                    else
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Dates {fromDate.ToShortDateString()}-{toDate.ToShortDateString()}");


                    var allowSplittingEDWSetting = head.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.State == (int)SoeEntityState.Active && a.StrData == "AllowSplittingEDW");
                    var allowSplittingEDW = false;

                    if (allowSplittingEDWSetting == null)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "AllowSplittingEDW setting (ExportKey:AllowSplittingEDW) is null (ExportKey with value AllowSplittingEDW does not exist)");
                    }
                    else
                    {
                        allowSplittingEDW = true;
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "AllowSplittingEDW setting is not null and active");
                    }


                    var result = ImportExportManager.RunAxfoodExport(company, fromDate, toDate, split800: split800, batchNr: batchNr, jobHeadId: head?.ScheduledJobHeadId, allowSplittingEDW: allowSplittingEDW);
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, result);
                    return new ActionResult() { InfoMessage = result };
                }
                else if (base.IsAxfood() && setting.StrData == "AxfoodPEO2")
                {
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Axfood database confirmed with job AxfoodPEO2");
                    var company = CompanyManager.GetCompany(entities, actorCompanyId);
                    var jobHead = entities.ScheduledJobHead.Include(i => i.ScheduledJobSetting).FirstOrDefault(f => f.ScheduledJobHeadId == head.ScheduledJobHeadId);

                    if (jobHead == null)
                    {
                        LogInfo($"{actorCompanyId} JobHead is null");
                        return new ActionResult((int)ActionResultSelect.EntityNotFound, "ScheduledJobHead");
                    }

                    var scheduledJobSettings = head.ScheduledJobSetting.Where(w => w.State == (int)SoeEntityState.Active).ToList();

                    var customJobSetting = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportCustomJob && a.State == (int)SoeEntityState.Active);

                    if (customJobSetting == null)
                    {
                        LogInfo($"{actorCompanyId} {jobHead.Name} is not a CustomJob");
                        return new ActionResult(false);
                    }

                    var result = ImportExportManager.RunAxfoodPeo2Export(company.ActorCompanyId, head.ScheduledJobHeadId, batchNr);
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, result);
                    return new ActionResult() { InfoMessage = result };
                }
                else
                {
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Not Axf database");
                }
            }
            return new ActionResult(false);
        }

        public ActionResult RunSpecifiedTypeJob(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow parentRow = null)
        {
            var row = head.ScheduledJobRow.FirstOrDefault(f => f.NextExecutionTime < now);

            if (row == null)
                row = head.ScheduledJobRow.FirstOrDefault(f => f.State == (int)SoeEntityState.Active);

            if (row == null)
                return new ActionResult(false);

            parameterObject.SetSoeUser(UserManager.GetSoeUserAdmin(actorCompanyId));
            parameterObject.SetSoeCompany(CompanyManager.GetSoeCompany(actorCompanyId));


            var interval = row?.SysTimeIntervalId != null ? CalendarManager.GetSysTimeIntervalDateRange(row.SysTimeIntervalId.Value, row.NextExecutionTime) : parentRow?.SysTimeIntervalId != null ? CalendarManager.GetSysTimeIntervalDateRange(parentRow.SysTimeIntervalId.Value, parentRow.NextExecutionTime) : null;

            using (CompEntities entities = new CompEntities())
            {
                var setting = entities.ScheduledJobSetting.FirstOrDefault(w => w.ScheduledJobHeadId == head.ScheduledJobHeadId && w.Type == (int)TermGroup_ScheduledJobSettingType.SpecifiedType && w.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(w.StrData));

                if (setting == null)
                {
                    LogError($"{actorCompanyId} {head.Name} SpecifiedType is null");
                    return new ActionResult((int)ActionResultSelect.EntityNotFound, "ScheduledJobSetting");
                }


                if (setting.IntData == 1)
                {
                    int limit = 55;
                    var employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, true);

                    var monday = CalendarUtility.GetBeginningOfWeek(DateTime.Now);
                    var until = DateTime.Today;

                    if (monday == until)
                        return new ActionResult(false);

                    var timeBlocks = entities.TimeBlock.Include(i => i.TimeCode).Where(w => w.Employee.ActorCompanyId == actorCompanyId && w.TimeBlockDate.Date >= monday && w.TimeBlockDate.Date <= until).ToList();

                    foreach (var employee in employees)
                    {
                        var timeBlocksForEmployee = timeBlocks.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                        if (timeBlocks.IsNullOrEmpty())
                            continue;

                        var precense = timeBlocksForEmployee.GetPresence();

                        if (!precense.IsNullOrEmpty())
                            continue;

                        var hours = decimal.Round(decimal.Divide(timeBlocksForEmployee.GetMinutes(), 60), 2);

                        if (hours > (limit - 15))
                        {
                            var timeStampsToday = TimeStampManager.GetTimeStampEntries(entities, DateTime.Today, DateTime.Today, employee.EmployeeId);

                            if (!timeStampsToday.IsNullOrEmpty())
                                hours += timeStampsToday.GetPrecenseMinutesAccordingToTimeStamps();
                        }

                        if (hours > limit)
                        {
                            var message = $"Employee {employee.EmployeeNr} {employee.Name} has worked {hours} hours this week";
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, message);
                            List<UserDTO> execUsers = UserManager.GetEmployeeNearestExecutives(employee, DateTime.Today, DateTime.Today.AddDays(10), actorCompanyId);
                            if (execUsers != null)
                            {
                                var text = $"Employee {employee.EmployeeNrAndName} is over limit of {limit}, precense now {hours} hours";
                                var subject = text;
                                foreach (var execUser in execUsers)
                                {
                                    SystemInfoLog systemInfoLog = new SystemInfoLog()
                                    {
                                        Type = (int)SystemInfoType.TimeAccumulatorWarning,
                                        Entity = (int)SoeEntityType.XEMail,
                                        LogLevel = (int)SystemInfoLogLevel.Warning,
                                        RecordId = execUser.UserId,
                                        Text = $"Employee is over limit of {limit}, precense now {hours} hours",
                                        Date = DateTime.Now,
                                        DeleteManually = true,
                                        EmployeeId = employee.EmployeeId,

                                        //Set FK
                                        ActorCompanyId = actorCompanyId,
                                    };

                                    var result = GeneralManager.AddSystemInfoLogEntry(entities, systemInfoLog);

                                    MessageEditDTO messageEdit = new MessageEditDTO()
                                    {
                                        Entity = SoeEntityType.User,
                                        RecordId = 0,
                                        ActorCompanyId = null,
                                        LicenseId = parameterObject.SoeUser.LicenseId,
                                        SenderUserId = parameterObject.UserId,
                                        Subject = subject,
                                        Text = Common.Util.StringUtility.ConvertNewLineToHtml(text),
                                        ShortText = text,
                                        SenderName = execUser.Name,
                                        SenderEmail = "",
                                        Created = DateTime.Now,
                                        SentDate = DateTime.Now,
                                        MessagePriority = TermGroup_MessagePriority.None,
                                        MessageType = TermGroup_MessageType.AutomaticInformation,
                                        MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                                        MessageTextType = TermGroup_MessageTextType.Text,
                                    };

                                    MessageRecipientDTO recipient = new MessageRecipientDTO()
                                    {
                                        UserId = execUser.UserId,
                                        UserName = execUser.LoginName,
                                        Name = execUser.Name,
                                        EmailAddress = execUser.Email,
                                        Type = XEMailRecipientType.User,
                                    };

                                    messageEdit.Recievers.Add(recipient);

                                    if (result.Success)
                                    {
                                        CommunicationManager.SendXEMail(messageEdit, actorCompanyId, 0, parameterObject.UserId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new ActionResult(false);
        }
        public void RunBridgeJobFireAndForget(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow row, ActionResult previousActionResult = null, bool isChild = false, string eventInfo = null)
        {
            Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => RunBridgeJob(actorCompanyId, batchNr, now, head, row, previousActionResult, isChild, eventInfo)));
        }

        public ActionResult RunBridgeJob(int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow row, ActionResult previousActionResult = null, bool isChild = false, string eventInfo = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                return RunBridgeJob(entities, actorCompanyId, batchNr, now, head, row, previousActionResult, isChild, eventInfo);
            }
        }

        public ActionResult RunBridgeJob(CompEntities entities, int actorCompanyId, int batchNr, DateTime now, ScheduledJobHead head, ScheduledJobRow row, ActionResult previousActionResult = null, bool isChild = false, string eventInfo = null)
        {
            ActionResult result = new ActionResult();
            var trackingGuid = Guid.NewGuid();

            var jobHead = entities.ScheduledJobHead.Include(i => i.ScheduledJobSetting).FirstOrDefault(f => f.ScheduledJobHeadId == head.ScheduledJobHeadId);

            if (jobHead == null)
            {
                LogInfo($"{actorCompanyId} JobHead is null");
                return new ActionResult("JobHead is null");
            }

            if (!jobHead.ScheduledJobSetting.Any(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJob && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.BoolData.Value && a.State == (int)SoeEntityState.Active))
            {
                LogInfo($"{jobHead.ActorCompanyId} {jobHead.Name} is not a BridgeJob");
                return new ActionResult("Not a BridgeJob");
            }
            if (!jobHead.ScheduledJobSetting.Any(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJobType && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != 0 && a.State == (int)SoeEntityState.Active))
            {
                LogInfo($"{jobHead.ActorCompanyId} {jobHead.Name} Bridgejobtype is invalid");
                return new ActionResult("Bridgejobtype is invalid");
            }

            var eventEmployeeSettings = jobHead.ScheduledJobSetting.Where(a => a.Type == (int)TermGroup_ScheduledJobSettingType.EventActivationType && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != (int)TermGroup_ScheduleJobEventActivationType.EmployeeCreated && a.State == (int)SoeEntityState.Active);

            var typeSetting = jobHead.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJobType && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != (int)TermGroup_BridgeJobType.Unknown && a.State == (int)SoeEntityState.Active);

            if (typeSetting == null)
                return new ActionResult("BridgeJobType is invalid");

            TermGroup_BridgeJobType type = (TermGroup_BridgeJobType)typeSetting.IntData;

            using (CompEntities entities2 = new CompEntities())
            {
                if (BridgeManager.EncryptionUpdate(entities2, jobHead.ScheduledJobSetting.ToList()))
                {
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    jobHead = entitiesReadOnly.ScheduledJobHead.Include(i => i.ScheduledJobSetting).FirstOrDefault(f => f.ScheduledJobHeadId == head.ScheduledJobHeadId);
                }
                if (base.UseAccountHierarchyOnCompanyFromCache(entities2, actorCompanyId))
                {
                    UserCompanySetting userCompanySetting = SettingManager.GetUserCompanySetting(entities2, SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, UserId, actorCompanyId, 0);
                    if (userCompanySetting != null)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Resetting to all accounts");
                        userCompanySetting.StrData = null;
                        SetModifiedProperties(userCompanySetting);
                        result = SaveChanges(entities2);
                    }
                }
            }
            var scheduledJobSettings = jobHead.ScheduledJobSetting.Where(w => w.State == (int)SoeEntityState.Active).ToList();
            if (jobHead.ParentId.HasValue)
            {
                var parentHead = entities.ScheduledJobHead.Include(i => i.ScheduledJobSetting).FirstOrDefault(f => f.ScheduledJobHeadId == jobHead.ParentId && f.Company.LicenseId == LicenseId);

                if (parentHead?.ScheduledJobSetting != null)
                {
                    foreach (var item in parentHead.ScheduledJobSetting.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        if (!scheduledJobSettings.Any(a => a.Type == item.Type && a.State == (int)SoeEntityState.Active))
                            scheduledJobSettings.Add(item);
                    }
                }
            }

            switch (type)
            {
                case TermGroup_BridgeJobType.Unknown:
                    break;
                case TermGroup_BridgeJobType.VismaPayroll:
                    var accs = head.Name.ToLower().Contains("saldo");
                    var las = head.Name.ToLower().Contains("las");
                    var agda = head.Name.ToLower().Contains("agda");

                    if (!accs && !las && !agda)
                    {
                        if (!accs)
                            accs = scheduledJobSettings.Any(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.StrData.ToLower().Contains("saldo"));
                        else if (!las)
                            las = scheduledJobSettings.Any(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.StrData.ToLower().Contains("las"));
                        else if (!agda)
                            agda = scheduledJobSettings.Any(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.StrData.ToLower().Contains("agda"));
                    }

                    if (accs)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll Saldo started");
                        var timeBalances = BridgeManager.GetTimeBalancesIOsFromVismaPayroll(jobHead.ActorCompanyId, scheduledJobSettings);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll Saldo fetched information");
                        result = ImportExportManager.ImportFromTimeBalanceIO(timeBalances, jobHead.ActorCompanyId);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll Saldo finished");
                    }
                    else if (las)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll LAS started");
                        var lasBalances = BridgeManager.GetLasBalancesFromVismaPayroll(jobHead.ActorCompanyId, scheduledJobSettings);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll LAS fetched information");
                        result = ImportExportManager.ImportFromTimeBalanceIO(lasBalances, jobHead.ActorCompanyId);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll LAS finished");
                    }
                    else if (agda)
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Agda Employee started");
                        var employeeChangeIOs = BridgeManager.GetEmployeeChangesFromAgda(jobHead.ActorCompanyId, scheduledJobSettings);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Agda Employee fetched information. Changes {employeeChangeIOs.Count}");
                        var batchResult = ApiManager.ImportEmployeeChangesFromBridge(employeeChangeIOs, new List<ScheduledJobSetting>(), out result);
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Agda Employee finished");
                    }
                    else
                    {
                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll Employee started");
                        var setting = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.StrData.StartsWith("SyncEmploymentsBatchId:") && a.State == (int)SoeEntityState.Active);

                        if (setting != null)
                        {
                            var parts = setting.StrData.Split(':');
                            if (parts.Length == 2 && int.TryParse(parts[1], out int syncBatch))
                            {
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"SyncEmploymentsBatchId setting found with value {syncBatch}");
                                var logFromSync = BridgeManager.SyncVismaEmployments(jobHead.ActorCompanyId, syncBatch, scheduledJobSettings);
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"{logFromSync}");
                            }
                            else
                            {
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"SyncEmploymentsBatchId setting found but could not parse value from {setting.StrData}, using default");
                            }
                        }
                        else
                        {
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"VismaPayroll Employee starting");
                            var employeeChangeIOs = BridgeManager.GetEmployeeChangesFromVismaPayroll(jobHead.ActorCompanyId, scheduledJobSettings);
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"VismaPayroll Employee fetched information. Changes {employeeChangeIOs.Count}");
                            ApiManager.ImportEmployeeChangesFromBridge(employeeChangeIOs, scheduledJobSettings, out result);
                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "VismaPayroll Employee finished");
                        }
                    }
                    break;
                case TermGroup_BridgeJobType.Mqqt_Add_Message_To_Queue:
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Mqqt_Add_Message_To_Queue started");
                    if (eventInfo != null)
                    {
                        var splitted = eventInfo.Split(new string[] { "####" }, StringSplitOptions.None);

                        var brokerAddress = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.State == (int)SoeEntityState.Active)?.StrData;
                        var userName = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.State == (int)SoeEntityState.Active)?.StrData;
                        var password = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.State == (int)SoeEntityState.Active)?.StrData;
                        var topic = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.State == (int)SoeEntityState.Active)?.StrData ?? "";

                        if (!string.IsNullOrEmpty(brokerAddress))
                        {
                            try
                            {
                                var mqttFactory = new MqttFactory();
                                using (var mqttClient = mqttFactory.CreateMqttClient())
                                {
                                    var options = new MqttClientOptionsBuilder()
                                        .WithTcpServer(brokerAddress)
                                        .WithCredentials(userName, password)
                                        .WithCleanSession()
                                        .Build();

                                    mqttClient.ConnectAsync(options, CancellationToken.None).Wait();

                                    foreach (var jsonMessage in splitted)
                                    {
                                        try
                                        {
                                            var message = new MqttApplicationMessageBuilder()
                                                      .WithPayload(Encoding.UTF8.GetBytes(jsonMessage))
                                                      .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                                      .WithRetainFlag()
                                                      .Build();


                                            if (!string.IsNullOrEmpty(topic))
                                                message.Topic = topic;

                                            mqttClient.PublishAsync(message, CancellationToken.None).Wait();
                                        }
                                        catch (Exception ex)
                                        {
                                            LogCollector.LogError(ex);
                                        }
                                    }
                                    mqttClient.DisconnectAsync().Wait();
                                }

                            }
                            catch (Exception ex)
                            {
                                LogCollector.LogError(ex);
                            }
                        }
                    }
                    break;
                case TermGroup_BridgeJobType.FTP_File_Transfer_Upload:
                case TermGroup_BridgeJobType.SFTP_File_Transfer_Upload:
                case TermGroup_BridgeJobType.AzureStorage_File_Transfer_Upload:
                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "File_Transfer_Upload started " + EnumUtility.GetName<TermGroup_BridgeJobType>(type));

                    var sendVismaHrplusAbsence = scheduledJobSettings.Any(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.StrData.ToLower().Contains("sendvismahrplusabsence"));

                    if (sendVismaHrplusAbsence)
                    {
                        result = VismaHrPlusAbsence.ExportAbsence(
                                                   scheduledJobSettings,
                                                   entities,
                                                   BridgeManager,
                                                   ImportExportManager,
                                                   ActorManager,
                                                   AccountManager,
                                                   TimeSalaryManager,
                                                   actorCompanyId,
                                                   head?.ScheduledJobHeadId ?? 0,
                                                   row?.ScheduledJobRowId ?? 0,
                                                   batchNr,
                                                   (e, h, r, b, m) => InfoLog(entities, h, r, b, m),
                                                   (h, r, b, m) => ErrorLog(h, r, b, m)
                                               );

                        return result;
                    }

                    var exportFileTypeSetting = jobHead.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJobFileType && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != (int)TermGroup_BridgeJobFileType.Unknown && a.State == (int)SoeEntityState.Active);
                    var mergeFilesWithSameParent = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeMergeFileWithPrevious && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.State == (int)SoeEntityState.Active)?.BoolData ?? false;
                    var fileNameSetting = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupFileName && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.State == (int)SoeEntityState.Active);

                    FileExportResult fileExportResult = null;
                    FileExportResult fileExportResult2 = null;
                    List<FileExportResult> fileExportResults = new List<FileExportResult>();

                    if (exportFileTypeSetting?.IntData != null)
                    {
                        if (exportFileTypeSetting.IntData == (int)TermGroup_BridgeJobFileType.SalaryExportFile)
                            if (row?.SysTimeIntervalId != null)
                            {
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "SalaryExportFile started " + EnumUtility.GetName<TermGroup_BridgeJobType>(type));
                                DateRangeDTO interval = CalendarManager.GetSysTimeIntervalDateRange(row.SysTimeIntervalId.Value, DateTime.Now);
                                var exportTarget = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportTarget, 0, actorCompanyId, 0);
                                bool lockPeriod = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportLock && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.State == (int)SoeEntityState.Active)?.BoolData ?? false;
                                bool isPreliminary = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportIsPreliminary && a.DataType == (int)SettingDataType.Boolean && a.BoolData.HasValue && a.State == (int)SoeEntityState.Active)?.BoolData ?? false;

                                var exportResult = TimeSalaryManager.Export(interval.Start, interval.Stop, actorCompanyId, base.UserId, lockPeriod, isPreliminary);

                                if (!exportResult.Success)
                                    LogCollector.LogError(new Exception($"Track {trackingGuid} {exportResult?.ErrorMessage}"), "Exportsalaryjob failed");

                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"SalaryExportFile done success:{result.Success} Track {trackingGuid}");
                                var parsedfileName = BridgeManager.ParseFileName(base.ActorCompanyId, fileNameSetting?.StrData ?? Guid.NewGuid().ToString());
                                InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"FileName {parsedfileName}");

                                if (exportResult.Success && exportResult.IntegerValue2 != 0)
                                {
                                    var export = entities.TimeSalaryExport.FirstOrDefault(f => f.TimeSalaryExportId == exportResult.IntegerValue2);

                                    if (exportTarget == (int)SoeTimeSalaryExportTarget.Pol && export.File1 != null)
                                    {
                                        var files = ZipUtility.UnzipFilesInZipFile(export.File1);

                                        foreach (var file in files)
                                        {
                                            fileExportResults.Add(new FileExportResult
                                            {
                                                Base64Data = Convert.ToBase64String(file.Value),
                                                Data = file.Value,
                                                FileName = file.Key
                                            });
                                        }
                                    }
                                    else if (export.File1 != null)
                                    {
                                        fileExportResult = new FileExportResult()
                                        {
                                            Base64Data = Convert.ToBase64String(export.File1),
                                            Data = export.File1,
                                            FileName = parsedfileName,
                                        };
                                    }
                                    if (export.File2 != null && export.File2.Length > 0)
                                    {
                                        fileExportResult2 = new FileExportResult()
                                        {
                                            Base64Data = Convert.ToBase64String(export.File2),
                                            Data = export.File2,
                                            FileName = parsedfileName,
                                        };
                                    }
                                }
                            }
                    }
                    else
                    {
                        var exportIdSetting = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.ExportId && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != 0 && a.State == (int)SoeEntityState.Active);

                        if (exportIdSetting != null)
                        {
                            InfoLog(entities, head.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Export started " + EnumUtility.GetName<TermGroup_BridgeJobType>(type) + " ExportId " + exportIdSetting.IntData.Value);

                            fileExportResult = ImportExportManager.GetFileExportResult(entities, trackingGuid, actorCompanyId, exportIdSetting.IntData.Value, fileNameSetting?.StrData, isChild, eventInfo);

                            if (fileExportResult == null)
                                LogCollector.LogError(new Exception($"Track {trackingGuid} FileExportResult is null"), "Exportjob failed");
                            else if (!fileExportResult.Result.Success)
                                LogCollector.LogError(new Exception($"Track {trackingGuid} {fileExportResult.Result.ErrorMessage}"), "Exportjob failed");

                            InfoLog(entities, head.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Export done success:" + fileExportResult?.Result?.Success + " Track " + trackingGuid);

                            if (!Directory.Exists(@"c:\temp\"))
                                Directory.CreateDirectory(@"c:\temp\");

                            if (fileExportResult?.Base64Data != null)
                                File.WriteAllBytes(@"c:\temp\temp" + $"_{actorCompanyId}_{CalendarUtility.ToFileFriendlyDateTime(DateTime.Now)}_" + fileExportResult.FileName, Convert.FromBase64String(fileExportResult.Base64Data));
                            else if (fileExportResult != null && fileExportResult.Base64Data == null)
                                fileExportResult = null;

                            FileExportResult previousFileExportResult = null;
                            if (mergeFilesWithSameParent && previousActionResult?.Value != null)
                            {
                                try
                                {
                                    previousFileExportResult = previousActionResult.Value as FileExportResult;
                                }
                                catch (Exception ex)
                                {
                                    InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, "Failed to cast previous file export result");
                                    LogCollector.LogError(ex);
                                }
                            }

                            if (mergeFilesWithSameParent)
                            {
                                if (previousFileExportResult != null)
                                {
                                    if (fileExportResult == null)
                                    {
                                        fileExportResult = new FileExportResult()
                                        {
                                            Base64Data = previousFileExportResult.Base64Data,
                                            Data = previousFileExportResult.Data,
                                            FileName = previousFileExportResult.FileName,
                                            FilePath = previousFileExportResult.FilePath,
                                            Result = new ActionResult()
                                        };
                                    }
                                    else
                                    {
                                        InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"Merging files {fileExportResult.FileName} and {previousFileExportResult.FileName}");
                                        List<byte[]> arrays = new List<byte[]>() { fileExportResult.Data, previousFileExportResult.Data };
                                        var data = MergeFiles(arrays);
                                        fileExportResult.Base64Data = Convert.ToBase64String(data);
                                        fileExportResult.Data = data;
                                    }
                                    try
                                    {

                                        File.WriteAllBytes(@"c:\temp\merged" + $"_{actorCompanyId}_{CalendarUtility.ToFileFriendlyDateTime(DateTime.Now)}_" + fileExportResult.FileName, Convert.FromBase64String(fileExportResult.Base64Data));
                                    }
                                    catch (Exception ex)
                                    {
                                        LogCollector.LogError(ex, $"RunBridgeJob {trackingGuid}");
                                    }
                                }

                                result.Value = fileExportResult;
                            }
                        }
                    }
                    if (!mergeFilesWithSameParent || !jobHead.ParentId.HasValue)
                    {
                        if (fileExportResult != null)
                            fileExportResults.Add(fileExportResult);
                        if (fileExportResult2 != null)
                            fileExportResults.Add(fileExportResult2);

                        foreach (var fileExport in fileExportResults.Where(w => w != null))
                        {
                            ActionResult resultFileUpload = new ActionResult();
                            if (type == TermGroup_BridgeJobType.FTP_File_Transfer_Upload)
                                resultFileUpload = BridgeManager.FTPUpload(scheduledJobSettings, fileExport);
                            else if (type == TermGroup_BridgeJobType.SFTP_File_Transfer_Upload)
                                resultFileUpload = BridgeManager.SSHUpload(scheduledJobSettings, fileExport);
                            else if (type == TermGroup_BridgeJobType.AzureStorage_File_Transfer_Upload)
                                resultFileUpload = BridgeManager.AzureStorageUpload(scheduledJobSettings, fileExport);

                            InfoLog(entities, head?.ScheduledJobHeadId, row?.ScheduledJobRowId, batchNr, $"File Uploaded {fileExport.FileName} success:{resultFileUpload.Success} {EnumUtility.GetName<TermGroup_BridgeJobType>(type)} Track {trackingGuid}");

                            if (!resultFileUpload.Success)
                                LogError($"Track:{trackingGuid} " + resultFileUpload.ErrorMessage != null ? "File not uploaded failed " + resultFileUpload.ErrorMessage : "File not uploaded");
                            else
                                LogInfo($"Track:{trackingGuid} " + $"File Uploaded {fileExport.FileName} from {Environment.MachineName}");

                        }
                    }

                    break;
                case TermGroup_BridgeJobType.FTP_File_Transfer_Download:
                case TermGroup_BridgeJobType.SFTP_File_Transfer_Download:
                case TermGroup_BridgeJobType.AzureStorage_File_Transfer_Download:
                    var matchExpressionSetting = scheduledJobSettings.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeFileInformationMatchExpression && a.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(a.StrData) && a.State == (int)SoeEntityState.Active);
                    var fileTypeSetting = jobHead.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJobFileType && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != (int)TermGroup_BridgeJobFileType.Unknown && a.State == (int)SoeEntityState.Active);

                    if (fileTypeSetting == null)
                        return new ActionResult("fileTypeSetting missing");

                    if (!Enum.TryParse(fileTypeSetting.IntData.Value.ToString(), out TermGroup_BridgeJobFileType fileType))
                        return new ActionResult("fileTypeSetting is invalid");

                    if (matchExpressionSetting != null)
                    {
                        List<BridgeFileInformation> bridgeFileInformations = new List<BridgeFileInformation>();

                        if (type == TermGroup_BridgeJobType.FTP_File_Transfer_Download)
                            bridgeFileInformations = BridgeManager.FTPGetFiles(scheduledJobSettings);
                        else if (type == TermGroup_BridgeJobType.SFTP_File_Transfer_Download)
                            bridgeFileInformations = BridgeManager.SSHGetFiles(scheduledJobSettings);
                        else if (type == TermGroup_BridgeJobType.AzureStorage_File_Transfer_Download)
                            bridgeFileInformations = BridgeManager.SSHGetFiles(scheduledJobSettings); //TODO

                        foreach (var bridgeFileInformation in bridgeFileInformations.OrderBy(o => o.Path))
                        {
                            var fileImported = false;

                            try
                            {
                                switch (fileType)
                                {
                                    case TermGroup_BridgeJobFileType.Unknown:
                                        break;
                                    case TermGroup_BridgeJobFileType.StaffingneedFrequencyIODTO:
                                        if (bridgeFileInformation.Base64 != null)
                                        {
                                            List<StaffingNeedsFrequencyIODTO> staffingNeedsFrequencies = Base64Util.GetObjectFromBase64String<List<StaffingNeedsFrequencyIODTO>>(bridgeFileInformation.Base64);
                                            if (!staffingNeedsFrequencies.IsNullOrEmpty())
                                            {
                                                StaffingNeedsFrequencyIOItem item = new StaffingNeedsFrequencyIOItem() { frequencies = staffingNeedsFrequencies };
                                                result = ImportExportManager.ImportStaffingNeedsFrequencyIO(item, TermGroup_IOImportHeadType.StaffingNeedsFrequency, TermGroup_IOSource.Bridge, TermGroup_IOType.Bridge, actorCompanyId, true);

                                                if (result.Success)
                                                    fileImported = true;
                                            }
                                        }
                                        break;
                                    case TermGroup_BridgeJobFileType.StaffingneedFrequencyFile:
                                        if (bridgeFileInformation.Base64 != null)
                                        {
                                            var arr = Base64Util.GetDataFromBase64String(bridgeFileInformation.Base64);

                                            var definitionIdSetting = jobHead.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeFileInformationDefinitionId && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != 0 && a.State == (int)SoeEntityState.Active);

                                            var importHeadIdSetting = jobHead.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeFileInformationImportHeadId && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != 0 && a.State == (int)SoeEntityState.Active);

                                            if (arr != null && definitionIdSetting != null && importHeadIdSetting != null)
                                            {
                                                result = ImportExportManager.XEConnectImport(actorCompanyId, 0, 0, 0, new List<byte[]>() { arr }, false, definitionIdSetting.IntData.Value, importHeadIdSetting.IntData.Value, true, true);

                                                if (result.Success)
                                                    fileImported = true;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                if (fileImported)
                                {
                                    BridgeManager.SSHMove(scheduledJobSettings, new FileExportResult() { Base64Data = bridgeFileInformation.Base64, FileName = bridgeFileInformation.Path }, bridgeFileInformation);
                                }
                                else
                                {
                                    base.LogInfo(new Exception($"BridgeFileInformation: File not imported {bridgeFileInformation.Path} actorcompanyId {actorCompanyId} settings {JsonConvert.SerializeObject(scheduledJobSettings)}"), this.log);
                                }
                            }
                            catch (Exception ex)
                            {
                                base.LogError(ex, this.log);
                            }
                        }
                    }
                    break;
                default:
                    break;

            }

            return result;
        }

        public List<ScheduledJobSetting> GetScheduledJobSettingsWithEventActivaction(CompEntities entities, int actorCompanyId)
        {
            try
            {
                return entities.ScheduledJobSetting.Where(w => w.State == (int)SoeEntityState.Active &&
                                w.ScheduledJobHead.ActorCompanyId == actorCompanyId &&
                                w.Type == (int)TermGroup_ScheduledJobSettingType.EventActivationType &&
                                w.IntData.HasValue && w.IntData != 0 &&
                                w.State == (int)SoeEntityState.Active).ToList();
            }
            catch
            {
                return new List<ScheduledJobSetting>();
            }

        }


        private byte[] MergeFiles(List<byte[]> arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
            //byte[] rv = new byte[arrays.Sum(a => a.Length)];
            //int offset = 0;
            //foreach (byte[] array in arrays)
            //{
            //    System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
            //    offset += array.Length;
            //}
            //return rv;
        }



        private List<int> RunTogetherHeads(ScheduledJobHead head, List<ScheduledJobHead> allScheduleJobHeads)
        {
            List<int> allScheduleJobHeadids = new List<int>() { head.ScheduledJobHeadId };

            foreach (var otherHead in allScheduleJobHeads)
            {
                foreach (var rowInOtherHead in otherHead.ScheduledJobRow)
                {
                    foreach (var row in head.ScheduledJobRow)
                    {
                        if (rowInOtherHead.NextExecutionTime == row.NextExecutionTime)
                            allScheduleJobHeadids.Add(rowInOtherHead.ScheduledJobHeadId);
                    }
                }
            }

            return allScheduleJobHeadids.Distinct().ToList();
        }

        #endregion
    }
}
