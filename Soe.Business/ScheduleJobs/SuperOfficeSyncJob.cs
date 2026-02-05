using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.ExternalMiddleService;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SuperOfficeSyncJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            try
            {
                #region Init
                // Check out scheduled job
                base.Init(scheduledJob, batchNr);

                ActionResult result = CheckOutScheduledJob();

                // Execute job
                CreateLogEntry(ScheduledJobLogLevel.Information, "Startar SuperOffice jobb");

                CompanyManager cm = new CompanyManager(parameterObject);
                SettingManager sm = new SettingManager(parameterObject);

                // Get mandatory parameters
                //int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
                string paramCompanyApiKey = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "companyapikey").Select(s => s.StrData).FirstOrDefault();
                int? paramCustomerId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "customerid").Select(s => s.IntData).FirstOrDefault();
                string paramCountryName = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "countryname").Select(s => s.StrData).FirstOrDefault();
                int? paramDaysBack = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "daysback").Select(s => s.IntData)?.FirstOrDefault();


                CreateLogEntry(ScheduledJobLogLevel.Information, "paramCompanyApiKey = " + (paramCompanyApiKey));
                CreateLogEntry(ScheduledJobLogLevel.Information, "customerid = " + (paramCustomerId.HasValue ? paramCustomerId.Value.ToString() : ""));
                CreateLogEntry(ScheduledJobLogLevel.Information, "countryname = " + (!string.IsNullOrEmpty(paramCountryName) ? paramCountryName.ToString() : "Sweden"));
                if (paramCompanyApiKey == null)
                    return;

                #endregion
                //if (paramCompanyId != null)
                //{
                    
                //}
                //SuperOfficeManager superOfficeManager = new SuperOfficeManager(parameterObject);
                //result = superOfficeManager.Sync(paramCompanyId.Value, paramCustomerId, paramCountryName, paramDaysBack);
                result = EMSSuperOfficeConnector.StartSync(paramCompanyApiKey,  paramCustomerId, paramCountryName, paramDaysBack);

                if (result.Success)
                {
                    // Check in scheduled job
                    CheckInScheduledJob(result.Success);
                }

            }
            catch (Exception ex)
            {
                string msg = String.Format("Fel vid exekvering av jobb: {0}. {1}", ex.Message, SoftOne.Soe.Util.Exceptions.SoeException.GetStackTrace());
                if (ex.InnerException != null)
                    msg += "\n" + ex.InnerException.Message;
                CreateLogEntry(ScheduledJobLogLevel.Error, msg);
                base.LogError(ex);

                CheckInScheduledJob(false);
            }
        }
   }

}
