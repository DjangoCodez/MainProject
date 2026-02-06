using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.Communicator;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;


namespace SoftOne.Soe.Business.Core
{
    public class SysScheduledJobManager : ManagerBase, IDisposable
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static Dictionary<SysScheduledJobDTO, BackgroundWorker> runningJobsServer = new Dictionary<SysScheduledJobDTO, BackgroundWorker>();
        private static Random random;
        private static DateTime prevRun;

        #endregion

        #region Ctor

        public SysScheduledJobManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Run

        public void CheckJobs()
        {
            if (random == null)
                random = new Random();

            if (random.Next(1, 30) != 10)
                return;

            Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => BackupRunJobs()));
        }

        public void BackupRunJobs()
        {
            try
            {

                var timeLimit = DateTime.Now.AddMinutes(-60);
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                if (!sysEntitiesReadOnly.SysScheduledJobLog.Any(a => a.Time > timeLimit))
                {
                    DateTime time = DateTime.Now;
                    prevRun = time;
                    #region Get running DataBase Name

                    string[] separator = new string[1];
                    separator[0] = ";";
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    string connectionString = (entitiesReadOnly.Connection as System.Data.Entity.Core.EntityClient.EntityConnection).StoreConnection.ConnectionString;
                    string[] parts = connectionString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    String dbPart = parts.FirstOrDefault(x => x.StartsWith("Initial Catalog"));
                    separator[0] = "=";
                    parts = dbPart.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Count() != 2)
                        return;

                    string connectionDBName = parts[1].Trim().ToLower();

                    #endregion
                    Thread.Sleep(random.Next(1, 10000));
                    if (time == prevRun && !sysEntitiesReadOnly.SysScheduledJobLog.Any(a => a.Time > timeLimit))
                    {
                        LogInfo("BackupRunJobs on " + connectionDBName + " started");
                        CommunicatorConnector.SendServerAlertMailMessage("BackupRunJobs on " + connectionDBName + " started", "BackupRunJobs on " + connectionDBName + " started from machine " + Environment.MachineName + " at time: " + DateTime.Now.ToString());
                        Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => RunJobs()));
                    }
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex.ToString());
            }
        }

        public ActionResult RunJobs(int? sysScheduleJobId = null)
        {
            ActionResult result = new ActionResult();


            try
            {
                List<SysScheduledJobDTO> scheduledJobs = GetScheduledJobsReadyToRun(DateTime.Now, null).ToDTOs(true, true, false).ToList();

                if (sysScheduleJobId.HasValue)
                {

                    scheduledJobs = new List<SysScheduledJobDTO>();

                    using (CompEntities entities = new CompEntities())
                    {
                        var schedulejob = GetScheduledJob(sysScheduleJobId.Value, true, true);
                        schedulejob.State = (int)ScheduledJobState.Active;
                        SaveChanges(entities);
                        scheduledJobs.Add(schedulejob.ToDTO(true, true, true));
                    }
                }

                if (scheduledJobs.Any())
                {
                    int batchNr = GetLatestBatchNr() + 1;
                    int nrOfJobsExcecuted = ExecuteJobs(scheduledJobs, batchNr);

                    result.IntegerValue = nrOfJobsExcecuted;
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ex, "RunJobs failed");
            }

            return result;
        }

        public int ExecuteJobs(IEnumerable<SysScheduledJobDTO> scheduledJobs, int batchNr)
        {
            SysScheduledJobManager jm = new SysScheduledJobManager(null);
            int nrOfJobsExcecuted = 0;

            //Do not start a new job between 01:00 and 01.30 UTC.
            if (DateTime.UtcNow.Hour == 1 && DateTime.UtcNow.Minute < 30)
                return nrOfJobsExcecuted;

            // Loop through the jobs and execute them
            foreach (SysScheduledJobDTO scheduledJob in scheduledJobs)
            {
                try
                {

                    var result = jm.ExecuteScheduledJobSync(scheduledJob, batchNr);
                    if (result.Success)
                        nrOfJobsExcecuted++;
                    else if (!string.IsNullOrEmpty(result.ErrorMessage))
                        LogError("Error when executing scheduled job " + scheduledJob.Name + ". Error message: " + result.ErrorMessage);
                    else if (result.Exception != null)
                        LogError(result.Exception, log);

                }
                catch (Exception ex)
                {
                    // Write error to event log
                    LogError(ex, log);
                }
            }

            return nrOfJobsExcecuted;
        }

        #endregion

        #region SysJob

        /// <summary>
        /// Get one specified job
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="sysJobId">Id of expected job</param>
        /// <param name="loadSettings">If True, load SysJobSetting relation</param>
        /// <param name="loadScheduledJobs">If True, load SysScheduledJob relation</param>
        /// <returns>One job or null if not found</returns>
        public SysJob GetJob(SOESysEntities entities, int sysJobId, bool loadSettings, bool loadScheduledJobs)
        {
            IQueryable<SysJob> query = entities.Set<SysJob>();
            if (loadSettings)
                query.Include("SysJobSettingJob.SysJobSetting");

            if (loadScheduledJobs)
                query.Include("SysScheduledJob");

            SysJob job = (from j in query
                          where j.SysJobId == sysJobId
                          select j).FirstOrDefault();

            return job;
        }

        /// <summary>
        /// Get one specified job
        /// </summary>
        /// <param name="sysJobId">Id of expected job</param>
        /// <param name="loadSettings">If True, load SysJobSetting relation</param>
        /// <param name="loadScheduledJobs">If True, load SysScheduledJob relation</param>
        /// <returns>One job or null if not found</returns>
        public SysJob GetJob(int sysJobId, bool loadSettings, bool loadScheduledJobs)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetJob(sysEntitiesReadOnly, sysJobId, loadSettings, loadScheduledJobs);
        }

        /// <summary>
        /// Get jobs with a specified state (active or inactive)
        /// Deleted jobs will not be returned
        /// </summary>
        /// <param name="active">true = active jobs, false = inactive jobs, null = both active and inactive jobs</param>
        /// <returns>Collection of jobs</returns>
        public IEnumerable<SysJob> GetJobs(bool? active)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = (from j in sysEntitiesReadOnly.SysJob
                         where j.State != (int)SoeEntityState.Deleted
                         select j).ToList<SysJob>();

            if (active == true)
                query = query.Where(j => j.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                query = query.Where(j => j.State == (int)SoeEntityState.Inactive).ToList();

            return query.OrderBy(j => j.Name);
        }

        public ActionResult SaveJob(SysJobDTO jobInput, List<SysJobSettingDTO> settingsInput)
        {
            if (jobInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysJob");

            // Default result is successful
            ActionResult result = new ActionResult();

            int sysJobId = jobInput.SysJobId;

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region SysJob

                        // Get existing job
                        SysJob job = GetJob(entities, sysJobId, true, false);
                        if (job == null)
                        {
                            #region SysJob Add

                            job = new SysJob()
                            {
                                Name = jobInput.Name,
                                Description = jobInput.Description,
                                AssemblyName = jobInput.AssemblyName,
                                ClassName = jobInput.ClassName,
                                AllowParallelExecution = jobInput.AllowParallelExecution,
                                State = (int)jobInput.State
                            };

                            SetCreatedPropertiesOnEntity(job);

                            entities.SysJob.Add(job);

                            #endregion
                        }
                        else
                        {
                            #region SysJob Update

                            job.Name = jobInput.Name;
                            job.Description = jobInput.Description;
                            job.AssemblyName = jobInput.AssemblyName;
                            job.ClassName = jobInput.ClassName;
                            job.AllowParallelExecution = jobInput.AllowParallelExecution;
                            job.State = (int)jobInput.State;

                            SetModifiedPropertiesOnEntity(job);

                            #endregion
                        }

                        #endregion

                        #region SysJobSetting

                        #region SysJobSetting Update/Delete

                        List<SysJobSetting> deleteSettings = new List<SysJobSetting>();
                        // Update or Delete existing SysJobSettings
                        foreach (SysJobSetting setting in job.SysJobSettingJob.Select(s => s.SysJobSetting))
                        {
                            // Try get setting from input
                            SysJobSettingDTO settingInput = (from s in settingsInput
                                                             where s.SysJobSettingId == setting.SysJobSettingId
                                                             select s).FirstOrDefault();

                            if (settingInput != null)
                            {
                                #region SysJobSetting Update

                                // Update existing setting
                                setting.DataType = (int)settingInput.DataType;
                                setting.StrData = settingInput.StrData;
                                setting.IntData = settingInput.IntData;
                                setting.DecimalData = settingInput.DecimalData;
                                setting.BoolData = settingInput.BoolData;
                                setting.DateData = settingInput.DateData;
                                setting.TimeData = settingInput.TimeData;

                                #endregion
                            }
                            else
                            {
                                #region SysJobSetting Delete
                                deleteSettings.Add(setting);
                                #endregion
                            }
                        }

                        if (deleteSettings.Any())
                        {
                            foreach (var setting in deleteSettings)
                            {
                                entities.SysJobSettingJob.RemoveRange(setting.SysJobSettingJob.ToList());
                                entities.SysJobSetting.Remove(setting);
                            }
                        }

                        #endregion

                        #region SysJobSetting Add

                        // Get new settings
                        IEnumerable<SysJobSettingDTO> settingsToAdd = (from s in settingsInput
                                                                       where s.SysJobSettingId == 0
                                                                       select s).ToList();

                        foreach (SysJobSettingDTO settingToAdd in settingsToAdd)
                        {
                            // Add setting to job
                            var setting = new SysJobSetting()
                            {
                                Type = (int)SysJobSettingType.Job,
                                DataType = (int)settingToAdd.DataType,
                                Name = settingToAdd.Name,
                                StrData = settingToAdd.StrData,
                                IntData = settingToAdd.IntData,
                                DecimalData = settingToAdd.DecimalData,
                                BoolData = settingToAdd.BoolData,
                                DateData = settingToAdd.DateData,
                                TimeData = settingToAdd.TimeData
                            };

                            var map = new SysJobSettingJob()
                            {
                                SysJob = job,
                                SysJobSetting = setting
                            };

                            job.SysJobSettingJob.Add(map);
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            sysJobId = job.SysJobId;
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
                    {
                        result.IntegerValue = sysJobId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult UpdateJobState(Dictionary<int, int> itemsToUpdate)
        {
            if (itemsToUpdate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysJob");

            // Default result is successful
            ActionResult result = new ActionResult();

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (KeyValuePair<int, int> item in itemsToUpdate)
                        {
                            SysJob job = GetJob(entities, item.Key, true, false);
                            if (job != null)
                            {
                                job.State = item.Value;

                                SetModifiedPropertiesOnEntity(job);
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                            transaction.Complete();
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
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Delete job with specified Id (Set state to deleted)
        /// </summary>
        /// <param name="sysJobId">Id of job to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteJob(int sysJobId)
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                // Get job
                SysJob job = GetJob(entities, sysJobId, false, true);
                if (job == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysJob");

                // Check if there are any active related scheduled jobs
                if (job.SysScheduledJob.Any(j => j.State == (int)ScheduledJobState.Active || j.State == (int)ScheduledJobState.Running))
                    return new ActionResult((int)ActionResultDelete.JobHasScheduledJobs, GetText(3399, "Jobbet har aktiva schemaläggningar"));

                // Change state on job
                ChangeEntityStateOnEntity(job, SoeEntityState.Deleted);

                // Change state on related scheduled jobs
                foreach (SysScheduledJob scheduledJob in job.SysScheduledJob)
                {
                    scheduledJob.State = (int)ScheduledJobState.Deleted;
                    SetModifiedPropertiesOnEntity(scheduledJob);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region SysScheduledJob

        /// <summary>
        /// Get one specified scheduled job
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="sysScheduledJobId">Id of expected scheduled job</param>
        /// <param name="loadSettings">If True, load SysJobSettings relation</param>
        /// <param name="loadJob">True if related SysJob should be loaded</param>
        /// <returns>One scheduled job or null if not found</returns>
        /// 
        public SysScheduledJob GetScheduledJob(int sysScheduledJobId, bool loadSettings = false, bool loadJob = false)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return GetScheduledJob(sysEntitiesReadOnly, sysScheduledJobId, loadSettings, loadJob);
        }
        public SysScheduledJob GetScheduledJob(SOESysEntities entities, int sysScheduledJobId, bool loadSettings, bool loadJob)
        {
            IQueryable<SysScheduledJob> query = entities.Set<SysScheduledJob>();
            if (loadSettings)
                query = query.Include("SysJobSettingScheduledJob.SysJobSetting");

            if (loadJob)
            {
                query.Include("SysJob");
                if (loadSettings)
                    query = query.Include("SysJob.SysJobSettingJob.SysJobSetting");
            }

            var job = (from j in query
                       where j.SysScheduledJobId == sysScheduledJobId
                       select j).FirstOrDefault();

            return job;
        }

        /// <summary>
        /// Get all scheduled jobs, or all related to specified job
        /// </summary>
        /// <param name="sysJobId">If specified, limit result to scheduled jobs related to specified job</param>
        /// <returns>Collection of scheduled jobs</returns>
        public IEnumerable<SysScheduledJob> GetScheduledJobs(int? sysJobId)
        {
            int deletedState = (int)ScheduledJobState.Deleted;
            IEnumerable<SysScheduledJob> jobs;

            if (sysJobId.HasValue)
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                jobs = (from j in sysEntitiesReadOnly.SysScheduledJob
                        where j.State != deletedState &&
                        j.SysJobId == sysJobId
                        select j).ToList();
            }
            else
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                jobs = (from j in sysEntitiesReadOnly.SysScheduledJob
                        where j.State != deletedState
                        select j).ToList();
            }

            // Set StateName virtual column
            foreach (SysScheduledJob job in jobs)
            {
                job.StateName = GetText(job.State, (int)TermGroup.ScheduledJobState);
            }

            return jobs.OrderBy(j => j.Name);
        }

        public int GetNumberOfActiveScheduledJobs()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from j in sysEntitiesReadOnly.SysScheduledJob
                    where j.State != (int)ScheduledJobState.Deleted
                    select j).Count();
        }

        /// <summary>
        /// Get scheduled jobs with a specified state
        /// </summary>
        /// <param name="state">Expected state</param>
        /// <returns>Collection of scheduled jobs</returns>
        public IEnumerable<SysScheduledJob> GetScheduledJobs(ScheduledJobState state, ScheduledJobType type = ScheduledJobType.Task)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from j in sysEntitiesReadOnly.SysScheduledJob
                    where j.State == (int)state &&
                    j.Type == (int)type
                    select j).ToList();
        }

        public IEnumerable<SysScheduledJob> GetScheduledJobsReadyToRun(DateTime executeTime, ScheduledJobType? type = ScheduledJobType.Task)
        {
            ResurrectScheduledJobsInLimbo();
            return this.GetScheduledJobsByState(executeTime, ScheduledJobState.Active, type);
        }

        public void ResurrectScheduledJobsInLimbo()
        {
            #region Get running DataBase Name

            string[] separator = new string[1];
            separator[0] = ";";
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string connectionString = (entitiesReadOnly.Connection as System.Data.Entity.Core.EntityClient.EntityConnection).StoreConnection.ConnectionString;
            string[] parts = connectionString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            String dbPart = parts.FirstOrDefault(x => x.StartsWith("Initial Catalog"));
            separator[0] = "=";
            parts = dbPart.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Count() != 2)
                return;

            string connectionDBName = parts[1].Trim().ToLower();

            #endregion

            DateTime executeTime = DateTime.Now.AddHours(-1);
            var running = this.GetScheduledJobsByState(executeTime, ScheduledJobState.Running, ScheduledJobType.Task);

            if (running.Any())
            {
                var timeLimit = DateTime.Now.AddHours(-4);

                foreach (var job in running)
                {
                    if (job.Name.Equals("AzureSearchJob", StringComparison.OrdinalIgnoreCase))
                        timeLimit = DateTime.Now.AddHours(-12);

                    if (job.Name.ToLower().Contains("edi"))
                        timeLimit = DateTime.Now.AddHours(-2);

                    if (job.Name.ToLower().Contains("axfood"))
                        timeLimit = DateTime.Now.AddHours(-3);

                    using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                    var logs = (from j in sysEntitiesReadOnly.SysScheduledJobLog
                                where j.SysScheduledJobId == job.SysScheduledJobId && j.Time > timeLimit
                                select j).ToList();

                    if (!logs.Any() && job.ExecuteTime > DateTime.Now.AddDays(-1))
                    {
                        LogInfo($"Job {job.Name} {job.DatabaseName} resurrected from running state to active");
                        using (SOESysEntities sysEntities = new SOESysEntities())
                        {
                            var j = sysEntities.SysScheduledJob.First(f => f.SysScheduledJobId == job.SysScheduledJobId);
                            j.State = (int)ScheduledJobState.Active;
                            j.ExecuteTime = DateTime.Now;
                            SaveChanges(sysEntities);
                        }
                    }
                }
            }

            var interrupted = this.GetScheduledJobsByState(executeTime, ScheduledJobState.Interrupted, ScheduledJobType.Task);

            if (interrupted.Any())
            {
                var timeLimit = DateTime.Now.AddHours(-4);

                foreach (var job in interrupted)
                {
                    using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                    var logs = (from j in sysEntitiesReadOnly.SysScheduledJobLog
                                where j.SysScheduledJobId == job.SysScheduledJobId && j.Time > timeLimit
                                select j).ToList();

                    if (!logs.Any() && job.ExecuteTime > DateTime.Now.AddDays(-1))
                    {
                        LogInfo($"Job {job.Name} {job.DatabaseName} resurrected from interrupted state to active");
                        using (SOESysEntities sysEntities = new SOESysEntities())
                        {
                            var j = sysEntities.SysScheduledJob.First(f => f.SysScheduledJobId == job.SysScheduledJobId);
                            j.State = (int)ScheduledJobState.Active;
                            j.ExecuteTime = DateTime.Now;
                            SaveChanges(sysEntities);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get scheduled jobs to run (waiting jobs where execute time has passed)
        /// </summary>
        /// <param name="executeTime">Date and time to compare with</param>
        /// <returns>Collection of scheduled jobs</returns>
        private IEnumerable<SysScheduledJob> GetScheduledJobsByState(DateTime executeTime, ScheduledJobState state, ScheduledJobType? type = ScheduledJobType.Task)
        {
            #region Get running DataBase Name

            string[] separator = new string[1];
            separator[0] = ";";
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string connectionString = (entitiesReadOnly.Connection as System.Data.Entity.Core.EntityClient.EntityConnection).StoreConnection.ConnectionString;
            string[] parts = connectionString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            String dbPart = parts.FirstOrDefault(x => x.StartsWith("Initial Catalog"));
            separator[0] = "=";
            parts = dbPart.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Count() != 2)
                return new List<SysScheduledJob>();

            string connectionDBName = parts[1].Trim().ToLower();

            #endregion

            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from j in sysEntitiesReadOnly.SysScheduledJob
                                    .Include("SysJobSettingScheduledJob.SysJobSetting")
                                    .Include("SysJob")
                                    .Include("SysJob.SysJobSettingJob.SysJobSetting")
                    where j.ExecuteTime <= executeTime &&
                    j.DatabaseName == connectionDBName &&
                    j.State == (int)state &&
                    (!type.HasValue || j.Type == (int)type.Value)
                    select j).ToList();
        }

        public bool IsAnyJobRunning(string databaseName)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from j in sysEntitiesReadOnly.SysScheduledJob
                    where j.State == (int)ScheduledJobState.Running &&
                    j.DatabaseName.ToLower() == databaseName.ToLower()
                    select j).Any();
        }

        public ActionResult SaveScheduledJobState(int scheduledJobId, ScheduledJobState state)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    #region SysJob

                    // Get SysJob
                    SysScheduledJob sysJob = entities.SysScheduledJob.Where(j => j.SysScheduledJobId == scheduledJobId).FirstOrDefault();
                    if (sysJob == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "SysScheduledJob");

                    if (sysJob.State != (int)state)
                        sysJob.State = (int)state;

                    result = SaveChanges(entities);

                    #endregion
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
            }

            return result;
        }

        public DateTime? LastStartOfJob(int sysScheduleJobId, string startMessage)
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    var lastLogStart = entities.SysScheduledJobLog.Where(f => f.SysScheduledJobId == sysScheduleJobId && f.Message == startMessage).OrderByDescending(o => o.Time).FirstOrDefault();
                    if (lastLogStart != null)
                        return lastLogStart.Time;
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                }
            }

            return null;
        }

        public ActionResult RunScheduleJobByService(int sysScheduledJobId)
        {
            var job = GetScheduledJob(sysScheduledJobId, true, true).ToDTO(true, true, true);
            if (job != null)
            {
                job.ExecuteTime = DateTime.Now;
                return SaveScheduledJob(job, job.SysJobSettings, job.SysJobId);
            }
            else
            {
                return new ActionResult(false);
            }
        }

        public ActionResult SaveScheduledJob(SysScheduledJobDTO scheduledJobInput, List<SysJobSettingDTO> settingsInput, int sysJobId)
        {
            if (scheduledJobInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SysScheduledJob");

            // Default result is successful
            ActionResult result = new ActionResult();

            int sysScheduledJobId = scheduledJobInput.SysScheduledJobId;

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region SysJob

                        // Get SysJob
                        SysJob sysJob = GetJob(entities, sysJobId, false, false);
                        if (sysJob == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "SysJob");

                        #endregion

                        #region SysScheduledJob

                        // Get existing job
                        SysScheduledJob scheduledJob = GetScheduledJob(entities, sysScheduledJobId, true, true);
                        if (scheduledJob == null)
                        {
                            #region SysJob Add

                            scheduledJob = new SysScheduledJob()
                            {
                                // Job
                                Name = scheduledJobInput.Name,
                                Description = scheduledJobInput.Description,
                                DatabaseName = scheduledJobInput.DatabaseName,
                                SysJob = sysJob,
                                State = (int)scheduledJobInput.State,

                                // Execution
                                ExecuteTime = scheduledJobInput.ExecuteTime,
                                ExecuteUserId = scheduledJobInput.ExecuteUserId,

                                // Recurrence
                                RecurrenceType = (int)scheduledJobInput.RecurrenceType,
                                RecurrenceCount = scheduledJobInput.RecurrenceCount,
                                RecurrenceDate = scheduledJobInput.RecurrenceDate,
                                RecurrenceInterval = scheduledJobInput.RecurrenceInterval,

                                // Error handling
                                RetryTypeForInternalError = (int)scheduledJobInput.RetryTypeForInternalError,
                                RetryTypeForExternalError = (int)scheduledJobInput.RetryTypeForExternalError,
                                RetryCount = scheduledJobInput.RetryCount,

                                // Settings
                                AllowParallelExecution = scheduledJobInput.AllowParallelExecution
                            };

                            SetCreatedPropertiesOnEntity(scheduledJob);

                            entities.SysScheduledJob.Add(scheduledJob);

                            #endregion
                        }
                        else
                        {
                            #region SysJob Update

                            // Job
                            scheduledJob.Name = scheduledJobInput.Name;
                            scheduledJob.Description = scheduledJobInput.Description;
                            scheduledJob.DatabaseName = scheduledJobInput.DatabaseName;
                            scheduledJob.State = (int)scheduledJobInput.State;

                            // Execution
                            scheduledJob.ExecuteTime = scheduledJobInput.ExecuteTime;
                            scheduledJob.ExecuteUserId = scheduledJobInput.ExecuteUserId;

                            // Recurrence
                            scheduledJob.RecurrenceType = (int)scheduledJobInput.RecurrenceType;
                            scheduledJob.RecurrenceCount = scheduledJobInput.RecurrenceCount;
                            scheduledJob.RecurrenceDate = scheduledJobInput.RecurrenceDate;
                            scheduledJob.RecurrenceInterval = scheduledJobInput.RecurrenceInterval;

                            // Error hadling
                            scheduledJob.RetryTypeForInternalError = (int)scheduledJobInput.RetryTypeForInternalError;
                            scheduledJob.RetryTypeForExternalError = (int)scheduledJobInput.RetryTypeForExternalError;
                            scheduledJob.RetryCount = scheduledJobInput.RetryCount;

                            // Settings
                            scheduledJob.AllowParallelExecution = scheduledJobInput.AllowParallelExecution;

                            SetModifiedPropertiesOnEntity(scheduledJob);

                            #endregion
                        }

                        #endregion

                        #region SysJobSetting

                        #region SysJobSetting Update/Delete
                        List<SysJobSetting> deleteSettings = new List<SysJobSetting>();
                        // Update or Delete existing SysJobSettings
                        foreach (SysJobSetting setting in scheduledJob.SysJobSettingScheduledJob.Select(s => s.SysJobSetting))
                        {
                            // Try get setting from input
                            SysJobSettingDTO settingInput = (from s in settingsInput
                                                             where s.SysJobSettingId == setting.SysJobSettingId
                                                             select s).FirstOrDefault();

                            if (settingInput != null)
                            {
                                #region SysJobSetting Update

                                // Update existing setting
                                setting.DataType = (int)settingInput.DataType;
                                setting.Name = settingInput.Name;
                                setting.StrData = settingInput.StrData;
                                setting.IntData = settingInput.IntData;
                                setting.DecimalData = settingInput.DecimalData;
                                setting.BoolData = settingInput.BoolData;
                                setting.DateData = settingInput.DateData;
                                setting.TimeData = settingInput.TimeData;

                                #endregion
                            }
                            else
                            {
                                #region SysJobSetting Delete
                                deleteSettings.Add(setting);
                                #endregion
                            }
                        }

                        if (deleteSettings.Any())
                        {
                            foreach (var setting in deleteSettings)
                            {
                                entities.SysJobSettingJob.RemoveRange(setting.SysJobSettingJob.ToList());
                                entities.SysJobSetting.Remove(setting);
                            }
                        }


                        #endregion

                        #region SysJobSetting Add

                        // Get new settings
                        IEnumerable<SysJobSettingDTO> settingsToAdd = (from s in settingsInput
                                                                       where s.SysJobSettingId == 0
                                                                       select s).ToList();

                        foreach (SysJobSettingDTO settingToAdd in settingsToAdd)
                        {
                            // Add setting to job
                            var setting = new SysJobSetting()
                            {
                                Type = (int)SysJobSettingType.Job,
                                DataType = (int)settingToAdd.DataType,
                                Name = settingToAdd.Name,
                                StrData = settingToAdd.StrData,
                                IntData = settingToAdd.IntData,
                                DecimalData = settingToAdd.DecimalData,
                                BoolData = settingToAdd.BoolData,
                                DateData = settingToAdd.DateData,
                                TimeData = settingToAdd.TimeData
                            };

                            var map = new SysJobSettingScheduledJob()
                            {
                                SysScheduledJob = scheduledJob,
                                SysJobSetting = setting
                            };

                            scheduledJob.SysJobSettingScheduledJob.Add(map);
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();

                            sysScheduledJobId = scheduledJob.SysScheduledJobId;
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
                    {
                        result.IntegerValue = sysScheduledJobId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Delete job with specified Id (Set state to deleted)
        /// </summary>
        /// <param name="sysJobId">Id of job to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteScheduledJob(int sysScheduledJobId)
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                // Get job
                SysScheduledJob job = GetScheduledJob(entities, sysScheduledJobId, false, false);
                if (job == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "SysScheduledJob");

                // ChangeEntityState cannot be used here since it doesn't use SoeEntityState
                job.State = (int)ScheduledJobState.Deleted;
                SetModifiedPropertiesOnEntity(job);

                ActionResult result = SaveChanges(entities);
                if (result.ObjectsAffected == 0)
                    result.Success = false;

                return result;
            }
        }

        public ActionResult ChangeScheduledJobState(SOESysEntities entities, SysScheduledJob sysScheduledJob, ScheduledJobState state, bool saveChanges)
        {
            ActionResult result = new ActionResult(true);

            // ChangeEntityState cannot be used here since it doesn't use SoeEntityState
            sysScheduledJob.State = (int)state;
            SetModifiedPropertiesOnEntity(sysScheduledJob);

            if (saveChanges)
                result = SaveChanges(entities);

            return result;

        }

        public ActionResult ChangeScheduledJobExecuteTime(SOESysEntities entities, SysScheduledJob sysScheduledJob, DateTime time, bool saveChanges)
        {
            ActionResult result = new ActionResult(true);

            sysScheduledJob.ExecuteTime = time;
            SetModifiedPropertiesOnEntity(sysScheduledJob);

            if (saveChanges)
                result = SaveChanges(entities);

            return result;

        }

        public ActionResult ExecuteScheduledJobSync(int scheduledJobId, int batchNr)
        {
            return ExecuteScheduledJobSync(GetScheduledJob(scheduledJobId, true, true).ToDTO(true, true, true), batchNr);
        }

        public ActionResult ExecuteScheduledJobSync(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            try
            {
                return this.ExecuteScheduledJobThreadSafe(scheduledJob, batchNr);
            }
            catch (Exception ex)
            {
                return new ActionResult(false)
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                };
            }
        }

        public ActionResult ExecuteScheduledJobAsync(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            ActionResult result = new ActionResult();
            try
            {
                var runningJobs = this.GetScheduledJobsByState(DateTime.Now, ScheduledJobState.Running).ToDTOs(true, true, false).ToList();
                bool runJob = runningJobs.Count == 0 && runningJobsServer.Count == 0;
                if (!runJob)
                {
                    // Check that this current job is NOT running in db and on this server
                    if (!runningJobs.Any(j => j.SysScheduledJobId == scheduledJob.SysScheduledJobId) &&
                       !runningJobsServer.Any(j => j.Key.SysScheduledJobId == scheduledJob.SysScheduledJobId))
                    {
                        if (scheduledJob.AllowParallelExecution)
                            runJob = true;
                        else if (runningJobs.All(j => j.AllowParallelExecution) && runningJobsServer.All(j => j.Key.AllowParallelExecution))
                            runJob = true;
                    }
                    else if (runningJobsServer.Any(j => j.Key.SysScheduledJobId == scheduledJob.SysScheduledJobId))
                    {
                        // This job is running on this server, check if it's busy
                        var alreadyRunningWorker = runningJobsServer.FirstOrDefault(j => j.Key.SysScheduledJobId == scheduledJob.SysScheduledJobId);
                        if (alreadyRunningWorker.Value != null && alreadyRunningWorker.Value.IsBusy)
                        {
                            // Log error here
                            LogError("ExecuteScheduledJob: Worker is already running but the scheduled job is in the wrong state");
                            runJob = false;
                        }
                        else
                        {
                            // Worker is not busy, so we can cancel it and remove it from the running jobs
                            if (alreadyRunningWorker.Value.WorkerSupportsCancellation)
                                alreadyRunningWorker.Value.CancelAsync();
                            runningJobsServer.Remove(alreadyRunningWorker.Key);
                            runJob = true;
                        }
                    }

                    if (!runJob)
                    {
                        // Error handling, remove all running jobs that has not made any log entries for the last 2 hours
                        foreach (var runningJob in this.GetOldRunningJobs(DateTime.Now.AddHours(-2)))
                        {
                            //Check if it's running on this server
                            var runningWorker = runningJobsServer.FirstOrDefault(j => j.Key.SysScheduledJobId == runningJob.SysScheduledJobId);
                            if (runningWorker.Value == null || !runningWorker.Value.IsBusy)
                            {
                                this.SaveScheduledJobState(runningJob.SysScheduledJobId, ScheduledJobState.Interrupted);
                                if (runningWorker.Key != null)
                                    runningJobsServer.Remove(runningWorker.Key);
                            }
                        }
                    }
                }

                if (!runJob)
                    return new ActionResult(false);

                // Set up worker
                BackgroundWorker worker = new BackgroundWorker()
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = true,
                };
                // Not using lamba here in order to make sure it's thread safe
                worker.DoWork += scheduledJobWorker_DoWork;
                worker.RunWorkerCompleted += (o, e) =>
                    {
                        try
                        {
                            runningJobsServer.Remove(scheduledJob);

                            using (var entities = new SOESysEntities())
                            {
                                // Error handling, Verify that this job is not set as running
                                var j = this.GetScheduledJob(entities, scheduledJob.SysScheduledJobId, false, false);
                                if (j.State == (int)ScheduledJobState.Running)
                                {
                                    j.State = (int)ScheduledJobState.Active;
                                    SaveChanges(entities);
                                }

                                if (e.Error != null)
                                {
                                    string msg = "Unhandled exception by job with SysScheduledJobId=" + scheduledJob.SysScheduledJobId + "(" + scheduledJob.Name + "). BatchNr: " + batchNr + ". Exception: " + e.Error.Message;
                                    DateTime? nextRunTime = null;
                                    string nextRunTimeText;

                                    if (j.RetryTypeForExternalError == (int)ScheduledJobRetryType.Immediately && j.RetryCount == 0)
                                    {
                                        j.State = (int)ScheduledJobState.Active;
                                        j.RetryCount++;
                                        j.ExecuteTime = DateTime.Now;
                                        nextRunTime = DateTime.Now;
                                        SaveChanges(entities);
                                    }
                                    else if (j.RetryTypeForExternalError == (int)ScheduledJobRetryType.NextInterval)
                                    {
                                        j.State = (int)ScheduledJobState.Active;
                                        nextRunTime = this.ReScheduleScheduledJob(entities, j, true, true);
                                    }
                                    else
                                    {
                                        j.State = (int)ScheduledJobState.Interrupted;
                                        SaveChanges(entities);
                                    }

                                    if (nextRunTime.HasValue)
                                        nextRunTimeText = Environment.NewLine + "Job will try to run again at " + nextRunTime.Value;
                                    else
                                        nextRunTimeText = Environment.NewLine + "Job will not run again";

                                    // Log
                                    this.LogExecuteJobError(e.Error, j.SysScheduledJobId, batchNr, nextRunTimeText);
                                }
                                else if (e.Cancelled)
                                {
                                    this.LogExecuteJobError(new OperationCanceledException("Worker was cancelled"), scheduledJob.SysScheduledJobId, batchNr);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.LogExecuteJobError(ex, scheduledJob.SysScheduledJobId, batchNr);
                        }
                    };

                // Actually we would like to update the savescheduledjobstate here, but the jobs doesn't expect this so it might be dangerus
                // this.SaveScheduledJobState(scheduledJob.SysScheduledJobId, ScheduledJobState.Running);
                runningJobsServer.Add(scheduledJob, worker);
                runningJobs.Add(scheduledJob);

                // Run worker now
                worker.RunWorkerAsync(new object[] { scheduledJob.SysScheduledJobId, batchNr });
            }
            catch (Exception ex)
            {
                if (ex is System.ArgumentNullException || ex is System.IndexOutOfRangeException)
                {
                    // Error handling
                    runningJobsServer.Clear();
                }

                if (scheduledJob != null)
                    this.LogExecuteJobError(ex, scheduledJob.SysScheduledJobId, batchNr);

                result.Success = false;
                result.Exception = ex;
            }

            return result;
        }

        private ActionResult ExecuteScheduledJobThreadSafe(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            // CRITICAL, this method is used by a background worker and needs to be thread safe, only use variables inside scope
            ActionResult result = new ActionResult();

            // Get assembly name and class name from SysJob
            string assemblyName = String.Empty;
            string className = String.Empty;
            if (scheduledJob.SysJob != null)
            {
                // assemblyName = scheduledJob.SysJob.AssemblyName;
                className = scheduledJob.SysJob.ClassName;
            }
            else
            {
                // Get SysJob
                SysJob sysJob = GetJob(scheduledJob.SysJobId, false, false);
                if (sysJob != null)
                {
                    //assemblyName = sysJob.AssemblyName;
                    className = sysJob.ClassName;
                }
            }

            assemblyName = "SoftOne.Soe.Business";

            // Create class
            object cls = ObjectFactory.Create(assemblyName, className);

            // Setup parameters
            object[] parameters = new object[2];
            parameters[0] = scheduledJob;
            parameters[1] = batchNr;

            // Call execute method on class
            if (scheduledJob.Type == ScheduledJobType.Service && cls is IScheduledService)
            {
                ((IScheduledService)cls).ExecuteService(scheduledJob);
            }
            else
            {
                // Do not check type here, maybe we should?
                MethodInfo methodInfo = typeof(IScheduledJob).GetMethod("Execute");
                methodInfo.Invoke(cls, parameters);
            }

            return result;
        }

        private void scheduledJobWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var arguments = e.Argument as object[];
            int sysScheduledJobId;
            int batchNr;
            SysScheduledJobDTO scheduledJob;

            try
            {
                sysScheduledJobId = (int)arguments[0];
                batchNr = (int)arguments[1];
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Worker arguments are not valid", ex);
            }

            // Declare entities in order to keep it thread safe

            using (var entities = new SOESysEntities())
            {
                scheduledJob = this.GetScheduledJob(entities, sysScheduledJobId, true, true).ToDTO(true, true, false);
            }

            this.ExecuteScheduledJobThreadSafe(scheduledJob, batchNr);
        }

        public ActionResult CheckOutScheduledJob(SysScheduledJobDTO sysScheduledJob, int batchNr)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    // Get original scheduled job
                    SysScheduledJob originalSysScheduledJob = GetScheduledJob(entities, sysScheduledJob.SysScheduledJobId, false, false);
                    if (originalSysScheduledJob == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "SysScheduledJob");

                    if (originalSysScheduledJob.State == (int)ScheduledJobState.Running && batchNr != 0)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Job already running";
                    }
                    else
                    {
                        originalSysScheduledJob.LastStartTime = DateTime.Now;
                        result = ChangeScheduledJobState(entities, originalSysScheduledJob, ScheduledJobState.Running, false);

                        if (result.Success)
                        {
                            result = SaveChanges(entities);
                            if (result.ObjectsAffected == 0)
                                result.Success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
            }

            // Create log entry
            string msg = result.Success ? "Schemalagt jobb startat" : String.Format("Fel vid utcheckning av jobb: ({0}) '{1}'", result.ErrorNumber, result.ErrorMessage);
            CreateLogEntry(sysScheduledJob.SysScheduledJobId, batchNr, result.Success ? ScheduledJobLogLevel.Information : ScheduledJobLogLevel.Error, msg);

            Thread.Sleep(300);
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            if (batchNr != 0 && sysEntitiesReadOnly.SysScheduledJobLog.Where(w => w.BatchNr == batchNr && w.SysScheduledJobId == sysScheduledJob.SysScheduledJobId).Count() > 1)
            {
                CreateLogEntry(sysScheduledJob.SysScheduledJobId, batchNr, result.Success ? ScheduledJobLogLevel.Information : ScheduledJobLogLevel.Error, "More than 1 entry on batchNr");
                return new ActionResult(false);
            }

            return result;
        }

        public ActionResult CheckInScheduledJob(SysScheduledJobDTO sysScheduledJob, int batchNr, bool success, TimeSpan jobExecutionTime = default(TimeSpan))
        {
            ActionResult result = new ActionResult();
            //bool? willRetry = false;

            using (SOESysEntities entities = new SOESysEntities())
            {
                try
                {
                    // Get original scheduled job
                    SysScheduledJob originalSysScheduledJob = GetScheduledJob(entities, sysScheduledJob.SysScheduledJobId, false, false);
                    if (originalSysScheduledJob == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, "SysScheduledJob");
                        return result;
                    }

                    // Change state
                    if (success)
                        ChangeScheduledJobState(entities, originalSysScheduledJob, ScheduledJobState.Finished, false);
                    else
                        ChangeScheduledJobState(entities, originalSysScheduledJob, ScheduledJobState.Interrupted, false);

                    if (success)
                    {
                        this.ReScheduleScheduledJob(entities, originalSysScheduledJob);
                        originalSysScheduledJob.RetryCount = 0;
                    }
                    else
                    {
                        if (originalSysScheduledJob.RetryTypeForInternalError != (int)ScheduledJobRetryType.Abort)
                        {
                            // Retry
                            originalSysScheduledJob.State = (int)ScheduledJobState.Active;
                            //willRetry = true;
                            originalSysScheduledJob.RecurrenceInterval = originalSysScheduledJob.RecurrenceInterval.Trim();

                            // Update schedule based on retry type
                            switch ((ScheduledJobRetryType)originalSysScheduledJob.RetryTypeForInternalError)
                            {
                                case ScheduledJobRetryType.Immediately:
                                    originalSysScheduledJob.ExecuteTime = DateTime.Now;
                                    break;
                                case ScheduledJobRetryType.NextInterval:
                                    // Get new execution time based on interval
                                    originalSysScheduledJob.ExecuteTime = SchedulerUtility.GetNextExecutionTime(originalSysScheduledJob.RecurrenceInterval);
                                    break;
                            }
                        }
                    }

                    result = SaveChanges(entities);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.ErrorMessage = ex.Message;
                }
            }

            // Create log entry
            string msg = "Schemalagt jobb ";
            msg += success ? "avslutat" : "avbrutet";
            // TODO store executionTime in db?
            if (jobExecutionTime != default(TimeSpan))
                msg += ". Batch-tid: " + jobExecutionTime.ToLongTimeString() + "." + jobExecutionTime.Milliseconds;
            CreateLogEntry(sysScheduledJob.SysScheduledJobId, batchNr, success ? ScheduledJobLogLevel.Success : ScheduledJobLogLevel.Error, msg);
            if (!success)
            {
                // Send email
                //string emailMessage = "Fel vid incheckning av jobb. SysScheduledJobId: " + sysScheduledJob.SysScheduledJobId + " (" + sysScheduledJob.Name + "), BatchNr: " + batchNr + (willRetry.HasValue && willRetry == true ? ", Nytt försök kommer att göras" : string.Empty) + Environment.NewLine + result.GetErrorMsg();
                ////ActionResult emailResult = this.SendEmail("rickard.dahlgren@softone.se", msg, emailMessage, false, "xeteamet@softone.se", "johan.svanborg@softone.se");
                ////var emailResult = Soe.Util.EmailUtil.SendMailSMTP("Ekonomi@softone.se", "rickard.dahlgren@softone.se", msg, emailMessage, null, "xeteamet@softone.se", "johan.svanborg@softone.se");
                //if (!emailResult.Success)
                //    base.LogError(new SoeGeneralException("Could not send email. " + emailResult.ErrorMessage, this.ToString()), this.log);
                //else
                TermCacheManager.MailHasBeenSend = true;
            }

            if (!result.Success)
                CreateLogEntry(sysScheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, "Generellt fel vid incheckning av jobb");

            return result;
        }

        private DateTime? ReScheduleScheduledJob(SOESysEntities entities, SysScheduledJob originalSysScheduledJob, bool save = false, bool updateRetryCount = false)
        {
            DateTime? newExecuteTime = null;

            // Check recurrence
            if (originalSysScheduledJob.RecurrenceType != (int)ScheduledJobRecurrenceType.RunOnce)
            {
                if (updateRetryCount)
                    originalSysScheduledJob.RetryCount++;

                originalSysScheduledJob.RecurrenceInterval = originalSysScheduledJob.RecurrenceInterval.Trim();

                // Get new execution time
                newExecuteTime = SchedulerUtility.GetNextExecutionTime(originalSysScheduledJob.RecurrenceInterval);

                // Update schedule based on recurrence type and interval
                switch ((ScheduledJobRecurrenceType)originalSysScheduledJob.RecurrenceType)
                {
                    case ScheduledJobRecurrenceType.RunInfinite:
                        originalSysScheduledJob.ExecuteTime = newExecuteTime.Value;
                        originalSysScheduledJob.State = (int)ScheduledJobState.Active;
                        break;
                    case ScheduledJobRecurrenceType.RunNbrOfTimes:
                        if (originalSysScheduledJob.RecurrenceCount > 1)
                        {
                            originalSysScheduledJob.RecurrenceCount--;
                            originalSysScheduledJob.ExecuteTime = newExecuteTime.Value;
                            originalSysScheduledJob.State = (int)ScheduledJobState.Active;
                        }
                        break;
                    case ScheduledJobRecurrenceType.RunUntilDate:
                        if (originalSysScheduledJob.RecurrenceDate >= newExecuteTime)
                        {
                            originalSysScheduledJob.ExecuteTime = newExecuteTime.Value;
                            originalSysScheduledJob.State = (int)ScheduledJobState.Active;
                        }
                        break;
                }

                if (save && !SaveChanges(entities).Success)
                    return null;
            }

            return newExecuteTime;
        }

        public ActionResult SendEmail(String to, String subject, String body, bool emailcontentIsHtml, params string[] cc)
        {
            var listCC = cc == null ? new List<string>() : cc.ToList();
            // I belive from must be from an external address, please verify?
            return EmailManager.SendEmail("XE_NoReply@softone.se", to, listCC, subject, body, false);
        }

        public ActionResult PostponeScheduledJob(SysScheduledJobDTO sysScheduledJob, int minutes)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (SOESysEntities entities = new SOESysEntities())
                {
                    // Get original scheduled job
                    SysScheduledJob originalSysScheduledJob = GetScheduledJob(entities, sysScheduledJob.SysScheduledJobId, false, false);
                    if (originalSysScheduledJob == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "SysScheduledJob");

                    result = ChangeScheduledJobExecuteTime(entities, originalSysScheduledJob, originalSysScheduledJob.ExecuteTime.AddMinutes(5), false);
                    if (result.Success)
                    {
                        result = ChangeScheduledJobState(entities, originalSysScheduledJob, ScheduledJobState.Active, false);
                        if (result.Success)
                        {
                            result = SaveChanges(entities);
                            if (result.ObjectsAffected == 0)
                                result.Success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
            }

            return result;
        }

        #endregion

        #region SysScheduledJobLog

        /// <summary>
        /// Get highest uses batch number from the log table
        /// </summary>
        /// <returns>Batch number</returns>
        public int GetLatestBatchNr()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysScheduledJobLog.Max(l => l.BatchNr);
        }

        /// <summary>
        /// Create an entry in the SysScheduledJobLog table
        /// </summary>
        /// <param name="sysScheduledJobId">Id of the scheduled job to relate the log entry to</param>
        /// <param name="batchNr">Execution batch number</param>
        /// <param name="level">Log level</param>
        /// <param name="message">Log message</param>
        /// <returns>ActionResult from the insert</returns>
        public ActionResult CreateLogEntry(int sysScheduledJobId, int batchNr, ScheduledJobLogLevel level, string message)
        {
            if (sysScheduledJobId == 0)
            {
                if (level == ScheduledJobLogLevel.Error)
                    base.LogError("Scheduled job error found but no job id specified for log. Message: " + message);
                return new ActionResult(true);
            }

            using (SOESysEntities entities = new SOESysEntities())
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_SUPPRESS, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    // Creat new log entry
                    SysScheduledJobLog jobLog = new SysScheduledJobLog()
                    {
                        BatchNr = batchNr,
                        LogLevel = (int)level,
                        Time = DateTime.Now,
                        Message = message,

                        //Set FK
                        SysScheduledJobId = sysScheduledJobId,
                    };

                    entities.SysScheduledJobLog.Add(jobLog);
                    return SaveChanges(entities);
                }
            }
        }

        /// <summary>
        /// Get all log entrys for specified scheduled job
        /// </summary>
        /// <param name="sysscheduledJobId">Scheduled job Id</param>
        /// <returns>Collection of log entrys</returns>
        public IEnumerable<SysScheduledJobLog> GetScheduledJobLogs(int sysScheduledJobId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var query = (from l in sysEntitiesReadOnly.SysScheduledJobLog
                         where l.SysScheduledJobId == sysScheduledJobId
                         select l).OrderByDescending(l => l.Time).Take(500).ToList();

            return SetLogLevelName(query);
        }

        public IEnumerable<SysScheduledJob> GetOldRunningJobs(DateTime olderThanLogEntryTime)
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                var query = (from j in entities.SysScheduledJob
                             where j.State == (int)ScheduledJobState.Running
                             //&& !j.FK_SysScheduledJobLog_SysScheduledJob.Any(l => l.Time > olderThanLogEntryTime)
                             //&& !(l.Time > olderThanLogEntryTime)
                             select j).Distinct();

                foreach (var item in query.ToList())
                {
                    // Check if any running job has newer log entries then "olderThenLogEntryTime"
                    bool hasNewLogFiles = (from l in entities.SysScheduledJobLog
                                           where l.SysScheduledJob.SysScheduledJobId == item.SysScheduledJobId
                                           orderby l.Time descending
                                           select l.Time).FirstOrDefault() > olderThanLogEntryTime;

                    if (!hasNewLogFiles)
                        yield return item;
                }
            }
        }

        private void LogExecuteJobError(Exception ex, int sysScheduledJobId, int batchNr, string message = null)
        {
            if (ex == null)
                return;

            string msg = String.Format("Fel vid exekvering av jobb: Exception: {0}", ex.Message);
            if (ex.InnerException != null)
                msg += "\n" + ex.InnerException.Message;

            if (!string.IsNullOrEmpty(message))
                msg += message;

            CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, msg);
        }

        private IEnumerable<SysScheduledJobLog> SetLogLevelName(IEnumerable<SysScheduledJobLog> jobLogs)
        {
            List<GenericType> levels = base.GetTermGroupContent(TermGroup.ScheduledJobLogLevel);

            // Set virtual field LogLevelName
            foreach (SysScheduledJobLog jobLog in jobLogs)
            {
                GenericType level = levels.FirstOrDefault(l => l.Id == jobLog.LogLevel);
                if (level != null)
                    jobLog.LogLevelName = level.Name;
            }

            return jobLogs;
        }

        #endregion

        public void Dispose()
        {

        }
    }
}
