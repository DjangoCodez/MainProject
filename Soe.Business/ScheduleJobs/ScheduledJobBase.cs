using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class ScheduledJobBase
    {
        #region Constants

        protected const string THREAD = "Scheduler";

        #endregion

        #region Variables

        protected ParameterObject parameterObject;
        protected SysScheduledJobManager jm;
        protected CompanyManager cm;
        
        protected SysScheduledJobDTO scheduledJob;
        protected int batchNr;
        protected string errorMsg = string.Empty;
        private readonly Stopwatch watch = new Stopwatch();
        private string email;
        private string cc;

        #endregion

        #region Protected methods

        protected void Init(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            this.scheduledJob = scheduledJob;
            this.batchNr = batchNr;

            string settingEmail = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "email").Select(s => s.StrData).FirstOrDefault();
            if (!string.IsNullOrEmpty(settingEmail))
                this.email = settingEmail;

            string settingCC = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "cc").Select(s => s.StrData).FirstOrDefault();
            if (!string.IsNullOrEmpty(settingCC))
                this.cc = settingCC;

            this.parameterObject = GetParameterObject(0, this.scheduledJob.ExecuteUserId);
            this.jm = new SysScheduledJobManager(parameterObject);
            this.cm = new CompanyManager(parameterObject);
        }

        /// <summary>
        /// Get a ParameterObject to pass to methods where its needed. 
        /// I.e. methods that will interact with TermCacheManager.
        /// </summary>
        /// <returns></returns>
        protected ParameterObject GetParameterObject(int actorCompanyId, int userId)
        {
            Company company = actorCompanyId > 0 ? new CompanyManager(null).GetCompany(actorCompanyId, true) : null;
            User user = userId > 0 ? new UserManager(null).GetUser(userId) : new User() { LoginName = this.ToString() };

            return ParameterObject.Create(
                user: user.ToDTO(),
                company: company?.ToCompanyDTO(),
                thread: THREAD,
                roleId: new UserManager(null).GetDefaultRoleId(actorCompanyId, userId));
        }

        protected ActionResult CheckOutScheduledJob()
        {
            this.watch.Start();
            return jm.CheckOutScheduledJob(scheduledJob, batchNr);
        }

        protected ActionResult CheckInScheduledJob(bool success)
        {
            this.watch.Stop();
            if (!string.IsNullOrEmpty(errorMsg) && !string.IsNullOrEmpty(email))
                this.SendEmail();

            return jm.CheckInScheduledJob(scheduledJob, batchNr, success);
        }

        protected DateTime? LastStartOfJob(int sysScheduleJobId, string startMessage)
        {
            var startTime = jm.GetScheduledJob(sysScheduleJobId)?.LastStartTime;
            if (startTime.HasValue)
                return startTime;

            return jm.LastStartOfJob(sysScheduleJobId, startMessage);
        }

        protected List<Company> GetCompanies(int? paramCompanyId = null, bool loadLicense = false)
        {
            return paramCompanyId.HasValue ? cm.GetCompany(paramCompanyId.Value, loadLicense: loadLicense).ObjToList() : cm.GetCompanies(loadLicense: loadLicense);
        }

        protected void CreateLogEntry(ScheduledJobLogLevel level, string message)
        {
            if (level == ScheduledJobLogLevel.Error)
                errorMsg += message + Environment.NewLine;

            jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, level, message);
        }

        protected void LogError(Exception ex)
        {
            LogCollector.LogError(ex, "Fel vid exekvering av jobb");
        }

        #endregion

        #region Private methods

        private ActionResult SendEmail()
        {
            if (string.IsNullOrEmpty(email))
                return new ActionResult();

            var emailManager = new EmailManager(null);
            var listCC = cc == null ? new List<string>() : cc.Split(';').ToList();
            // I belive from must be from an external address, please verify?
            return emailManager.SendEmail("XE_NoReply@softone.se", email, listCC, "Error when executing scheduled job " + this.scheduledJob.Name, this.errorMsg, false);
        }

        #endregion
    }
}
